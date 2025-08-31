namespace CESDK.System
{
    /// <summary>
    /// Exception thrown when Cheat Engine information retrieval fails.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the CheatEngineInfoException class.
    /// </remarks>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public class CheatEngineInfoException(string message, Exception? innerException = null) : Exception($"Failed to retrieve CE information: {message}", innerException)
    {
    }

    /// <summary>
    /// Represents comprehensive information about the Cheat Engine installation and runtime.
    /// </summary>
    public class CheatEngineDetails
    {
        /// <summary>
        /// Gets the directory where Cheat Engine is installed.
        /// </summary>
        public DirectoryInfo InstallDirectory { get; }

        /// <summary>
        /// Gets the process ID of the running Cheat Engine instance.
        /// </summary>
        public int ProcessId { get; }

        /// <summary>
        /// Gets the version of the running Cheat Engine instance.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the full path to the Cheat Engine executable.
        /// </summary>
        public FileInfo ExecutablePath => new FileInfo(Path.Combine(InstallDirectory.FullName, "cheatengine-x86_64.exe"));

        /// <summary>
        /// Gets whether the CE installation directory exists and is valid.
        /// </summary>
        public bool IsInstallDirectoryValid => InstallDirectory.Exists;

        internal CheatEngineDetails(string directory, int processId, string version)
        {
            InstallDirectory = new DirectoryInfo(directory);
            ProcessId = processId;
            Version = version;
        }

        /// <summary>
        /// Returns a string representation of the CE details.
        /// </summary>
        public override string ToString()
        {
            return $"Cheat Engine v{Version} (PID: {ProcessId}) at {InstallDirectory.FullName}";
        }
    }

    /// <summary>
    /// Provides access to Cheat Engine installation and runtime information.
    /// Wraps CE's system information functions with high-level, type-safe C# methods.
    /// </summary>
    /// <remarks>
    /// <para>This class provides information about the CE installation, process, and runtime environment.</para>
    /// <para>All methods use CE's native functions for accurate system information.</para>
    /// <para>Information is cached for performance and consistency.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get CE installation directory
    /// var ceDir = CheatEngineInfo.GetInstallDirectory();
    /// Console.WriteLine($"CE installed at: {ceDir.FullName}");
    /// 
    /// // Get CE process information
    /// var processId = CheatEngineInfo.GetProcessId();
    /// Console.WriteLine($"CE running as PID: {processId}");
    /// 
    /// // Get CE version
    /// var version = CheatEngineInfo.GetVersion();
    /// Console.WriteLine($"CE Version: {version}");
    /// 
    /// // Get comprehensive details
    /// var details = CheatEngineInfo.GetDetails();
    /// Console.WriteLine($"Details: {details}");
    /// 
    /// // Check if CE directory is valid
    /// if (details.IsInstallDirectoryValid)
    /// {
    ///     Console.WriteLine("CE installation is valid");
    /// }
    /// </code>
    /// </example>
    public static class CheatEngineInfo
    {
        private static CheatEngineDetails? _details;

        /// <summary>
        /// Gets the directory where Cheat Engine is installed.
        /// </summary>
        /// <returns>DirectoryInfo representing the CE installation directory.</returns>
        /// <exception cref="CheatEngineInfoException">Thrown when directory retrieval fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getCheatEngineDir()</c> Lua function.</para>
        /// <para>The directory contains CE's executable, libraries, and configuration files.</para>
        /// <para>Useful for locating CE resources, plugins, or configuration files.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var ceDir = CheatEngineInfo.GetInstallDirectory();
        /// Console.WriteLine($"CE Directory: {ceDir.FullName}");
        /// 
        /// // Check if directory exists
        /// if (ceDir.Exists)
        /// {
        ///     // Look for specific files
        ///     var configFile = Path.Combine(ceDir.FullName, "config.xml");
        ///     var pluginsDir = Path.Combine(ceDir.FullName, "plugins");
        /// }
        /// </code>
        /// </example>
        public static DirectoryInfo GetInstallDirectory()
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "getCheatEngineDir");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new CheatEngineInfoException("getCheatEngineDir function not available in this CE version");
                }

                var result = native.PCall(state, 0, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new CheatEngineInfoException($"getCheatEngineDir() call failed: {error}");
                }

                var dirPath = native.ToString(state, -1);
                native.Pop(state, 1);

                if (string.IsNullOrEmpty(dirPath))
                {
                    throw new CheatEngineInfoException("getCheatEngineDir returned empty path");
                }

                return new DirectoryInfo(dirPath);
            }
            catch (Exception ex) when (ex is not CheatEngineInfoException)
            {
                throw new CheatEngineInfoException($"Error getting CE directory: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the process ID of the running Cheat Engine instance.
        /// </summary>
        /// <returns>The process ID of the current CE instance.</returns>
        /// <exception cref="CheatEngineInfoException">Thrown when process ID retrieval fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getCheatEngineProcessID()</c> Lua function.</para>
        /// <para>Useful for process management, debugging, or inter-process communication.</para>
        /// <para>The PID remains constant for the lifetime of the CE instance.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var ceProcessId = CheatEngineInfo.GetProcessId();
        /// Console.WriteLine($"Cheat Engine PID: {ceProcessId}");
        /// 
        /// // Use with System.Diagnostics.Process
        /// var ceProcess = System.Diagnostics.Process.GetProcessById(ceProcessId);
        /// Console.WriteLine($"CE Memory Usage: {ceProcess.WorkingSet64 / 1024 / 1024} MB");
        /// </code>
        /// </example>
        public static int GetProcessId()
        {
            try
            {
                var result = PluginContext.Lua.CallIntegerFunction("getCheatEngineProcessID");
                return (int)result;
            }
            catch (InvalidOperationException ex)
            {
                throw new CheatEngineInfoException("getCheatEngineProcessID", ex);
            }
        }

        /// <summary>
        /// Gets the version string of the running Cheat Engine instance.
        /// </summary>
        /// <returns>The version string (e.g., "7.5.1").</returns>
        /// <exception cref="CheatEngineInfoException">Thrown when version retrieval fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getCEVersion()</c> Lua function.</para>
        /// <para>Version string format may vary between CE releases.</para>
        /// <para>Useful for version-specific feature detection or compatibility checks.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var version = CheatEngineInfo.GetVersion();
        /// Console.WriteLine($"Cheat Engine Version: {version}");
        /// 
        /// // Version-specific logic
        /// if (Version.Parse(version) >= Version.Parse("7.5"))
        /// {
        ///     Console.WriteLine("Advanced features available");
        /// }
        /// </code>
        /// </example>
        public static string GetVersion()
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "getCEVersion");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new CheatEngineInfoException("getCEVersion function not available in this CE version");
                }

                var result = native.PCall(state, 0, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new CheatEngineInfoException($"getCEVersion() call failed: {error}");
                }

                var version = native.ToString(state, -1);
                native.Pop(state, 1);

                return version ?? "Unknown";
            }
            catch (Exception ex) when (ex is not CheatEngineInfoException)
            {
                throw new CheatEngineInfoException($"Error getting CE version: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the temp directory used by Cheat Engine.
        /// </summary>
        /// <returns>DirectoryInfo representing the CE temp directory.</returns>
        /// <exception cref="CheatEngineInfoException">Thrown when temp directory retrieval fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getTempDir()</c> Lua function.</para>
        /// <para>CE uses this directory for temporary files during operations.</para>
        /// <para>Useful for plugin temporary storage or cleanup operations.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var tempDir = CheatEngineInfo.GetTempDirectory();
        /// Console.WriteLine($"CE Temp Directory: {tempDir.FullName}");
        /// 
        /// // Create plugin temp file
        /// var pluginTempFile = Path.Combine(tempDir.FullName, "myplugin_temp.dat");
        /// File.WriteAllText(pluginTempFile, "Temporary data");
        /// </code>
        /// </example>
        public static DirectoryInfo GetTempDirectory()
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "getTempDir");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new CheatEngineInfoException("getTempDir function not available in this CE version");
                }

                var result = native.PCall(state, 0, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new CheatEngineInfoException($"getTempDir() call failed: {error}");
                }

                var dirPath = native.ToString(state, -1);
                native.Pop(state, 1);

                if (string.IsNullOrEmpty(dirPath))
                {
                    throw new CheatEngineInfoException("getTempDir returned empty path");
                }

                return new DirectoryInfo(dirPath);
            }
            catch (Exception ex) when (ex is not CheatEngineInfoException)
            {
                throw new CheatEngineInfoException($"Error getting CE temp directory: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets comprehensive details about the Cheat Engine installation and runtime.
        /// </summary>
        /// <returns>CheatEngineDetails containing all CE information.</returns>
        /// <exception cref="CheatEngineInfoException">Thrown when information retrieval fails.</exception>
        /// <remarks>
        /// <para>This method combines multiple CE information sources into a single object.</para>
        /// <para>Results are cached for performance and consistency.</para>
        /// <para>Use ClearCache() to force fresh information retrieval.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var details = CheatEngineInfo.GetDetails();
        /// Console.WriteLine($"CE Details: {details}");
        /// Console.WriteLine($"Install Directory: {details.InstallDirectory.FullName}");
        /// Console.WriteLine($"Process ID: {details.ProcessId}");
        /// Console.WriteLine($"Version: {details.Version}");
        /// Console.WriteLine($"Executable: {details.ExecutablePath.FullName}");
        /// Console.WriteLine($"Valid Installation: {details.IsInstallDirectoryValid}");
        /// </code>
        /// </example>
        public static CheatEngineDetails GetDetails()
        {
            if (_details == null)
            {
                var directory = GetInstallDirectory();
                var processId = GetProcessId();
                var version = GetVersion();

                _details = new CheatEngineDetails(directory.FullName, processId, version);
            }

            return _details;
        }

        /// <summary>
        /// Clears the cached CE information, forcing fresh retrieval on next call.
        /// </summary>
        /// <remarks>
        /// <para>Use this when CE information may have changed or needs to be refreshed.</para>
        /// <para>CE information rarely changes during runtime, but version or directory info could be updated.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Clear cached info to get fresh CE details
        /// CheatEngineInfo.ClearCache();
        /// 
        /// var freshDetails = CheatEngineInfo.GetDetails();
        /// </code>
        /// </example>
        public static void ClearCache()
        {
            _details = null;
        }
    }
}