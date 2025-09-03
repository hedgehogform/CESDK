#nullable enable
using System;
using System.Globalization;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class LuaLoggerException : Exception
    {
        public LuaLoggerException(string message) : base(message) { }
        public LuaLoggerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class LuaLogger
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static void Print(string? message)
        {
            try
            {
                lua.GetGlobal("print");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new LuaLoggerException("print function not available in Lua environment");
                }

                lua.PushString(message ?? "");

                var result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new LuaLoggerException($"print() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not LuaLoggerException)
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

            try
            {
                lua.GetGlobal("print");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new LuaLoggerException("print function not available in Lua environment");
                }

                foreach (var value in values)
                {
                    var stringValue = value?.ToString() ?? "nil";
                    lua.PushString(stringValue);
                }

                var result = lua.PCall(values.Length, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new LuaLoggerException($"print() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not LuaLoggerException)
            {
                throw new LuaLoggerException($"Error calling Lua print(): {ex.Message}", ex);
            }
        }

        public static void Printf(string format, params object?[] args)
        {
            try
            {
                var message = string.Format(CultureInfo.InvariantCulture, format, args);
                Print(message);
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
            {
                Print($"Stack Trace:\n{exception.StackTrace}");
            }

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

        public static void PrintDebug(string? message)
        {
            Print($"DEBUG: {message ?? ""}");
        }

        public static void PrintWarning(string? message)
        {
            Print($"WARNING: {message ?? ""}");
        }

        public static void PrintInfo(string? message)
        {
            Print($"INFO: {message ?? ""}");
        }

        public static bool TryPrint(string? message)
        {
            try
            {
                Print(message);
                return true;
            }
            catch (LuaLoggerException)
            {
                return false;
            }
        }
    }
}