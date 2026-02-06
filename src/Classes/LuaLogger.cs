using System;
using System.Globalization;
using CESDK.Utils;

namespace CESDK.Classes
{
    public class LuaLoggerException : CesdkException
    {
        public LuaLoggerException(string message) : base(message) { }
        public LuaLoggerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class LuaLogger
    {
        public static void Print(string? message)
        {
            try
            {
                LuaUtils.CallVoidLuaFunction("print", "print to Lua console", message ?? "");
            }
            catch (InvalidOperationException ex)
            {
                throw new LuaLoggerException($"Error calling Lua print(): {ex.Message}", ex);
            }
        }

        public static void Print(params object?[] values)
        {
            if (values == null || values.Length == 0)
            {
                Print("");
                return;
            }

            // Convert all values to strings and print as single concatenated message
            var parts = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
                parts[i] = values[i]?.ToString() ?? "nil";

            Print(string.Join("\t", parts));
        }

        public static void Printf(string format, params object?[] args)
        {
            try
            {
                Print(string.Format(CultureInfo.InvariantCulture, format, args));
            }
            catch (FormatException ex)
            {
                throw new LuaLoggerException($"String formatting failed: {ex.Message}", ex);
            }
        }

        public static void PrintError(Exception? exception, bool includeStackTrace = true)
        {
            if (exception == null)
            {
                Print("Error: (null exception)");
                return;
            }

            Print($"ERROR: {exception.GetType().Name}: {exception.Message}");

            if (includeStackTrace && !string.IsNullOrEmpty(exception.StackTrace))
                Print($"Stack Trace:\n{exception.StackTrace}");

            var inner = exception.InnerException;
            while (inner != null)
            {
                Print($"  Inner Exception: {inner.GetType().Name}: {inner.Message}");
                inner = inner.InnerException;
            }
        }

        public static void PrintTimestamped(string? message, string format = "yyyy-MM-dd HH:mm:ss")
        {
            var timestamp = DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
            Print($"[{timestamp}] {message ?? ""}");
        }

        public static void PrintDebug(string? message) => Print($"DEBUG: {message ?? ""}");
        public static void PrintWarning(string? message) => Print($"WARNING: {message ?? ""}");
        public static void PrintInfo(string? message) => Print($"INFO: {message ?? ""}");

        public static bool TryPrint(string? message)
        {
            try { Print(message); return true; }
            catch (LuaLoggerException) { return false; }
        }
    }
}