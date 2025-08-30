using System;
using CESDK.Core;
using CESDK.Lua;

namespace CESDK
{
    /// <summary>
    /// Global context providing access to plugin services
    /// </summary>
    public static class PluginContext
    {
        private static LuaEngine? _lua;
        private static PluginManager? _manager;

        /// <summary>
        /// Access to the Lua engine
        /// </summary>
        public static LuaEngine Lua => _lua ?? throw new InvalidOperationException("Plugin not initialized");

        /// <summary>
        /// Initialize the plugin context (internal use only)
        /// </summary>
        internal static void Initialize(LuaEngine lua, PluginManager manager)
        {
            _lua = lua ?? throw new ArgumentNullException(nameof(lua));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <summary>
        /// Clean up the plugin context (internal use only)
        /// </summary>
        internal static void Cleanup()
        {
            _lua = null;
            _manager = null;
        }

        /// <summary>
        /// Process pending Windows messages
        /// </summary>
        public static void ProcessMessages()
        {
            _manager?.ProcessMessages();
        }

        /// <summary>
        /// Check for synchronization events
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>True if synchronization occurred</returns>
        public static bool CheckSynchronize(int timeout)
        {
            return _manager?.CheckSynchronize(timeout) ?? false;
        }
    }
}