#nullable enable
using System;
using System.Globalization;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class DisassemblerException : Exception
    {
        public DisassemblerException(string message) : base(message) { }
        public DisassemblerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class Disassembler
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static string? Disassemble(ulong address, int maxSize = 512)
        {
            try
            {
                lua.GetGlobal("disassemble");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new DisassemblerException("disassemble function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(maxSize);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DisassemblerException($"disassemble() call failed: {error}");
                }

                var instruction = lua.ToString(-1);
                lua.Pop(1);
                return instruction;
            }
            catch (Exception ex) when (ex is not DisassemblerException)
            {
                throw new DisassemblerException($"Failed to disassemble address 0x{address:X}", ex);
            }
        }

        public static int GetInstructionSize(ulong address)
        {
            try
            {
                lua.GetGlobal("getInstructionSize");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new DisassemblerException("getInstructionSize function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DisassemblerException($"getInstructionSize() call failed: {error}");
                }

                var size = (int)lua.ToInteger(-1);
                lua.Pop(1);
                return size;
            }
            catch (Exception ex) when (ex is not DisassemblerException)
            {
                throw new DisassemblerException($"Failed to get instruction size at 0x{address:X}", ex);
            }
        }

        public static string? GetComment(ulong address)
        {
            try
            {
                lua.GetGlobal("getComment");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new DisassemblerException("getComment function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DisassemblerException($"getComment() call failed: {error}");
                }

                var comment = lua.ToString(-1);
                lua.Pop(1);
                return comment;
            }
            catch (Exception ex) when (ex is not DisassemblerException)
            {
                throw new DisassemblerException($"Failed to get comment at 0x{address:X}", ex);
            }
        }

        public static void SetComment(ulong address, string comment)
        {
            try
            {
                lua.GetGlobal("setComment");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new DisassemblerException("setComment function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushString(comment);

                var result = lua.PCall(2, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DisassemblerException($"setComment() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DisassemblerException)
            {
                throw new DisassemblerException($"Failed to set comment at 0x{address:X}", ex);
            }
        }
    }
}