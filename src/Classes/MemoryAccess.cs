#nullable enable
using System;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class MemoryAccessException : Exception
    {
        public MemoryAccessException(string message) : base(message) { }
        public MemoryAccessException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class MemoryAccess
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static byte ReadByte(ulong address)
        {
            try
            {
                lua.GetGlobal("readByte");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readByte function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readByte() call failed: {error}");
                }

                var value = (byte)lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read byte at 0x{address:X}", ex);
            }
        }

        public static short ReadSmallInteger(ulong address)
        {
            try
            {
                lua.GetGlobal("readSmallInteger");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readSmallInteger function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readSmallInteger() call failed: {error}");
                }

                var value = (short)lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read small integer at 0x{address:X}", ex);
            }
        }

        public static int ReadInteger(ulong address)
        {
            try
            {
                lua.GetGlobal("readInteger");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readInteger function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readInteger() call failed: {error}");
                }

                var value = lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read integer at 0x{address:X}", ex);
            }
        }

        public static long ReadQword(ulong address)
        {
            try
            {
                lua.GetGlobal("readQword");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readQword function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readQword() call failed: {error}");
                }

                var value = lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read qword at 0x{address:X}", ex);
            }
        }

        public static ulong ReadPointer(ulong address)
        {
            try
            {
                lua.GetGlobal("readPointer");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readPointer function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readPointer() call failed: {error}");
                }

                var value = (ulong)lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read pointer at 0x{address:X}", ex);
            }
        }

        public static float ReadFloat(ulong address)
        {
            try
            {
                lua.GetGlobal("readFloat");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readFloat function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readFloat() call failed: {error}");
                }

                var value = (float)lua.ToNumber(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read float at 0x{address:X}", ex);
            }
        }

        public static double ReadDouble(ulong address)
        {
            try
            {
                lua.GetGlobal("readDouble");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readDouble function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readDouble() call failed: {error}");
                }

                var value = lua.ToNumber(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read double at 0x{address:X}", ex);
            }
        }

        public static string ReadString(ulong address, int maxLength = 1000, bool isWideChar = false)
        {
            try
            {
                lua.GetGlobal("readString");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readString function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(maxLength);
                lua.PushBoolean(isWideChar);

                var result = lua.PCall(3, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readString() call failed: {error}");
                }

                var value = lua.ToString(-1) ?? "";
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read string at 0x{address:X}", ex);
            }
        }

        public static byte[] ReadBytes(ulong address, int count, bool returnAsTable = true)
        {
            try
            {
                lua.GetGlobal("readBytes");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readBytes function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(count);
                lua.PushBoolean(returnAsTable);

                var result = lua.PCall(3, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readBytes() call failed: {error}");
                }

                var bytes = new List<byte>();
                if (lua.IsUserData(-1) || lua.IsTable(-1))
                {
                    lua.PushNil();
                    while (lua.Next(-2) != 0)
                    {
                        if (lua.IsNumber(-1))
                        {
                            var byteValue = (byte)lua.ToInteger(-1);
                            bytes.Add(byteValue);
                        }
                        lua.Pop(1);
                    }
                }

                lua.Pop(1);
                return bytes.ToArray();
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read {count} bytes at 0x{address:X}", ex);
            }
        }

        // Write functions
        public static bool WriteByte(ulong address, byte value)
        {
            try
            {
                lua.GetGlobal("writeByte");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeByte function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeByte() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write byte at 0x{address:X}", ex);
            }
        }

        public static bool WriteSmallInteger(ulong address, short value)
        {
            try
            {
                lua.GetGlobal("writeSmallInteger");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeSmallInteger function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeSmallInteger() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write small integer at 0x{address:X}", ex);
            }
        }

        public static bool WriteInteger(ulong address, int value)
        {
            try
            {
                lua.GetGlobal("writeInteger");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeInteger function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeInteger() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write integer at 0x{address:X}", ex);
            }
        }

        public static bool WriteQword(ulong address, long value)
        {
            try
            {
                lua.GetGlobal("writeQword");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeQword function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeQword() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write qword at 0x{address:X}", ex);
            }
        }

        public static bool WriteFloat(ulong address, float value)
        {
            try
            {
                lua.GetGlobal("writeFloat");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeFloat function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushNumber(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeFloat() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write float at 0x{address:X}", ex);
            }
        }

        public static bool WriteDouble(ulong address, double value)
        {
            try
            {
                lua.GetGlobal("writeDouble");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeDouble function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushNumber(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeDouble() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write double at 0x{address:X}", ex);
            }
        }

        public static bool WriteString(ulong address, string value, bool isWideChar = false)
        {
            try
            {
                lua.GetGlobal("writeString");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeString function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushString(value);
                lua.PushBoolean(isWideChar);

                var result = lua.PCall(3, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeString() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write string at 0x{address:X}", ex);
            }
        }

        public static bool WriteBytes(ulong address, byte[] bytes)
        {
            try
            {
                lua.GetGlobal("writeBytes");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeBytes function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.CreateTable();

                for (int i = 0; i < bytes.Length; i++)
                {
                    lua.PushInteger(i + 1);
                    lua.PushInteger(bytes[i]);
                    lua.SetTable(-3);
                }

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeBytes() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write {bytes.Length} bytes at 0x{address:X}", ex);
            }
        }

        // Local CE memory functions  
        public static short ReadSmallIntegerLocal(ulong address)
        {
            try
            {
                lua.GetGlobal("readSmallIntegerLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readSmallIntegerLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readSmallIntegerLocal() call failed: {error}");
                }

                var value = (short)lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local small integer at 0x{address:X}", ex);
            }
        }

        public static int ReadIntegerLocal(ulong address)
        {
            try
            {
                lua.GetGlobal("readIntegerLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readIntegerLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readIntegerLocal() call failed: {error}");
                }

                var value = lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local integer at 0x{address:X}", ex);
            }
        }

        public static long ReadQwordLocal(ulong address)
        {
            try
            {
                lua.GetGlobal("readQwordLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readQwordLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readQwordLocal() call failed: {error}");
                }

                var value = lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local qword at 0x{address:X}", ex);
            }
        }

        public static ulong ReadPointerLocal(ulong address)
        {
            try
            {
                lua.GetGlobal("readPointerLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readPointerLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readPointerLocal() call failed: {error}");
                }

                var value = (ulong)lua.ToInteger(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local pointer at 0x{address:X}", ex);
            }
        }

        public static float ReadFloatLocal(ulong address)
        {
            try
            {
                lua.GetGlobal("readFloatLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readFloatLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readFloatLocal() call failed: {error}");
                }

                var value = (float)lua.ToNumber(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local float at 0x{address:X}", ex);
            }
        }

        public static double ReadDoubleLocal(ulong address)
        {
            try
            {
                lua.GetGlobal("readDoubleLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readDoubleLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readDoubleLocal() call failed: {error}");
                }

                var value = lua.ToNumber(-1);
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local double at 0x{address:X}", ex);
            }
        }

        public static string ReadStringLocal(ulong address, int maxLength = 1000, bool isWideChar = false)
        {
            try
            {
                lua.GetGlobal("readStringLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readStringLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(maxLength);
                lua.PushBoolean(isWideChar);

                var result = lua.PCall(3, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readStringLocal() call failed: {error}");
                }

                var value = lua.ToString(-1) ?? "";
                lua.Pop(1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local string at 0x{address:X}", ex);
            }
        }

        // Write Local functions
        public static bool WriteSmallIntegerLocal(ulong address, short value)
        {
            try
            {
                lua.GetGlobal("writeSmallIntegerLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeSmallIntegerLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeSmallIntegerLocal() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local small integer at 0x{address:X}", ex);
            }
        }

        public static bool WriteIntegerLocal(ulong address, int value)
        {
            try
            {
                lua.GetGlobal("writeIntegerLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeIntegerLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeIntegerLocal() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local integer at 0x{address:X}", ex);
            }
        }

        public static bool WriteQwordLocal(ulong address, long value)
        {
            try
            {
                lua.GetGlobal("writeQwordLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeQwordLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeQwordLocal() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local qword at 0x{address:X}", ex);
            }
        }

        public static bool WriteFloatLocal(ulong address, float value)
        {
            try
            {
                lua.GetGlobal("writeFloatLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeFloatLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushNumber(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeFloatLocal() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local float at 0x{address:X}", ex);
            }
        }

        public static bool WriteDoubleLocal(ulong address, double value)
        {
            try
            {
                lua.GetGlobal("writeDoubleLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeDoubleLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushNumber(value);

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeDoubleLocal() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local double at 0x{address:X}", ex);
            }
        }

        public static bool WriteStringLocal(ulong address, string value, bool isWideChar = false)
        {
            try
            {
                lua.GetGlobal("writeStringLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeStringLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushString(value);
                lua.PushBoolean(isWideChar);

                var result = lua.PCall(3, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeStringLocal() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local string at 0x{address:X}", ex);
            }
        }

        public static byte[] ReadBytesLocal(ulong address, int count, bool returnAsTable = true)
        {
            try
            {
                lua.GetGlobal("readBytesLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("readBytesLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushInteger(count);
                lua.PushBoolean(returnAsTable);

                var result = lua.PCall(3, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"readBytesLocal() call failed: {error}");
                }

                var bytes = new List<byte>();
                if (lua.IsUserData(-1) || lua.IsTable(-1))
                {
                    lua.PushNil();
                    while (lua.Next(-2) != 0)
                    {
                        if (lua.IsNumber(-1))
                        {
                            var byteValue = (byte)lua.ToInteger(-1);
                            bytes.Add(byteValue);
                        }
                        lua.Pop(1);
                    }
                }

                lua.Pop(1);
                return bytes.ToArray();
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local {count} bytes at 0x{address:X}", ex);
            }
        }

        public static bool WriteBytesLocal(ulong address, byte[] bytes)
        {
            try
            {
                lua.GetGlobal("writeBytesLocal");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemoryAccessException("writeBytesLocal function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.CreateTable();

                for (int i = 0; i < bytes.Length; i++)
                {
                    lua.PushInteger(i + 1);
                    lua.PushInteger(bytes[i]);
                    lua.SetTable(-3);
                }

                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemoryAccessException($"writeBytesLocal() call failed: {error}");
                }

                var success = lua.ToBoolean(-1);
                lua.Pop(1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local {bytes.Length} bytes at 0x{address:X}", ex);
            }
        }
    }
}