using System;
using System.IO;
using PulseChatServer.Utils;

namespace PulseChatServer.Utils
{
    public static class ImageStorage
    {
        private static readonly string ImageFolder = @"C:\PulseChat\Images";
        private static readonly string FileFolder = @"C:\PulseChat\Files";

        public static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(ImageFolder))
            {
                Directory.CreateDirectory(ImageFolder);
                Logger.Info($"Image storage created: {ImageFolder}");
            }
            else
            {
                Logger.Info($"Image storage ready: {ImageFolder}");
            }

            if (!Directory.Exists(FileFolder))
            {
                Directory.CreateDirectory(FileFolder);
                Logger.Info($"File storage created: {FileFolder}");
            }
        }

        // ==================== IMAGE METHODS ====================

        public static string SaveImage(byte[] imageData, string extension)
        {
            EnsureDirectoryExists();
            string fileName = $"img_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 6)}{extension}";
            string fullPath = Path.Combine(ImageFolder, fileName);
            File.WriteAllBytes(fullPath, imageData);

            string relativePath = $"/Images/{fileName}";
            Logger.Image($"Image saved: {fileName} ({imageData.Length / 1024} KB)");
            return relativePath;
        }

        public static byte[] GetImageBytes(string relativePath)
        {
            string fileName = Path.GetFileName(relativePath);

            // Check both folders
            string imgPath = Path.Combine(ImageFolder, fileName);
            if (File.Exists(imgPath)) return File.ReadAllBytes(imgPath);

            string filePath = Path.Combine(FileFolder, fileName);
            if (File.Exists(filePath)) return File.ReadAllBytes(filePath);

            Logger.Error($"File not found: {fileName}");
            return null;
        }

        // ==================== FILE METHODS ====================

        public static string SaveFile(byte[] fileData, string originalFileName)
        {
            EnsureDirectoryExists();
            string ext = Path.GetExtension(originalFileName);
            string safeName = $"file_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 6)}{ext}";
            string fullPath = Path.Combine(FileFolder, safeName);
            File.WriteAllBytes(fullPath, fileData);

            string relativePath = $"/Files/{safeName}";
            Logger.Info($"File saved: {originalFileName} → {safeName} ({fileData.Length / 1024} KB)");
            return relativePath;
        }

        public static string GetFullPath(string relativePath)
        {
            string fileName = Path.GetFileName(relativePath);
            string imgPath = Path.Combine(ImageFolder, fileName);
            if (File.Exists(imgPath)) return imgPath;
            return Path.Combine(FileFolder, fileName);
        }
    }
}
