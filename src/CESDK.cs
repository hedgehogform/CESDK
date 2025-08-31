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
        public static CheatEnginePlugin? currentPlugin;
        public LuaEngine? lua;
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
        public Core.TExportedFunctions pluginexports;

        private CESDK()
        {
            delGetVersion = GetVersion;
            delEnablePlugin = EnablePlugin;
            delDisablePlugin = DisablePlugin;
        }

        private static Boolean GetVersion(ref TPluginVersion pluginVersion, int TPluginVersionSize)
        {
            pluginVersion.name = PluginNamePtr;
            pluginVersion.version = PLUGIN_VERSION;
            return true;
        }

        private static Boolean EnablePlugin(ref Core.TExportedFunctions exportedFunctions, UInt32 pluginid)
        {
            try
            {
                if (mainself == null) return false;

                mainself.pluginexports = exportedFunctions;

                // Setup the delegates for CE functions
                delProcessMessages ??= Marshal.GetDelegateForFunctionPointer<DelegateProcessMessages>(exportedFunctions.ProcessMessages);

                delCheckSynchronize ??= Marshal.GetDelegateForFunctionPointer<DelegateCheckSynchronize>(exportedFunctions.CheckSynchronize);

                mainself.lua ??= new LuaEngine(exportedFunctions);

                PluginContext.Initialize(mainself.lua);

                currentPlugin?.InternalOnEnable();
                return true;
            }
            catch (Exception ex)
            {
                // System.Console.WriteLine($"[CESDK] EnablePlugin failed: {ex}");
                return false;
            }
        }

        private static Boolean DisablePlugin()
        {
            try
            {
                currentPlugin?.InternalOnDisable();
                PluginContext.Cleanup();
                return true;
            }
            catch (Exception ex)
            {
                // System.Console.WriteLine($"[CESDK] DisablePlugin failed: {ex}");
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
                            currentPlugin = (CheatEnginePlugin)Activator.CreateInstance(types[i]);
                            break;
                        }
                    }

                    if (currentPlugin == null)
                        return 0;

                    PluginNamePtr = Marshal.StringToHGlobalAnsi(currentPlugin.Name);
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
            catch (Exception ex)
            {
                // System.Console.WriteLine($"[CESDK] CEPluginInitialize failed: {ex}");
                return 0;
            }
        }
    }
}