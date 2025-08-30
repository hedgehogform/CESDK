using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CESDK.Lua;

namespace CESDK.Core
{
    /// <summary>
    /// Manages plugin lifecycle and discovery
    /// </summary>
    internal class PluginManager
    {
        private CheatEnginePlugin? _plugin;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void DelegateProcessMessages();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool DelegateCheckSynchronize(int timeout);

        private DelegateProcessMessages? _processMessages;
        private DelegateCheckSynchronize? _checkSynchronize;

        public PluginManager()
        {
            DiscoverPlugin();
        }

        private void DiscoverPlugin()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var pluginType = assembly.GetTypes()
                .FirstOrDefault(t => t.IsSubclassOf(typeof(CheatEnginePlugin)) && !t.IsAbstract) ?? throw new InvalidOperationException("No CheatEnginePlugin implementation found in assembly");
            try
            {
                _plugin = (CheatEnginePlugin)Activator.CreateInstance(pluginType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create plugin instance: {ex.Message}", ex);
            }
        }

        public string GetPluginName()
        {
            return _plugin?.Name ?? "Unknown Plugin";
        }

        public bool EnablePlugin(PluginCore.TExportedFunctions exportedFunctions, uint pluginId)
        {
            try
            {
                if (_plugin == null)
                    throw new InvalidOperationException("Plugin not discovered");

                // Setup delegates for CE functions
                _processMessages = Marshal.GetDelegateForFunctionPointer<DelegateProcessMessages>(exportedFunctions.ProcessMessages);
                _checkSynchronize = Marshal.GetDelegateForFunctionPointer<DelegateCheckSynchronize>(exportedFunctions.CheckSynchronize);

                // Initialize Lua engine
                var luaEngine = new LuaEngine(exportedFunctions);

                // Initialize plugin context
                PluginContext.Initialize(luaEngine, this);

                // Call user's enable logic
                _plugin.InternalOnEnable();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Plugin '{_plugin?.Name}' failed to enable: {ex.Message}");
                return false;
            }
        }

        public bool DisablePlugin()
        {
            try
            {
                _plugin?.InternalOnDisable();
                PluginContext.Cleanup();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Plugin '{_plugin?.Name}' failed to disable: {ex.Message}");
                return false;
            }
        }

        public void ProcessMessages()
        {
            _processMessages?.Invoke();
        }

        public bool CheckSynchronize(int timeout)
        {
            return _checkSynchronize?.Invoke(timeout) ?? false;
        }
    }
}