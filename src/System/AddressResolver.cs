using System.Globalization;

namespace CESDK.System
{
    /// <summary>
    /// Represents information about a symbol from the symbol table.
    /// </summary>
    public class SymbolInfo
    {
        /// <summary>
        /// Gets the name of the module containing this symbol.
        /// </summary>
        public string ModuleName { get; }

        /// <summary>
        /// Gets the search key or symbol name used to find this symbol.
        /// </summary>
        public string SearchKey { get; }

        /// <summary>
        /// Gets the memory address of the symbol.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Gets the address formatted as a hexadecimal string with 0x prefix.
        /// </summary>
        public string HexAddress => $"0x{Address:X}";

        /// <summary>
        /// Gets the size of the symbol in bytes.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// Gets the address as a pointer for interop scenarios.
        /// </summary>
        public IntPtr Pointer => new IntPtr((long)Address);

        internal SymbolInfo(string moduleName, string searchKey, ulong address, ulong size)
        {
            ModuleName = moduleName;
            SearchKey = searchKey;
            Address = address;
            Size = size;
        }

        /// <summary>
        /// Returns a string representation of the symbol information.
        /// </summary>
        public override string ToString()
        {
            return $"{ModuleName}!{SearchKey} at {HexAddress} (size: {Size} bytes)";
        }
    }

    /// <summary>
    /// Exception thrown when address resolution fails.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the AddressResolutionException class.
    /// </remarks>
    /// <param name="symbolName">The symbol name that failed to resolve.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public class AddressResolutionException(string symbolName, string message, Exception? innerException = null) : Exception($"Failed to resolve symbol '{symbolName}': {message}", innerException)
    {
        /// <summary>
        /// Gets the symbol name that failed to resolve.
        /// </summary>
        public string SymbolName { get; } = symbolName;
    }

    /// <summary>
    /// Provides high-level address resolution and symbol lookup functionality using Cheat Engine's symbol handling.
    /// Wraps CE's getAddress, getAddressSafe, getSymbolInfo, and getModuleSize functions with type-safe C# methods.
    /// </summary>
    /// <remarks>
    /// <para>This class provides convenient access to CE's powerful symbol resolution capabilities.</para>
    /// <para>Symbols can be module names, exported function names, or memory addresses in string format.</para>
    /// <para>Use the safe methods for error-tolerant symbol resolution.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Resolve module base addresses
    /// var kernelBase = AddressResolver.GetAddress("kernel32.dll");
    /// var notepadBase = AddressResolver.GetAddress("notepad.exe");
    /// 
    /// // Resolve exported functions
    /// var createFileAddr = AddressResolver.GetAddress("kernel32.CreateFileA");
    /// 
    /// // Safe resolution with error handling
    /// var addr = AddressResolver.TryGetAddress("some_symbol");
    /// if (addr.HasValue)
    /// {
    ///     Console.WriteLine($"Symbol found at: {addr.Value:X}");
    /// }
    /// 
    /// // Get detailed symbol information
    /// var symbolInfo = AddressResolver.GetSymbolInfo("kernel32.CreateFileA");
    /// Console.WriteLine($"Function: {symbolInfo}");
    /// 
    /// // Get module size
    /// var moduleSize = AddressResolver.GetModuleSize("kernel32.dll");
    /// Console.WriteLine($"kernel32.dll size: {moduleSize} bytes");
    /// </code>
    /// </example>
    public static class AddressResolver
    {
        /// <summary>
        /// Resolves a symbol name to its memory address.
        /// </summary>
        /// <param name="symbolName">The symbol name to resolve (module name, export name, etc.).</param>
        /// <param name="searchLocal">If true, searches the Cheat Engine process symbol table instead of the target process.</param>
        /// <returns>The memory address of the symbol.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="symbolName"/> is null or empty.</exception>
        /// <exception cref="AddressResolutionException">Thrown when the symbol cannot be resolved.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getAddress()</c> Lua function.</para>
        /// <para>Throws an exception if the symbol cannot be found. Use <see cref="TryGetAddress"/> for safe resolution.</para>
        /// <para>Common symbol formats: "kernel32.dll", "kernel32.CreateFileA", "0x12345678", etc.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     var kernelBase = AddressResolver.GetAddress("kernel32.dll");
        ///     var createFile = AddressResolver.GetAddress("kernel32.CreateFileA");
        ///     Console.WriteLine($"kernel32 base: 0x{kernelBase:X}");
        ///     Console.WriteLine($"CreateFileA: 0x{createFile:X}");
        /// }
        /// catch (AddressResolutionException ex)
        /// {
        ///     Console.WriteLine($"Resolution failed: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public static ulong GetAddress(string symbolName, bool searchLocal = false)
        {
            if (string.IsNullOrWhiteSpace(symbolName))
                throw new ArgumentException("Symbol name cannot be null or empty", nameof(symbolName));

            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the getAddress function
                native.GetGlobal(state, "getAddress");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new AddressResolutionException(symbolName, "getAddress function not available in this CE version");
                }

                // Push parameters
                native.PushString(state, symbolName);
                int paramCount = 1;

                if (searchLocal)
                {
                    native.PushBoolean(state, true);
                    paramCount++;
                }

                // Call getAddress
                var result = native.PCall(state, paramCount, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new AddressResolutionException(symbolName, $"getAddress() call failed: {error}");
                }

                // Get the result
                ulong address;
                if (native.IsNumber(state, -1))
                {
                    address = (ulong)native.ToInteger(state, -1);
                }
                else if (native.IsString(state, -1))
                {
                    var addressStr = native.ToString(state, -1);
                    if (!ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out address))
                    {
                        if (!ulong.TryParse(addressStr, out address))
                        {
                            native.Pop(state, 1);
                            throw new AddressResolutionException(symbolName, $"Invalid address format returned: {addressStr}");
                        }
                    }
                }
                else
                {
                    native.Pop(state, 1);
                    throw new AddressResolutionException(symbolName, "Symbol not found or address could not be resolved");
                }

                native.Pop(state, 1);
                return address;
            }
            catch (Exception ex) when (!(ex is AddressResolutionException || ex is ArgumentException))
            {
                native.SetTop(state, 0);
                throw new AddressResolutionException(symbolName, ex.Message, ex);
            }
        }

        /// <summary>
        /// Safely attempts to resolve a symbol name to its memory address.
        /// </summary>
        /// <param name="symbolName">The symbol name to resolve.</param>
        /// <param name="searchLocal">If true, searches the Cheat Engine process symbol table instead of the target process.</param>
        /// <param name="shallow">If true, performs a shallow search (faster but may miss some symbols).</param>
        /// <returns>The memory address of the symbol, or <see langword="null"/> if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="symbolName"/> is null or empty.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getAddressSafe()</c> Lua function.</para>
        /// <para>Returns <see langword="null"/> instead of throwing exceptions when symbols cannot be found.</para>
        /// <para>Use this method when you need error-tolerant symbol resolution.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var kernelAddr = AddressResolver.TryGetAddress("kernel32.dll");
        /// if (kernelAddr.HasValue)
        /// {
        ///     Console.WriteLine($"kernel32 found at: 0x{kernelAddr.Value:X}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("kernel32 not found");
        /// }
        /// 
        /// // Quick shallow search
        /// var quickAddr = AddressResolver.TryGetAddress("some_symbol", shallow: true);
        /// </code>
        /// </example>
        public static ulong? TryGetAddress(string symbolName, bool searchLocal = false, bool shallow = false)
        {
            if (string.IsNullOrWhiteSpace(symbolName))
                throw new ArgumentException("Symbol name cannot be null or empty", nameof(symbolName));

            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the getAddressSafe function
                native.GetGlobal(state, "getAddressSafe");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new InvalidOperationException("getAddressSafe function not available in this CE version");
                }

                // Push parameters
                native.PushString(state, symbolName);
                int paramCount = 1;

                if (searchLocal)
                {
                    native.PushBoolean(state, true);
                    paramCount++;
                }

                if (shallow)
                {
                    if (!searchLocal)
                    {
                        native.PushBoolean(state, false); // searchLocal = false
                        paramCount++;
                    }
                    native.PushBoolean(state, true); // shallow = true
                    paramCount++;
                }

                // Call getAddressSafe
                var result = native.PCall(state, paramCount, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new InvalidOperationException($"getAddressSafe() call failed: {error}");
                }

                // Get the result (number, string, or nil)
                ulong? address = null;
                if (native.IsNumber(state, -1))
                {
                    address = (ulong)native.ToInteger(state, -1);
                }
                else if (native.IsString(state, -1))
                {
                    var addressStr = native.ToString(state, -1);
                    if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var hexAddr))
                    {
                        address = hexAddr;
                    }
                    else if (ulong.TryParse(addressStr, out var decAddr))
                    {
                        address = decAddr;
                    }
                }
                // If it's nil, address remains null

                native.Pop(state, 1);
                return address;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException || ex is ArgumentException))
            {
                native.SetTop(state, 0);
                throw new InvalidOperationException($"Failed to resolve symbol '{symbolName}' safely: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets detailed information about a symbol from the symbol table.
        /// </summary>
        /// <param name="symbolName">The symbol name to lookup.</param>
        /// <returns>Detailed symbol information including module, address, and size.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="symbolName"/> is null or empty.</exception>
        /// <exception cref="AddressResolutionException">Thrown when the symbol cannot be found or symbol info retrieval fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getSymbolInfo()</c> Lua function.</para>
        /// <para>Returns comprehensive information about the symbol including its containing module and size.</para>
        /// <para>Use this when you need more than just the address of a symbol.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     var symbolInfo = AddressResolver.GetSymbolInfo("kernel32.CreateFileA");
        ///     Console.WriteLine($"Module: {symbolInfo.ModuleName}");
        ///     Console.WriteLine($"Address: {symbolInfo.HexAddress}");
        ///     Console.WriteLine($"Size: {symbolInfo.Size} bytes");
        /// }
        /// catch (AddressResolutionException ex)
        /// {
        ///     Console.WriteLine($"Symbol info lookup failed: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public static SymbolInfo GetSymbolInfo(string symbolName)
        {
            if (string.IsNullOrWhiteSpace(symbolName))
                throw new ArgumentException("Symbol name cannot be null or empty", nameof(symbolName));

            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the getSymbolInfo function
                native.GetGlobal(state, "getSymbolInfo");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new AddressResolutionException(symbolName, "getSymbolInfo function not available in this CE version");
                }

                // Push symbol name parameter
                native.PushString(state, symbolName);

                // Call getSymbolInfo
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new AddressResolutionException(symbolName, $"getSymbolInfo() call failed: {error}");
                }

                // The result should be a table with: modulename, searchkey, address, size
                if (!native.IsTable(state, -1))
                {
                    native.Pop(state, 1);
                    throw new AddressResolutionException(symbolName, "Symbol not found or invalid symbol info returned");
                }

                // Extract fields from the table
                string moduleName = "";
                string searchKey = "";
                ulong address = 0;
                ulong size = 0;

                // Get modulename
                native.GetField(state, -1, "modulename");
                if (native.IsString(state, -1))
                {
                    moduleName = native.ToString(state, -1) ?? "";
                }
                native.Pop(state, 1);

                // Get searchkey
                native.GetField(state, -1, "searchkey");
                if (native.IsString(state, -1))
                {
                    searchKey = native.ToString(state, -1) ?? "";
                }
                native.Pop(state, 1);

                // Get address
                native.GetField(state, -1, "address");
                if (native.IsNumber(state, -1))
                {
                    address = (ulong)native.ToInteger(state, -1);
                }
                else if (native.IsString(state, -1))
                {
                    var addressStr = native.ToString(state, -1);
                    ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out address);
                }
                native.Pop(state, 1);

                // Get size
                native.GetField(state, -1, "size");
                if (native.IsNumber(state, -1))
                {
                    size = (ulong)native.ToInteger(state, -1);
                }
                native.Pop(state, 1);

                // Pop the table
                native.Pop(state, 1);

                return new SymbolInfo(moduleName, searchKey, address, size);
            }
            catch (Exception ex) when (!(ex is AddressResolutionException || ex is ArgumentException))
            {
                native.SetTop(state, 0);
                throw new AddressResolutionException(symbolName, ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets the size of a loaded module in bytes.
        /// </summary>
        /// <param name="moduleName">The name of the module (e.g., "kernel32.dll", "notepad.exe").</param>
        /// <returns>The size of the module in bytes.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="moduleName"/> is null or empty.</exception>
        /// <exception cref="AddressResolutionException">Thrown when the module cannot be found or size retrieval fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getModuleSize()</c> Lua function.</para>
        /// <para>Use <see cref="GetAddress"/> to get the base address of the module.</para>
        /// <para>Module name matching is case-insensitive.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     var kernelBase = AddressResolver.GetAddress("kernel32.dll");
        ///     var kernelSize = AddressResolver.GetModuleSize("kernel32.dll");
        ///     
        ///     Console.WriteLine($"kernel32.dll:");
        ///     Console.WriteLine($"  Base: 0x{kernelBase:X}");
        ///     Console.WriteLine($"  Size: {kernelSize:N0} bytes");
        ///     Console.WriteLine($"  End:  0x{kernelBase + kernelSize:X}");
        /// }
        /// catch (AddressResolutionException ex)
        /// {
        ///     Console.WriteLine($"Module info lookup failed: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public static ulong GetModuleSize(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("Module name cannot be null or empty", nameof(moduleName));

            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the getModuleSize function
                native.GetGlobal(state, "getModuleSize");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new AddressResolutionException(moduleName, "getModuleSize function not available in this CE version");
                }

                // Push module name parameter
                native.PushString(state, moduleName);

                // Call getModuleSize
                var result = native.PCall(state, 1, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new AddressResolutionException(moduleName, $"getModuleSize() call failed: {error}");
                }

                // Get the size result
                ulong size;
                if (native.IsNumber(state, -1))
                {
                    size = (ulong)native.ToInteger(state, -1);
                }
                else if (native.IsString(state, -1))
                {
                    var sizeStr = native.ToString(state, -1);
                    if (!ulong.TryParse(sizeStr, out size))
                    {
                        native.Pop(state, 1);
                        throw new AddressResolutionException(moduleName, $"Invalid size format returned: {sizeStr}");
                    }
                }
                else
                {
                    native.Pop(state, 1);
                    throw new AddressResolutionException(moduleName, "Module not found or size could not be determined");
                }

                native.Pop(state, 1);
                return size;
            }
            catch (Exception ex) when (!(ex is AddressResolutionException || ex is ArgumentException))
            {
                native.SetTop(state, 0);
                throw new AddressResolutionException(moduleName, ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets both the base address and size of a module in a single operation.
        /// </summary>
        /// <param name="moduleName">The name of the module.</param>
        /// <returns>A tuple containing the base address and size of the module.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="moduleName"/> is null or empty.</exception>
        /// <exception cref="AddressResolutionException">Thrown when the module cannot be found.</exception>
        /// <example>
        /// <code>
        /// var (baseAddress, size) = AddressResolver.GetModuleInfo("kernel32.dll");
        /// Console.WriteLine($"kernel32: 0x{baseAddress:X} - 0x{baseAddress + size:X} ({size:N0} bytes)");
        /// </code>
        /// </example>
        public static (ulong BaseAddress, ulong Size) GetModuleInfo(string moduleName)
        {
            var baseAddress = GetAddress(moduleName);
            var size = GetModuleSize(moduleName);
            return (baseAddress, size);
        }

        /// <summary>
        /// Checks if a symbol exists without throwing exceptions.
        /// </summary>
        /// <param name="symbolName">The symbol name to check.</param>
        /// <param name="searchLocal">If true, searches the Cheat Engine process symbol table.</param>
        /// <returns>True if the symbol exists; otherwise, false.</returns>
        /// <example>
        /// <code>
        /// if (AddressResolver.SymbolExists("kernel32.CreateFileA"))
        /// {
        ///     var addr = AddressResolver.GetAddress("kernel32.CreateFileA");
        ///     // Use the address...
        /// }
        /// </code>
        /// </example>
        public static bool SymbolExists(string symbolName, bool searchLocal = false)
        {
            return TryGetAddress(symbolName, searchLocal).HasValue;
        }
    }
}