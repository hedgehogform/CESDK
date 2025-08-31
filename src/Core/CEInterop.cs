using System;
using System.Runtime.InteropServices;

namespace CESDK.Core
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct TPluginVersion
    {
        public uint version;
        public IntPtr name;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TPluginInit
    {
        public IntPtr name;
        public IntPtr GetVersion;
        public IntPtr EnablePlugin;
        public IntPtr DisablePlugin;
        public int version;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate Boolean DelegateGetVersion([MarshalAs(UnmanagedType.Struct)] ref TPluginVersion pluginVersion, int TPluginVersionSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate Boolean DelegateEnablePlugin([MarshalAs(UnmanagedType.Struct)] ref TExportedFunctions exportedFunctions, UInt32 pluginid);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate Boolean DelegateDisablePlugin();
}