namespace CESDK.System
{
    /// <summary>
    /// Represents different processor architectures.
    /// </summary>
    public enum ProcessorArchitecture
    {
        /// <summary>
        /// Unknown or unsupported architecture.
        /// </summary>
        Unknown,

        /// <summary>
        /// x86 (32-bit Intel/AMD) architecture.
        /// </summary>
        X86,

        /// <summary>
        /// x64 (64-bit Intel/AMD) architecture.
        /// </summary>
        X64,

        /// <summary>
        /// ARM (Advanced RISC Machine) architecture.
        /// </summary>
        ARM
    }

    /// <summary>
    /// Represents different operating systems.
    /// </summary>
    public enum OperatingSystemType
    {
        /// <summary>
        /// Microsoft Windows.
        /// </summary>
        Windows = 0,

        /// <summary>
        /// Apple macOS.
        /// </summary>
        Mac = 1,

        /// <summary>
        /// Google Android.
        /// </summary>
        Android,

        /// <summary>
        /// Linux or other Unix-like systems.
        /// </summary>
        Linux
    }

    /// <summary>
    /// Represents different Application Binary Interfaces (ABI).
    /// </summary>
    public enum ABIType
    {
        /// <summary>
        /// Windows calling convention.
        /// </summary>
        Windows = 0,

        /// <summary>
        /// Unix/Linux calling convention.
        /// </summary>
        Unix = 1
    }

    /// <summary>
    /// Represents comprehensive information about the Cheat Engine platform.
    /// </summary>
    public class CheatEnginePlatformInfo
    {
        /// <summary>
        /// Gets whether Cheat Engine is running as a 64-bit process.
        /// </summary>
        public bool Is64Bit { get; }

        /// <summary>
        /// Gets whether Cheat Engine is running as a 32-bit process.
        /// </summary>
        public bool Is32Bit => !Is64Bit;

        /// <summary>
        /// Gets the operating system Cheat Engine is running on.
        /// </summary>
        public OperatingSystemType OperatingSystem { get; }

        /// <summary>
        /// Gets whether Cheat Engine is running on Windows.
        /// </summary>
        public bool IsWindows => OperatingSystem == OperatingSystemType.Windows;

        /// <summary>
        /// Gets whether Cheat Engine is running on macOS.
        /// </summary>
        public bool IsMac => OperatingSystem == OperatingSystemType.Mac;

        /// <summary>
        /// Gets whether Cheat Engine is running in Windows Dark Mode (Windows only).
        /// </summary>
        public bool IsDarkMode { get; }

        internal CheatEnginePlatformInfo(bool is64Bit, OperatingSystemType os, bool isDarkMode)
        {
            Is64Bit = is64Bit;
            OperatingSystem = os;
            IsDarkMode = isDarkMode;
        }

        /// <summary>
        /// Returns a string representation of the CE platform information.
        /// </summary>
        public override string ToString()
        {
            var bitness = Is64Bit ? "64-bit" : "32-bit";
            var darkMode = IsDarkMode ? " (Dark Mode)" : "";
            return $"Cheat Engine {bitness} on {OperatingSystem}{darkMode}";
        }
    }

    /// <summary>
    /// Represents comprehensive information about the target process platform.
    /// </summary>
    public class TargetPlatformInfo
    {
        /// <summary>
        /// Gets whether the target process is 64-bit.
        /// </summary>
        public bool Is64Bit { get; }

        /// <summary>
        /// Gets whether the target process is 32-bit.
        /// </summary>
        public bool Is32Bit => !Is64Bit;

        /// <summary>
        /// Gets the processor architecture of the target process.
        /// </summary>
        public ProcessorArchitecture Architecture { get; }

        /// <summary>
        /// Gets whether the target process is x86-based (x86 or x64).
        /// </summary>
        public bool IsX86Based => Architecture == ProcessorArchitecture.X86 || Architecture == ProcessorArchitecture.X64;

        /// <summary>
        /// Gets whether the target process is ARM-based.
        /// </summary>
        public bool IsARM => Architecture == ProcessorArchitecture.ARM;

        /// <summary>
        /// Gets the operating system the target process is running on.
        /// </summary>
        public OperatingSystemType OperatingSystem { get; }

        /// <summary>
        /// Gets whether the target process is running on Android.
        /// </summary>
        public bool IsAndroid => OperatingSystem == OperatingSystemType.Android;

        /// <summary>
        /// Gets whether the target process is running on Windows.
        /// </summary>
        public bool IsWindows => OperatingSystem == OperatingSystemType.Windows;

        /// <summary>
        /// Gets whether the target process is running on macOS.
        /// </summary>
        public bool IsMac => OperatingSystem == OperatingSystemType.Mac;

        /// <summary>
        /// Gets whether the target process is running under Rosetta emulation (macOS only).
        /// </summary>
        public bool IsRosetta { get; }

        /// <summary>
        /// Gets the Application Binary Interface (ABI) used by the target.
        /// </summary>
        public ABIType ABI { get; }

        internal TargetPlatformInfo(bool is64Bit, ProcessorArchitecture arch, OperatingSystemType os, bool isRosetta, ABIType abi)
        {
            Is64Bit = is64Bit;
            Architecture = arch;
            OperatingSystem = os;
            IsRosetta = isRosetta;
            ABI = abi;
        }

        /// <summary>
        /// Returns a string representation of the target platform information.
        /// </summary>
        public override string ToString()
        {
            var bitness = Is64Bit ? "64-bit" : "32-bit";
            var rosetta = IsRosetta ? " (Rosetta)" : "";
            return $"Target: {bitness} {Architecture} on {OperatingSystem}{rosetta} (ABI: {ABI})";
        }
    }

    /// <summary>
    /// Exception thrown when platform information retrieval fails.
    /// </summary>
    public class PlatformInfoException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the PlatformInfoException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PlatformInfoException(string message, Exception? innerException = null)
            : base($"Failed to retrieve platform information: {message}", innerException)
        {
        }
    }

    /// <summary>
    /// Provides comprehensive platform and architecture detection for both Cheat Engine and target processes.
    /// Wraps CE's platform detection functions with high-level, type-safe C# methods.
    /// </summary>
    /// <remarks>
    /// <para>This class provides detailed information about both CE's platform and the target process platform.</para>
    /// <para>All methods use CE's native platform detection for accurate architecture and OS identification.</para>
    /// <para>Platform information is cached for performance and consistency.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get Cheat Engine platform info
    /// var ceInfo = PlatformInfo.GetCheatEnginePlatform();
    /// Console.WriteLine($"CE Platform: {ceInfo}");
    /// 
    /// // Get target process platform info
    /// var targetInfo = PlatformInfo.GetTargetPlatform();
    /// Console.WriteLine($"Target Platform: {targetInfo}");
    /// 
    /// // Check specific capabilities
    /// if (PlatformInfo.IsCheatEngine64Bit())
    /// {
    ///     Console.WriteLine("CE can handle 64-bit processes");
    /// }
    /// 
    /// if (PlatformInfo.IsTargetX86())
    /// {
    ///     Console.WriteLine("Target uses x86 instruction set");
    /// }
    /// 
    /// // Cross-platform compatibility
    /// if (PlatformInfo.IsTargetAndroid())
    /// {
    ///     Console.WriteLine("Target is Android app");
    /// }
    /// else if (PlatformInfo.IsTargetRosetta())
    /// {
    ///     Console.WriteLine("Target is Intel app on Apple Silicon");
    /// }
    /// </code>
    /// </example>
    public static class PlatformInfo
    {
        private static CheatEnginePlatformInfo? _ceInfo;
        private static TargetPlatformInfo? _targetInfo;

        /// <summary>
        /// Gets whether Cheat Engine is running as a 64-bit process.
        /// </summary>
        /// <returns>True if CE is 64-bit; otherwise, false.</returns>
        /// <exception cref="PlatformInfoException">Thrown when platform detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>cheatEngineIs64Bit()</c> Lua function.</para>
        /// <para>64-bit CE can attach to both 32-bit and 64-bit processes.</para>
        /// <para>32-bit CE can only attach to 32-bit processes.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (PlatformInfo.IsCheatEngine64Bit())
        /// {
        ///     Console.WriteLine("CE can handle any process architecture");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("CE limited to 32-bit processes");
        /// }
        /// </code>
        /// </example>
        public static bool IsCheatEngine64Bit()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("cheatEngineIs64Bit");
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("cheatEngineIs64Bit", ex);
            }
        }

        /// <summary>
        /// Gets whether the target process is 64-bit.
        /// </summary>
        /// <returns>True if target is 64-bit; otherwise, false.</returns>
        /// <exception cref="PlatformInfoException">Thrown when platform detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>targetIs64Bit()</c> Lua function.</para>
        /// <para>64-bit processes have larger address spaces and different calling conventions.</para>
        /// <para>This affects memory scanning, code injection, and address calculations.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (PlatformInfo.IsTarget64Bit())
        /// {
        ///     Console.WriteLine("Target uses 64-bit addressing");
        ///     // Use 8-byte pointers
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Target uses 32-bit addressing");
        ///     // Use 4-byte pointers
        /// }
        /// </code>
        /// </example>
        public static bool IsTarget64Bit()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("targetIs64Bit");
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("targetIs64Bit", ex);
            }
        }

        /// <summary>
        /// Gets whether the target process is x86-based (Intel/AMD architecture).
        /// </summary>
        /// <returns>True if target is x86-based; otherwise, false.</returns>
        /// <exception cref="PlatformInfoException">Thrown when platform detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>targetIsX86()</c> Lua function.</para>
        /// <para>x86-based includes both 32-bit x86 and 64-bit x64 architectures.</para>
        /// <para>This affects assembly syntax, instruction sets, and register names.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (PlatformInfo.IsTargetX86())
        /// {
        ///     Console.WriteLine("Target uses x86 instruction set");
        ///     // Can use x86/x64 assembly code
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Target uses different architecture (ARM, etc.)");
        /// }
        /// </code>
        /// </example>
        public static bool IsTargetX86()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("targetIsX86");
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("targetIsX86", ex);
            }
        }

        /// <summary>
        /// Gets whether the target process is ARM-based (ARM architecture).
        /// </summary>
        /// <returns>True if target is ARM-based; otherwise, false.</returns>
        /// <exception cref="PlatformInfoException">Thrown when platform detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>targetIsArm()</c> Lua function.</para>
        /// <para>ARM architecture is common on mobile devices and Apple Silicon Macs.</para>
        /// <para>This affects instruction sets, assembly syntax, and calling conventions.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (PlatformInfo.IsTargetARM())
        /// {
        ///     Console.WriteLine("Target uses ARM instruction set");
        ///     // Use ARM assembly code
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Target uses x86 instruction set");
        /// }
        /// </code>
        /// </example>
        public static bool IsTargetARM()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("targetIsArm");
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("targetIsArm", ex);
            }
        }

        /// <summary>
        /// Gets whether the target process is running on Android.
        /// </summary>
        /// <returns>True if target is on Android; otherwise, false.</returns>
        /// <exception cref="PlatformInfoException">Thrown when platform detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>targetIsAndroid()</c> Lua function.</para>
        /// <para>Android targets may have different memory layouts and security restrictions.</para>
        /// <para>This affects process attachment, memory access, and debugging capabilities.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (PlatformInfo.IsTargetAndroid())
        /// {
        ///     Console.WriteLine("Target is Android application");
        ///     // Handle Android-specific limitations
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Target is desktop application");
        /// }
        /// </code>
        /// </example>
        public static bool IsTargetAndroid()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("targetIsAndroid");
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("targetIsAndroid", ex);
            }
        }

        /// <summary>
        /// Gets whether the target process is running under Rosetta emulation (macOS only).
        /// </summary>
        /// <returns>True if target is under Rosetta; otherwise, false.</returns>
        /// <exception cref="PlatformInfoException">Thrown when platform detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>targetIsRosetta()</c> Lua function.</para>
        /// <para>Rosetta allows Intel x86 applications to run on Apple Silicon Macs.</para>
        /// <para>This affects performance characteristics and some low-level operations.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (PlatformInfo.IsTargetRosetta())
        /// {
        ///     Console.WriteLine("Target is Intel app running on Apple Silicon");
        ///     // May have performance implications
        /// }
        /// else if (PlatformInfo.IsTargetARM())
        /// {
        ///     Console.WriteLine("Target is native Apple Silicon app");
        /// }
        /// </code>
        /// </example>
        public static bool IsTargetRosetta()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("targetIsRosetta");
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("targetIsRosetta", ex);
            }
        }

        /// <summary>
        /// Gets the Application Binary Interface (ABI) used by the target.
        /// </summary>
        /// <returns>The ABI type (Windows or Unix calling convention).</returns>
        /// <exception cref="PlatformInfoException">Thrown when ABI detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getABI()</c> Lua function.</para>
        /// <para>ABI affects calling conventions, parameter passing, and stack management.</para>
        /// <para>Windows ABI differs from Unix/Linux ABI in significant ways.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var abi = PlatformInfo.GetABI();
        /// switch (abi)
        /// {
        ///     case ABIType.Windows:
        ///         Console.WriteLine("Using Windows calling convention");
        ///         // Handle Windows ABI specifics
        ///         break;
        ///         
        ///     case ABIType.Unix:
        ///         Console.WriteLine("Using Unix/Linux calling convention");
        ///         // Handle Unix ABI specifics
        ///         break;
        /// }
        /// </code>
        /// </example>
        public static ABIType GetABI()
        {
            try
            {
                var result = PluginContext.Lua.CallIntegerFunction("getABI");
                return result == 0 ? ABIType.Windows : ABIType.Unix;
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("getABI", ex);
            }
        }

        /// <summary>
        /// Gets whether Cheat Engine is running in Windows Dark Mode.
        /// </summary>
        /// <returns>True if CE is in dark mode; otherwise, false.</returns>
        /// <exception cref="PlatformInfoException">Thrown when dark mode detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>darkMode()</c> Lua function.</para>
        /// <para>Only applicable on Windows - always returns false on other platforms.</para>
        /// <para>Useful for adapting plugin UI to match the system theme.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (PlatformInfo.IsDarkModeEnabled())
        /// {
        ///     // Use dark theme colors
        ///     SetButtonColor(Color.FromArgb(45, 45, 48));
        ///     SetTextColor(Color.White);
        /// }
        /// else
        /// {
        ///     // Use light theme colors
        ///     SetButtonColor(Color.FromArgb(240, 240, 240));
        ///     SetTextColor(Color.Black);
        /// }
        /// </code>
        /// </example>
        public static bool IsDarkModeEnabled()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("darkMode");
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("darkMode", ex);
            }
        }

        /// <summary>
        /// Gets the operating system that Cheat Engine is running on.
        /// </summary>
        /// <returns>The operating system (Windows = 0, Mac = 1).</returns>
        /// <exception cref="PlatformInfoException">Thrown when OS detection fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>getOperatingSystem()</c> Lua function.</para>
        /// <para>Returns 0 for Windows, 1 for Mac according to CE's convention.</para>
        /// </remarks>
        private static int GetCheatEngineOperatingSystem()
        {
            try
            {
                return (int)PluginContext.Lua.CallIntegerFunction("getOperatingSystem");
            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformInfoException("getOperatingSystem", ex);
            }
        }

        /// <summary>
        /// Gets comprehensive platform information about Cheat Engine.
        /// </summary>
        /// <returns>Detailed CE platform information.</returns>
        /// <exception cref="PlatformInfoException">Thrown when platform detection fails.</exception>
        /// <example>
        /// <code>
        /// var ceInfo = PlatformInfo.GetCheatEnginePlatform();
        /// Console.WriteLine($"Cheat Engine: {ceInfo}");
        /// Console.WriteLine($"64-bit: {ceInfo.Is64Bit}");
        /// Console.WriteLine($"Dark Mode: {ceInfo.IsDarkMode}");
        /// Console.WriteLine($"OS: {ceInfo.OperatingSystem}");
        /// </code>
        /// </example>
        public static CheatEnginePlatformInfo GetCheatEnginePlatform()
        {
            if (_ceInfo == null)
            {
                var is64Bit = IsCheatEngine64Bit();
                var os = GetCheatEngineOperatingSystem();
                var isDarkMode = IsDarkModeEnabled();

                var osType = os == 0 ? OperatingSystemType.Windows : OperatingSystemType.Mac;

                _ceInfo = new CheatEnginePlatformInfo(is64Bit, osType, isDarkMode);
            }

            return _ceInfo;
        }

        /// <summary>
        /// Gets comprehensive platform information about the target process.
        /// </summary>
        /// <returns>Detailed target platform information.</returns>
        /// <exception cref="PlatformInfoException">Thrown when platform detection fails.</exception>
        /// <example>
        /// <code>
        /// var targetInfo = PlatformInfo.GetTargetPlatform();
        /// Console.WriteLine($"Target: {targetInfo}");
        /// Console.WriteLine($"Architecture: {targetInfo.Architecture}");
        /// Console.WriteLine($"OS: {targetInfo.OperatingSystem}");
        /// Console.WriteLine($"ABI: {targetInfo.ABI}");
        /// 
        /// if (targetInfo.IsRosetta)
        ///     Console.WriteLine("Running under Rosetta emulation");
        /// </code>
        /// </example>
        public static TargetPlatformInfo GetTargetPlatform()
        {
            if (_targetInfo == null)
            {
                var is64Bit = IsTarget64Bit();
                var isX86 = IsTargetX86();
                var isARM = IsTargetARM();
                var isAndroid = IsTargetAndroid();
                var isRosetta = IsTargetRosetta();
                var abi = GetABI();

                // Determine architecture
                ProcessorArchitecture arch = ProcessorArchitecture.Unknown;
                if (isX86)
                {
                    arch = is64Bit ? ProcessorArchitecture.X64 : ProcessorArchitecture.X86;
                }
                else if (isARM)
                {
                    arch = ProcessorArchitecture.ARM;
                }

                // Determine OS
                OperatingSystemType os = OperatingSystemType.Windows;
                if (isAndroid)
                {
                    os = OperatingSystemType.Android;
                }
                else if (abi == ABIType.Unix)
                {
                    os = OperatingSystemType.Linux; // Default for Unix ABI
                }

                _targetInfo = new TargetPlatformInfo(is64Bit, arch, os, isRosetta, abi);
            }

            return _targetInfo;
        }

        /// <summary>
        /// Clears the cached platform information, forcing fresh detection on next call.
        /// </summary>
        /// <remarks>
        /// <para>Use this when the target process changes or platform detection needs to be refreshed.</para>
        /// <para>CE platform info rarely changes, but target info changes with each attached process.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Attach to new process
        /// Process.OpenByName("different_app.exe");
        /// 
        /// // Clear cached info to get fresh target platform data
        /// PlatformInfo.ClearCache();
        /// 
        /// var newTargetInfo = PlatformInfo.GetTargetPlatform();
        /// </code>
        /// </example>
        public static void ClearCache()
        {
            _ceInfo = null;
            _targetInfo = null;
        }
    }
}