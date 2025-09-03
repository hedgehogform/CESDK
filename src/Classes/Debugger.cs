using System;
using CESDK.Utils;

namespace CESDK.Classes
{
    public class DebuggerException : Exception
    {
        public DebuggerException(string message) : base(message) { }
        public DebuggerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class Debugger
    {
        public static void DebugProcess() =>
            WrapException(() => LuaUtils.CallVoidLuaFunction("debugProcess", "start debugging process"));

        public static void DetachIfPossible() =>
            WrapException(() => LuaUtils.CallVoidLuaFunction("detachIfPossible", "detach debugger"));

        public static void OutputDebugString(string message) =>
            WrapException(() => LuaUtils.CallVoidLuaFunction("outputDebugString", "output debug string", message));

        public static string? Disassemble(ulong address, int maxSize = 512) =>
            WrapException(() => LuaUtils.CallLuaFunction("disassemble", $"disassemble address 0x{address:X}", () => PluginContext.Lua.ToString(-1), address, maxSize));

        public static int GetInstructionSize(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("getInstructionSize", $"get instruction size at 0x{address:X}", () => (int)PluginContext.Lua.ToInteger(-1), address));

        private static T WrapException<T>(Func<T> operation)
        {
            try
            {
                return operation();
            }
            catch (InvalidOperationException ex)
            {
                throw new DebuggerException(ex.Message, ex);
            }
        }

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