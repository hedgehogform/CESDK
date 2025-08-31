using CESDK.Lua;

namespace CESDK
{
    /// <summary>
    /// Base class for creating Cheat Engine plugins with a clean, modern API.
    /// Inherit from this class to create your plugin with minimal boilerplate code.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyPlugin : CheatEnginePlugin
    /// {
    ///     public override string Name => "My Custom Plugin";
    ///     
    ///     protected override void OnEnable()
    ///     {
    ///         Lua.Execute("print('Plugin enabled!')");
    ///         Lua.RegisterFunction("my_function", MyFunction);
    ///     }
    ///     
    ///     private int MyFunction()
    ///     {
    ///         Console.WriteLine("Called from Lua!");
    ///         return 1;
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class CheatEnginePlugin
    {
        /// <summary>
        /// Gets the display name of your plugin as shown in Cheat Engine.
        /// This name will appear in CE's plugin list and any error messages.
        /// </summary>
        /// <value>A descriptive name for your plugin.</value>
        /// <example>
        /// <code>
        /// public override string Name => "Advanced Memory Scanner v2.1";
        /// </code>
        /// </example>
        public abstract string Name { get; }

        /// <summary>
        /// Gets access to Cheat Engine's Lua scripting environment.
        /// Use this to execute Lua scripts, register C# functions, and interact with CE's API.
        /// </summary>
        /// <value>The <see cref="LuaEngine"/> instance for this plugin.</value>
        /// <exception cref="InvalidOperationException">Thrown if the plugin is not properly initialized.</exception>
        /// <example>
        /// <code>
        /// Lua.Execute("print('Hello from C#!')");
        /// Lua.RegisterFunction("test", () => { Console.WriteLine("Test called!"); return 1; });
        /// var processName = Lua.GetGlobalString("targetProcessName");
        /// </code>
        /// </example>
        public LuaEngine Lua => PluginContext.Lua;

        /// <summary>
        /// Called when the plugin is enabled in Cheat Engine.
        /// Override this method to perform initialization tasks such as registering Lua functions,
        /// setting up UI elements, or preparing resources.
        /// </summary>
        /// <remarks>
        /// <para>This method is called automatically by the Cheat Engine SDK when the user enables your plugin.</para>
        /// <para>Any exceptions thrown in this method will prevent the plugin from loading and will be logged.</para>
        /// <para>You don't need to return a boolean value - the SDK handles success/failure automatically.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// protected override void OnEnable()
        /// {
        ///     // Register functions that can be called from Lua
        ///     Lua.RegisterFunction("scan_memory", ScanMemoryFunction);
        ///     
        ///     // Set up UI in Cheat Engine
        ///     Lua.Execute(@"
        ///         local menu = MainForm.Menu
        ///         local pluginMenu = createMenuItem(menu)
        ///         pluginMenu.Caption = 'My Plugin'
        ///         menu.Items.add(pluginMenu)
        ///     ");
        ///     
        ///     Console.WriteLine($"Plugin '{Name}' enabled successfully!");
        /// }
        /// </code>
        /// </example>
        protected virtual void OnEnable() { }

        /// <summary>
        /// Called when the plugin is disabled in Cheat Engine.
        /// Override this method to perform cleanup tasks such as disposing resources,
        /// removing UI elements, or saving configuration data.
        /// </summary>
        /// <remarks>
        /// <para>This method is called automatically by the Cheat Engine SDK when the user disables your plugin or when CE is closing.</para>
        /// <para>Any exceptions thrown in this method will be logged but won't prevent the plugin from being disabled.</para>
        /// <para>You don't need to return a boolean value - the SDK handles success/failure automatically.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// protected override void OnDisable()
        /// {
        ///     // Clean up resources
        ///     _backgroundTimer?.Stop();
        ///     _backgroundTimer?.Dispose();
        ///     
        ///     // Save configuration
        ///     SaveSettings();
        ///     
        ///     Console.WriteLine($"Plugin '{Name}' disabled successfully!");
        /// }
        /// </code>
        /// </example>
        protected virtual void OnDisable() { }

        #region Internal Plugin Interface (Hidden from users)
        
        /// <summary>
        /// Internal method called by the SDK to enable the plugin.
        /// </summary>
        internal void InternalOnEnable() => OnEnable();

        /// <summary>
        /// Internal method called by the SDK to disable the plugin.
        /// </summary>
        internal void InternalOnDisable() => OnDisable();
        
        #endregion

        /// <summary>
        /// Processes pending Windows messages in the main Cheat Engine thread.
        /// Call this method during long-running operations to prevent CE from appearing frozen.
        /// </summary>
        /// <remarks>
        /// <para>This is particularly useful when performing time-consuming operations in the main thread,
        /// such as large memory scans or complex calculations.</para>
        /// <para>Calling this method allows the UI to remain responsive and prevents Windows from marking
        /// Cheat Engine as "Not Responding".</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// private void PerformLongOperation()
        /// {
        ///     for (int i = 0; i &lt; 1000000; i++)
        ///     {
        ///         // Do some work
        ///         DoComplexCalculation(i);
        ///         
        ///         // Keep UI responsive every 1000 iterations
        ///         if (i % 1000 == 0)
        ///             ProcessMessages();
        ///     }
        /// }
        /// </code>
        /// </example>
        protected void ProcessMessages() => PluginContext.ProcessMessages();

        /// <summary>
        /// Checks for pending synchronization events with a specified timeout.
        /// Useful when working with threads that need to synchronize with the main CE thread.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for synchronization events, in milliseconds.</param>
        /// <returns><see langword="true"/> if a synchronization event was processed; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is typically used when you have background threads that need to execute code
        /// on the main Cheat Engine thread (e.g., updating UI elements or calling certain CE functions).</para>
        /// <para>The synchronization is handled through CE's built-in thread synchronization mechanism.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// private void BackgroundWorker()
        /// {
        ///     var thread = new Thread(() =>
        ///     {
        ///         // Do background work
        ///         var result = CalculateComplexData();
        ///         
        ///         // This will queue the print to run on the main thread
        ///         Lua.Execute($"print('Background calculation result: {result}')");
        ///     });
        ///     
        ///     thread.Start();
        ///     
        ///     // Wait for the background thread while processing sync events
        ///     while (thread.IsAlive)
        ///     {
        ///         CheckSynchronize(10); // Check every 10ms
        ///         Thread.Sleep(1);
        ///     }
        /// }
        /// </code>
        /// </example>
        protected bool CheckSynchronize(int timeout) => PluginContext.CheckSynchronize(timeout);

    }
}