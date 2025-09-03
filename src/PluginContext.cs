using CESDK.Lua;
using System;
using System.Runtime.InteropServices;

namespace CESDK
{
    /// <summary>
    /// Provides a global shared Lua state.
    /// </summary>
    public static class PluginContext
    {
        // This will be initialized once when the plugin is enabled
        private static LuaNative? _lua;

        /// <summary>
        /// Access the shared Lua state. Must be initialized first.
        /// </summary>
        public static LuaNative Lua
        {
            get
            {
                if (_lua == null)
                    throw new InvalidOperationException("LuaNative is not initialized yet. Call PluginContext.Initialize() first.");
                return _lua;
            }
        }

        /// <summary>
        /// Initialize the shared Lua state with CE's GetLuaState function pointer.
        /// </summary>
        /// <param name="getLuaStatePtr">Function pointer to CE's GetLuaState function</param>
        public static void Initialize(IntPtr getLuaStatePtr)
        {
            if (_lua == null)
            {
                // Call CE's GetLuaState function to get the actual Lua state
                var getLuaState = Marshal.GetDelegateForFunctionPointer<GetLuaStateDelegate>(getLuaStatePtr);
                var actualLuaState = getLuaState();
                _lua = new LuaNative(actualLuaState);
            }
        }
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GetLuaStateDelegate();
    }
}
