using System;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class DebuggerException : Exception
    {
        public DebuggerException(string message) : base(message) { }
        public DebuggerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class Debugger
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static void DebugProcess()
        {
            try
            {
                lua.GetGlobal("debugProcess");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new DebuggerException("debugProcess function not available in this CE version");
                }

                var result = lua.PCall(0, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DebuggerException($"debugProcess() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException("Failed to start debugging process", ex);
            }
        }

        public static void DetachIfPossible()
        {
            try
            {
                lua.GetGlobal("detachIfPossible");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new DebuggerException("detachIfPossible function not available in this CE version");
                }

                var result = lua.PCall(0, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DebuggerException($"detachIfPossible() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException("Failed to detach debugger", ex);
            }
        }

        public static void OutputDebugString(string message)
        {
            try
            {
                lua.GetGlobal("outputDebugString");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new DebuggerException("outputDebugString function not available in this CE version");
                }

                lua.PushString(message);

                var result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DebuggerException($"outputDebugString() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException("Failed to output debug string", ex);
            }
        }

        public static string? Disassemble(ulong address, int maxSize = 512)
        {
            try
            {
                lua.GetGlobal("disassemble");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new DebuggerException("disassemble function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(maxSize);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DebuggerException($"disassemble() call failed: {error}");
                }

                var instruction = lua.ToString(-1);
                lua.Pop(1);
                return instruction;
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Failed to disassemble address 0x{address:X}", ex);
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
                    throw new DebuggerException("getInstructionSize function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new DebuggerException($"getInstructionSize() call failed: {error}");
                }

                var size = (int)lua.ToInteger(-1);
                lua.Pop(1);
                return size;
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Failed to get instruction size at 0x{address:X}", ex);
            }
        }
    }
}