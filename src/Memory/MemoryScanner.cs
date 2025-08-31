using CESDK.Lua;

namespace CESDK.Memory
{
    /// <summary>
    /// Modern, programmatic memory scanner with full user control.
    /// Provides a clean API for performing first scans, next scans, and managing scan results.
    /// </summary>
    /// <example>
    /// <code>
    /// var scanner = new MemoryScanner();
    /// 
    /// // Configure and perform first scan
    /// var config = new ScanConfiguration()
    ///     .ForValue("100")
    ///     .AsInteger()
    ///     .InRange(0x1000000, 0x2000000);
    ///     
    /// await scanner.FirstScanAsync(config);
    /// 
    /// // Perform next scan for changed values
    /// var nextConfig = new ScanConfiguration().ForChangedValues();
    /// await scanner.NextScanAsync(nextConfig);
    /// 
    /// var results = scanner.GetResults();
    /// Console.WriteLine($"Found {results.Count} matches");
    /// </code>
    /// </example>
    public class MemoryScanner : IDisposable
    {
        private readonly LuaEngine _lua;
        private readonly IntPtr _memScanObject;
        private bool _scanStarted;
        private bool _disposed;

        /// <summary>
        /// Event raised when a scan operation completes.
        /// </summary>
        public event EventHandler<ScanCompletedEventArgs>? ScanCompleted;

        /// <summary>
        /// Event raised during scanning to report progress.
        /// </summary>
        public event EventHandler<ScanProgressEventArgs>? ScanProgress;

        /// <summary>
        /// Gets whether a scan has been started and is available for next scans.
        /// </summary>
        /// <value><see langword="true"/> if a first scan has been performed; otherwise, <see langword="false"/>.</value>
        public bool IsScanStarted => _scanStarted;

        /// <summary>
        /// Initializes a new instance of the MemoryScanner class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the Cheat Engine memory scan object cannot be created.</exception>
        /// <example>
        /// <code>
        /// using var scanner = new MemoryScanner();
        /// // Use scanner for memory operations
        /// </code>
        /// </example>
        public MemoryScanner()
        {
            _lua = PluginContext.Lua;
            _memScanObject = CreateMemScanObject();
        }

        /// <summary>
        /// Performs the initial memory scan with the specified configuration.
        /// This must be called before any next scans can be performed.
        /// </summary>
        /// <param name="config">The scan configuration specifying what to search for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the scan operation fails.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scanner has been disposed.</exception>
        /// <remarks>
        /// <para>The first scan searches through all accessible memory in the target process.</para>
        /// <para>This operation can take significant time depending on the search range and value type.</para>
        /// <para>Use the <see cref="ScanProgress"/> event to monitor scan progress.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var config = new ScanConfiguration()
        ///     .ForValue("1000")
        ///     .AsInteger()
        ///     .WithProtection("+W-C"); // Writable memory only
        ///     
        /// scanner.FirstScan(config);
        /// </code>
        /// </example>
        public void FirstScan(ScanConfiguration config)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MemoryScanner));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                var state = _lua.State;
                var native = _lua.Native;

                // Push the memscan object
                native.PushInteger(state, _memScanObject.ToInt64());

                // Call firstScan method
                native.PushString(state, "firstScan");
                native.GetTable(state, -2);

                if (!native.IsFunction(state, -1))
                    throw new InvalidOperationException("MemScan object does not have a firstScan method");

                // Push parameters for firstScan
                native.PushInteger(state, (long)config.ScanType);
                native.PushInteger(state, (long)config.ValueType);
                native.PushInteger(state, (long)config.RoundingType);
                native.PushString(state, config.Value);
                native.PushString(state, config.Value2);
                native.PushInteger(state, (long)config.StartAddress);
                native.PushInteger(state, (long)config.EndAddress);
                native.PushString(state, config.ProtectionFlags);
                native.PushInteger(state, (long)config.AlignmentType);
                native.PushString(state, config.AlignmentValue);
                native.PushBoolean(state, config.IsHexadecimal);
                native.PushBoolean(state, true); // isNotABinaryString
                native.PushBoolean(state, config.IsUtf16);
                native.PushBoolean(state, config.IsCaseSensitive);

                // Call the function
                var result = native.PCall(state, 14, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    throw new InvalidOperationException($"FirstScan failed: {error}");
                }

                _scanStarted = true;
            }
            finally
            {
                _lua.Native.SetTop(_lua.State, 0);
            }
        }

        /// <summary>
        /// Performs a subsequent scan on the results of the previous scan.
        /// </summary>
        /// <param name="config">The scan configuration specifying the next scan criteria.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no first scan has been performed or the scan operation fails.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scanner has been disposed.</exception>
        /// <remarks>
        /// <para>Next scans operate only on the results from the previous scan, making them much faster.</para>
        /// <para>You must call <see cref="FirstScan"/> before calling this method.</para>
        /// <para>Common next scan operations include searching for changed, unchanged, or specific values.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // After performing a first scan, search for values that have changed
        /// var nextConfig = new ScanConfiguration()
        ///     .ForChangedValues();
        ///     
        /// scanner.NextScan(nextConfig);
        /// 
        /// // Or search for a specific value in the previous results
        /// var exactConfig = new ScanConfiguration()
        ///     .ForValue("500")
        ///     .AsInteger();
        ///     
        /// scanner.NextScan(exactConfig);
        /// </code>
        /// </example>
        public void NextScan(ScanConfiguration config)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MemoryScanner));
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (!_scanStarted)
                throw new InvalidOperationException("No first scan has been performed. Call FirstScan() first.");

            try
            {
                var state = _lua.State;
                var native = _lua.Native;

                // Push the memscan object
                native.PushInteger(state, _memScanObject.ToInt64());

                // Call nextScan method
                native.PushString(state, "nextScan");
                native.GetTable(state, -2);

                if (!native.IsFunction(state, -1))
                    throw new InvalidOperationException("MemScan object does not have a nextScan method");

                // Push parameters for nextScan
                native.PushInteger(state, (long)config.ScanType);
                native.PushInteger(state, (long)config.RoundingType);
                native.PushString(state, config.Value);
                native.PushString(state, config.Value2);
                native.PushBoolean(state, config.IsHexadecimal);
                native.PushBoolean(state, true); // isNotABinaryString
                native.PushBoolean(state, config.IsUtf16);
                native.PushBoolean(state, config.IsCaseSensitive);
                native.PushBoolean(state, config.IsPercentageScan);

                // Call the function
                var result = native.PCall(state, 9, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    throw new InvalidOperationException($"NextScan failed: {error}");
                }
            }
            finally
            {
                _lua.Native.SetTop(_lua.State, 0);
            }
        }

        /// <summary>
        /// Waits for the current scan operation to complete.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no scan is in progress or the wait operation fails.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scanner has been disposed.</exception>
        /// <remarks>
        /// <para>This method blocks the current thread until the scan completes.</para>
        /// <para>For non-blocking operations, use the <see cref="ScanCompleted"/> event instead.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// scanner.FirstScan(config);
        /// scanner.WaitForCompletion(); // Blocks until scan is done
        /// var results = scanner.GetResults();
        /// </code>
        /// </example>
        public void WaitForCompletion()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MemoryScanner));
            if (!_scanStarted)
                throw new InvalidOperationException("No scan has been started.");

            try
            {
                var state = _lua.State;
                var native = _lua.Native;

                // Push the memscan object
                native.PushInteger(state, _memScanObject.ToInt64());

                // Call waitTillDone method
                native.PushString(state, "waitTillDone");
                native.GetTable(state, -2);

                if (!native.IsFunction(state, -1))
                    throw new InvalidOperationException("MemScan object does not have a waitTillDone method");

                // Call the function
                var result = native.PCall(state, 0, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    throw new InvalidOperationException($"WaitForCompletion failed: {error}");
                }
            }
            finally
            {
                _lua.Native.SetTop(_lua.State, 0);
            }
        }

        /// <summary>
        /// Gets the results from the last completed scan operation.
        /// </summary>
        /// <returns>A <see cref="ScanResults"/> object containing the found memory addresses.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no scan has been performed.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scanner has been disposed.</exception>
        /// <example>
        /// <code>
        /// scanner.FirstScan(config);
        /// scanner.WaitForCompletion();
        /// 
        /// var results = scanner.GetResults();
        /// Console.WriteLine($"Found {results.Count} matches");
        /// 
        /// foreach (var address in results.Addresses)
        /// {
        ///     Console.WriteLine($"Match at: 0x{address:X8}");
        /// }
        /// </code>
        /// </example>
        public ScanResults GetResults()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MemoryScanner));
            if (!_scanStarted)
                throw new InvalidOperationException("No scan has been performed.");

            return new ScanResults(_lua, _memScanObject);
        }

        /// <summary>
        /// Resets the scanner to perform a new first scan.
        /// This clears all previous scan results.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the reset operation fails.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scanner has been disposed.</exception>
        /// <remarks>
        /// <para>After calling this method, you must call <see cref="FirstScan"/> before performing any next scans.</para>
        /// <para>This is equivalent to creating a new MemoryScanner instance but reuses the existing CE object.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Perform first scan
        /// scanner.FirstScan(config1);
        /// 
        /// // Reset and start a completely new scan
        /// scanner.Reset();
        /// scanner.FirstScan(config2);
        /// </code>
        /// </example>
        public void Reset()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MemoryScanner));

            try
            {
                var state = _lua.State;
                var native = _lua.Native;

                // Push the memscan object
                native.PushInteger(state, _memScanObject.ToInt64());

                // Call newScan method
                native.PushString(state, "newScan");
                native.GetTable(state, -2);

                if (!native.IsFunction(state, -1))
                    throw new InvalidOperationException("MemScan object does not have a newScan method");

                // Call the function
                var result = native.PCall(state, 0, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    throw new InvalidOperationException($"Reset failed: {error}");
                }

                _scanStarted = false;
            }
            finally
            {
                _lua.Native.SetTop(_lua.State, 0);
            }
        }

        /// <summary>
        /// Creates the CE memory scan object.
        /// </summary>
        private IntPtr CreateMemScanObject()
        {
            try
            {
                var state = _lua.State;
                var native = _lua.Native;

                // Call createMemScan()
                native.GetGlobal(state, "createMemScan");
                if (native.IsNil(state, -1))
                    throw new InvalidOperationException("createMemScan function is not available in Cheat Engine");

                var result = native.PCall(state, 0, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    throw new InvalidOperationException($"Failed to create MemScan object: {error}");
                }

                // Get the CE object pointer
                var objectPtr = native.ToInteger(state, -1);
                native.Pop(state, 1);

                // Register event handlers
                RegisterEventHandlers(new IntPtr(objectPtr));

                return new IntPtr(objectPtr);
            }
            catch
            {
                _lua.Native.SetTop(_lua.State, 0);
                throw;
            }
        }

        /// <summary>
        /// Registers event handlers for scan completion and progress.
        /// </summary>
        private void RegisterEventHandlers(IntPtr memScanObject)
        {
            // Register OnScanDone callback
            _lua.RegisterFunction("__memscan_done_" + memScanObject.ToInt64(), () =>
            {
                ScanCompleted?.Invoke(this, new ScanCompletedEventArgs());
            });

            // Register OnGuiUpdate callback  
            _lua.RegisterRawFunction("__memscan_progress_" + memScanObject.ToInt64(), (IntPtr luaState) =>
            {
                var native = _lua.Native;
                if (native.GetTop(luaState) >= 4)
                {
                    var totalAddresses = (ulong)native.ToInteger(luaState, 2);
                    var currentScanned = (ulong)native.ToInteger(luaState, 3);
                    var resultsFound = (ulong)native.ToInteger(luaState, 4);

                    var args = new ScanProgressEventArgs(totalAddresses, currentScanned, resultsFound);
                    ScanProgress?.Invoke(this, args);
                }
                return 0;
            });

            // Set the callbacks on the CE object
            try
            {
                var state = _lua.State;
                var native = _lua.Native;

                native.PushInteger(state, memScanObject.ToInt64());

                // Set OnScanDone
                native.PushString(state, "OnScanDone");
                _lua.Execute($"__memscan_done_{memScanObject.ToInt64()}");
                native.SetTable(state, -3);

                // Set OnGuiUpdate
                native.PushString(state, "OnGuiUpdate");
                _lua.Execute($"__memscan_progress_{memScanObject.ToInt64()}");
                native.SetTable(state, -3);
            }
            finally
            {
                _lua.Native.SetTop(_lua.State, 0);
            }
        }

        /// <summary>
        /// Releases all resources used by the MemoryScanner.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _memScanObject != IntPtr.Zero)
                {
                    try
                    {
                        // Destroy the CE object
                        var state = _lua.State;
                        var native = _lua.Native;

                        native.PushInteger(state, _memScanObject.ToInt64());
                        native.PushString(state, "destroy");
                        native.GetTable(state, -2);

                        if (native.IsFunction(state, -1))
                        {
                            native.PCall(state, 0, 0);
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                    finally
                    {
                        _lua.Native.SetTop(_lua.State, 0);
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the MemoryScanner.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the MemoryScanner instance.
        /// </summary>
        ~MemoryScanner()
        {
            Dispose(false);
        }
    }
}