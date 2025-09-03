using System;
using CESDK.Lua;

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
        /// Internal method to initialize with an existing FoundList Lua object on the stack
        /// </summary>
        internal void InitializeWithLuaObject()
        {
            // The FoundList Lua object should be on top of the stack
            // Create a reference to it so we can use it later
            lua.PushValue(-1); // Duplicate the object on stack
            
            // Store reference in Lua registry
            lua.GetGlobal(FOUNDLIST_REFS_TABLE);
            if (lua.IsNil(-1))
            {
                lua.Pop(1);
                lua.CreateTable();
                lua.SetGlobal(FOUNDLIST_REFS_TABLE);
                lua.GetGlobal(FOUNDLIST_REFS_TABLE);
            }
            
            // Generate a unique reference ID
            _luaObjectRef = GetHashCode();
            lua.PushInteger(_luaObjectRef);
            lua.PushValue(-3); // Push the FoundList object
            lua.SetTable(-3); // _FOUNDLIST_REFS[ref] = foundListObject
            
            lua.Pop(2); // Pop registry table and original FoundList object
            _foundListObjectCreated = true;
        }

        private void EnsureFoundListObject()
        {
            if (!_foundListObjectCreated)
                throw new FoundListException("FoundList object not properly initialized. Use MemScan.GetAttachedFoundList() to get a valid FoundList.");
        }

        /// <summary>
        /// Pushes the FoundList Lua object onto the stack
        /// </summary>
        private void PushFoundListObject()
        {
            EnsureFoundListObject();
            
            lua.GetGlobal(FOUNDLIST_REFS_TABLE);
            if (lua.IsNil(-1))
            {
                lua.Pop(1);
                throw new FoundListException("FoundList reference table not found");
            }
            
            lua.PushInteger(_luaObjectRef);
            lua.GetTable(-2); // Get _FOUNDLIST_REFS[ref]
            
            if (lua.IsNil(-1))
            {
                lua.Pop(2);
                throw new FoundListException("FoundList object reference not found");
            }
            
            lua.Remove(-2); // Remove the reference table, leave FoundList object on stack
        }

        /// <summary>
        /// Initializes the FoundList for reading results. Call this when a MemScan has finished scanning.
        /// </summary>
        public void Initialize()
        {
            try
            {
                PushFoundListObject();

                // Get initialize method
                lua.GetField(-1, "initialize");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new FoundListException("initialize method not available on FoundList object");
                }

                // Push self (FoundList object)
                lua.PushValue(-2);

                // Call initialize(self)
                var result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new FoundListException($"initialize() call failed: {error}");
                }

                lua.Pop(1); // Pop FoundList object
                _initialized = true;
            }
            catch (Exception ex) when (ex is not FoundListException)
            {
                throw new FoundListException("Failed to initialize FoundList", ex);
            }
        }

        /// <summary>
        /// Releases the FoundList results
        /// </summary>
        public void Deinitialize()
        {
            try
            {
                PushFoundListObject();

                // Get deinitialize method
                lua.GetField(-1, "deinitialize");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new FoundListException("deinitialize method not available on FoundList object");
                }

                // Push self (FoundList object)
                lua.PushValue(-2);

                // Call deinitialize(self)
                var result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new FoundListException($"deinitialize() call failed: {error}");
                }

                lua.Pop(1); // Pop FoundList object
                _initialized = false;
            }
            catch (Exception ex) when (ex is not FoundListException)
            {
                throw new FoundListException("Failed to deinitialize FoundList", ex);
            }
        }

        /// <summary>
        /// Gets the number of results found
        /// </summary>
        /// <returns>Number of results, or -1 if count cannot be retrieved</returns>
        public int GetCount()
        {
            try
            {
                PushFoundListObject();

                // Try property access first
                lua.GetField(-1, "Count");

                if (!lua.IsNil(-1))
                {
                    var count = lua.ToInteger(-1);
                    lua.Pop(2); // Pop count and FoundList object
                    return count;
                }

                // If property doesn't exist, try method
                lua.Pop(1); // Pop nil
                lua.GetField(-1, "getCount");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    return -1; // Return -1 to indicate failure instead of throwing
                }

                // Push self (FoundList object)
                lua.PushValue(-2);

                // Call getCount(self)
                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    lua.Pop(2); // Pop error and FoundList object
                    return -1; // Return -1 to indicate failure instead of throwing
                }

                var countFromMethod = lua.ToInteger(-1);
                lua.Pop(2); // Pop count and FoundList object
                return countFromMethod;
            }
            catch
            {
                return -1; // Return -1 to indicate failure instead of throwing
            }
        }

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
                PushFoundListObject();

                // Try property access first (Address[index])
                lua.GetField(-1, "Address");

                if (!lua.IsNil(-1))
                {
                    lua.PushInteger(index);
                    lua.GetTable(-2);
                    var address = lua.ToString(-1);
                    lua.Pop(3); // Pop address, Address table, and FoundList object
                    return address ?? "";
                }

                // If property doesn't exist, try method
                lua.Pop(1); // Pop nil
                lua.GetField(-1, "getAddress");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new FoundListException("Address property and getAddress method not available on FoundList object");
                }

                // Push self (FoundList object) and index
                lua.PushValue(-2);
                lua.PushInteger(index);

                // Call getAddress(self, index)
                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new FoundListException($"getAddress({index}) call failed: {error}");
                }

                var addressFromMethod = lua.ToString(-1);
                lua.Pop(2); // Pop address and FoundList object
                return addressFromMethod ?? "";
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
                PushFoundListObject();

                // Try property access first (Value[index])
                lua.GetField(-1, "Value");

                if (!lua.IsNil(-1))
                {
                    lua.PushInteger(index);
                    lua.GetTable(-2);
                    var value = lua.ToString(-1);
                    lua.Pop(3); // Pop value, Value table, and FoundList object
                    return value ?? "";
                }

                // If property doesn't exist, try method
                lua.Pop(1); // Pop nil
                lua.GetField(-1, "getValue");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new FoundListException("Value property and getValue method not available on FoundList object");
                }

                // Push self (FoundList object) and index
                lua.PushValue(-2);
                lua.PushInteger(index);

                // Call getValue(self, index)
                var result = lua.PCall(2, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new FoundListException($"getValue({index}) call failed: {error}");
                }

                var valueFromMethod = lua.ToString(-1);
                lua.Pop(2); // Pop value and FoundList object
                return valueFromMethod ?? "";
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