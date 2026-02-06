using System;
using CESDK.Utils;

namespace CESDK.Classes
{
    public class DebuggerException : CesdkException
    {
        public DebuggerException(string message) : base(message) { }
        public DebuggerException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Debugger-specific operations (attach, detach, debug output).
    /// For disassembly, use <see cref="Disassembler"/>.
    /// </summary>
    public static class Debugger
    {
        public static void DebugProcess() =>
            WrapException(() => LuaUtils.CallVoidLuaFunction("debugProcess", "start debugging process"));

        public static void DetachIfPossible() =>
            WrapException(() => LuaUtils.CallVoidLuaFunction("detachIfPossible", "detach debugger"));

        public static void OutputDebugString(string message) =>
            WrapException(() => LuaUtils.CallVoidLuaFunction("outputDebugString", "output debug string", message));

        private static void WrapException(Action operation)
        {
            try
            {
                operation();
            }
            catch (InvalidOperationException ex)
            {
                throw new DebuggerException(ex.Message, ex);
            }
        }
    }
}