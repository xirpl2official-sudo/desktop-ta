using System;
using System.Runtime.InteropServices;

namespace AbsenSholat.Services
{
    /// <summary>
    /// Logger service that outputs to a Win32 console window for debugging.
    /// </summary>
    public static class Logger
    {
        private static bool _consoleAllocated = false;
        private static readonly object _lock = new object();

        // Win32 API for console allocation
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleTitle(string lpConsoleTitle);

        private const int STD_OUTPUT_HANDLE = -11;

        /// <summary>
        /// Initialize the console window for logging
        /// </summary>
        public static void Initialize()
        {
            lock (_lock)
            {
                if (!_consoleAllocated)
                {
                    AllocConsole();
                    SetConsoleTitle("AbsenSholat - Debug Console");
                    _consoleAllocated = true;

                    // Set console colors
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Clear();

                    // Print header
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║           ABSEN SHOLAT - DEBUG CONSOLE                       ║");
                    Console.WriteLine("║           Application Logger v1.0                            ║");
                    Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();

                    Info("Logger", "Console initialized successfully");
                }
            }
        }

        /// <summary>
        /// Close the console window
        /// </summary>
        public static void Shutdown()
        {
            lock (_lock)
            {
                if (_consoleAllocated)
                {
                    Info("Logger", "Shutting down console...");
                    FreeConsole();
                    _consoleAllocated = false;
                }
            }
        }

        /// <summary>
        /// Log an informational message
        /// </summary>
        public static void Info(string source, string message)
        {
            Log(LogLevel.Info, source, message);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void Warning(string source, string message)
        {
            Log(LogLevel.Warning, source, message);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void Error(string source, string message)
        {
            Log(LogLevel.Error, source, message);
        }

        /// <summary>
        /// Log an error with exception details
        /// </summary>
        public static void Error(string source, string message, Exception ex)
        {
            Log(LogLevel.Error, source, $"{message}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Log(LogLevel.Error, source, $"  Inner: {ex.InnerException.Message}");
            }
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void Debug(string source, string message)
        {
            Log(LogLevel.Debug, source, message);
        }

        /// <summary>
        /// Log a success message
        /// </summary>
        public static void Success(string source, string message)
        {
            Log(LogLevel.Success, source, message);
        }

        private static void Log(LogLevel level, string source, string message)
        {
            if (!_consoleAllocated) return;

            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var originalColor = Console.ForegroundColor;

                // Print timestamp
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"[{timestamp}] ");

                // Print level with color
                Console.ForegroundColor = GetLevelColor(level);
                Console.Write($"[{level.ToString().ToUpper().PadRight(7)}] ");

                // Print source
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{source}] ");

                // Print message
                Console.ForegroundColor = GetMessageColor(level);
                Console.WriteLine(message);

                Console.ForegroundColor = originalColor;
            }
        }

        private static ConsoleColor GetLevelColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.Cyan,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Success => ConsoleColor.Green,
                _ => ConsoleColor.White
            };
        }

        private static ConsoleColor GetMessageColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Success => ConsoleColor.Green,
                _ => ConsoleColor.White
            };
        }

        private enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Success
        }
    }
}
