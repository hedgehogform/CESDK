namespace CESDK.System
{
    public static class Converter
    {
        public static string StringToMD5(string value)
        {
            var lua = PluginContext.Lua;
            var state = lua.State;
            var native = lua.Native;

            // stringToMD5String
            native.GetGlobal(state, "stringToMD5String");
            if (!native.IsFunction(state, -1))
            {
                native.Pop(state, 1);
                throw new CheatEngineInfoException("stringToMD5String function not available in this CE version");
            }

            native.PushString(state, value);
            var result = native.PCall(state, 1, 1);
            if (result != 0)
            {
                var error = native.ToString(state, -1);
                native.Pop(state, 1);
                throw new CheatEngineInfoException($"stringToMD5String() call failed: {error}");
            }

            var md5 = native.ToString(state, -1);
            native.Pop(state, 1);
            return md5;
        }
    }
}