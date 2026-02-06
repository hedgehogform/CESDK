using System;
using System.Collections.Generic;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class ThreadListException : CesdkException
    {
        public ThreadListException(string message) : base(message) { }
        public ThreadListException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// ThreadList class that wraps Cheat Engine's getThreadlist function
    /// </summary>
    public class ThreadList
    {
        private readonly LuaNative lua;
        private readonly List<string> threadIds = [];

        public ThreadList()
        {
            lua = PluginContext.Lua;
            Refresh();
        }

        /// <summary>
        /// Refreshes the thread list by reloading it from the process
        /// </summary>
        public void Refresh()
        {
            try
            {
                threadIds.Clear();

                lua.GetGlobal("createStringlist");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new ThreadListException("createStringlist function not available in this CE version");
                }

                var result = lua.PCall(0, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new ThreadListException($"createStringlist() call failed: {error}");
                }

                int stringListRef = lua.GetTop();

                lua.GetGlobal("getThreadlist");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new ThreadListException("getThreadlist function not available in this CE version");
                }

                lua.PushValue(stringListRef);

                result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(2);
                    throw new ThreadListException($"getThreadlist() call failed: {error}");
                }

                // Read the filled StringList
                lua.PushValue(stringListRef);
                lua.GetField(-1, "Count");
                if (!lua.IsInteger(-1))
                {
                    lua.Pop(2);
                    throw new ThreadListException("Could not get Count property from StringList");
                }

                int count = lua.ToInteger(-1);
                lua.Pop(1);

                for (int i = 0; i < count; i++)
                {
                    lua.PushInteger(i);
                    lua.GetTable(-2);

                    if (lua.IsString(-1))
                    {
                        var threadId = lua.ToString(-1);
                        if (!string.IsNullOrEmpty(threadId))
                            threadIds.Add(threadId);
                    }
                    lua.Pop(1);
                }

                lua.Pop(1); // Pop StringList object
            }
            catch (Exception ex) when (ex is not ThreadListException)
            {
                throw new ThreadListException("Failed to load thread list", ex);
            }
        }

        /// <summary>
        /// Gets the number of threads
        /// </summary>
        public int Count => threadIds.Count;

        /// <summary>
        /// Gets the thread ID at the specified index as a hex string
        /// </summary>
        public string GetThreadId(int index)
        {
            if (index < 0 || index >= threadIds.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Thread count: {threadIds.Count}");

            return threadIds[index];
        }

        /// <summary>
        /// Gets the thread ID at the specified index as an integer
        /// </summary>
        public int GetThreadIdAsInt(int index)
        {
            var hexString = GetThreadId(index);
            if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hexString = hexString.Substring(2);

            return Convert.ToInt32(hexString, 16);
        }

        /// <summary>
        /// Gets all thread IDs as hex strings
        /// </summary>
        public string[] GetAllThreadIds() => threadIds.ToArray();

        /// <summary>
        /// Gets all thread IDs as integers
        /// </summary>
        public int[] GetAllThreadIdsAsInt()
        {
            var intIds = new int[threadIds.Count];
            for (int i = 0; i < threadIds.Count; i++)
                intIds[i] = GetThreadIdAsInt(i);
            return intIds;
        }

        /// <summary>
        /// Indexer for thread access
        /// </summary>
        public string this[int index] => GetThreadId(index);
    }
}