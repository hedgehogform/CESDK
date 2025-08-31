namespace CESDK.System
{
    /// <summary>
    /// System related functions
    /// </summary>
    public static class Process
    {
        public static long GetOpenedProcessID()
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // Call getOpenedProcessID. returns 0 if nothing is open
            native.GetGlobal(state, "getOpenedProcessID");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("getOpenedProcessID function not available in this CE version");
            }

            var result = native.PCall(state, 0, 1);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"getOpenedProcessID() call failed: {error}");
            }

            var processId = native.ToInteger(state, -1);
            native.Pop(state, 1);

            return processId;
        }

        public static long GetProcessIDFromProcessName(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                throw new ArgumentException("Process name cannot be null or empty", nameof(processName));

            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // Call getProcessIDFromProcessName
            native.GetGlobal(state, "getProcessIDFromProcessName");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("getProcessIDFromProcessName function not available in this CE version");
            }

            native.PushString(state, processName);
            var result = native.PCall(state, 1, 1);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"getProcessIDFromProcessName('{processName}') call failed: {error}");
            }

            var processId = native.ToInteger(state, -1);
            native.Pop(state, 1);

            return processId;
        }

        public static void OpenProcess(object ident)
        {
            // Only allow program exe name or process id
            if (ident is not string && ident is not int && ident is not long)
                throw new ArgumentException("Name must be a string or process ID", nameof(ident));

            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // Call openProcess
            native.GetGlobal(state, "openProcess");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("openProcess function not available in this CE version");
            }

            if (ident is string processName)
            {
                native.PushString(state, processName);
            }
            else
            {
                native.PushInteger(state, Convert.ToInt64(ident));
            }

            var result = native.PCall(state, 1, 1);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"openProcess('{ident}') call failed: {error}");
            }
        }

        public static void Pause()
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // Call pause
            native.GetGlobal(state, "pause");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("pause function not available in this CE version");
            }

            var result = native.PCall(state, 0, 0);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"pause() call failed: {error}");
            }
        }

        public static void Unpause()
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // Call unpause
            native.GetGlobal(state, "unpause");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("unpause function not available in this CE version");
            }

            var result = native.PCall(state, 0, 0);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"unpause() call failed: {error}");
            }
        }
    }
}