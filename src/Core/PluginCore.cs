using System;
using System.Runtime.InteropServices;

namespace CESDK.Core
{
    /// <summary>
    /// Core plugin infrastructure - handles the low-level CE plugin interface
    /// </summary>
    internal static class PluginCore
    {
        private const int PLUGIN_VERSION = 6;
        private static PluginManager? _manager;
        private static IntPtr _pluginNamePtr;

        #region CE Plugin Structures

        [StructLayout(LayoutKind.Sequential)]
        private struct TPluginVersion
        {
            public uint version;
            public IntPtr name;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TPluginInit
        {
            public IntPtr name;
            public IntPtr GetVersionPtr;
            public IntPtr EnablePluginPtr;
            public IntPtr DisablePluginPtr;
            public int version;
        }

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

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool DelegateGetVersion([MarshalAs(UnmanagedType.Struct)] ref TPluginVersion pluginVersion, int size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool DelegateEnablePlugin([MarshalAs(UnmanagedType.Struct)] ref TExportedFunctions exportedFunctions, uint pluginId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool DelegateDisablePlugin();

        private static readonly DelegateGetVersion _getVersion = GetVersion;
        private static readonly DelegateEnablePlugin _enablePlugin = EnablePlugin;
        private static readonly DelegateDisablePlugin _disablePlugin = DisablePlugin;

        #endregion

        /// <summary>
        /// CE calls this to initialize the plugin
        /// </summary>
        public static int CEPluginInitialize(string parameters)
        {
            try
            {
                _manager ??= new PluginManager();

                if (_pluginNamePtr == IntPtr.Zero)
                {
                    var pluginName = _manager.GetPluginName();
                    _pluginNamePtr = Marshal.StringToHGlobalAnsi(pluginName);
                }

                var address = ulong.Parse(parameters);
                var pluginInit = new TPluginInit
                {
                    name = _pluginNamePtr,
                    GetVersionPtr = Marshal.GetFunctionPointerForDelegate(_getVersion),
                    EnablePluginPtr = Marshal.GetFunctionPointerForDelegate(_enablePlugin),
                    DisablePluginPtr = Marshal.GetFunctionPointerForDelegate(_disablePlugin),
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

        private static bool GetVersion(ref TPluginVersion pluginVersion, int size)
        {
            pluginVersion.name = _pluginNamePtr;
            pluginVersion.version = PLUGIN_VERSION;
            return true;
        }

        private static bool EnablePlugin(ref TExportedFunctions exportedFunctions, uint pluginId)
        {
            try
            {
                return _manager?.EnablePlugin(exportedFunctions, pluginId) ?? false;
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
                return _manager?.DisablePlugin() ?? false;
            }
            catch
            {
                return false;
            }
        }
    }
}