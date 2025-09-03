using System;
using CESDK.Lua;

namespace CESDK.Classes
{
    public static class Process
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static void OpenProcess(object pid)
        {
            if (pid is not int && pid is not string)
                throw new ArgumentException("PID must be an int or string", nameof(pid));

            lua.GetGlobal("openProcess");
            if (!lua.IsFunction(-1))
            {
                lua.Pop(1);
                throw new InvalidOperationException("openProcess function not available in this CE version");
            }

            if (pid is int intPid)
                lua.PushInteger(intPid);
            else if (pid is string strPid)
                lua.PushString(strPid);

            var result = lua.PCall(1, 0);
            if (result != 0)
            {
                var error = lua.ToString(-1);
                lua.Pop(1);
                throw new InvalidOperationException($"openProcess({pid}) call failed: {error}");
            }
        }

        public static int GetOpenedProcessID()
        {
            lua.GetGlobal("getOpenedProcessID");
            if (!lua.IsFunction(-1))
            {
                lua.Pop(1);
                throw new InvalidOperationException("getOpenedProcessID function not available in this CE version");
            }

            var result = lua.PCall(0, 1);
            if (result != 0)
            {
                var error = lua.ToString(-1);
                lua.Pop(1);
                throw new InvalidOperationException($"getOpenedProcessID() call failed: {error}");
            }

            if (!lua.IsInteger(-1))
            {
                lua.Pop(1);
                throw new InvalidOperationException("getOpenedProcessID() did not return an integer");
            }

            int pid = lua.ToInteger(-1);
            lua.Pop(1); // Pop the result
            return pid;
        }
    }
}
