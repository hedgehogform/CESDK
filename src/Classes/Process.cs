using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Gets a list of all running processes on the system
        /// </summary>
        /// <returns>Dictionary with process ID as key and process name as value</returns>
        public static Dictionary<int, string> GetProcessList()
        {
            var processList = new Dictionary<int, string>();

            try
            {
                // Call getProcesslist() function which returns a table
                lua.GetGlobal("getProcesslist");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new InvalidOperationException("getProcesslist function not available in this CE version");
                }

                // Call the function with no parameters to get the table format
                var result = lua.PCall(0, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new InvalidOperationException($"getProcesslist() call failed: {error}");
                }

                // The result should be a table (pid - name format)
                if (!lua.IsTable(-1))
                {
                    lua.Pop(1);
                    throw new InvalidOperationException("getProcesslist() did not return a table");
                }

                // Iterate through the table
                // According to celua.txt, the table format is (pid - name)
                lua.PushNil(); // First key
                while (lua.Next(-2) != 0) // table index is -2, lua.Next returns non-zero if more elements
                {
                    // Key should be the PID (number), value should be the process name (string)
                    if (lua.IsInteger(-2) && lua.IsString(-1))
                    {
                        int pid = lua.ToInteger(-2);
                        string processName = lua.ToString(-1) ?? "";
                        processList[pid] = processName;
                    }
                    lua.Pop(1); // Remove value, keep key for next iteration
                }

                lua.Pop(1); // Remove the table from stack
                return processList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get process list: {ex.Message}", ex);
            }
        }
    }
}
