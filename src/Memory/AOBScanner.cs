using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CESDK.Lua;

namespace CESDK.Memory
{
    /// <summary>
    /// Represents a memory address found during AOB scanning.
    /// </summary>
    public class AOBMatch
    {
        /// <summary>
        /// Gets the memory address where the pattern was found.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Gets the address formatted as a hexadecimal string with 0x prefix.
        /// </summary>
        public string HexAddress => $"0x{Address:X}";

        /// <summary>
        /// Gets the address as a pointer-sized integer for interop scenarios.
        /// </summary>
        public IntPtr Pointer => new IntPtr((long)Address);

        internal AOBMatch(ulong address)
        {
            Address = address;
        }

        /// <summary>
        /// Returns the hexadecimal representation of the address.
        /// </summary>
        public override string ToString() => HexAddress;
    }

    /// <summary>
    /// Represents the result of an AOB scan operation.
    /// </summary>
    public class AOBScanResult
    {
        /// <summary>
        /// Gets a value indicating whether any matches were found.
        /// </summary>
        public bool HasMatches => Matches.Any();

        /// <summary>
        /// Gets the number of matches found.
        /// </summary>
        public int Count => Matches.Count;

        /// <summary>
        /// Gets all matches found during the scan.
        /// </summary>
        public IReadOnlyList<AOBMatch> Matches { get; }

        /// <summary>
        /// Gets the first match, or null if no matches were found.
        /// </summary>
        public AOBMatch? FirstMatch => Matches.FirstOrDefault();

        /// <summary>
        /// Gets a value indicating whether exactly one match was found.
        /// </summary>
        public bool IsUnique => Count == 1;

        /// <summary>
        /// Gets the pattern that was searched for.
        /// </summary>
        public string Pattern { get; }

        internal AOBScanResult(string pattern, IEnumerable<ulong> addresses)
        {
            Pattern = pattern;
            Matches = addresses.Select(addr => new AOBMatch(addr)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the unique match if exactly one was found, otherwise throws an exception.
        /// </summary>
        /// <returns>The unique match found.</returns>
        /// <exception cref="InvalidOperationException">Thrown when zero matches or multiple matches were found.</exception>
        public AOBMatch GetUniqueMatch()
        {
            return Count switch
            {
                0 => throw new InvalidOperationException($"No matches found for pattern: {Pattern}"),
                1 => Matches[0],
                _ => throw new InvalidOperationException($"Multiple matches ({Count}) found for pattern: {Pattern}. Expected exactly one match.")
            };
        }

        /// <summary>
        /// Tries to get the unique match if exactly one was found.
        /// </summary>
        /// <param name="match">The unique match, or null if not exactly one match was found.</param>
        /// <returns>True if exactly one match was found; otherwise, false.</returns>
        public bool TryGetUniqueMatch(out AOBMatch? match)
        {
            if (IsUnique)
            {
                match = Matches[0];
                return true;
            }

            match = null;
            return false;
        }

        /// <summary>
        /// Returns a string representation of the scan result.
        /// </summary>
        public override string ToString()
        {
            return Count switch
            {
                0 => $"No matches found for pattern: {Pattern}",
                1 => $"Unique match at {FirstMatch} for pattern: {Pattern}",
                _ => $"{Count} matches found for pattern: {Pattern}"
            };
        }
    }

    /// <summary>
    /// Configuration options for AOB scanning operations.
    /// </summary>
    public class AOBScanOptions
    {
        /// <summary>
        /// Gets or sets the memory protection flags to filter regions.
        /// </summary>
        /// <remarks>
        /// <para>Format: X=Executable, W=Writable, C=Copy On Write.</para>
        /// <para>Use + for required flags, - for excluded flags (e.g., "+X-W" = executable, non-writable).</para>
        /// </remarks>
        public string? ProtectionFlags { get; set; }

        /// <summary>
        /// Gets or sets the memory alignment requirements.
        /// </summary>
        public MemoryAlignment? Alignment { get; set; }

        /// <summary>
        /// Gets or sets the module name to limit scanning to (optional).
        /// </summary>
        public string? ModuleName { get; set; }

        /// <summary>
        /// Creates scan options for executable memory only.
        /// </summary>
        public static AOBScanOptions ExecutableMemory() => new() { ProtectionFlags = "+X" };

        /// <summary>
        /// Creates scan options for writable memory only.
        /// </summary>
        public static AOBScanOptions WritableMemory() => new() { ProtectionFlags = "+W" };

        /// <summary>
        /// Creates scan options for a specific module.
        /// </summary>
        /// <param name="moduleName">The name of the module to scan within.</param>
        public static AOBScanOptions InModule(string moduleName) => new() { ModuleName = moduleName };

        /// <summary>
        /// Creates scan options with memory alignment requirements.
        /// </summary>
        /// <param name="alignment">The memory alignment requirements.</param>
        public static AOBScanOptions WithAlignment(MemoryAlignment alignment) => new() { Alignment = alignment };
    }

    /// <summary>
    /// Exception thrown when an AOB pattern is invalid or malformed.
    /// </summary>
    public class InvalidAOBPatternException : Exception
    {
        /// <summary>
        /// Gets the invalid pattern that caused the exception.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Initializes a new instance of the InvalidAOBPatternException class.
        /// </summary>
        /// <param name="pattern">The invalid pattern.</param>
        /// <param name="message">The error message.</param>
        public InvalidAOBPatternException(string pattern, string message)
            : base($"Invalid AOB pattern '{pattern}': {message}")
        {
            Pattern = pattern;
        }
    }

    /// <summary>
    /// Exception thrown when an AOB scan operation fails.
    /// </summary>
    public class AOBScanException : Exception
    {
        /// <summary>
        /// Gets the pattern that was being scanned when the exception occurred.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Initializes a new instance of the AOBScanException class.
        /// </summary>
        /// <param name="pattern">The pattern being scanned.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public AOBScanException(string pattern, string message, Exception? innerException = null)
            : base($"AOB scan failed for pattern '{pattern}': {message}", innerException)
        {
            Pattern = pattern;
        }
    }
    /// <summary>
    /// Provides high-level Array of Bytes (AOB) scanning functionality using Cheat Engine's native scanning capabilities.
    /// AOB scanning allows finding byte patterns in process memory with wildcard support.
    /// </summary>
    /// <remarks>
    /// <para>AOB patterns are represented as space-separated hex bytes (e.g., "48 8B 05 ?? ?? ?? ?? 48 85 C0").</para>
    /// <para>Use "??" for wildcard bytes that match any value.</para>
    /// <para>This class provides a user-friendly interface with strongly-typed results and comprehensive error handling.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic pattern scanning with high-level results
    /// var result = AOBScanner.Scan("48 8B 05 ?? ?? ?? ?? 48 85 C0");
    /// if (result.HasMatches)
    /// {
    ///     Console.WriteLine($"Found {result.Count} matches");
    ///     foreach (var match in result.Matches)
    ///     {
    ///         Console.WriteLine($"Match at: {match.HexAddress}");
    ///     }
    /// }
    /// 
    /// // Scan for unique pattern with exception handling
    /// try
    /// {
    ///     var uniqueMatch = AOBScanner.Scan("E8 ?? ?? ?? ?? 85 C0 74").GetUniqueMatch();
    ///     Console.WriteLine($"Unique match found at: {uniqueMatch}");
    /// }
    /// catch (InvalidOperationException ex)
    /// {
    ///     Console.WriteLine($"Error: {ex.Message}");
    /// }
    /// 
    /// // Scan with options
    /// var options = AOBScanOptions.InModule("kernel32.dll");
    /// var moduleResult = AOBScanner.Scan("FF 25", options);
    /// </code>
    /// </example>
    public static class AOBScanner
    {
        /// <summary>
        /// Scans the entire process memory for the specified AOB pattern.
        /// </summary>
        /// <param name="pattern">The AOB pattern string (e.g., "48 8B 05 ?? ?? ?? ?? 48 85 C0").</param>
        /// <param name="options">Scan options to configure the search (optional).</param>
        /// <returns>A result object containing all matches found and helper methods.</returns>
        /// <exception cref="InvalidAOBPatternException">Thrown when the pattern format is invalid.</exception>
        /// <exception cref="AOBScanException">Thrown when the scan operation fails.</exception>
        /// <remarks>
        /// <para>This method scans the entire process memory and returns a comprehensive result object.</para>
        /// <para>Use the result object's properties and methods to work with the found addresses.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Basic scan with fluent API
        /// var result = AOBScanner.Scan("48 8B 05 ?? ?? ?? ?? 48 85 C0");
        /// Console.WriteLine(result); // "X matches found for pattern: ..."
        /// 
        /// // Check for matches
        /// if (result.HasMatches)
        /// {
        ///     foreach (var match in result.Matches)
        ///         Console.WriteLine($"Found at: {match.HexAddress}");
        /// }
        /// 
        /// // Scan with options
        /// var execResult = AOBScanner.Scan("E8 ?? ?? ?? ??", AOBScanOptions.ExecutableMemory());
        /// </code>
        /// </example>
        public static AOBScanResult Scan(string pattern, AOBScanOptions? options = null)
        {
            ValidatePattern(pattern);
            options ??= new AOBScanOptions();

            try
            {
                // If a module is specified, use module-specific scanning
                if (!string.IsNullOrEmpty(options.ModuleName))
                {
                    var address = ScanModuleUniqueInternal(options.ModuleName!, pattern, options.ProtectionFlags, options.Alignment);
                    var addresses = address.HasValue ? new[] { address.Value } : Array.Empty<ulong>();
                    return new AOBScanResult(pattern, addresses);
                }

                var results = ScanInternal(pattern, options.ProtectionFlags, options.Alignment);
                return new AOBScanResult(pattern, results);
            }
            catch (Exception ex) when (ex is not InvalidAOBPatternException)
            {
                throw new AOBScanException(pattern, ex.Message, ex);
            }
        }

        /// <summary>
        /// Scans the entire process memory for the specified AOB pattern (legacy overload).
        /// </summary>
        /// <param name="pattern">The AOB pattern string.</param>
        /// <param name="protectionFlags">Memory protection flags to filter regions (optional).</param>
        /// <param name="alignment">Memory alignment requirements (optional).</param>
        /// <returns>A result object containing all matches found.</returns>
        public static AOBScanResult Scan(string pattern, string? protectionFlags, MemoryAlignment? alignment = null)
        {
            var options = new AOBScanOptions
            {
                ProtectionFlags = protectionFlags,
                Alignment = alignment
            };
            return Scan(pattern, options);
        }

        /// <summary>
        /// Validates an AOB pattern and throws an exception if invalid.
        /// </summary>
        /// <param name="pattern">The pattern to validate.</param>
        /// <exception cref="InvalidAOBPatternException">Thrown when the pattern is invalid.</exception>
        private static void ValidatePattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new InvalidAOBPatternException(pattern ?? "", "Pattern cannot be null or empty");

            if (!IsValidPattern(pattern))
                throw new InvalidAOBPatternException(pattern, "Pattern contains invalid hex bytes or format");
        }

        private static List<ulong> ScanInternal(string pattern, string? protectionFlags = null, MemoryAlignment? alignment = null)
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the AOBScan function
                native.GetGlobal(state, "AOBScan");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new InvalidOperationException("AOBScan function not available");
                }

                // Push parameters
                native.PushString(state, pattern);
                int paramCount = 1;

                if (!string.IsNullOrEmpty(protectionFlags))
                {
                    native.PushString(state, protectionFlags!);
                    paramCount++;
                }

                if (alignment != null)
                {
                    if (string.IsNullOrEmpty(protectionFlags))
                    {
                        native.PushNil(state);
                        paramCount++;
                    }

                    native.PushInteger(state, (long)alignment.Type);
                    paramCount++;

                    if (!string.IsNullOrEmpty(alignment.Parameter))
                    {
                        native.PushString(state, alignment.Parameter);
                        paramCount++;
                    }
                }

                // Call AOBScan
                var result = native.PCall(state, paramCount, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new InvalidOperationException($"AOBScan failed: {error}");
                }

                // Get the StringList result
                var addresses = new List<ulong>();
                if (native.IsUserData(state, -1))
                {
                    // The result is a StringList object, we need to iterate through it
                    // Get the Count property
                    native.GetField(state, -1, "Count");
                    if (native.IsNumber(state, -1))
                    {
                        var count = native.ToInteger(state, -1);
                        native.Pop(state, 1);

                        // Iterate through the strings
                        for (int i = 0; i < count; i++)
                        {
                            native.PushInteger(state, i);
                            native.GetTable(state, -2);

                            if (native.IsString(state, -1))
                            {
                                var addressStr = native.ToString(state, -1);
                                if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var address))
                                {
                                    addresses.Add(address);
                                }
                            }
                            native.Pop(state, 1);
                        }
                    }
                    else
                    {
                        native.Pop(state, 1);
                    }
                }

                native.Pop(state, 1);
                return addresses;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException || ex is ArgumentException))
            {
                native.SetTop(state, 0);
                throw new InvalidOperationException($"AOB scan failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convenience method to scan for a unique pattern and return the first match.
        /// </summary>
        /// <param name="pattern">The AOB pattern string.</param>
        /// <param name="options">Scan options (optional).</param>
        /// <returns>The first match found, or null if no matches found.</returns>
        /// <exception cref="InvalidAOBPatternException">Thrown when the pattern format is invalid.</exception>
        /// <exception cref="AOBScanException">Thrown when the scan operation fails.</exception>
        /// <example>
        /// <code>
        /// var match = AOBScanner.ScanFirst("E8 ?? ?? ?? ?? 85 C0 74");
        /// if (match != null)
        /// {
        ///     Console.WriteLine($"Found pattern at: {match}");
        /// }
        /// </code>
        /// </example>
        public static AOBMatch? ScanFirst(string pattern, AOBScanOptions? options = null)
        {
            var result = Scan(pattern, options);
            return result.FirstMatch;
        }

        /// <summary>
        /// Convenience method to scan for a unique pattern using CE's optimized unique scan.
        /// </summary>
        /// <param name="pattern">The AOB pattern string.</param>
        /// <param name="options">Scan options (optional).</param>
        /// <returns>The unique match found, or null if no match found.</returns>
        /// <exception cref="InvalidAOBPatternException">Thrown when the pattern format is invalid.</exception>
        /// <exception cref="AOBScanException">Thrown when the scan operation fails.</exception>
        /// <remarks>
        /// This method uses CE's AOBScanUnique function which is optimized for single results.
        /// </remarks>
        public static AOBMatch? ScanUnique(string pattern, AOBScanOptions? options = null)
        {
            ValidatePattern(pattern);
            options ??= new AOBScanOptions();

            try
            {
                // If a module is specified, use module-specific scanning
                if (!string.IsNullOrEmpty(options.ModuleName))
                {
                    var address = ScanModuleUniqueInternal(options.ModuleName!, pattern, options.ProtectionFlags, options.Alignment);
                    return address.HasValue ? new AOBMatch(address.Value) : null;
                }

                var address2 = ScanUniqueInternal(pattern, options.ProtectionFlags, options.Alignment);
                return address2.HasValue ? new AOBMatch(address2.Value) : null;
            }
            catch (Exception ex) when (ex is not InvalidAOBPatternException)
            {
                throw new AOBScanException(pattern, ex.Message, ex);
            }
        }

        private static ulong? ScanUniqueInternal(string pattern, string? protectionFlags = null, MemoryAlignment? alignment = null)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("AOB pattern cannot be null or empty", nameof(pattern));

            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the AOBScanUnique function
                native.GetGlobal(state, "AOBScanUnique");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new InvalidOperationException("AOBScanUnique function not available");
                }

                // Push parameters
                native.PushString(state, pattern);
                int paramCount = 1;

                if (!string.IsNullOrEmpty(protectionFlags))
                {
                    native.PushString(state, protectionFlags!);
                    paramCount++;
                }

                if (alignment != null)
                {
                    if (string.IsNullOrEmpty(protectionFlags))
                    {
                        native.PushNil(state);
                        paramCount++;
                    }

                    native.PushInteger(state, (long)alignment.Type);
                    paramCount++;

                    if (!string.IsNullOrEmpty(alignment.Parameter))
                    {
                        native.PushString(state, alignment.Parameter);
                        paramCount++;
                    }
                }

                // Call AOBScanUnique
                var result = native.PCall(state, paramCount, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new InvalidOperationException($"AOBScanUnique failed: {error}");
                }

                // Get the result (integer address or nil)
                ulong? address = null;
                if (native.IsNumber(state, -1))
                {
                    address = (ulong)native.ToInteger(state, -1);
                }
                else if (native.IsString(state, -1))
                {
                    var addressStr = native.ToString(state, -1);
                    if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var parsedAddress))
                    {
                        address = parsedAddress;
                    }
                }

                native.Pop(state, 1);
                return address;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException || ex is ArgumentException))
            {
                native.SetTop(state, 0);
                throw new InvalidOperationException($"AOB unique scan failed: {ex.Message}", ex);
            }
        }

        private static ulong? ScanModuleUniqueInternal(string moduleName, string pattern, string? protectionFlags = null, MemoryAlignment? alignment = null)
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the AOBScanModuleUnique function
                native.GetGlobal(state, "AOBScanModuleUnique");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new InvalidOperationException("AOBScanModuleUnique function not available");
                }

                // Push parameters
                native.PushString(state, moduleName);
                native.PushString(state, pattern);
                int paramCount = 2;

                if (!string.IsNullOrEmpty(protectionFlags))
                {
                    native.PushString(state, protectionFlags!);
                    paramCount++;
                }

                if (alignment != null)
                {
                    if (string.IsNullOrEmpty(protectionFlags))
                    {
                        native.PushNil(state);
                        paramCount++;
                    }

                    native.PushInteger(state, (long)alignment.Type);
                    paramCount++;

                    if (!string.IsNullOrEmpty(alignment.Parameter))
                    {
                        native.PushString(state, alignment.Parameter);
                        paramCount++;
                    }
                }

                // Call AOBScanModuleUnique
                var result = native.PCall(state, paramCount, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new InvalidOperationException($"AOBScanModuleUnique failed: {error}");
                }

                // Get the result (integer address or nil)
                ulong? address = null;
                if (native.IsNumber(state, -1))
                {
                    address = (ulong)native.ToInteger(state, -1);
                }
                else if (native.IsString(state, -1))
                {
                    var addressStr = native.ToString(state, -1);
                    if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var parsedAddress))
                    {
                        address = parsedAddress;
                    }
                }

                native.Pop(state, 1);
                return address;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException || ex is ArgumentException))
            {
                native.SetTop(state, 0);
                throw new InvalidOperationException($"AOB module unique scan failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Scans for a unique AOB pattern within a specific module.
        /// </summary>
        /// <param name="moduleName">The name of the module to scan within (e.g., "kernel32.dll", "notepad.exe").</param>
        /// <param name="pattern">The AOB pattern string (e.g., "48 8B 05 ?? ?? ?? ?? 48 85 C0").</param>
        /// <param name="protectionFlags">Memory protection flags to filter regions (optional).</param>
        /// <param name="alignment">Memory alignment requirements (optional).</param>
        /// <returns>The address of the first match within the module, or <see langword="null"/> if no match is found.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="moduleName"/> or <paramref name="pattern"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the scan fails or Lua engine is unavailable.</exception>
        /// <remarks>
        /// <para>This method limits the scan to memory regions belonging to the specified module.</para>
        /// <para>Module-specific scanning is more efficient and reduces false positives.</para>
        /// <para>Module name matching is case-insensitive.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Scan within main executable
        /// var mainAddress = AOBScanner.ScanModuleUnique("myapp.exe", "48 8B 05 ?? ?? ?? ?? 48 85 C0");
        /// 
        /// // Scan within system DLL
        /// var kernelAddress = AOBScanner.ScanModuleUnique("kernel32.dll", "FF 25");
        /// 
        /// // Scan with protection flags
        /// var address = AOBScanner.ScanModuleUnique("user32.dll", "E8 ?? ?? ?? ??", "+X");
        /// </code>
        /// </example>
        public static ulong? ScanModuleUnique(string moduleName, string pattern, string? protectionFlags = null, MemoryAlignment? alignment = null)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("Module name cannot be null or empty", nameof(moduleName));
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("AOB pattern cannot be null or empty", nameof(pattern));

            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the AOBScanModuleUnique function
                native.GetGlobal(state, "AOBScanModuleUnique");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new InvalidOperationException("AOBScanModuleUnique function not available");
                }

                // Push parameters
                native.PushString(state, moduleName);
                native.PushString(state, pattern);
                int paramCount = 2;

                if (!string.IsNullOrEmpty(protectionFlags))
                {
                    native.PushString(state, protectionFlags!);
                    paramCount++;
                }

                if (alignment != null)
                {
                    if (string.IsNullOrEmpty(protectionFlags))
                    {
                        native.PushNil(state);
                        paramCount++;
                    }

                    native.PushInteger(state, (long)alignment.Type);
                    paramCount++;

                    if (!string.IsNullOrEmpty(alignment.Parameter))
                    {
                        native.PushString(state, alignment.Parameter);
                        paramCount++;
                    }
                }

                // Call AOBScanModuleUnique
                var result = native.PCall(state, paramCount, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new InvalidOperationException($"AOBScanModuleUnique failed: {error}");
                }

                // Get the result (integer address or nil)
                ulong? address = null;
                if (native.IsNumber(state, -1))
                {
                    address = (ulong)native.ToInteger(state, -1);
                }
                else if (native.IsString(state, -1))
                {
                    var addressStr = native.ToString(state, -1);
                    if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var parsedAddress))
                    {
                        address = parsedAddress;
                    }
                }

                native.Pop(state, 1);
                return address;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException || ex is ArgumentException))
            {
                native.SetTop(state, 0);
                throw new InvalidOperationException($"AOB module scan failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates an AOB pattern string for correct format.
        /// </summary>
        /// <param name="pattern">The AOB pattern to validate.</param>
        /// <returns><see langword="true"/> if the pattern is valid; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Valid patterns contain space-separated hex bytes (00-FF) or wildcard bytes (??).</para>
        /// <para>Examples of valid patterns: "48 8B 05", "?? ?? ?? 48", "E8 ?? ?? ?? ?? 85 C0".</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// bool isValid = AOBScanner.IsValidPattern("48 8B 05 ?? ?? ?? ??"); // returns true
        /// bool isInvalid = AOBScanner.IsValidPattern("XX YY ZZ");            // returns false
        /// </code>
        /// </example>
        public static bool IsValidPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return false;

            var parts = pattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (part == "??")
                    continue;

                if (part.Length != 2)
                    return false;

                if (!byte.TryParse(part, NumberStyles.HexNumber, null, out _))
                    return false;
            }

            return parts.Length > 0;
        }
    }

    /// <summary>
    /// Extension methods for AOB scanning results to provide additional convenience methods.
    /// </summary>
    public static class AOBScanExtensions
    {
        /// <summary>
        /// Throws an exception if the result contains no matches.
        /// </summary>
        /// <param name="result">The scan result to check.</param>
        /// <returns>The same result for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no matches are found.</exception>
        /// <example>
        /// <code>
        /// var result = AOBScanner.Scan("48 8B 05 ?? ?? ?? ??")
        ///     .EnsureFound()
        ///     .GetUniqueMatch();
        /// </code>
        /// </example>
        public static AOBScanResult EnsureFound(this AOBScanResult result)
        {
            if (!result.HasMatches)
                throw new InvalidOperationException($"No matches found for pattern: {result.Pattern}");
            return result;
        }

        /// <summary>
        /// Throws an exception if the result doesn't contain exactly one match.
        /// </summary>
        /// <param name="result">The scan result to check.</param>
        /// <returns>The same result for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when zero or multiple matches are found.</exception>
        /// <example>
        /// <code>
        /// var uniqueMatch = AOBScanner.Scan("48 8B 05 ?? ?? ?? ??")
        ///     .EnsureUnique()
        ///     .FirstMatch;
        /// </code>
        /// </example>
        public static AOBScanResult EnsureUnique(this AOBScanResult result)
        {
            result.GetUniqueMatch(); // This will throw if not unique
            return result;
        }

        /// <summary>
        /// Filters matches to only those within a specific address range.
        /// </summary>
        /// <param name="result">The scan result to filter.</param>
        /// <param name="startAddress">The minimum address (inclusive).</param>
        /// <param name="endAddress">The maximum address (inclusive).</param>
        /// <returns>A new result containing only matches within the specified range.</returns>
        /// <example>
        /// <code>
        /// var filteredResult = AOBScanner.Scan("48 8B 05")
        ///     .FilterByRange(0x10000000, 0x20000000);
        /// </code>
        /// </example>
        public static AOBScanResult FilterByRange(this AOBScanResult result, ulong startAddress, ulong endAddress)
        {
            var filteredAddresses = result.Matches
                .Where(match => match.Address >= startAddress && match.Address <= endAddress)
                .Select(match => match.Address);

            return new AOBScanResult(result.Pattern, filteredAddresses);
        }

        /// <summary>
        /// Converts an AOBMatch to a formatted string with custom formatting.
        /// </summary>
        /// <param name="match">The match to format.</param>
        /// <param name="format">The format string ("X" for uppercase hex, "x" for lowercase, "D" for decimal).</param>
        /// <returns>The formatted address string.</returns>
        /// <example>
        /// <code>
        /// var match = AOBScanner.ScanFirst("48 8B 05");
        /// Console.WriteLine(match?.ToFormattedString("X8")); // "12345678"
        /// Console.WriteLine(match?.ToFormattedString("D"));  // "305419896"
        /// </code>
        /// </example>
        public static string ToFormattedString(this AOBMatch match, string format = "X")
        {
            return match.Address.ToString(format);
        }
    }

    /// <summary>
    /// Represents memory alignment requirements for AOB scanning.
    /// </summary>
    public class MemoryAlignment
    {
        /// <summary>
        /// Gets the alignment type.
        /// </summary>
        public AlignmentType Type { get; }

        /// <summary>
        /// Gets the alignment parameter (divisor for modulo alignment or suffix for last digits alignment).
        /// </summary>
        public string Parameter { get; }

        /// <summary>
        /// Initializes a new instance for aligned scanning (addresses must be divisible by the specified value).
        /// </summary>
        /// <param name="divisor">The value that addresses must be divisible by.</param>
        public MemoryAlignment(int divisor)
        {
            Type = AlignmentType.Aligned;
            Parameter = divisor.ToString();
        }

        /// <summary>
        /// Initializes a new instance for last digits alignment (addresses must end with specific digits).
        /// </summary>
        /// <param name="lastDigits">The digits that addresses must end with (e.g., "00" for addresses ending in 00).</param>
        public MemoryAlignment(string lastDigits)
        {
            Type = AlignmentType.LastDigits;
            Parameter = lastDigits ?? throw new ArgumentNullException(nameof(lastDigits));
        }

        /// <summary>
        /// Creates a memory alignment for no alignment requirements.
        /// </summary>
        public static MemoryAlignment None => new();

        private MemoryAlignment()
        {
            Type = AlignmentType.NotAligned;
            Parameter = string.Empty;
        }
    }

}