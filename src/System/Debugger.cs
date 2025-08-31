namespace CESDK.System
{
    /// <summary>
    /// Represents different debugger interfaces available in Cheat Engine.
    /// </summary>
    public enum DebuggerInterface
    {
        /// <summary>
        /// No debugging interface is active.
        /// </summary>
        None = 0,

        /// <summary>
        /// Windows native debugger interface.
        /// </summary>
        Windows = 1,

        /// <summary>
        /// Vectored Exception Handler (VEH) debugger.
        /// </summary>
        VEH = 2,

        /// <summary>
        /// Kernel-mode debugger interface.
        /// </summary>
        Kernel = 3,

        /// <summary>
        /// macOS native debugger interface.
        /// </summary>
        MacNative = 4,

        /// <summary>
        /// GDB (GNU Debugger) interface.
        /// </summary>
        GDB = 5
    }

    /// <summary>
    /// Represents different breakpoint trigger types.
    /// </summary>
    public enum BreakpointTrigger
    {
        /// <summary>
        /// Break on instruction execution.
        /// </summary>
        Execute = 0,

        /// <summary>
        /// Break on memory access (read or write).
        /// </summary>
        Access = 1,

        /// <summary>
        /// Break on memory write.
        /// </summary>
        Write = 2
    }

    /// <summary>
    /// Represents different breakpoint methods.
    /// </summary>
    public enum BreakpointMethod
    {
        /// <summary>
        /// Hardware breakpoint (uses debug registers).
        /// </summary>
        Hardware = 0,

        /// <summary>
        /// Software breakpoint (INT3 instruction).
        /// </summary>
        Software = 1,

        /// <summary>
        /// Find and replace breakpoint.
        /// </summary>
        FindAndReplace = 2
    }

    /// <summary>
    /// Represents different continue methods after hitting a breakpoint.
    /// </summary>
    public enum ContinueMethod
    {
        /// <summary>
        /// Continue normal execution.
        /// </summary>
        Run = 0,

        /// <summary>
        /// Step into function calls.
        /// </summary>
        StepInto = 1,

        /// <summary>
        /// Step over function calls.
        /// </summary>
        StepOver = 2
    }

    /// <summary>
    /// Represents CPU register context information.
    /// </summary>
    public class RegisterContext
    {
        /// <summary>
        /// Gets or sets the EAX/RAX register value.
        /// </summary>
        public ulong EAX { get; set; }

        /// <summary>
        /// Gets or sets the EBX/RBX register value.
        /// </summary>
        public ulong EBX { get; set; }

        /// <summary>
        /// Gets or sets the ECX/RCX register value.
        /// </summary>
        public ulong ECX { get; set; }

        /// <summary>
        /// Gets or sets the EDX/RDX register value.
        /// </summary>
        public ulong EDX { get; set; }

        /// <summary>
        /// Gets or sets the ESI/RSI register value.
        /// </summary>
        public ulong ESI { get; set; }

        /// <summary>
        /// Gets or sets the EDI/RDI register value.
        /// </summary>
        public ulong EDI { get; set; }

        /// <summary>
        /// Gets or sets the EBP/RBP register value.
        /// </summary>
        public ulong EBP { get; set; }

        /// <summary>
        /// Gets or sets the ESP/RSP register value.
        /// </summary>
        public ulong ESP { get; set; }

        /// <summary>
        /// Gets or sets the EIP/RIP register value.
        /// </summary>
        public ulong EIP { get; set; }

        /// <summary>
        /// Gets or sets the EFLAGS/RFLAGS register value.
        /// </summary>
        public ulong EFLAGS { get; set; }

        /// <summary>
        /// Returns a string representation of the register context.
        /// </summary>
        public override string ToString()
        {
            return $"EAX={EAX:X8} EBX={EBX:X8} ECX={ECX:X8} EDX={EDX:X8} " +
                   $"ESI={ESI:X8} EDI={EDI:X8} EBP={EBP:X8} ESP={ESP:X8} " +
                   $"EIP={EIP:X8} EFLAGS={EFLAGS:X8}";
        }
    }

    /// <summary>
    /// Exception thrown when debugger operations fail.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the DebuggerException class.
    /// </remarks>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public class DebuggerException(string message, Exception? innerException = null) : Exception($"Debugger operation failed: {message}", innerException)
    {
    }

    /// <summary>
    /// Provides comprehensive debugging capabilities for Cheat Engine.
    /// Wraps CE's debug functions with high-level, type-safe C# methods.
    /// </summary>
    /// <remarks>
    /// <para>This class provides access to CE's powerful debugging features including breakpoints, single stepping, and register manipulation.</para>
    /// <para>All methods use CE's native debugger functions for accurate debugging control.</para>
    /// <para>Debugging operations require the target process to be attached and debugger to be started.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Start debugging
    /// Debugger.StartDebugger();
    /// 
    /// // Set a breakpoint
    /// Debugger.SetBreakpoint(0x12345678, onBreak: () => {
    ///     Console.WriteLine("Breakpoint hit!");
    ///     var context = Debugger.GetContext();
    ///     Console.WriteLine($"EAX = 0x{context.EAX:X}");
    ///     
    ///     // Modify register and continue
    ///     context.EAX = 0x1000;
    ///     Debugger.SetContext(context);
    ///     Debugger.Continue();
    /// });
    /// 
    /// // Check debugging state
    /// if (Debugger.IsDebugging())
    /// {
    ///     Console.WriteLine("Debugger is active");
    /// }
    /// </code>
    /// </example>
    public static class Debugger
    {
        /// <summary>
        /// Starts the debugger for the currently opened process.
        /// </summary>
        /// <param name="interface">The debugger interface to use. Default uses CE's default interface.</param>
        /// <exception cref="DebuggerException">Thrown when starting the debugger fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debugProcess()</c> Lua function.</para>
        /// <para>The debugger must be started before using breakpoints or other debug features.</para>
        /// <para>Different interfaces have different capabilities and limitations.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Start with default debugger interface
        /// Debugger.StartDebugger();
        /// 
        /// // Start with specific interface
        /// Debugger.StartDebugger(DebuggerInterface.VEH);
        /// </code>
        /// </example>
        public static void StartDebugger(DebuggerInterface @interface = DebuggerInterface.None)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "debugProcess");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("debugProcess function not available in this CE version");
                }

                if (@interface != DebuggerInterface.None)
                {
                    native.PushInteger(state, (int)@interface);
                    var result = native.PCall(state, 1, 0);
                    if (result != 0)
                    {
                        var error = native.ToString(state, -1);
                        native.Pop(state, 1);
                        throw new DebuggerException($"debugProcess({@interface}) call failed: {error}");
                    }
                }
                else
                {
                    var result = native.PCall(state, 0, 0);
                    if (result != 0)
                    {
                        var error = native.ToString(state, -1);
                        native.Pop(state, 1);
                        throw new DebuggerException($"debugProcess() call failed: {error}");
                    }
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error starting debugger: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets whether the debugger is currently active.
        /// </summary>
        /// <returns>True if debugging is active; otherwise, false.</returns>
        /// <exception cref="DebuggerException">Thrown when checking debug state fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_isDebugging()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Debugger.IsDebugging())
        /// {
        ///     Console.WriteLine("Debugger is active");
        ///     Debugger.SetBreakpoint(someAddress);
        /// }
        /// </code>
        /// </example>
        public static bool IsDebugging()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("debug_isDebugging");
            }
            catch (InvalidOperationException ex)
            {
                throw new DebuggerException("debug_isDebugging", ex);
            }
        }

        /// <summary>
        /// Gets the current debugger interface being used.
        /// </summary>
        /// <returns>The current debugger interface, or None if not debugging.</returns>
        /// <exception cref="DebuggerException">Thrown when getting interface fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_getCurrentDebuggerInterface()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var interface = Debugger.GetCurrentInterface();
        /// Console.WriteLine($"Using debugger interface: {interface}");
        /// </code>
        /// </example>
        public static DebuggerInterface GetCurrentInterface()
        {
            try
            {
                var result = PluginContext.Lua.CallIntegerFunction("debug_getCurrentDebuggerInterface");
                return (DebuggerInterface)result;
            }
            catch (InvalidOperationException ex)
            {
                throw new DebuggerException("debug_getCurrentDebuggerInterface", ex);
            }
        }

        /// <summary>
        /// Gets whether the debugger can break (set breakpoints).
        /// </summary>
        /// <returns>True if breakpoints can be set; otherwise, false.</returns>
        /// <exception cref="DebuggerException">Thrown when checking break capability fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_canBreak()</c> Lua function.</para>
        /// <para>Some debugger interfaces or target processes may not support breakpoints.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Debugger.CanBreak())
        /// {
        ///     Debugger.SetBreakpoint(targetAddress);
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Breakpoints not supported");
        /// }
        /// </code>
        /// </example>
        public static bool CanBreak()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("debug_canBreak");
            }
            catch (InvalidOperationException ex)
            {
                throw new DebuggerException("debug_canBreak", ex);
            }
        }

        /// <summary>
        /// Gets whether the debugger is currently broken (halted on a breakpoint).
        /// </summary>
        /// <returns>True if execution is halted; otherwise, false.</returns>
        /// <exception cref="DebuggerException">Thrown when checking break state fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_isBroken()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Debugger.IsBroken())
        /// {
        ///     var context = Debugger.GetContext();
        ///     Console.WriteLine($"Stopped at: 0x{context.EIP:X}");
        /// }
        /// </code>
        /// </example>
        public static bool IsBroken()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("debug_isBroken");
            }
            catch (InvalidOperationException ex)
            {
                throw new DebuggerException("debug_isBroken", ex);
            }
        }

        /// <summary>
        /// Gets whether the debugger is currently single stepping.
        /// </summary>
        /// <returns>True if single stepping; otherwise, false.</returns>
        /// <exception cref="DebuggerException">Thrown when checking step state fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_isStepping()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Debugger.IsStepping())
        /// {
        ///     Console.WriteLine("Single stepping in progress");
        /// }
        /// </code>
        /// </example>
        public static bool IsStepping()
        {
            try
            {
                return PluginContext.Lua.CallBooleanFunction("debug_isStepping");
            }
            catch (InvalidOperationException ex)
            {
                throw new DebuggerException("debug_isStepping", ex);
            }
        }

        /// <summary>
        /// Gets a list of all currently set breakpoints.
        /// </summary>
        /// <returns>List of breakpoint addresses.</returns>
        /// <exception cref="DebuggerException">Thrown when getting breakpoint list fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_getBreakpointList()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var breakpoints = Debugger.GetBreakpointList();
        /// Console.WriteLine($"Active breakpoints: {breakpoints.Count}");
        /// foreach (var bp in breakpoints)
        /// {
        ///     Console.WriteLine($"  0x{bp:X}");
        /// }
        /// </code>
        /// </example>
        public static List<ulong> GetBreakpointList()
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "debug_getBreakpointList");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("debug_getBreakpointList function not available");
                }

                var result = native.PCall(state, 0, 1);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DebuggerException($"debug_getBreakpointList() call failed: {error}");
                }

                var breakpoints = new List<ulong>();
                if (native.IsTable(state, -1))
                {
                    // Iterate through the table
                    native.PushNil(state);
                    while (native.Next(state, -2) != 0)
                    {
                        if (native.IsNumber(state, -1))
                        {
                            var address = (ulong)native.ToInteger(state, -1);
                            breakpoints.Add(address);
                        }
                        native.Pop(state, 1); // Remove value, keep key for next iteration
                    }
                }

                native.Pop(state, 1); // Remove table
                return breakpoints;
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error getting breakpoint list: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sets a breakpoint at the specified address.
        /// </summary>
        /// <param name="address">The memory address to set the breakpoint.</param>
        /// <param name="size">The size for memory breakpoints. Ignored for execute breakpoints.</param>
        /// <param name="trigger">The breakpoint trigger type.</param>
        /// <param name="method">The breakpoint method to use.</param>
        /// <param name="onBreak">Optional callback function to call when breakpoint is hit.</param>
        /// <exception cref="DebuggerException">Thrown when setting breakpoint fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_setBreakpoint()</c> Lua function.</para>
        /// <para>Execute breakpoints ignore the size parameter.</para>
        /// <para>The callback function will be called in CE's Lua context when the breakpoint is hit.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Simple execute breakpoint
        /// Debugger.SetBreakpoint(0x12345678);
        /// 
        /// // Memory write breakpoint
        /// Debugger.SetBreakpoint(0x12345678, size: 4, trigger: BreakpointTrigger.Write);
        /// 
        /// // Breakpoint with callback
        /// Debugger.SetBreakpoint(0x12345678, onBreak: () => {
        ///     Console.WriteLine("Breakpoint hit!");
        ///     var ctx = Debugger.GetContext();
        ///     Console.WriteLine($"EAX: 0x{ctx.EAX:X}");
        /// });
        /// </code>
        /// </example>
        public static void SetBreakpoint(ulong address, int size = 1, BreakpointTrigger trigger = BreakpointTrigger.Execute,
            BreakpointMethod method = BreakpointMethod.Hardware, Action? onBreak = null)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "debug_setBreakpoint");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("debug_setBreakpoint function not available");
                }

                // Push parameters
                native.PushInteger(state, (long)address);

                if (trigger != BreakpointTrigger.Execute)
                {
                    native.PushInteger(state, size);
                    native.PushInteger(state, (int)trigger);
                    native.PushInteger(state, (int)method);

                    if (onBreak != null)
                    {
                        // Register callback function
                        lua.RegisterFunction($"bp_callback_{address:X}", onBreak);
                        native.GetGlobal(state, $"bp_callback_{address:X}");

                        var result = native.PCall(state, 5, 0);
                        if (result != 0)
                        {
                            var error = native.ToString(state, -1);
                            native.Pop(state, 1);
                            throw new DebuggerException($"debug_setBreakpoint call failed: {error}");
                        }
                    }
                    else
                    {
                        var result = native.PCall(state, 4, 0);
                        if (result != 0)
                        {
                            var error = native.ToString(state, -1);
                            native.Pop(state, 1);
                            throw new DebuggerException($"debug_setBreakpoint call failed: {error}");
                        }
                    }
                }
                else
                {
                    if (onBreak != null)
                    {
                        // Register callback function
                        lua.RegisterFunction($"bp_callback_{address:X}", onBreak);
                        native.GetGlobal(state, $"bp_callback_{address:X}");

                        var result = native.PCall(state, 2, 0);
                        if (result != 0)
                        {
                            var error = native.ToString(state, -1);
                            native.Pop(state, 1);
                            throw new DebuggerException($"debug_setBreakpoint call failed: {error}");
                        }
                    }
                    else
                    {
                        var result = native.PCall(state, 1, 0);
                        if (result != 0)
                        {
                            var error = native.ToString(state, -1);
                            native.Pop(state, 1);
                            throw new DebuggerException($"debug_setBreakpoint call failed: {error}");
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error setting breakpoint at 0x{address:X}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Removes a breakpoint at the specified address.
        /// </summary>
        /// <param name="address">The address of the breakpoint to remove.</param>
        /// <exception cref="DebuggerException">Thrown when removing breakpoint fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_removeBreakpoint()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Remove breakpoint
        /// Debugger.RemoveBreakpoint(0x12345678);
        /// </code>
        /// </example>
        public static void RemoveBreakpoint(ulong address)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "debug_removeBreakpoint");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("debug_removeBreakpoint function not available");
                }

                native.PushInteger(state, (long)address);
                var result = native.PCall(state, 1, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DebuggerException($"debug_removeBreakpoint call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error removing breakpoint at 0x{address:X}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Continues execution from a breakpoint.
        /// </summary>
        /// <param name="method">The continue method to use.</param>
        /// <exception cref="DebuggerException">Thrown when continuing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_continueFromBreakpoint()</c> Lua function.</para>
        /// <para>Only call this when the debugger is currently broken on a breakpoint.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Debugger.IsBroken())
        /// {
        ///     // Continue normal execution
        ///     Debugger.Continue();
        ///     
        ///     // Or step into next instruction
        ///     Debugger.Continue(ContinueMethod.StepInto);
        /// }
        /// </code>
        /// </example>
        public static void Continue(ContinueMethod method = ContinueMethod.Run)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "debug_continueFromBreakpoint");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("debug_continueFromBreakpoint function not available");
                }

                native.PushInteger(state, (int)method);
                var result = native.PCall(state, 1, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DebuggerException($"debug_continueFromBreakpoint call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error continuing from breakpoint: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the current CPU register context.
        /// </summary>
        /// <param name="includeExtended">Whether to include extended registers (FP, XMM).</param>
        /// <returns>The current register context.</returns>
        /// <exception cref="DebuggerException">Thrown when getting context fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_getContext()</c> Lua function.</para>
        /// <para>Only call this when the debugger is currently broken.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Debugger.IsBroken())
        /// {
        ///     var context = Debugger.GetContext();
        ///     Console.WriteLine($"EIP: 0x{context.EIP:X}");
        ///     Console.WriteLine($"EAX: 0x{context.EAX:X}");
        /// }
        /// </code>
        /// </example>
        public static RegisterContext GetContext(bool includeExtended = false)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "debug_getContext");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("debug_getContext function not available");
                }

                native.PushBoolean(state, includeExtended);
                var result = native.PCall(state, 1, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DebuggerException($"debug_getContext call failed: {error}");
                }

                // Read register values from global variables
                var context = new RegisterContext();

                native.GetGlobal(state, "EAX");
                if (native.IsNumber(state, -1))
                    context.EAX = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "EBX");
                if (native.IsNumber(state, -1))
                    context.EBX = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "ECX");
                if (native.IsNumber(state, -1))
                    context.ECX = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "EDX");
                if (native.IsNumber(state, -1))
                    context.EDX = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "ESI");
                if (native.IsNumber(state, -1))
                    context.ESI = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "EDI");
                if (native.IsNumber(state, -1))
                    context.EDI = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "EBP");
                if (native.IsNumber(state, -1))
                    context.EBP = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "ESP");
                if (native.IsNumber(state, -1))
                    context.ESP = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "EIP");
                if (native.IsNumber(state, -1))
                    context.EIP = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                native.GetGlobal(state, "EFLAGS");
                if (native.IsNumber(state, -1))
                    context.EFLAGS = (ulong)native.ToInteger(state, -1);
                native.Pop(state, 1);

                return context;
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error getting register context: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sets the CPU register context.
        /// </summary>
        /// <param name="context">The register context to set.</param>
        /// <param name="includeExtended">Whether to include extended registers.</param>
        /// <exception cref="DebuggerException">Thrown when setting context fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_setContext()</c> Lua function.</para>
        /// <para>Only call this when the debugger is currently broken.</para>
        /// <para>Changes take effect when execution continues.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Debugger.IsBroken())
        /// {
        ///     var context = Debugger.GetContext();
        ///     context.EAX = 0x1234; // Modify EAX
        ///     Debugger.SetContext(context);
        ///     Debugger.Continue();
        /// }
        /// </code>
        /// </example>
        public static void SetContext(RegisterContext context, bool includeExtended = false)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                // Set register values as global variables
                native.PushInteger(state, (long)context.EAX);
                native.SetGlobal(state, "EAX");

                native.PushInteger(state, (long)context.EBX);
                native.SetGlobal(state, "EBX");

                native.PushInteger(state, (long)context.ECX);
                native.SetGlobal(state, "ECX");

                native.PushInteger(state, (long)context.EDX);
                native.SetGlobal(state, "EDX");

                native.PushInteger(state, (long)context.ESI);
                native.SetGlobal(state, "ESI");

                native.PushInteger(state, (long)context.EDI);
                native.SetGlobal(state, "EDI");

                native.PushInteger(state, (long)context.EBP);
                native.SetGlobal(state, "EBP");

                native.PushInteger(state, (long)context.ESP);
                native.SetGlobal(state, "ESP");

                native.PushInteger(state, (long)context.EIP);
                native.SetGlobal(state, "EIP");

                native.PushInteger(state, (long)context.EFLAGS);
                native.SetGlobal(state, "EFLAGS");

                // Call debug_setContext
                native.GetGlobal(state, "debug_setContext");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("debug_setContext function not available");
                }

                native.PushBoolean(state, includeExtended);
                var result = native.PCall(state, 1, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DebuggerException($"debug_setContext call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error setting register context: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates the CE GUI to reflect the current debug state.
        /// </summary>
        /// <exception cref="DebuggerException">Thrown when updating GUI fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>debug_updateGUI()</c> Lua function.</para>
        /// <para>Call this after modifying registers to refresh the CE interface.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var context = Debugger.GetContext();
        /// context.EAX = 0x1000;
        /// Debugger.SetContext(context);
        /// Debugger.UpdateGUI(); // Refresh CE interface
        /// </code>
        /// </example>
        public static void UpdateGUI()
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "debug_updateGUI");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("debug_updateGUI function not available");
                }

                var result = native.PCall(state, 0, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DebuggerException($"debug_updateGUI call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error updating GUI: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Detaches the debugger from the target process if possible.
        /// </summary>
        /// <exception cref="DebuggerException">Thrown when detaching fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>detachIfPossible()</c> Lua function.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Debugger.IsDebugging())
        /// {
        ///     Debugger.Detach();
        ///     Console.WriteLine("Debugger detached");
        /// }
        /// </code>
        /// </example>
        public static void Detach()
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "detachIfPossible");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("detachIfPossible function not available");
                }

                var result = native.PCall(state, 0, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DebuggerException($"detachIfPossible call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error detaching debugger: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Outputs a debug message using Windows OutputDebugString.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <exception cref="DebuggerException">Thrown when outputting debug message fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>outputDebugString()</c> Lua function.</para>
        /// <para>Messages can be viewed with tools like DebugView.</para>
        /// <para>Useful for debugging when CE's GUI freezes.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Debugger.OutputDebugString("Plugin loaded successfully");
        /// Debugger.OutputDebugString($"Breakpoint hit at 0x{address:X}");
        /// </code>
        /// </example>
        public static void OutputDebugString(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                native.GetGlobal(state, "outputDebugString");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new DebuggerException("outputDebugString function not available");
                }

                native.PushString(state, message);
                var result = native.PCall(state, 1, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new DebuggerException($"outputDebugString call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not DebuggerException)
            {
                throw new DebuggerException($"Error outputting debug string: {ex.Message}", ex);
            }
        }
    }
}