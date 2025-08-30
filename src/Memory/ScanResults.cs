using System;
using System.Collections.Generic;
using CESDK.Lua;

namespace CESDK.Memory
{
    /// <summary>
    /// Represents the results of a memory scan operation.
    /// Provides access to found addresses and methods to retrieve values using native C# collections.
    /// </summary>
    /// <example>
    /// <code>
    /// var results = scanner.GetResults();
    /// Console.WriteLine($"Found {results.Count} matches");
    /// 
    /// foreach (var address in results.Addresses.Take(10))
    /// {
    ///     var value = results.ReadInteger(address);
    ///     Console.WriteLine($"0x{address:X8}: {value}");
    /// }
    /// </code>
    /// </example>
    public class ScanResults
    {
        private readonly LuaEngine _lua;
        private readonly List<ulong> _addresses;

        internal ScanResults(LuaEngine lua, IntPtr memScanObject)
        {
            _lua = lua ?? throw new ArgumentNullException(nameof(lua));
            _addresses = LoadAddressesFromScan(memScanObject);
        }

        /// <summary>
        /// Gets the number of addresses found in the scan.
        /// </summary>
        /// <value>The total number of matching addresses.</value>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// if (results.Count > 1000)
        /// {
        ///     Console.WriteLine("Too many results, try narrowing the search");
        /// }
        /// </code>
        /// </example>
        public int Count => _addresses.Count;

        /// <summary>
        /// Gets all found addresses as a read-only list.
        /// </summary>
        /// <value>A read-only list of memory addresses.</value>
        /// <remarks>
        /// <para>Each address represents a location in memory where the scan criteria was met.</para>
        /// <para>This list is loaded once when the results are created for optimal performance.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// 
        /// // Process first 100 results
        /// foreach (var address in results.Addresses.Take(100))
        /// {
        ///     Console.WriteLine($"Match found at: 0x{address:X8}");
        /// }
        /// </code>
        /// </example>
        public IReadOnlyList<ulong> Addresses => _addresses.AsReadOnly();

        /// <summary>
        /// Gets a specific address by its index in the results.
        /// </summary>
        /// <param name="index">The zero-based index of the address to retrieve.</param>
        /// <returns>The memory address at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is outside the valid range.</exception>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// if (results.Count > 0)
        /// {
        ///     var firstAddress = results[0];
        ///     var lastAddress = results[results.Count - 1];
        ///     Console.WriteLine($"First: 0x{firstAddress:X8}, Last: 0x{lastAddress:X8}");
        /// }
        /// </code>
        /// </example>
        public ulong this[int index]
        {
            get
            {
                if (index < 0 || index >= _addresses.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {_addresses.Count - 1}");
                return _addresses[index];
            }
        }

        /// <summary>
        /// Gets a range of addresses from the results.
        /// </summary>
        /// <param name="startIndex">The starting index (inclusive).</param>
        /// <param name="count">The number of addresses to retrieve.</param>
        /// <returns>A list of memory addresses.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the range is invalid.</exception>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// 
        /// // Get first 50 addresses
        /// var firstBatch = results.GetRange(0, 50);
        /// 
        /// // Get next 50 addresses
        /// var secondBatch = results.GetRange(50, 50);
        /// 
        /// foreach (var address in firstBatch)
        /// {
        ///     Console.WriteLine($"0x{address:X8}");
        /// }
        /// </code>
        /// </example>
        public List<ulong> GetRange(int startIndex, int count)
        {
            if (startIndex < 0 || startIndex >= _addresses.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > _addresses.Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            return _addresses.GetRange(startIndex, count);
        }

        /// <summary>
        /// Reads a 4-byte integer value from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The integer value at the specified address.</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to read the memory.</exception>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// var firstAddress = results.GetAddress(0);
        /// var value = results.ReadInteger(firstAddress);
        /// Console.WriteLine($"Value at 0x{firstAddress:X8}: {value}");
        /// </code>
        /// </example>
        public int ReadInteger(ulong address)
        {
            try
            {
                _lua.Execute($"__temp_value = readInteger(0x{address:X})");
                var value = _lua.GetGlobalInteger("__temp_value");
                return (int)(value ?? 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read integer from 0x{address:X}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Reads a 4-byte floating-point value from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The float value at the specified address.</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to read the memory.</exception>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// var firstAddress = results.GetAddress(0);
        /// var value = results.ReadFloat(firstAddress);
        /// Console.WriteLine($"Float value at 0x{firstAddress:X8}: {value:F2}");
        /// </code>
        /// </example>
        public float ReadFloat(ulong address)
        {
            try
            {
                _lua.Execute($"__temp_value = readFloat(0x{address:X})");
                var value = _lua.GetGlobalString("__temp_value");
                return float.TryParse(value, out var result) ? result : 0f;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read float from 0x{address:X}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Reads a string value from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <param name="length">The maximum length of the string to read.</param>
        /// <returns>The string value at the specified address.</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to read the memory.</exception>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// var firstAddress = results.GetAddress(0);
        /// var text = results.ReadString(firstAddress, 50);
        /// Console.WriteLine($"String at 0x{firstAddress:X8}: {text}");
        /// </code>
        /// </example>
        public string ReadString(ulong address, int length = 50)
        {
            try
            {
                _lua.Execute($"__temp_value = readString(0x{address:X}, {length})");
                return _lua.GetGlobalString("__temp_value") ?? "";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read string from 0x{address:X}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes a 4-byte integer value to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The integer value to write.</param>
        /// <exception cref="InvalidOperationException">Thrown when unable to write to the memory.</exception>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// var firstAddress = results.GetAddress(0);
        /// results.WriteInteger(firstAddress, 9999);
        /// Console.WriteLine($"Set value at 0x{firstAddress:X8} to 9999");
        /// </code>
        /// </example>
        public void WriteInteger(ulong address, int value)
        {
            try
            {
                _lua.Execute($"writeInteger(0x{address:X}, {value})");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to write integer to 0x{address:X}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes a 4-byte floating-point value to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The float value to write.</param>
        /// <exception cref="InvalidOperationException">Thrown when unable to write to the memory.</exception>
        /// <example>
        /// <code>
        /// var results = scanner.GetResults();
        /// var firstAddress = results.GetAddress(0);
        /// results.WriteFloat(firstAddress, 99.99f);
        /// Console.WriteLine($"Set float value at 0x{firstAddress:X8} to 99.99");
        /// </code>
        /// </example>
        public void WriteFloat(ulong address, float value)
        {
            try
            {
                _lua.Execute($"writeFloat(0x{address:X}, {value})");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to write float to 0x{address:X}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads all addresses from the memory scan into a native C# list.
        /// </summary>
        private List<ulong> LoadAddressesFromScan(IntPtr memScanObject)
        {
            var addresses = new List<ulong>();
            
            try
            {
                var state = _lua.State;
                var native = _lua.Native;

                // Get the found list object
                native.PushInteger(state, memScanObject.ToInt64());
                native.PushString(state, "getAttachedFoundlist");
                native.GetTable(state, -2);

                if (!native.IsFunction(state, -1))
                    throw new InvalidOperationException("MemScan object does not have getAttachedFoundlist method");

                var result = native.PCall(state, 0, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    throw new InvalidOperationException($"Failed to get found list: {error}");
                }

                var foundListPtr = native.ToInteger(state, -1);
                native.Pop(state, 1);

                // Get the count of results
                native.PushInteger(state, foundListPtr);
                native.PushString(state, "Count");
                native.GetTable(state, -2);
                var count = (int)native.ToInteger(state, -1);
                native.Pop(state, 2);

                // Load all addresses into the list
                for (int i = 0; i < count; i++)
                {
                    native.PushInteger(state, foundListPtr);
                    native.PushString(state, "Address");
                    native.GetTable(state, -2);
                    native.PushInteger(state, i);
                    native.GetTable(state, -2);

                    var address = (ulong)native.ToInteger(state, -1);
                    addresses.Add(address);
                    
                    native.Pop(state, 3);
                }

                return addresses;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load scan results: {ex.Message}", ex);
            }
            finally
            {
                _lua.Native.SetTop(_lua.State, 0);
            }
        }
    }

    /// <summary>
    /// Event arguments for scan completion events.
    /// </summary>
    public class ScanCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ScanCompletedEventArgs class.
        /// </summary>
        public ScanCompletedEventArgs()
        {
        }
    }

    /// <summary>
    /// Event arguments for scan progress events.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the ScanProgressEventArgs class.
    /// </remarks>
    /// <param name="totalAddresses">The total number of addresses to scan.</param>
    /// <param name="scannedAddresses">The number of addresses currently scanned.</param>
    /// <param name="resultsFound">The number of matching results found.</param>
    public class ScanProgressEventArgs(ulong totalAddresses, ulong scannedAddresses, ulong resultsFound) : EventArgs
    {
        /// <summary>
        /// Gets the total number of addresses to scan.
        /// </summary>
        /// <value>The total number of memory addresses in the scan range.</value>
        public ulong TotalAddresses { get; } = totalAddresses;

        /// <summary>
        /// Gets the number of addresses currently scanned.
        /// </summary>
        /// <value>The number of addresses processed so far.</value>
        public ulong ScannedAddresses { get; } = scannedAddresses;

        /// <summary>
        /// Gets the number of matching results found so far.
        /// </summary>
        /// <value>The current number of matches found during scanning.</value>
        public ulong ResultsFound { get; } = resultsFound;

        /// <summary>
        /// Gets the scan progress as a percentage (0.0 to 1.0).
        /// </summary>
        /// <value>The completion percentage of the scan operation.</value>
        public double ProgressPercentage =>
            TotalAddresses > 0 ? (double)ScannedAddresses / TotalAddresses : 0.0;
    }
}