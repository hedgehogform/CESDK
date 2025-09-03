#nullable enable
using System;
using System.Runtime.InteropServices;
using CESDK.Core;
using CESDK.Lua;

namespace CESDK
{
    /// <summary>
    /// Main CESDK class - provides the entry point for Cheat Engine
    /// </summary>
    public class CESDK
    {
        // Private backing field for current plugin
        private static CheatEnginePlugin? _currentPlugin;

        /// <summary>
        /// Public read-only property exposing the current plugin.
        /// </summary>
        public static CheatEnginePlugin? CurrentPlugin => _currentPlugin;

        // Private field holding the shared Lua state
        private LuaNative _lua = PluginContext.Lua;

        /// <summary>
        /// Public property exposing the shared Lua state.
        /// </summary>
        public LuaNative Lua => _lua;

        private const int PLUGIN_VERSION = 6;
        private static CESDK? mainself;
        private static IntPtr PluginNamePtr;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void DelegateProcessMessages();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool DelegateCheckSynchronize(int timeout);

        private readonly DelegateGetVersion delGetVersion;
        private readonly DelegateEnablePlugin delEnablePlugin;
        private readonly DelegateDisablePlugin delDisablePlugin;
        private static DelegateProcessMessages? delProcessMessages;
        private static DelegateCheckSynchronize? delCheckSynchronize;

        private CESDK()
        {
            delGetVersion = GetVersion;
            delEnablePlugin = EnablePlugin;
            delDisablePlugin = DisablePlugin;
        }

        private static bool GetVersion(ref TPluginVersion pluginVersion, int TPluginVersionSize)
        {
            pluginVersion.name = PluginNamePtr;
            pluginVersion.version = PLUGIN_VERSION;
            return true;
        }

        private static bool EnablePlugin(ref TExportedFunctions exportedFunctions, uint pluginid)
        {
            try
            {
                if (mainself == null) return false;



                // Setup delegates for CE functions
                delProcessMessages ??= Marshal.GetDelegateForFunctionPointer<DelegateProcessMessages>(exportedFunctions.ProcessMessages);
                delCheckSynchronize ??= Marshal.GetDelegateForFunctionPointer<DelegateCheckSynchronize>(exportedFunctions.CheckSynchronize);

                // Use the shared Lua state from PluginContext
                mainself._lua ??= PluginContext.Lua;

                _currentPlugin?.InternalOnEnable();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool DisablePlugin()
        {
            try
            {
                _currentPlugin?.InternalOnDisable();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void ProcessMessages()
        {
            delProcessMessages?.Invoke();
        }

        public static bool CheckSynchronize(int timeout)
        {
            return delCheckSynchronize?.Invoke(timeout) ?? false;
        }

        /// <summary>
        /// Entry point called by Cheat Engine to initialize the plugin
        /// </summary>
        public static int CEPluginInitialize(string parameters)
        {
            try
            {
                mainself ??= new CESDK();

                if (PluginNamePtr == IntPtr.Zero)
                {
                    // Search for plugin using legacy pattern
                    var types = typeof(CheatEnginePlugin).Assembly.GetTypes();

                    for (int i = 0; i < types.Length; i++)
                    {
                        if (types[i].IsSubclassOf(typeof(CheatEnginePlugin)) && !types[i].IsAbstract)
                        {
                            _currentPlugin = (CheatEnginePlugin)Activator.CreateInstance(types[i]);
                            break;
                        }
                    }

                    if (_currentPlugin == null)
                        return 0;

                    PluginNamePtr = Marshal.StringToHGlobalAnsi(_currentPlugin.Name);
                }

                var address = ulong.Parse(parameters);
                var pluginInit = new TPluginInit
                {
                    name = PluginNamePtr,
                    GetVersion = Marshal.GetFunctionPointerForDelegate(mainself.delGetVersion),
                    EnablePlugin = Marshal.GetFunctionPointerForDelegate(mainself.delEnablePlugin),
                    DisablePlugin = Marshal.GetFunctionPointerForDelegate(mainself.delDisablePlugin),
                    version = PLUGIN_VERSION
                };

                Marshal.StructureToPtr(pluginInit, (IntPtr)address, false);
                return 1;
            }
            catch
            {
                return 0;
            }
        }
    }
}
