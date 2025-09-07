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
        /// Initialize the shared Lua state with CE's exported functions.
        /// </summary>
        /// <param name="getLuaStatePtr">Function pointer to CE's GetLuaState function</param>
        /// <param name="luaPushClassInstancePtr">Function pointer to CE's LuaPushClassInstance function</param>
        public static void Initialize(IntPtr getLuaStatePtr, IntPtr luaPushClassInstancePtr)
        {
            if (_lua == null)
            {
                // Call CE's GetLuaState function to get the actual Lua state
                var getLuaState = Marshal.GetDelegateForFunctionPointer<GetLuaStateDelegate>(getLuaStatePtr);
                var actualLuaState = getLuaState();
                _lua = new LuaNative(actualLuaState, luaPushClassInstancePtr);
            }
        }

        /// <summary>
        /// Initialize with just GetLuaState (for backward compatibility)
        /// </summary>
        /// <param name="getLuaStatePtr">Function pointer to CE's GetLuaState function</param>
        public static void Initialize(IntPtr getLuaStatePtr)
        {
            Initialize(getLuaStatePtr, IntPtr.Zero);
        }
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GetLuaStateDelegate();
    }
}
