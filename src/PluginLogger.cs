using System;
using System.IO;

namespace CESDK
{
    public static class PluginLogger
    {
        private static readonly string logPath = GetLogPath();

        private static string GetLogPath()
        {
            try
            {
                // Get the directory where Cheat Engine is running from
                var ceDirectory = AppContext.BaseDirectory;
                if (string.IsNullOrEmpty(ceDirectory))
                    ceDirectory = Environment.CurrentDirectory;

                // Create plugin-logs directory in CE directory
                var pluginLogsDir = Path.Combine(ceDirectory, "plugin-logs");
                Directory.CreateDirectory(pluginLogsDir);

                // Create log file path
                var pluginName = CESDK.CurrentPlugin?.Name ?? "CESDK_ERROR";
                return Path.Combine(pluginLogsDir, $"{pluginName}.log");
            }
            catch
            {
                // Fallback to current directory if something fails
                return $"./{CESDK.CurrentPlugin?.Name ?? "CESDK_ERROR"}.log";
            }
        }

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