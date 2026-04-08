using System;

namespace PulseChatServer.Utils
{
    public static class Logger
    {
        private static readonly object _lock = new object();

        public static void Banner()
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.WriteLine("  ╔══════════════════════════════════════════════╗");
                Console.WriteLine("  ║                                              ║");
                Console.WriteLine("  ║        ⚡ PULSECHAT SERVER v1.0 ⚡           ║");
                Console.WriteLine("  ║        Real-Time Messaging System            ║");
                Console.WriteLine("  ║                                              ║");
                Console.WriteLine("  ╚══════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public static void Info(string message)
        {
            Log("INFO", ConsoleColor.Cyan, message);
        }

        public static void Connection(string message)
        {
            Log("CONN", ConsoleColor.Green, message);
        }

        public static void Disconnect(string message)
        {
            Log("DISC", ConsoleColor.Yellow, message);
        }

        public static void Message(string message)
        {
            Log("MSG ", ConsoleColor.White, message);
        }

        public static void Image(string message)
        {
            Log("IMG ", ConsoleColor.Magenta, message);
        }

        public static void Error(string message)
        {
            Log("ERR ", ConsoleColor.Red, message);
        }

        public static void Auth(string message)
        {
            Log("AUTH", ConsoleColor.DarkCyan, message);
        }

        private static void Log(string tag, ConsoleColor color, string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  {DateTime.Now:HH:mm:ss}  ");
                Console.ForegroundColor = color;
                Console.Write($"[{tag}]");
                Console.ResetColor();
                Console.WriteLine($"  {message}");
            }
        }
    }
}
