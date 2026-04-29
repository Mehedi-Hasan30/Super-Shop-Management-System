using System;
using System.IO;

namespace Super_Shop_Management_System.Helpers
{
    public static class ErrorLogger
    {
        private static readonly object FileLock = new object();

        public static void Log(string context, Exception ex)
        {
            try
            {
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string path = Path.Combine(directory, $"errors_{DateTime.Now:yyyyMMdd}.log");
                string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}{Environment.NewLine}{ex}{Environment.NewLine}";

                lock (FileLock)
                {
                    File.AppendAllText(path, message);
                }
            }
            catch
            {
            }
        }
    }
}
