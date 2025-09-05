using System;
using System.Collections.Generic;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class ThreadListException : Exception
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
        private bool _loaded = false;

        public ThreadList()
        {
            lua = PluginContext.Lua;
            LoadThreadList();
        }

        /// <summary>
        /// Loads the thread list from the currently opened process
        /// </summary>
        private void LoadThreadList()
        {
            try
            {
                threadIds.Clear();

                // Create a StringList object for getThreadlist
                lua.GetGlobal("createStringlist");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new ThreadListException("createStringlist function not available in this CE version");
                }

                // Call createStringlist() to create a proper StringList object
                var result = lua.PCall(0, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new ThreadListException($"createStringlist() call failed: {error}");
                }

                // The StringList object should now be on the stack
                int stringListRef = lua.GetTop(); // Keep reference to our StringList

                // Get getThreadlist function
                lua.GetGlobal("getThreadlist");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2); // Pop both function and StringList
                    throw new ThreadListException("getThreadlist function not available in this CE version");
                }

                // Push the StringList as parameter
                lua.PushValue(stringListRef);

                // Call getThreadlist(StringList)
                result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(2); // Pop error and StringList
                    throw new ThreadListException($"getThreadlist() call failed: {error}");
                }

                // Now read the filled StringList
                // According to celua.txt, getThreadlist fills the StringList with format %x (hex)
                // We need to access the StringList contents using its Count property and indexer

                // Get Count property of the StringList
                lua.PushValue(stringListRef); // Push StringList object
                lua.GetField(-1, "Count");
                if (!lua.IsInteger(-1))
                {
                    lua.Pop(2); // Pop Count result and StringList
                    throw new ThreadListException("Could not get Count property from StringList");
                }

                int count = lua.ToInteger(-1);
                lua.Pop(1); // Pop Count result, keep StringList

                // Read each string from the StringList using indexer (0-based)
                for (int i = 0; i < count; i++)
                {
                    // Access StringList[i]
                    lua.PushInteger(i);
                    lua.GetTable(-2); // StringList[i]

                    if (lua.IsString(-1))
                    {
                        var threadId = lua.ToString(-1);
                        if (!string.IsNullOrEmpty(threadId))
                        {
                            threadIds.Add(threadId);
                        }
                    }
                    lua.Pop(1); // Pop the string value
                }

                lua.Pop(1); // Pop StringList object
                _loaded = true;
            }
            catch (Exception ex) when (ex is not ThreadListException)
            {
                throw new ThreadListException("Failed to load thread list", ex);
            }
        }

        /// <summary>
        /// Refreshes the thread list by reloading it from the process
        /// </summary>
        public void Refresh()
        {
            LoadThreadList();
        }

        /// <summary>
        /// Gets the number of threads
        /// </summary>
        public int Count => threadIds.Count;

        /// <summary>
        /// Gets the thread ID at the specified index as a hex string
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>Thread ID as hex string</returns>
        public string GetThreadId(int index)
        {
            if (index < 0 || index >= threadIds.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Thread count: {threadIds.Count}");

            return threadIds[index];
        }

        /// <summary>
        /// Gets the thread ID at the specified index as an integer
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>Thread ID as integer</returns>
        public int GetThreadIdAsInt(int index)
        {
            var hexString = GetThreadId(index);
            try
            {
                // Remove "0x" prefix if present
                if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    hexString = hexString.Substring(2);

                return Convert.ToInt32(hexString, 16);
            }
            catch (Exception ex)
            {
                throw new ThreadListException($"Failed to convert thread ID '{hexString}' to integer", ex);
            }
        }

        /// <summary>
        /// Gets thread information at the specified index
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>ThreadInfo object</returns>
        public ThreadInfo GetThreadInfo(int index)
        {
            var threadIdHex = GetThreadId(index);
            var threadIdInt = GetThreadIdAsInt(index);

            return new ThreadInfo
            {
                Id = threadIdInt,
                HexId = threadIdHex
            };
        }

        /// <summary>
        /// Gets all thread information
        /// </summary>
        /// <returns>Array of ThreadInfo objects</returns>
        public ThreadInfo[] GetAllThreads()
        {
            var threads = new ThreadInfo[threadIds.Count];
            for (int i = 0; i < threadIds.Count; i++)
            {
                threads[i] = GetThreadInfo(i);
            }
            return threads;
        }

        /// <summary>
        /// Gets all thread IDs as hex strings
        /// </summary>
        /// <returns>Array of thread ID hex strings</returns>
        public string[] GetAllThreadIds()
        {
            return [.. threadIds];
        }

        /// <summary>
        /// Gets all thread IDs as integers
        /// </summary>
        /// <returns>Array of thread ID integers</returns>
        public int[] GetAllThreadIdsAsInt()
        {
            var intIds = new int[threadIds.Count];
            for (int i = 0; i < threadIds.Count; i++)
            {
                intIds[i] = GetThreadIdAsInt(i);
            }
            return intIds;
        }

        /// <summary>
        /// Indexer for thread access
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>ThreadInfo object</returns>
        public ThreadInfo this[int index] => GetThreadInfo(index);

        /// <summary>
        /// Gets whether the thread list has been loaded
        /// </summary>
        public bool IsLoaded => _loaded;
    }

    /// <summary>
    /// Information about a thread
    /// </summary>
    public class ThreadInfo
    {
        /// <summary>
        /// Thread ID as integer
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Thread ID as hex string (original format from getThreadlist)
        /// </summary>
        public string HexId { get; set; } = "";

        public override string ToString()
        {
            return $"Thread {Id} (0x{Id:X})";
        }
    }
}