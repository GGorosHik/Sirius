using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Server.Helper
{
    public static class ConsoleLogger
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private static bool consoleAllocated = false;
        private static StreamWriter consoleWriter;

        public static void Initialize()
        {
            if (!consoleAllocated)
            {
                AllocConsole();
                consoleAllocated = true;
                
                // Redirect console output
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
                
                // Set console title
                Console.Title = "Liberium RAT - Debug Console";
                
                // Set console colors for better visibility
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Clear();
                
                LogInfo("=== Liberium RAT Console Logger Initialized ===");
                LogInfo($"Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                LogInfo("This console will display all RAT operations and network activity.");
                LogInfo("===============================================");
            }
        }

        public static void LogInfo(string message)
        {
            if (consoleAllocated)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [INFO] {message}");
            }
        }

        public static void LogSuccess(string message)
        {
            if (consoleAllocated)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [SUCCESS] {message}");
            }
        }

        public static void LogWarning(string message)
        {
            if (consoleAllocated)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [WARNING] {message}");
            }
        }

        public static void LogError(string message)
        {
            if (consoleAllocated)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ERROR] {message}");
            }
        }

        public static void LogNetwork(string message)
        {
            if (consoleAllocated)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [NETWORK] {message}");
            }
        }

        public static void LogBuild(string message)
        {
            if (consoleAllocated)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [BUILD] {message}");
            }
        }

        public static void LogClient(string message)
        {
            if (consoleAllocated)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [CLIENT] {message}");
            }
        }

        public static void LogRemoteDesktop(string message)
        {
            if (consoleAllocated)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [REMOTE_DESKTOP] {message}");
            }
        }

        public static void Cleanup()
        {
            if (consoleAllocated)
            {
                LogInfo("=== Console Logger Shutting Down ===");
                FreeConsole();
                consoleAllocated = false;
            }
        }

        public static void ShowConsole()
        {
            if (consoleAllocated)
            {
                IntPtr handle = GetConsoleWindow();
                ShowWindow(handle, SW_SHOW);
            }
        }

        public static void HideConsole()
        {
            if (consoleAllocated)
            {
                IntPtr handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);
            }
        }
    }
}

