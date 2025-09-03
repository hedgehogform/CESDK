using System;
using CESDK.Utils;

namespace CESDK.Classes
{
    public class MemoryAccessException : Exception
    {
        public MemoryAccessException(string message) : base(message) { }
        public MemoryAccessException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class MemoryAccess
    {
        // Read methods for target process
        public static byte ReadByte(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readByte", $"read byte at 0x{address:X}", () => (byte)PluginContext.Lua.ToInteger(-1), address));

        public static short ReadSmallInteger(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readSmallInteger", $"read small integer at 0x{address:X}", () => (short)PluginContext.Lua.ToInteger(-1), address));

        public static int ReadInteger(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readInteger", $"read integer at 0x{address:X}", () => PluginContext.Lua.ToInteger(-1), address));

        public static long ReadQword(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readQword", $"read qword at 0x{address:X}", () => PluginContext.Lua.ToInteger(-1), address));

        public static ulong ReadPointer(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readPointer", $"read pointer at 0x{address:X}", () => (ulong)PluginContext.Lua.ToInteger(-1), address));

        public static float ReadFloat(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readFloat", $"read float at 0x{address:X}", () => (float)PluginContext.Lua.ToNumber(-1), address));

        public static double ReadDouble(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readDouble", $"read double at 0x{address:X}", () => PluginContext.Lua.ToNumber(-1), address));

        public static string ReadString(ulong address, int maxLength = 1000, bool isWideChar = false) =>
            WrapException(() => LuaUtils.CallLuaFunction("readString", $"read string at 0x{address:X}", () => PluginContext.Lua.ToString(-1) ?? "", address, maxLength, isWideChar));

        public static byte[] ReadBytes(ulong address, int count, bool returnAsTable = true) =>
            WrapException(() => LuaUtils.CallLuaFunction("readBytes", $"read {count} bytes at 0x{address:X}", LuaUtils.ExtractBytesFromTable, address, count, returnAsTable));

        // Write methods for target process
        public static bool WriteByte(ulong address, byte value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeByte", $"write byte at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteSmallInteger(ulong address, short value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeSmallInteger", $"write small integer at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteInteger(ulong address, int value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeInteger", $"write integer at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteQword(ulong address, long value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeQword", $"write qword at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteFloat(ulong address, float value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeFloat", $"write float at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteDouble(ulong address, double value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeDouble", $"write double at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteString(ulong address, string value, bool isWideChar = false) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeString", $"write string at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value, isWideChar));

        public static bool WriteBytes(ulong address, byte[] bytes) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeBytes", $"write {bytes.Length} bytes at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, bytes));

        // Local CE memory read methods
        public static short ReadSmallIntegerLocal(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readSmallIntegerLocal", $"read local small integer at 0x{address:X}", () => (short)PluginContext.Lua.ToInteger(-1), address));

        public static int ReadIntegerLocal(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readIntegerLocal", $"read local integer at 0x{address:X}", () => PluginContext.Lua.ToInteger(-1), address));

        public static long ReadQwordLocal(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readQwordLocal", $"read local qword at 0x{address:X}", () => PluginContext.Lua.ToInteger(-1), address));

        public static ulong ReadPointerLocal(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readPointerLocal", $"read local pointer at 0x{address:X}", () => (ulong)PluginContext.Lua.ToInteger(-1), address));

        public static float ReadFloatLocal(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readFloatLocal", $"read local float at 0x{address:X}", () => (float)PluginContext.Lua.ToNumber(-1), address));

        public static double ReadDoubleLocal(ulong address) =>
            WrapException(() => LuaUtils.CallLuaFunction("readDoubleLocal", $"read local double at 0x{address:X}", () => PluginContext.Lua.ToNumber(-1), address));

        public static string ReadStringLocal(ulong address, int maxLength = 1000, bool isWideChar = false) =>
            WrapException(() => LuaUtils.CallLuaFunction("readStringLocal", $"read local string at 0x{address:X}", () => PluginContext.Lua.ToString(-1) ?? "", address, maxLength, isWideChar));

        public static byte[] ReadBytesLocal(ulong address, int count, bool returnAsTable = true) =>
            WrapException(() => LuaUtils.CallLuaFunction("readBytesLocal", $"read local {count} bytes at 0x{address:X}", LuaUtils.ExtractBytesFromTable, address, count, returnAsTable));

        // Local CE memory write methods
        public static bool WriteSmallIntegerLocal(ulong address, short value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeSmallIntegerLocal", $"write local small integer at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteIntegerLocal(ulong address, int value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeIntegerLocal", $"write local integer at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteQwordLocal(ulong address, long value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeQwordLocal", $"write local qword at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteFloatLocal(ulong address, float value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeFloatLocal", $"write local float at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteDoubleLocal(ulong address, double value) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeDoubleLocal", $"write local double at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value));

        public static bool WriteStringLocal(ulong address, string value, bool isWideChar = false) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeStringLocal", $"write local string at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, value, isWideChar));

        public static bool WriteBytesLocal(ulong address, byte[] bytes) =>
            WrapException(() => LuaUtils.CallLuaFunction("writeBytesLocal", $"write local {bytes.Length} bytes at 0x{address:X}", () => PluginContext.Lua.ToBoolean(-1), address, bytes));

        private static T WrapException<T>(Func<T> operation)
        {
            try
            {
                return operation();
            }
            catch (InvalidOperationException ex)
            {
                throw new MemoryAccessException(ex.Message, ex);
            }
        }
    }
}