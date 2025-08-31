namespace CESDK.System
{
    /// <summary>
    /// Speedhack related functions
    /// </summary>
    public static class Speedhack
    {
        /// <summary>
        /// Sets the speedhack speed.
        /// </summary>
        /// <param name="speed"></param>
        /// <exception cref="CheatEngineInfoException"></exception>
        public static void SetSpeed(float speed)
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // Call setSpeed
            native.GetGlobal(state, "speedhack_setSpeed");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("speedhack_setSpeed function not available in this CE version");
            }

            native.PushNumber(state, speed);
            var result = native.PCall(state, 1, 0);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"speedhack_setSpeed({speed}) call failed: {error}");
            }
        }

        /// <summary>
        /// Gets the current speedhack speed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CheatEngineInfoException"></exception>
        public static float GetSpeed()
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // Call getSpeed
            native.GetGlobal(state, "speedhack_getSpeed");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("speedhack_getSpeed function not available in this CE version");
            }

            var result = native.PCall(state, 0, 1);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"speedhack_getSpeed() call failed: {error}");
            }

            if (!native.IsNumber(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("speedhack_getSpeed() did not return a number");
            }

            var speed = (float)native.ToNumber(state, -1);
            native.Pop(state, 1);
            return speed;
        }
    }
}