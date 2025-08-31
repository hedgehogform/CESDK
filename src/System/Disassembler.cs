using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CESDK.Lua;

namespace CESDK.System
{
    /// <summary>
    /// Represents a disassembled instruction with all its components.
    /// </summary>
    public class DisassembledInstruction
    {
        /// <summary>
        /// Gets the memory address of the instruction.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Gets the address formatted as a hexadecimal string with 0x prefix.
        /// </summary>
        public string HexAddress => $"0x{Address:X}";

        /// <summary>
        /// Gets the raw bytes of the instruction as a hex string.
        /// </summary>
        public string Bytes { get; }

        /// <summary>
        /// Gets the instruction opcode (mnemonic).
        /// </summary>
        public string Opcode { get; }

        /// <summary>
        /// Gets the instruction parameters/operands.
        /// </summary>
        public string Parameters { get; }

        /// <summary>
        /// Gets additional information or comments about the instruction.
        /// </summary>
        public string Extra { get; }

        /// <summary>
        /// Gets the size of the instruction in bytes.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the full assembly representation of the instruction.
        /// </summary>
        public string Assembly => string.IsNullOrEmpty(Parameters) ? Opcode : $"{Opcode} {Parameters}";

        /// <summary>
        /// Gets the address as a pointer for interop scenarios.
        /// </summary>
        public IntPtr Pointer => new IntPtr((long)Address);

        internal DisassembledInstruction(ulong address, string bytes, string opcode, string parameters, string extra)
        {
            Address = address;
            Bytes = bytes ?? "";
            Opcode = opcode ?? "";
            Parameters = parameters ?? "";
            Extra = extra ?? "";

            // Calculate instruction size from bytes
            Size = CalculateInstructionSize(bytes ?? "");
        }

        private static int CalculateInstructionSize(string bytes)
        {
            if (string.IsNullOrWhiteSpace(bytes))
                return 0;

            // Remove spaces and count hex pairs
            var cleanBytes = bytes.Replace(" ", "");
            return cleanBytes.Length / 2;
        }

        /// <summary>
        /// Returns the full disassembly string in CE format.
        /// </summary>
        public override string ToString()
        {
            var result = $"{HexAddress} - {Bytes} - {Assembly}";
            if (!string.IsNullOrEmpty(Extra))
                result += $" : {Extra}";
            return result;
        }
    }

    /// <summary>
    /// Exception thrown when disassembly operations fail.
    /// </summary>
    public class DisassemblyException : Exception
    {
        /// <summary>
        /// Gets the address that caused the disassembly failure.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Initializes a new instance of the DisassemblyException class.
        /// </summary>
        /// <param name="address">The address that failed to disassemble.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DisassemblyException(ulong address, string message, Exception? innerException = null)
            : base($"Failed to disassemble address 0x{address:X}: {message}", innerException)
        {
            Address = address;
        }
    }

    /// <summary>
    /// Provides high-level disassembly functionality using Cheat Engine's disassembler.
    /// Wraps CE's disassemble and getInstructionSize functions with type-safe C# methods.
    /// </summary>
    /// <remarks>
    /// <para>This class provides convenient access to CE's x86/x64 disassembler capabilities.</para>
    /// <para>All methods use CE's native disassembler engine for accurate instruction decoding.</para>
    /// <para>The disassembler supports both x86 and x64 architectures automatically.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Disassemble a single instruction
    /// var kernelBase = AddressResolver.GetAddress("kernel32.dll");
    /// var instruction = Disassembler.DisassembleInstruction(kernelBase);
    /// Console.WriteLine($"First instruction: {instruction.Assembly}");
    /// Console.WriteLine($"Size: {instruction.Size} bytes");
    /// 
    /// // Disassemble multiple instructions
    /// var instructions = Disassembler.DisassembleRange(kernelBase, 10);
    /// foreach (var inst in instructions)
    /// {
    ///     Console.WriteLine($"{inst.HexAddress}: {inst.Assembly}");
    /// }
    /// 
    /// // Get instruction size only
    /// var size = Disassembler.GetInstructionSize(kernelBase);
    /// Console.WriteLine($"Instruction size: {size} bytes");
    /// 
    /// // Check if address contains valid instruction
    /// if (Disassembler.IsValidInstruction(kernelBase))
    /// {
    ///     var inst = Disassembler.DisassembleInstruction(kernelBase);
    ///     // Process instruction...
    /// }
    /// </code>
    /// </example>
    public static class Disassembler
    {
        /// <summary>
        /// Disassembles a single instruction at the specified address.
        /// </summary>
        /// <param name="address">The memory address to disassemble.</param>
        /// <returns>A detailed instruction object with all components parsed.</returns>
        /// <exception cref="DisassemblyException">Thrown when disassembly fails or address is invalid.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>disassemble()</c> and <c>splitDisassembledString()</c> functions.</para>
        /// <para>The returned object contains the address, bytes, opcode, parameters, and extra information.</para>
        /// <para>Use this when you need detailed information about a single instruction.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var kernelBase = AddressResolver.GetAddress("kernel32.dll");
        /// var instruction = Disassembler.DisassembleInstruction(kernelBase);
        /// 
        /// Console.WriteLine($"Address: {instruction.HexAddress}");
        /// Console.WriteLine($"Bytes: {instruction.Bytes}");
        /// Console.WriteLine($"Assembly: {instruction.Assembly}");
        /// Console.WriteLine($"Size: {instruction.Size} bytes");
        /// 
        /// if (!string.IsNullOrEmpty(instruction.Extra))
        ///     Console.WriteLine($"Extra: {instruction.Extra}");
        /// </code>
        /// </example>
        public static DisassembledInstruction DisassembleInstruction(ulong address)
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Call disassemble function
                native.GetGlobal(state, "disassemble");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DisassemblyException(address, "disassemble function not available in this CE version");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DisassemblyException(address, $"disassemble() call failed: {error}");
                }

                // Get the disassembly string result
                var disassemblyString = native.ToString(state, -1);
                native.Pop(state, 1);

                if (string.IsNullOrEmpty(disassemblyString))
                {
                    throw new DisassemblyException(address, "No disassembly result returned");
                }

                // Split the disassembly string using splitDisassembledString
                native.GetGlobal(state, "splitDisassembledString");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DisassemblyException(address, "splitDisassembledString function not available");
                }

                native.PushString(state, disassemblyString!);
                result = native.PCall(state, 1, 4);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DisassemblyException(address, $"splitDisassembledString() call failed: {error}");
                }

                // Get the 4 components: address, bytes, opcode, extra
                var addressStr = native.ToString(state, -4) ?? "";
                var bytesStr = native.ToString(state, -3) ?? "";
                var opcodeStr = native.ToString(state, -2) ?? "";
                var extraStr = native.ToString(state, -1) ?? "";

                native.Pop(state, 4);

                // Parse opcode and parameters
                var opcodeParts = opcodeStr.Split([' '], 2);
                var opcode = opcodeParts[0];
                var parameters = opcodeParts.Length > 1 ? opcodeParts[1] : "";

                // Parse address from string if needed
                ulong parsedAddress = address;
                if (!string.IsNullOrEmpty(addressStr))
                {
                    if (addressStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        ulong.TryParse(addressStr.Substring(2), NumberStyles.HexNumber, null, out parsedAddress);
                    }
                    else
                    {
                        ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out parsedAddress);
                    }
                }

                return new DisassembledInstruction(parsedAddress, bytesStr, opcode, parameters, extraStr);
            }
            catch (Exception ex) when (ex is not DisassemblyException)
            {
                native.SetTop(state, 0);
                throw new DisassemblyException(address, ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets the size in bytes of the instruction at the specified address.
        /// </summary>
        /// <param name="address">The memory address to analyze.</param>
        /// <returns>The size of the instruction in bytes.</returns>
        /// <exception cref="DisassemblyException">Thrown when size calculation fails or address is invalid.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getInstructionSize()</c> function.</para>
        /// <para>This is faster than full disassembly when you only need the instruction size.</para>
        /// <para>Use this for instruction parsing, code analysis, or address arithmetic.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var address = AddressResolver.GetAddress("kernel32.CreateFileA");
        /// var size = Disassembler.GetInstructionSize(address);
        /// Console.WriteLine($"First instruction is {size} bytes");
        /// 
        /// // Move to next instruction
        /// var nextAddress = address + (ulong)size;
        /// var nextSize = Disassembler.GetInstructionSize(nextAddress);
        /// </code>
        /// </example>
        public static int GetInstructionSize(ulong address)
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Call getInstructionSize function
                native.GetGlobal(state, "getInstructionSize");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DisassemblyException(address, "getInstructionSize function not available in this CE version");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DisassemblyException(address, $"getInstructionSize() call failed: {error}");
                }

                // Get the size result
                int size;
                if (native.IsNumber(state, -1))
                {
                    size = (int)native.ToInteger(state, -1);
                }
                else if (native.IsString(state, -1))
                {
                    var sizeStr = native.ToString(state, -1);
                    if (!int.TryParse(sizeStr, out size))
                    {
                        native.Pop(state, 1);
                        throw new DisassemblyException(address, $"Invalid size format returned: {sizeStr}");
                    }
                }
                else
                {
                    native.Pop(state, 1);
                    throw new DisassemblyException(address, "Invalid instruction or unable to determine size");
                }

                native.Pop(state, 1);

                if (size <= 0)
                {
                    throw new DisassemblyException(address, $"Invalid instruction size: {size}");
                }

                return size;
            }
            catch (Exception ex) when (ex is not DisassemblyException)
            {
                native.SetTop(state, 0);
                throw new DisassemblyException(address, ex.Message, ex);
            }
        }

        /// <summary>
        /// Disassembles a range of instructions starting at the specified address.
        /// </summary>
        /// <param name="startAddress">The starting memory address.</param>
        /// <param name="count">The number of instructions to disassemble.</param>
        /// <returns>A list of disassembled instructions in order.</returns>
        /// <exception cref="ArgumentException">Thrown when count is less than 1.</exception>
        /// <exception cref="DisassemblyException">Thrown when disassembly fails.</exception>
        /// <remarks>
        /// <para>This method disassembles multiple consecutive instructions.</para>
        /// <para>Each instruction's size is calculated to find the next instruction address.</para>
        /// <para>If any instruction fails to disassemble, the method stops and returns partial results.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var functionAddr = AddressResolver.GetAddress("kernel32.CreateFileA");
        /// var instructions = Disassembler.DisassembleRange(functionAddr, 5);
        /// 
        /// Console.WriteLine($"First 5 instructions of CreateFileA:");
        /// foreach (var inst in instructions)
        /// {
        ///     Console.WriteLine($"{inst.HexAddress}: {inst.Assembly}");
        /// }
        /// </code>
        /// </example>
        public static List<DisassembledInstruction> DisassembleRange(ulong startAddress, int count)
        {
            if (count < 1)
                throw new ArgumentException("Count must be at least 1", nameof(count));

            var instructions = new List<DisassembledInstruction>();
            var currentAddress = startAddress;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    var instruction = DisassembleInstruction(currentAddress);
                    instructions.Add(instruction);
                    currentAddress += (ulong)instruction.Size;
                }
                catch (DisassemblyException)
                {
                    // Stop on first failure
                    break;
                }
            }

            return instructions;
        }

        /// <summary>
        /// Attempts to disassemble an instruction without throwing exceptions.
        /// </summary>
        /// <param name="address">The memory address to disassemble.</param>
        /// <param name="instruction">The disassembled instruction, or null if failed.</param>
        /// <returns>True if disassembly succeeded; otherwise, false.</returns>
        /// <remarks>
        /// <para>Use this method when you need error-tolerant disassembly.</para>
        /// <para>Returns false for invalid addresses or malformed instructions.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Disassembler.TryDisassembleInstruction(someAddress, out var instruction))
        /// {
        ///     Console.WriteLine($"Valid instruction: {instruction.Assembly}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Invalid or unreadable instruction");
        /// }
        /// </code>
        /// </example>
        public static bool TryDisassembleInstruction(ulong address, out DisassembledInstruction? instruction)
        {
            try
            {
                instruction = DisassembleInstruction(address);
                return true;
            }
            catch (DisassemblyException)
            {
                instruction = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to get the instruction size without throwing exceptions.
        /// </summary>
        /// <param name="address">The memory address to analyze.</param>
        /// <param name="size">The instruction size, or 0 if failed.</param>
        /// <returns>True if size calculation succeeded; otherwise, false.</returns>
        /// <example>
        /// <code>
        /// if (Disassembler.TryGetInstructionSize(address, out var size))
        /// {
        ///     Console.WriteLine($"Instruction size: {size} bytes");
        ///     var nextAddr = address + (ulong)size;
        /// }
        /// </code>
        /// </example>
        public static bool TryGetInstructionSize(ulong address, out int size)
        {
            try
            {
                size = GetInstructionSize(address);
                return true;
            }
            catch (DisassemblyException)
            {
                size = 0;
                return false;
            }
        }

        /// <summary>
        /// Checks if the specified address contains a valid instruction.
        /// </summary>
        /// <param name="address">The memory address to check.</param>
        /// <returns>True if the address contains a valid instruction; otherwise, false.</returns>
        /// <example>
        /// <code>
        /// if (Disassembler.IsValidInstruction(someAddress))
        /// {
        ///     var instruction = Disassembler.DisassembleInstruction(someAddress);
        ///     // Process valid instruction...
        /// }
        /// </code>
        /// </example>
        public static bool IsValidInstruction(ulong address)
        {
            return TryGetInstructionSize(address, out _);
        }

        /// <summary>
        /// Disassembles instructions until a specific pattern or count is reached.
        /// </summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="maxCount">Maximum number of instructions to disassemble.</param>
        /// <param name="stopPredicate">Optional function to determine when to stop disassembling.</param>
        /// <returns>A list of disassembled instructions.</returns>
        /// <example>
        /// <code>
        /// // Disassemble until we hit a RET instruction
        /// var instructions = Disassembler.DisassembleUntil(
        ///     functionAddr, 
        ///     maxCount: 100,
        ///     stopPredicate: inst => inst.Opcode.Equals("ret", StringComparison.OrdinalIgnoreCase)
        /// );
        /// 
        /// // Disassemble first 10 instructions (no stop condition)
        /// var first10 = Disassembler.DisassembleUntil(functionAddr, 10);
        /// </code>
        /// </example>
        public static List<DisassembledInstruction> DisassembleUntil(ulong startAddress, int maxCount, Func<DisassembledInstruction, bool>? stopPredicate = null)
        {
            if (maxCount < 1)
                throw new ArgumentException("Max count must be at least 1", nameof(maxCount));

            var instructions = new List<DisassembledInstruction>();
            var currentAddress = startAddress;

            for (int i = 0; i < maxCount; i++)
            {
                if (!TryDisassembleInstruction(currentAddress, out var instruction) || instruction == null)
                    break;

                instructions.Add(instruction);

                // Check stop condition
                if (stopPredicate?.Invoke(instruction) == true)
                    break;

                currentAddress += (ulong)instruction.Size;
            }

            return instructions;
        }

        /// <summary>
        /// Parses a raw disassembled string from CE's disassemble() function into a structured instruction object.
        /// </summary>
        /// <param name="disassembledString">The raw disassembly string from CE's disassemble() function.</param>
        /// <returns>A parsed DisassembledInstruction object.</returns>
        /// <exception cref="DisassemblyException">Thrown when the string format is invalid or cannot be parsed.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>splitDisassembledString()</c> Lua function.</para>
        /// <para>The input string should be in CE's standard disassembly format.</para>
        /// <para>Use this with the output from CE's disassemble(address) function for structured access.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Get raw disassembly string from CE
        /// var rawDisasm = GetRawDisassemblyFromCE(address); // "0x12345678 - 48 8B 05 - mov rax,[0x12345678] : some comment"
        /// 
        /// // Parse it into structured object
        /// var instruction = Disassembler.SplitDisassembledString(rawDisasm);
        /// 
        /// Console.WriteLine($"Address: {instruction.HexAddress}");
        /// Console.WriteLine($"Bytes: {instruction.Bytes}");
        /// Console.WriteLine($"Opcode: {instruction.Opcode}");
        /// Console.WriteLine($"Parameters: {instruction.Parameters}");
        /// Console.WriteLine($"Extra: {instruction.Extra}");
        /// Console.WriteLine($"Assembly: {instruction.Assembly}");
        /// Console.WriteLine($"Size: {instruction.Size} bytes");
        /// </code>
        /// </example>
        public static DisassembledInstruction SplitDisassembledString(string disassembledString)
        {
            if (string.IsNullOrEmpty(disassembledString))
                throw new DisassemblyException(0, "Disassembled string cannot be null or empty");

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                // Get the splitDisassembledString function
                native.GetGlobal(state, "splitDisassembledString");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DisassemblyException(0, "splitDisassembledString function not available in this CE version");
                }

                // Push the disassembled string as parameter
                native.PushString(state, disassembledString);

                // Call the function (1 parameter, 5 return values: address, bytes, opcode, parameters, extra)
                var result = native.PCall(state, 1, 5);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DisassemblyException(0, $"splitDisassembledString() call failed: {error}");
                }

                // Read the 5 return values from the stack (in reverse order since Lua stack is LIFO)
                var extra = native.ToString(state, -1) ?? "";       // 5th return value (top of stack)
                var parameters = native.ToString(state, -2) ?? "";   // 4th return value
                var opcode = native.ToString(state, -3) ?? "";       // 3rd return value
                var bytes = native.ToString(state, -4) ?? "";        // 2nd return value
                var addressStr = native.ToString(state, -5) ?? "";   // 1st return value (bottom)

                // Clean up stack
                native.Pop(state, 5);

                // Parse the address
                ulong address = 0;
                if (!string.IsNullOrEmpty(addressStr))
                {
                    // Handle different address formats (0x prefix, plain hex, etc.)
                    var cleanAddr = addressStr.Trim().Replace("0x", "").Replace("0X", "");
                    if (!ulong.TryParse(cleanAddr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address))
                    {
                        throw new DisassemblyException(0, $"Invalid address format in disassembled string: {addressStr}");
                    }
                }

                return new DisassembledInstruction(address, bytes.Trim(), opcode.Trim(), parameters.Trim(), extra.Trim());
            }
            catch (Exception ex) when (ex is not DisassemblyException)
            {
                var lua = PluginContext.Lua;
                lua.Native.SetTop(lua.State, 0);
                throw new DisassemblyException(0, $"Failed to split disassembled string: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Attempts to parse a raw disassembled string without throwing exceptions.
        /// </summary>
        /// <param name="disassembledString">The raw disassembly string from CE's disassemble() function.</param>
        /// <param name="instruction">The parsed instruction, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; otherwise, false.</returns>
        /// <remarks>
        /// <para>Use this method when you need error-tolerant parsing of disassembly strings.</para>
        /// <para>Returns false for malformed or invalid disassembly strings.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var rawDisasm = GetRawDisassemblyFromCE(address);
        /// 
        /// if (Disassembler.TrySplitDisassembledString(rawDisasm, out var instruction))
        /// {
        ///     Console.WriteLine($"Successfully parsed: {instruction.Assembly}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Failed to parse disassembly string");
        /// }
        /// </code>
        /// </example>
        public static bool TrySplitDisassembledString(string disassembledString, out DisassembledInstruction? instruction)
        {
            try
            {
                instruction = SplitDisassembledString(disassembledString);
                return true;
            }
            catch (DisassemblyException)
            {
                instruction = null;
                return false;
            }
        }
    }
}