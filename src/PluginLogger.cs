using System;
using System.IO;

namespace CESDK
{
    public static class PluginLogger
    {
        private static readonly string logPath = $"./{CESDK.CurrentPlugin?.Name ?? "CESDK_ERROR"}.log";

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} - {message}\n");
            }
            catch { /* ignore */ }
        }

        public static void LogException(Exception ex)
        {
            Log($"EXCEPTION: {ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}