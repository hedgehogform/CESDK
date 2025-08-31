using CESDK.Lua;

namespace CESDK.System
{
    public static class Sound
    {
        /// <summary>
        /// Plays a beep sound.
        /// </summary>
        public static void Beep()
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // Call beep
            native.GetGlobal(state, "beep");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("beep function not available in this CE version");
            }

            var result = native.PCall(state, 0, 0);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"beep() call failed: {error}");
            }
        }
    }
}