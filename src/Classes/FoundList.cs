using System;
using CESDK.Lua;
using CESDK.Utils;

namespace CESDK.Classes
{
    public class FoundListException : Exception
    {
        public FoundListException(string message) : base(message) { }
        public FoundListException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// FoundList class that wraps Cheat Engine's FoundList Lua object for reading scan results
    /// </summary>
    public class FoundList
    {
        private const string FOUNDLIST_REFS_TABLE = "_FOUNDLIST_REFS";
        private readonly LuaNative lua;
        private bool _foundListObjectCreated = false;
        private bool _initialized = false;
        private int _luaObjectRef = -1; // Reference to the FoundList Lua object

        /// <summary>
        /// Creates a FoundList wrapper for an existing Lua FoundList object
        /// </summary>
        internal FoundList()
        {
            lua = PluginContext.Lua;
        }

        /// <summary>
        /// Internal method to set this as a wrapper for the current FoundList Lua object
        /// The FoundList object should be on the Lua stack when this is called
        /// </summary>
        internal void SetLuaFoundListObject()
        {
            _foundListObjectCreated = true;
            // The FoundList is created by the MemScan and should be accessible through global functions
            // We don't need to store references since we'll access it through the MemScan object
        }

        private void EnsureFoundListObject()
        {
            if (!_foundListObjectCreated)
                throw new FoundListException("FoundList object not properly initialized. Use MemScan.GetAttachedFoundList() to get a valid FoundList.");
        }

        /// <summary>
        /// Gets the current MemScan's attached FoundList object
        /// </summary>
        private void PushFoundListObject()
        {
            EnsureFoundListObject();
            
            // Get the current MemScan object
            lua.GetGlobal("getCurrentMemscan");
            if (!lua.IsFunction(-1))
            {
                lua.Pop(1);
                throw new FoundListException("getCurrentMemscan function not available");
            }
            
            var result = lua.PCall(0, 1);
            if (result != 0)
            {
                var error = lua.ToString(-1);
                lua.Pop(1);
                throw new FoundListException($"getCurrentMemscan() failed: {error}");
            }
            
            if (lua.IsNil(-1))
            {
                lua.Pop(1);
                throw new FoundListException("No current MemScan object available");
            }
            
            // Get the attached found list from the MemScan
            lua.GetField(-1, "getAttachedFoundlist");
            if (!lua.IsFunction(-1))
            {
                lua.Pop(2);
                throw new FoundListException("getAttachedFoundlist method not available on MemScan object");
            }
            
            lua.PushValue(-2); // Push MemScan object as self parameter
            result = lua.PCall(1, 1);
            if (result != 0)
            {
                lua.Pop(2);
                throw new FoundListException("Failed to get attached found list");
            }
            
            if (lua.IsNil(-1))
            {
                lua.Pop(2);
                throw new FoundListException("No found list attached to current MemScan");
            }
            
            lua.Remove(-2); // Remove MemScan object, keep FoundList on stack
        }

        /// <summary>
        /// Calls a method on the FoundList Lua object
        /// </summary>
        private void CallFoundListMethod(string methodName, string operationName)
        {
            try
            {
                PushFoundListObject();

                // Get method
                lua.GetField(-1, methodName);
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new FoundListException($"{methodName} method not available on FoundList object");
                }

                // Push self (FoundList object)
                lua.PushValue(-2);

                // Call method(self)
                var result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new FoundListException($"{methodName}() call failed: {error}");
                }

                lua.Pop(1); // Pop FoundList object
            }
            catch (Exception ex) when (ex is not FoundListException)
            {
                throw new FoundListException($"Failed to {operationName}", ex);
            }
        }

        /// <summary>
        /// Gets a value from FoundList using property or method fallback
        /// </summary>
        private T GetFoundListValue<T>(string propertyName, string methodName, int? index, Func<T> valueExtractor, T defaultValue, string operationName)
        {
            try
            {
                PushFoundListObject();

                // Try property access first
                lua.GetField(-1, propertyName);

                if (!lua.IsNil(-1))
                {
                    if (index.HasValue)
                    {
                        lua.PushInteger(index.Value);
                        lua.GetTable(-2);
                    }
                    
                    if (!lua.IsNil(-1))
                    {
                        var value = valueExtractor();
                        lua.Pop(index.HasValue ? 3 : 2); // Pop value, property (and FoundList object)
                        return value;
                    }
                    lua.Pop(1); // Pop nil value
                }

                // If property doesn't exist or returned nil, try method
                lua.Pop(1); // Pop property (or nil)
                lua.GetField(-1, methodName);
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    return defaultValue; // Return default instead of throwing for count
                }

                // Push self (FoundList object)
                lua.PushValue(-2);
                if (index.HasValue)
                {
                    lua.PushInteger(index.Value);
                }

                // Call method
                var paramCount = index.HasValue ? 2 : 1;
                var result = lua.PCall(paramCount, 1);
                if (result != 0)
                {
                    lua.Pop(2); // Pop error and FoundList object
                    return defaultValue;
                }

                var methodValue = valueExtractor();
                lua.Pop(2); // Pop value and FoundList object
                return methodValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Initializes the FoundList for reading results. Call this when a MemScan has finished scanning.
        /// </summary>
        public void Initialize()
        {
            CallFoundListMethod("initialize", "initialize FoundList");
            _initialized = true;
        }

        /// <summary>
        /// Releases the FoundList results
        /// </summary>
        public void Deinitialize()
        {
            CallFoundListMethod("deinitialize", "deinitialize FoundList");
            _initialized = false;
        }

        /// <summary>
        /// Gets the number of results found
        /// </summary>
        /// <returns>Number of results, or -1 if count cannot be retrieved</returns>
        public int GetCount() =>
            GetFoundListValue("Count", "getCount", null, () => lua.ToInteger(-1), -1, "get count");

        /// <summary>
        /// Gets the number of results found (safe property that doesn't throw)
        /// </summary>
        public int Count => GetCount();

        /// <summary>
        /// Gets the address at the specified index as a string
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>Address as string</returns>
        public string GetAddress(int index)
        {
            try
            {
                var result = GetFoundListValue("Address", "getAddress", index, () => lua.ToString(-1) ?? "", "", $"get address at index {index}");
                if (!string.IsNullOrEmpty(result)) return result;
                
                throw new FoundListException("Address property and getAddress method not available on FoundList object");
            }
            catch (Exception ex) when (ex is not FoundListException)
            {
                throw new FoundListException($"Failed to get address at index {index}", ex);
            }
        }

        /// <summary>
        /// Gets the value at the specified index as a string
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>Value as string</returns>
        public string GetValue(int index)
        {
            try
            {
                var result = GetFoundListValue("Value", "getValue", index, () => lua.ToString(-1) ?? "", "", $"get value at index {index}");
                if (!string.IsNullOrEmpty(result)) return result;
                
                throw new FoundListException("Value property and getValue method not available on FoundList object");
            }
            catch (Exception ex) when (ex is not FoundListException)
            {
                throw new FoundListException($"Failed to get value at index {index}", ex);
            }
        }

        /// <summary>
        /// Indexer for address access. According to celua.txt: foundlist[index] returns address
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>Address as string</returns>
        public string this[int index]
        {
            get
            {
                try
                {
                    PushFoundListObject();

                    // Try direct indexer access
                    lua.PushInteger(index);
                    lua.GetTable(-2);

                    if (!lua.IsNil(-1))
                    {
                        var address = lua.ToString(-1);
                        lua.Pop(2); // Pop address and FoundList object
                        return address ?? "";
                    }

                    lua.Pop(2); // Pop nil and FoundList object

                    // Fallback to GetAddress method
                    return GetAddress(index);
                }
                catch (Exception ex) when (ex is not FoundListException)
                {
                    throw new FoundListException($"Failed to get item at index {index}", ex);
                }
            }
        }

        /// <summary>
        /// Gets whether the FoundList has been initialized
        /// </summary>
        public bool IsInitialized => _initialized;
    }
}