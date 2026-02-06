using System;
using CESDK.Utils;

namespace CESDK.Classes
{
    public class DisassemblerException : CesdkException
    {
        public DisassemblerException(string message) : base(message) { }
        public DisassemblerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class Disassembler
    {
        public static string? Disassemble(ulong address, int maxSize = 512) =>
            WrapException(() => LuaUtils.CallLuaFunction("disassemble", $"disassemble at 0x{address:X}",
                () => PluginContext.Lua.ToString(-1), address, maxSize));

        public static int GetInstructionSize(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("getInstructionSize", $"get instruction size at 0x{address:X}",
                () => PluginContext.Lua.ToInteger(-1), address));

        public static string? GetComment(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("getComment", $"get comment at 0x{address:X}",
                () => PluginContext.Lua.ToString(-1), address));

        public static void SetComment(ulong address, string comment) =>
            WrapException(() => LuaUtils.CallVoidLuaFunction("setComment", $"set comment at 0x{address:X}", address, comment));

        private static T WrapException<T>(Func<T> operation)
        {
            try { return operation(); }
            catch (InvalidOperationException ex) { throw new DisassemblerException(ex.Message, ex); }
        }

        private static void WrapException(Action operation)
        {
            try { operation(); }
            catch (InvalidOperationException ex) { throw new DisassemblerException(ex.Message, ex); }
        }
    }
}