using System;
using Microsoft.Owin.Hosting;
using PulseChatServer.Data;
using PulseChatServer.Utils;

namespace PulseChatServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:5000";

            // Show banner
            Logger.Banner();

            try
            {
                // Initialize database
                Logger.Info("Initializing database...");
                DatabaseManager.Initialize();

                // Initialize image storage
                ImageStorage.EnsureDirectoryExists();

                // Start SignalR server
                Logger.Info("Starting SignalR server...");

                using (WebApp.Start<Startup>(url))
                {
                    Logger.Info($"Server started on {url}");
                    Logger.Info("Waiting for client connections...");

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine();
                    Console.WriteLine("  ─────────────────────────────────────────────");
                    Console.WriteLine("  Press [Enter] to stop the server");
                    Console.WriteLine("  ─────────────────────────────────────────────");
                    Console.ResetColor();
                    Console.WriteLine();

                    Console.ReadLine();

                    Logger.Info("Server shutting down...");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Server failed to start: {ex.Message}");

                if (ex.Message.Contains("access") || ex.Message.Contains("denied") || ex.Message.Contains("reserved"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("  ⚠️  Try running as Administrator, or run this command:");
                    Console.WriteLine($"     netsh http add urlacl url={url}/ user=Everyone");
                    Console.ResetColor();
                }

                Console.WriteLine();
                Console.WriteLine("  Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
