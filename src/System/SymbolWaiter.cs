namespace CESDK.System
{
    /// <summary>
    /// Represents different levels of symbol loading completion.
    /// </summary>
    public enum SymbolLevel
    {
        /// <summary>
        /// Only sections have been enumerated.
        /// </summary>
        Sections = 1,

        /// <summary>
        /// DLL exports are loaded (includes sections).
        /// </summary>
        Exports = 2,

        /// <summary>
        /// .NET symbols are loaded (includes exports and sections).
        /// </summary>
        DotNet = 3,

        /// <summary>
        /// PDB symbols are loaded (includes .NET, exports, and sections).
        /// </summary>
        PDB = 4
    }

    /// <summary>
    /// Exception thrown when symbol waiting operations fail.
    /// </summary>
    public class SymbolWaitException : Exception
    {
        /// <summary>
        /// Gets the symbol level that was being waited for.
        /// </summary>
        public SymbolLevel Level { get; }

        /// <summary>
        /// Initializes a new instance of the SymbolWaitException class.
        /// </summary>
        /// <param name="level">The symbol level that failed to load.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SymbolWaitException(SymbolLevel level, string message, Exception? innerException = null)
            : base($"Failed to wait for {level} symbols: {message}", innerException)
        {
            Level = level;
        }
    }

    /// <summary>
    /// Provides high-level symbol loading synchronization using Cheat Engine's waitFor functions.
    /// This class wraps CE's symbol loading waits with modern async patterns and timeout support.
    /// </summary>
    /// <remarks>
    /// <para>CE loads symbols asynchronously in the background. These methods let you wait for specific loading phases to complete.</para>
    /// <para>Symbol loading is hierarchical: PDB > .NET > Exports > Sections</para>
    /// <para>Higher levels include all lower levels (e.g., waiting for .NET also waits for exports and sections).</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Wait for basic symbol information
    /// await SymbolWaiter.WaitForSectionsAsync();
    /// Console.WriteLine("Sections loaded, can resolve module addresses");
    /// 
    /// // Wait for exported functions
    /// await SymbolWaiter.WaitForExportsAsync();
    /// var createFile = AddressResolver.GetAddress("kernel32.CreateFileA");
    /// 
    /// // Wait for full debugging symbols
    /// await SymbolWaiter.WaitForPDBAsync(TimeSpan.FromMinutes(5));
    /// Console.WriteLine("Full debugging symbols loaded");
    /// 
    /// // Synchronous waiting
    /// SymbolWaiter.WaitForDotNet();
    /// var managedSymbol = AddressResolver.GetAddress("System.String.get_Length");
    /// </code>
    /// </example>
    public static class SymbolWaiter
    {
        /// <summary>
        /// Waits for memory sections to be enumerated (synchronously).
        /// </summary>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <remarks>
        /// <para>This is the fastest level of symbol loading - just basic memory section information.</para>
        /// <para>After this completes, you can resolve module base addresses and basic memory layout.</para>
        /// <para>This method blocks the calling thread until sections are loaded.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// SymbolWaiter.WaitForSections();
        /// var kernelBase = AddressResolver.GetAddress("kernel32.dll");
        /// Console.WriteLine($"kernel32 loaded at: 0x{kernelBase:X}");
        /// </code>
        /// </example>
        public static void WaitForSections()
        {
            CallWaitFunction("waitForSections", SymbolLevel.Sections);
        }

        /// <summary>
        /// Waits for DLL exports to be loaded (synchronously).
        /// </summary>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <remarks>
        /// <para>This level includes sections plus all exported functions from DLLs.</para>
        /// <para>After this completes, you can resolve exported function addresses.</para>
        /// <para>This method blocks the calling thread until exports are loaded.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// SymbolWaiter.WaitForExports();
        /// var createFile = AddressResolver.GetAddress("kernel32.CreateFileA");
        /// var loadLibrary = AddressResolver.GetAddress("kernel32.LoadLibraryA");
        /// Console.WriteLine("Exported functions are available");
        /// </code>
        /// </example>
        public static void WaitForExports()
        {
            CallWaitFunction("waitForExports", SymbolLevel.Exports);
        }

        /// <summary>
        /// Waits for .NET symbols to be loaded (synchronously).
        /// </summary>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <remarks>
        /// <para>This level includes exports, sections, plus .NET managed code symbols.</para>
        /// <para>After this completes, you can resolve .NET method addresses and managed symbols.</para>
        /// <para>This method blocks the calling thread until .NET symbols are loaded.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// SymbolWaiter.WaitForDotNet();
        /// if (AddressResolver.SymbolExists("System.String.get_Length"))
        /// {
        ///     var stringLength = AddressResolver.GetAddress("System.String.get_Length");
        ///     Console.WriteLine(".NET symbols loaded and accessible");
        /// }
        /// </code>
        /// </example>
        public static void WaitForDotNet()
        {
            CallWaitFunction("waitForDotNet", SymbolLevel.DotNet);
        }

        /// <summary>
        /// Waits for PDB debugging symbols to be loaded (synchronously).
        /// </summary>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <remarks>
        /// <para>This is the highest level of symbol loading - includes everything plus full debugging symbols.</para>
        /// <para>After this completes, you can resolve internal functions, local variables, and full debugging info.</para>
        /// <para>This method blocks the calling thread until PDB symbols are loaded.</para>
        /// <para>Warning: This can take a very long time, especially on first run when downloading PDB files.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Console.WriteLine("Loading PDB symbols (this may take a while)...");
        /// SymbolWaiter.WaitForPDB();
        /// Console.WriteLine("Full debugging symbols available");
        /// 
        /// // Now can access internal Windows functions
        /// if (AddressResolver.SymbolExists("ntdll.NtCreateFile"))
        /// {
        ///     var ntCreateFile = AddressResolver.GetAddress("ntdll.NtCreateFile");
        /// }
        /// </code>
        /// </example>
        public static void WaitForPDB()
        {
            CallWaitFunction("waitForPDB", SymbolLevel.PDB);
        }

        /// <summary>
        /// Waits for memory sections to be enumerated (asynchronously).
        /// </summary>
        /// <param name="timeout">Maximum time to wait. If null, waits indefinitely.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous wait operation.</returns>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <exception cref="TimeoutException">Thrown when the timeout is exceeded.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     await SymbolWaiter.WaitForSectionsAsync(TimeSpan.FromSeconds(30));
        ///     Console.WriteLine("Sections loaded successfully");
        /// }
        /// catch (TimeoutException)
        /// {
        ///     Console.WriteLine("Section loading timed out");
        /// }
        /// </code>
        /// </example>
        public static async Task WaitForSectionsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await WaitAsync(() => WaitForSections(), SymbolLevel.Sections, timeout, cancellationToken);
        }

        /// <summary>
        /// Waits for DLL exports to be loaded (asynchronously).
        /// </summary>
        /// <param name="timeout">Maximum time to wait. If null, waits indefinitely.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous wait operation.</returns>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <exception cref="TimeoutException">Thrown when the timeout is exceeded.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        /// <example>
        /// <code>
        /// using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        /// await SymbolWaiter.WaitForExportsAsync(cancellationToken: cts.Token);
        /// 
        /// // Exports are now available
        /// var addr = AddressResolver.GetAddress("kernel32.LoadLibraryA");
        /// </code>
        /// </example>
        public static async Task WaitForExportsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await WaitAsync(() => WaitForExports(), SymbolLevel.Exports, timeout, cancellationToken);
        }

        /// <summary>
        /// Waits for .NET symbols to be loaded (asynchronously).
        /// </summary>
        /// <param name="timeout">Maximum time to wait. If null, waits indefinitely.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous wait operation.</returns>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <exception cref="TimeoutException">Thrown when the timeout is exceeded.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        /// <example>
        /// <code>
        /// await SymbolWaiter.WaitForDotNetAsync(TimeSpan.FromMinutes(1));
        /// 
        /// // .NET symbols are now available
        /// var stringLength = AddressResolver.TryGetAddress("System.String.get_Length");
        /// </code>
        /// </example>
        public static async Task WaitForDotNetAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await WaitAsync(() => WaitForDotNet(), SymbolLevel.DotNet, timeout, cancellationToken);
        }

        /// <summary>
        /// Waits for PDB debugging symbols to be loaded (asynchronously).
        /// </summary>
        /// <param name="timeout">Maximum time to wait. If null, waits indefinitely.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous wait operation.</returns>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <exception cref="TimeoutException">Thrown when the timeout is exceeded.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        /// <remarks>
        /// <para>PDB loading can take a very long time, especially on first run when downloading symbols.</para>
        /// <para>Consider using a generous timeout (several minutes) for this operation.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Console.WriteLine("Loading PDB symbols (may take several minutes)...");
        /// 
        /// try
        /// {
        ///     await SymbolWaiter.WaitForPDBAsync(TimeSpan.FromMinutes(10));
        ///     Console.WriteLine("Full debugging symbols loaded");
        /// }
        /// catch (TimeoutException)
        /// {
        ///     Console.WriteLine("PDB loading timed out - continuing with limited symbols");
        /// }
        /// </code>
        /// </example>
        public static async Task WaitForPDBAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await WaitAsync(() => WaitForPDB(), SymbolLevel.PDB, timeout, cancellationToken);
        }

        /// <summary>
        /// Waits for the specified symbol level to be loaded.
        /// </summary>
        /// <param name="level">The symbol level to wait for.</param>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <exception cref="ArgumentException">Thrown when an invalid symbol level is specified.</exception>
        /// <example>
        /// <code>
        /// // Wait for exports programmatically
        /// SymbolWaiter.WaitFor(SymbolLevel.Exports);
        /// 
        /// // Wait for the highest level
        /// SymbolWaiter.WaitFor(SymbolLevel.PDB);
        /// </code>
        /// </example>
        public static void WaitFor(SymbolLevel level)
        {
            switch (level)
            {
                case SymbolLevel.Sections:
                    WaitForSections();
                    break;
                case SymbolLevel.Exports:
                    WaitForExports();
                    break;
                case SymbolLevel.DotNet:
                    WaitForDotNet();
                    break;
                case SymbolLevel.PDB:
                    WaitForPDB();
                    break;
                default:
                    throw new ArgumentException($"Invalid symbol level: {level}", nameof(level));
            }
        }

        /// <summary>
        /// Waits for the specified symbol level to be loaded (asynchronously).
        /// </summary>
        /// <param name="level">The symbol level to wait for.</param>
        /// <param name="timeout">Maximum time to wait. If null, waits indefinitely.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous wait operation.</returns>
        /// <exception cref="SymbolWaitException">Thrown when the wait operation fails.</exception>
        /// <exception cref="ArgumentException">Thrown when an invalid symbol level is specified.</exception>
        /// <exception cref="TimeoutException">Thrown when the timeout is exceeded.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        /// <example>
        /// <code>
        /// // Wait for any level asynchronously
        /// await SymbolWaiter.WaitForAsync(SymbolLevel.DotNet, TimeSpan.FromMinutes(2));
        /// </code>
        /// </example>
        public static async Task WaitForAsync(SymbolLevel level, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            switch (level)
            {
                case SymbolLevel.Sections:
                    await WaitForSectionsAsync(timeout, cancellationToken);
                    break;
                case SymbolLevel.Exports:
                    await WaitForExportsAsync(timeout, cancellationToken);
                    break;
                case SymbolLevel.DotNet:
                    await WaitForDotNetAsync(timeout, cancellationToken);
                    break;
                case SymbolLevel.PDB:
                    await WaitForPDBAsync(timeout, cancellationToken);
                    break;
                default:
                    throw new ArgumentException($"Invalid symbol level: {level}", nameof(level));
            }
        }

        private static void CallWaitFunction(string functionName, SymbolLevel level)
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            try
            {
                // Get the wait function
                native.GetGlobal(state, functionName);
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new SymbolWaitException(level, $"{functionName} function not available in this CE version");
                }

                // Call the function (no parameters)
                var result = native.PCall(state, 0, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new SymbolWaitException(level, $"{functionName}() call failed: {error}");
                }

                // No return value expected - the function blocks until complete
            }
            catch (Exception ex) when (ex is not SymbolWaitException)
            {
                native.SetTop(state, 0);
                throw new SymbolWaitException(level, ex.Message, ex);
            }
        }

        private static async Task WaitAsync(Action waitAction, SymbolLevel level, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (timeout.HasValue)
            {
                cts.CancelAfter(timeout.Value);
            }

            try
            {
                await Task.Run(() =>
                {
                    // Check for cancellation before starting
                    cts.Token.ThrowIfCancellationRequested();

                    // Execute the synchronous wait function
                    waitAction();
                }, cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeout.HasValue)
            {
                throw new TimeoutException($"Timeout waiting for {level} symbols after {timeout.Value.TotalSeconds:F1} seconds");
            }
        }
    }
}