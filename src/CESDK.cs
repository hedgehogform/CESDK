using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CESDK.Lua;
using System.IO;

namespace CESDK
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TExportedFunctions
    {
        public int sizeofExportedFunctions;
        public IntPtr GetLuaState;
        public IntPtr LuaRegister;
        public IntPtr LuaPushClassInstance;
        public IntPtr ProcessMessages;
        public IntPtr CheckSynchronize;
    }

    public class CESDK
    {
        private const int PLUGIN_VERSION = 6;
        private static CESDK? mainSelf;
        private static CheatEnginePlugin? _currentPlugin;
        public static CheatEnginePlugin? CurrentPlugin => _currentPlugin;

        private UInt32 pluginId;
        private TExportedFunctions pluginExports;

        private static IntPtr PluginNamePtr;

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool delegateGetVersion(ref TPluginVersion PluginVersion, int TPluginVersionSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool delegateEnablePlugin(ref TExportedFunctions ExportedFunctions, UInt32 pluginid);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool delegateDisablePlugin();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void delegateProcessMessages();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool delegateCheckSynchronize(int timeout);

        private readonly delegateGetVersion? delGetVersion;
        private readonly delegateEnablePlugin? delEnablePlugin;
        private readonly delegateDisablePlugin? delDisablePlugin;
        private delegateProcessMessages? delProcessMessages;
        private delegateCheckSynchronize? delCheckSynchronize;

        #endregion

        #region Internal Structures

        [StructLayout(LayoutKind.Sequential)]
        private struct TPluginVersion
        {
            public UInt32 version;
            public IntPtr name;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TPluginInit
        {
            public IntPtr name;
            public IntPtr GetVersion;
            public IntPtr EnablePlugin;
            public IntPtr DisablePlugin;
            public int version;
        }

        #endregion

        private CESDK()
        {
            delGetVersion = GetVersion;
            delEnablePlugin = EnablePlugin;
            delDisablePlugin = DisablePlugin;
        }

        #region Delegate Implementations

        private static bool GetVersion(ref TPluginVersion PluginVersion, int TPluginVersionSize)
        {
            PluginVersion.name = PluginNamePtr;
            PluginVersion.version = PLUGIN_VERSION;
            return true;
        }

        private static bool EnablePlugin(ref TExportedFunctions ExportedFunctions, UInt32 pluginid)
        {
            try
            {
                if (mainSelf == null || CurrentPlugin == null)
                    return false;

                mainSelf.pluginId = pluginid;
                mainSelf.pluginExports = ExportedFunctions;

                mainSelf.delProcessMessages ??= Marshal.GetDelegateForFunctionPointer<delegateProcessMessages>(mainSelf.pluginExports.ProcessMessages);
                mainSelf.delCheckSynchronize ??= Marshal.GetDelegateForFunctionPointer<delegateCheckSynchronize>(mainSelf.pluginExports.CheckSynchronize);

                // Initialize Lua with CE's exported functions
                PluginContext.Initialize(ExportedFunctions.GetLuaState, ExportedFunctions.LuaRegister, ExportedFunctions.LuaPushClassInstance);

                // Call the plugin enable hook
                CurrentPlugin.EnablePlugin();

                return true; // Must return true to CE
            }
            catch (Exception ex)
            {
                PluginLogger.LogException(ex);
                return false;
            }
        }

        private static bool DisablePlugin()
        {
            try
            {
                CurrentPlugin?.DisablePlugin();
                return true;
            }
            catch
            {
                return false;
            }

        }

        #endregion

        #region Public Helpers

        public static void ProcessMessages()
        {
            mainSelf?.delProcessMessages?.Invoke();
        }

        public static bool CheckSynchronize(int timeout)
        {
            return mainSelf?.delCheckSynchronize?.Invoke(timeout) ?? false;
        }

        #endregion

#if NETFRAMEWORK
        public static int CEPluginInitialize(string parameters)
        {
            UInt64 args = UInt64.Parse(parameters);
#else
        public static int CEPluginInitialize(IntPtr args, int size)
        {
#endif
            try
            {
                mainSelf ??= new CESDK();

                if (PluginNamePtr == IntPtr.Zero)
                {
                    foreach (var type in typeof(CheatEnginePlugin).Assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(CheatEnginePlugin)) && !type.IsAbstract)
                        {
                            _currentPlugin = (CheatEnginePlugin)Activator.CreateInstance(type)!;
                            break;
                        }
                    }

                    if (_currentPlugin == null)
                        return 0;

                    PluginNamePtr = Marshal.StringToHGlobalAnsi(_currentPlugin.Name);
                }

                UInt64 address = (UInt64)args;
                var pluginInit = new TPluginInit
                {
                    name = PluginNamePtr,
                    GetVersion = Marshal.GetFunctionPointerForDelegate(mainSelf.delGetVersion!),
                    EnablePlugin = Marshal.GetFunctionPointerForDelegate(mainSelf.delEnablePlugin!),
                    DisablePlugin = Marshal.GetFunctionPointerForDelegate(mainSelf.delDisablePlugin!),
                    version = PLUGIN_VERSION
                };

                Marshal.StructureToPtr(pluginInit, (IntPtr)address, false);
                return 1;
            }
            catch (Exception ex)
            {
                try
                {
                    PluginLogger.LogException(ex);
                }
                catch
                {
                    // Fallback to console if logger fails
                }

                Console.WriteLine("CEPluginInitialize Exception:");
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }
    }
}
