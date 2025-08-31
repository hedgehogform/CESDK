namespace CESDK.Memory
{
    /// <summary>
    /// Exception thrown when memory access operations fail.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the MemoryAccessException class.
    /// </remarks>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public class MemoryAccessException(string message, Exception? innerException = null) : Exception($"Memory access failed: {message}", innerException)
    {
    }

    /// <summary>
    /// Provides comprehensive memory read and write operations for target processes and Cheat Engine itself.
    /// Wraps CE's memory access functions with high-level, type-safe C# methods.
    /// </summary>
    /// <remarks>
    /// <para>This class provides access to CE's powerful memory manipulation functions.</para>
    /// <para>All methods use CE's native memory access for accurate data handling.</para>
    /// <para>Supports both target process memory and CE's own memory space.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Read different data types
    /// var health = MemoryAccess.ReadFloat(0x12345678);
    /// var playerId = MemoryAccess.ReadInteger(0x12345680);
    /// var playerName = MemoryAccess.ReadString(0x12345684, 32);
    /// 
    /// // Write values
    /// MemoryAccess.WriteFloat(0x12345678, 100.0f);
    /// MemoryAccess.WriteInteger(0x12345680, 42);
    /// MemoryAccess.WriteString(0x12345684, "NewPlayerName");
    /// 
    /// // Read/write byte arrays
    /// var bytes = MemoryAccess.ReadBytes(0x12345678, 16);
    /// MemoryAccess.WriteBytes(0x12345690, new byte[] { 0x90, 0x90, 0x90 });
    /// 
    /// // Local CE memory access
    /// var ceValue = MemoryAccess.ReadIntegerLocal(0x12345678);
    /// MemoryAccess.WriteFloatLocal(0x12345678, 3.14f);
    /// </code>
    /// </example>
    public static class MemoryAccess
    {
        #region Target Process Memory - Read Operations

        /// <summary>
        /// Reads a byte (8-bit integer) from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The byte value at the specified address.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readByte()</c> Lua function.</para>
        /// <para>Also accessible as readShortInteger() in CE.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// byte flags = MemoryAccess.ReadByte(0x12345678);
        /// if ((flags &amp; 0x01) != 0)
        ///     Console.WriteLine("Flag 1 is set");
        /// </code>
        /// </example>
        public static byte ReadByte(ulong address)
        {
            try
            {
                var result = PluginContext.Lua.CallIntegerFunction("readByte");
                return (byte)result;
            }
            catch (InvalidOperationException ex)
            {
                throw new MemoryAccessException($"Failed to read byte at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a 16-bit integer from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 16-bit integer value at the specified address.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readSmallInteger()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// short playerLevel = MemoryAccess.ReadSmallInteger(0x12345678);
        /// Console.WriteLine($"Player level: {playerLevel}");
        /// </code>
        /// </example>
        public static short ReadSmallInteger(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readSmallInteger");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readSmallInteger function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readSmallInteger call failed: {error}");
                }

                var value = (short)native.ToInteger(state, -1);
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read small integer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a 32-bit integer from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 32-bit integer value at the specified address.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readInteger()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// int health = MemoryAccess.ReadInteger(0x12345678);
        /// Console.WriteLine($"Player health: {health}");
        /// </code>
        /// </example>
        public static int ReadInteger(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readInteger");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readInteger function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readInteger call failed: {error}");
                }

                var value = (int)native.ToInteger(state, -1);
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read integer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a 64-bit integer from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 64-bit integer value at the specified address.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readQword()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// long bigValue = MemoryAccess.ReadQword(0x12345678);
        /// Console.WriteLine($"Big value: {bigValue}");
        /// </code>
        /// </example>
        public static long ReadQword(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readQword");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readQword function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readQword call failed: {error}");
                }

                var value = native.ToInteger(state, -1);
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read qword at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a pointer value from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The pointer value (32-bit or 64-bit depending on target process).</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readPointer()</c> Lua function.</para>
        /// <para>In 64-bit processes this equals readQword, in 32-bit processes readInteger.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// ulong playerPointer = MemoryAccess.ReadPointer(0x12345678);
        /// int health = MemoryAccess.ReadInteger(playerPointer + 0x10);
        /// </code>
        /// </example>
        public static ulong ReadPointer(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readPointer");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readPointer function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readPointer call failed: {error}");
                }

                var value = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read pointer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a single-precision floating point value from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The float value at the specified address.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readFloat()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// float playerX = MemoryAccess.ReadFloat(0x12345678);
        /// float playerY = MemoryAccess.ReadFloat(0x1234567C);
        /// Console.WriteLine($"Player position: ({playerX:F2}, {playerY:F2})");
        /// </code>
        /// </example>
        public static float ReadFloat(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readFloat");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readFloat function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readFloat call failed: {error}");
                }

                var value = (float)native.ToNumber(state, -1);
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read float at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a double-precision floating point value from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The double value at the specified address.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readDouble()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// double preciseValue = MemoryAccess.ReadDouble(0x12345678);
        /// Console.WriteLine($"Precise value: {preciseValue:F6}");
        /// </code>
        /// </example>
        public static double ReadDouble(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readDouble");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readDouble function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readDouble call failed: {error}");
                }

                var value = native.ToNumber(state, -1);
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read double at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a null-terminated string from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <param name="maxLength">Maximum string length to prevent freezing. Default is 1000.</param>
        /// <param name="isWideChar">Whether the string uses wide character encoding (UTF-16). Default is false.</param>
        /// <returns>The string value at the specified address.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readString()</c> Lua function.</para>
        /// <para>Reading stops at the first null terminator or maxLength.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// string playerName = MemoryAccess.ReadString(0x12345678, 32);
        /// string wideText = MemoryAccess.ReadString(0x12345680, 64, isWideChar: true);
        /// Console.WriteLine($"Player: {playerName}, Text: {wideText}");
        /// </code>
        /// </example>
        public static string ReadString(ulong address, int maxLength = 1000, bool isWideChar = false)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readString");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readString function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, maxLength);
                native.PushBoolean(state, isWideChar);

                var result = native.PCall(state, 3, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readString call failed: {error}");
                }

                var value = native.ToString(state, -1) ?? "";
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read string at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a sequence of bytes from the target process memory.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>An array containing the read bytes.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readBytes()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// byte[] buffer = MemoryAccess.ReadBytes(0x12345678, 16);
        /// Console.WriteLine($"First byte: 0x{buffer[0]:X2}");
        /// </code>
        /// </example>
        public static byte[] ReadBytes(ulong address, int count)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be positive", nameof(count));

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readBytes");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readBytes function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, count);
                native.PushBoolean(state, true); // ReturnAsTable = true

                var result = native.PCall(state, 3, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readBytes call failed: {error}");
                }

                var bytes = new List<byte>();
                if (native.IsTable(state, -1))
                {
                    // Iterate through the table
                    native.PushNil(state);
                    while (native.Next(state, -2) != 0)
                    {
                        if (native.IsNumber(state, -1))
                        {
                            var byteValue = (byte)native.ToInteger(state, -1);
                            bytes.Add(byteValue);
                        }
                        native.Pop(state, 1); // Remove value, keep key
                    }
                }

                native.Pop(state, 1); // Remove table
                return bytes.ToArray();
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read {count} bytes at 0x{address:X}", ex);
            }
        }

        #endregion

        #region Target Process Memory - Write Operations

        /// <summary>
        /// Writes a byte (8-bit integer) to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The byte value to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeByte()</c> Lua function.</para>
        /// <para>Also accessible as writeShortInteger() in CE.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteByte(0x12345678, 0xFF);
        /// if (success)
        ///     Console.WriteLine("Byte written successfully");
        /// </code>
        /// </example>
        public static bool WriteByte(ulong address, byte value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeByte");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeByte function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeByte call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write byte at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a 16-bit integer to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 16-bit integer value to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeSmallInteger()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteSmallInteger(0x12345678, 1000);
        /// if (success)
        ///     Console.WriteLine("Small integer written successfully");
        /// </code>
        /// </example>
        public static bool WriteSmallInteger(ulong address, short value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeSmallInteger");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeSmallInteger function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeSmallInteger call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write small integer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a 32-bit integer to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 32-bit integer value to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeInteger()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteInteger(0x12345678, 999999);
        /// if (success)
        ///     Console.WriteLine("Integer written successfully");
        /// </code>
        /// </example>
        public static bool WriteInteger(ulong address, int value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeInteger");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeInteger function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeInteger call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write integer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a 64-bit integer to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 64-bit integer value to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeQword()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteQword(0x12345678, 9999999999L);
        /// if (success)
        ///     Console.WriteLine("Qword written successfully");
        /// </code>
        /// </example>
        public static bool WriteQword(ulong address, long value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeQword");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeQword function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeQword call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write qword at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a pointer value to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The pointer value to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writePointer()</c> Lua function.</para>
        /// <para>In 64-bit processes this equals writeQword, in 32-bit processes writeInteger.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WritePointer(0x12345678, 0x87654321UL);
        /// if (success)
        ///     Console.WriteLine("Pointer written successfully");
        /// </code>
        /// </example>
        public static bool WritePointer(ulong address, ulong value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writePointer");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writePointer function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, (long)value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writePointer call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write pointer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a single-precision floating point value to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The float value to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeFloat()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteFloat(0x12345678, 100.5f);
        /// if (success)
        ///     Console.WriteLine("Float written successfully");
        /// </code>
        /// </example>
        public static bool WriteFloat(ulong address, float value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeFloat");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeFloat function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushNumber(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeFloat call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write float at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a double-precision floating point value to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The double value to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeDouble()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteDouble(0x12345678, 999.123456);
        /// if (success)
        ///     Console.WriteLine("Double written successfully");
        /// </code>
        /// </example>
        public static bool WriteDouble(ulong address, double value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeDouble");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeDouble function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushNumber(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeDouble call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write double at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a string to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The string value to write.</param>
        /// <param name="isWideChar">Whether to write as wide character string (UTF-16). Default is false.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeString()</c> Lua function.</para>
        /// <para>The string is automatically null-terminated.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteString(0x12345678, "NewPlayerName");
        /// bool wideSuccess = MemoryAccess.WriteString(0x12345680, "WideText", isWideChar: true);
        /// </code>
        /// </example>
        public static bool WriteString(ulong address, string value, bool isWideChar = false)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeString");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeString function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushString(state, value);
                native.PushBoolean(state, isWideChar);

                var result = native.PCall(state, 3, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeString call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write string at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the target process memory.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="bytes">The byte array to write.</param>
        /// <returns>True if the write operation succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeBytes()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// byte[] nopBytes = { 0x90, 0x90, 0x90, 0x90 }; // NOP instructions
        /// bool success = MemoryAccess.WriteBytes(0x12345678, nopBytes);
        /// if (success)
        ///     Console.WriteLine("Bytes written successfully");
        /// </code>
        /// </example>
        public static bool WriteBytes(ulong address, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                throw new ArgumentException("Byte array cannot be empty", nameof(bytes));

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeBytes");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeBytes function not available");
                }

                // Create Lua table with bytes
                native.PushInteger(state, (long)address);
                native.CreateTable(state, bytes.Length, 0);

                for (int i = 0; i < bytes.Length; i++)
                {
                    native.PushInteger(state, i + 1); // Lua arrays are 1-indexed
                    native.PushInteger(state, bytes[i]);
                    native.SetTable(state, -3);
                }

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeBytes call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write {bytes.Length} bytes at 0x{address:X}", ex);
            }
        }

        #endregion

        #region Local CE Memory - Read Operations

        /// <summary>
        /// Reads a 16-bit integer from Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <returns>The 16-bit integer value.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readSmallIntegerLocal()</c> Lua function.</para>
        /// <para>Use this to read data from CE's own process memory.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// short ceValue = MemoryAccess.ReadSmallIntegerLocal(0x12345678);
        /// </code>
        /// </example>
        public static short ReadSmallIntegerLocal(ulong address)
        {
            try
            {
                var result = PluginContext.Lua.CallIntegerFunction("readSmallIntegerLocal");
                return (short)result;
            }
            catch (InvalidOperationException ex)
            {
                throw new MemoryAccessException($"Failed to read local small integer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a 32-bit integer from Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <returns>The 32-bit integer value.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readIntegerLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// int ceValue = MemoryAccess.ReadIntegerLocal(0x12345678);
        /// </code>
        /// </example>
        public static int ReadIntegerLocal(ulong address)
        {
            try
            {
                var result = PluginContext.Lua.CallIntegerFunction("readIntegerLocal");
                return (int)result;
            }
            catch (InvalidOperationException ex)
            {
                throw new MemoryAccessException($"Failed to read local integer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a 64-bit integer from Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <returns>The 64-bit integer value.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readQwordLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// long ceValue = MemoryAccess.ReadQwordLocal(0x12345678);
        /// </code>
        /// </example>
        public static long ReadQwordLocal(ulong address)
        {
            try
            {
                return PluginContext.Lua.CallIntegerFunction("readQwordLocal");
            }
            catch (InvalidOperationException ex)
            {
                throw new MemoryAccessException($"Failed to read local qword at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a pointer from Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <returns>The pointer value.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readPointerLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// ulong cePointer = MemoryAccess.ReadPointerLocal(0x12345678);
        /// </code>
        /// </example>
        public static ulong ReadPointerLocal(ulong address)
        {
            try
            {
                var result = PluginContext.Lua.CallIntegerFunction("readPointerLocal");
                return (ulong)result;
            }
            catch (InvalidOperationException ex)
            {
                throw new MemoryAccessException($"Failed to read local pointer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a single-precision floating point value from Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <returns>The float value.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readFloatLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// float ceFloat = MemoryAccess.ReadFloatLocal(0x12345678);
        /// </code>
        /// </example>
        public static float ReadFloatLocal(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readFloatLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readFloatLocal function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readFloatLocal call failed: {error}");
                }

                var value = (float)native.ToNumber(state, -1);
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local float at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a double-precision floating point value from Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <returns>The double value.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readDoubleLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// double ceDouble = MemoryAccess.ReadDoubleLocal(0x12345678);
        /// </code>
        /// </example>
        public static double ReadDoubleLocal(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readDoubleLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readDoubleLocal function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readDoubleLocal call failed: {error}");
                }

                var value = native.ToNumber(state, -1);
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local double at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Reads a string from Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <param name="maxLength">Maximum string length. Default is 1000.</param>
        /// <param name="isWideChar">Whether the string uses wide character encoding. Default is false.</param>
        /// <returns>The string value.</returns>
        /// <exception cref="MemoryAccessException">Thrown when reading fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>readStringLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// string ceString = MemoryAccess.ReadStringLocal(0x12345678, 64);
        /// </code>
        /// </example>
        public static string ReadStringLocal(ulong address, int maxLength = 1000, bool isWideChar = false)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "readStringLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("readStringLocal function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, maxLength);
                native.PushBoolean(state, isWideChar);

                var result = native.PCall(state, 3, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"readStringLocal call failed: {error}");
                }

                var value = native.ToString(state, -1) ?? "";
                native.Pop(state, 1);
                return value;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to read local string at 0x{address:X}", ex);
            }
        }

        #endregion

        #region Local CE Memory - Write Operations

        /// <summary>
        /// Writes a 16-bit integer to Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>True if the write succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeSmallIntegerLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteSmallIntegerLocal(0x12345678, 1000);
        /// </code>
        /// </example>
        public static bool WriteSmallIntegerLocal(ulong address, short value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeSmallIntegerLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeSmallIntegerLocal function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeSmallIntegerLocal call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local small integer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a 32-bit integer to Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>True if the write succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeIntegerLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteIntegerLocal(0x12345678, 999999);
        /// </code>
        /// </example>
        public static bool WriteIntegerLocal(ulong address, int value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeIntegerLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeIntegerLocal function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeIntegerLocal call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local integer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a 64-bit integer to Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>True if the write succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeQwordLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteQwordLocal(0x12345678, 9999999999L);
        /// </code>
        /// </example>
        public static bool WriteQwordLocal(ulong address, long value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeQwordLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeQwordLocal function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeQwordLocal call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local qword at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a pointer to Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <param name="value">The pointer value to write.</param>
        /// <returns>True if the write succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writePointerLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WritePointerLocal(0x12345678, 0x87654321UL);
        /// </code>
        /// </example>
        public static bool WritePointerLocal(ulong address, ulong value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writePointerLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writePointerLocal function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushInteger(state, (long)value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writePointerLocal call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local pointer at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a single-precision floating point value to Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <param name="value">The float value to write.</param>
        /// <returns>True if the write succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeFloatLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteFloatLocal(0x12345678, 3.14f);
        /// </code>
        /// </example>
        public static bool WriteFloatLocal(ulong address, float value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeFloatLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeFloatLocal function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushNumber(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeFloatLocal call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local float at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a double-precision floating point value to Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <param name="value">The double value to write.</param>
        /// <returns>True if the write succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeDoubleLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteDoubleLocal(0x12345678, 3.141592653589793);
        /// </code>
        /// </example>
        public static bool WriteDoubleLocal(ulong address, double value)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeDoubleLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeDoubleLocal function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushNumber(state, value);

                var result = native.PCall(state, 2, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeDoubleLocal call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local double at 0x{address:X}", ex);
            }
        }

        /// <summary>
        /// Writes a string to Cheat Engine's own memory space.
        /// </summary>
        /// <param name="address">The memory address in CE's memory space.</param>
        /// <param name="value">The string value to write.</param>
        /// <param name="isWideChar">Whether to write as wide character string. Default is false.</param>
        /// <returns>True if the write succeeded; otherwise, false.</returns>
        /// <exception cref="MemoryAccessException">Thrown when writing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>writeStringLocal()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = MemoryAccess.WriteStringLocal(0x12345678, "Test String");
        /// </code>
        /// </example>
        public static bool WriteStringLocal(ulong address, string value, bool isWideChar = false)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "writeStringLocal");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new MemoryAccessException("writeStringLocal function not available");
                }

                native.PushInteger(state, (long)address);
                native.PushString(state, value);
                native.PushBoolean(state, isWideChar);

                var result = native.PCall(state, 3, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new MemoryAccessException($"writeStringLocal call failed: {error}");
                }

                var success = native.ToBoolean(state, -1);
                native.Pop(state, 1);
                return success;
            }
            catch (Exception ex) when (ex is not MemoryAccessException)
            {
                throw new MemoryAccessException($"Failed to write local string at 0x{address:X}", ex);
            }
        }

        #endregion
    }
}