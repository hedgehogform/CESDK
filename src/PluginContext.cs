using CESDK.Lua;

namespace CESDK
{
    /// <summary>
    /// Provides a global shared Lua state.
    /// </summary>
    public static class PluginContext
    {
        // Single Lua instance, initialized once
        private static readonly LuaNative _lua = new();

        /// <summary>
        /// Access the shared Lua state.
        /// </summary>
        public static LuaNative Lua => _lua;
    }
}
