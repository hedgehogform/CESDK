using System;
using System.Collections.Generic;
using CESDK.Lua;
using CESDK.Utils;

namespace CESDK.Classes
{
    public static class Process
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static void OpenProcess(object pid)
        {
            if (pid is not int && pid is not string)
                throw new ArgumentException("PID must be an int or string", nameof(pid));

            LuaUtils.CallVoidLuaFunction("openProcess", $"open process {pid}", pid);
        }

        public static int GetOpenedProcessID() =>
            LuaUtils.CallLuaFunction("getOpenedProcessID", "get opened process ID", () => PluginContext.Lua.ToInteger(-1));

        /// <summary>
        /// Gets a list of all running processes on the system
        /// </summary>
        /// <returns>Dictionary with process ID as key and process name as value</returns>
        public static Dictionary<int, string> GetProcessList()
        {
            return LuaUtils.CallLuaFunction("getProcesslist", "get process list", () =>
            {
                var processList = new Dictionary<int, string>();

                if (!lua.IsTable(-1))
                    return processList;

                lua.PushNil();
                while (lua.Next(-2) != 0)
                {
                    if (lua.IsInteger(-2) && lua.IsString(-1))
                    {
                        int pid = lua.ToInteger(-2);
                        string processName = lua.ToString(-1) ?? "";
                        processList[pid] = processName;
                    }
                    lua.Pop(1);
                }

                return processList;
            });
        }
    }
}
