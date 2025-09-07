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
    public class FoundList : CEObjectWrapper
    {
        private bool _initialized = false;

        /// <summary>
        /// Creates an empty FoundList for setting CE object from stack
        /// </summary>
        internal FoundList()
        {
            // Empty constructor for setting CE object from stack
        }

        /// <summary>
        /// Creates a FoundList from a MemScan object using createFoundList
        /// </summary>
        internal FoundList(MemScan memScan)
        {
            CreateFoundListFromMemScan(memScan);
        }

        /// <summary>
        /// Creates a FoundList from a MemScan object using CE's createFoundList function
        /// </summary>
        private void CreateFoundListFromMemScan(MemScan memScan)
        {
            try
            {
                lua.GetGlobal("createFoundList");
                if (lua.IsNil(-1))
                    throw new FoundListException("You have no createFoundList (WTF)");

                lua.PushCEObject(memScan.obj);
                lua.PCall(1, 1);

                if (lua.IsCEObject(-1))
                    CEObject = lua.ToCEObject(-1);                        
                else
                    throw new FoundListException("No idea what createFoundList returned");
            }
            finally
            {
                lua.SetTop(0);
            }
        }

        /// <summary>
        /// Sets the CE object from a FoundList object on the Lua stack
        /// </summary>
        internal void SetCEObjectFromFoundListOnStack()
        {
            if (!lua.IsCEObject(-1))
                throw new FoundListException("Top of stack is not a FoundList CE object");
                
            SetCEObjectFromStack();
        }

        private void EnsureFoundListObject()
        {
            if (CEObject == IntPtr.Zero)
                throw new FoundListException("FoundList object not properly initialized. Use MemScan.GetAttachedFoundList() to get a valid FoundList.");
        }

        /// <summary>
        /// Pushes the FoundList CE object onto the Lua stack
        /// </summary>
        private void PushFoundListObject()
        {
            EnsureFoundListObject();
            PushCEObject();
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


        public void Initialize()
        {
            try
            {
                lua.PushCEObject(CEObject);

                lua.PushString("initialize");
                lua.GetTable(-2);

                if (!lua.IsFunction(-1))
                    throw new FoundListException("foundlist with no initialize method");

                lua.PCall(0, 0);
                _initialized = true;
            }
            finally
            {
                lua.SetTop(0);
            }
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
        public int Count { get { return GetCount(); } }

        int GetCount()
        {
            try
            {
                lua.PushCEObject(CEObject);
                lua.PushString("Count");
                lua.GetTable(-2);

                return lua.ToInteger(-1);
            }
            finally
            {
                lua.SetTop(0);
            }                
        }

        public string GetAddress(int i)
        {
            try
            {
                lua.PushCEObject(CEObject);
                lua.PushString("Address");
                lua.GetTable(-2);

                if (lua.IsTable(-1))
                {
                    lua.PushInteger(i);
                    lua.GetTable(-2); //gets index i from the Address table  (pushInteger increased the stack by 1 so the -1 turned to -2, just in case you wanted to know...)
                    return lua.ToString(-1) ?? "";
                }                
            }
            finally
            {
                lua.SetTop(0);
            }

            return "Error";
        }

        public string GetValue(int i)
        {
            try
            {
                lua.PushCEObject(CEObject);
                lua.PushString("Value");
                lua.GetTable(-2);

                if (lua.IsTable(-1))
                {
                    lua.PushInteger(i);
                    lua.GetTable(-2); //gets index i from the Address table  (pushInteger increased the stack by 1 so the -1 turned to -2, just in case you wanted to know...)
                    return lua.ToString(-1) ?? "";
                }
            }
            finally
            {
                lua.SetTop(0);
            }

            return "Error";
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