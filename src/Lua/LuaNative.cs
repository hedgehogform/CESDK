using System;
using System.Runtime.InteropServices;

namespace CESDK.Lua
{
    /// <summary>
    /// Low-level Lua API bindings - only the essential functions
    /// </summary>
    public class LuaNative
    {
        #region Lua Constants
        private const int LUA_TNONE = -1;
        private const int LUA_TNIL = 0;
        private const int LUA_TBOOLEAN = 1;
        private const int LUA_TLIGHTUSERDATA = 2;
        // Standard Lua constants kept for API completeness and future use
#pragma warning disable S1144
        private const int LUA_TNUMBER = 3;
        private const int LUA_TSTRING = 4;
#pragma warning restore S1144
        private const int LUA_TTABLE = 5;
        private const int LUA_TFUNCTION = 6;
        private const int LUA_TUSERDATA = 7;
        private const int LUA_TTHREAD = 8;
        #endregion

        #region DLL Imports
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibraryA([MarshalAs(UnmanagedType.LPStr)] string fileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr module, [MarshalAs(UnmanagedType.LPStr)] string procedureName);
        #endregion

        #region Delegates
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateGetTop(IntPtr state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateSetTop(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateGetGlobal(IntPtr state, [MarshalAs(UnmanagedType.LPStr)] string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateSetGlobal(IntPtr state, [MarshalAs(UnmanagedType.LPStr)] string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegatePushString(IntPtr state, [MarshalAs(UnmanagedType.LPStr)] string str);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegatePushInteger(IntPtr state, long value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegatePushNumber(IntPtr state, double value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegatePushBoolean(IntPtr state, bool value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateGetField(IntPtr state, int index, [MarshalAs(UnmanagedType.LPStr)] string key);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateCreateTable(IntPtr state, int arraySize, int recordSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateType(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsString(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsNumber(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DelegateToString(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long DelegateToInteger(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate double DelegateToNumber(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateToBoolean(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegatePCall(IntPtr state, int nargs, int nresults, int errfunc, IntPtr context, IntPtr k);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateLoadString(IntPtr state, [MarshalAs(UnmanagedType.LPStr)] string script);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateGetTable(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateSetTable(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegatePushValue(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegatePushNil(IntPtr state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateRotate(IntPtr state, int index, int n);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateCopy(IntPtr state, int fromIndex, int toIndex);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsInteger(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsCFunction(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsUserData(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateRawLen(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateCall(IntPtr state, int nargs, int nresults, IntPtr context, IntPtr k);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DelegateToUserData(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateNext(IntPtr state, int index);
        #endregion

        #region Function Pointers
        private readonly DelegateGetTop _getTop;
        private readonly DelegateSetTop _setTop;
        private readonly DelegateGetGlobal _getGlobal;
        private readonly DelegateSetGlobal _setGlobal;
        private readonly DelegatePushString _pushString;
        private readonly DelegatePushInteger _pushInteger;
        private readonly DelegatePushNumber _pushNumber;
        private readonly DelegatePushBoolean _pushBoolean;
        private readonly DelegateCreateTable _createTable;
        private readonly DelegateType _type;
        private readonly DelegateIsString _isString;
        private readonly DelegateIsNumber _isNumber;
        private readonly DelegateToString _toString;
        private readonly DelegateToInteger _toInteger;
        private readonly DelegateToNumber _toNumber;
        private readonly DelegateToBoolean _toBoolean;
        private readonly DelegatePCall _pcall;
        private readonly DelegateLoadString _loadString;
        private readonly DelegateGetTable _getTable;
        private readonly DelegateGetField _getField;
        private readonly DelegateSetTable _setTable;
        private readonly DelegatePushValue _pushValue;
        private readonly DelegatePushNil _pushNil;
        private readonly DelegateRotate _rotate;
        private readonly DelegateCopy _copy;
        private readonly DelegateIsInteger _isInteger;
        private readonly DelegateIsCFunction _isCFunction;
        private readonly DelegateIsUserData _isUserData;
        private readonly DelegateRawLen _rawLen;
        private readonly DelegateCall _call;
        private readonly DelegateToUserData _toUserData;
        private readonly DelegateNext _next;
        #endregion

        public LuaNative()
        {
            // Load Lua DLL - try both 32 and 64 bit versions
            var luaModule = LoadLibraryA("lua53-32.dll");
            if (luaModule == IntPtr.Zero)
                luaModule = LoadLibraryA("lua53-64.dll");

            if (luaModule == IntPtr.Zero)
                throw new InvalidOperationException("Could not load Lua library");

            // Initialize function pointers
            _getTop = GetDelegate<DelegateGetTop>(luaModule, "lua_gettop");
            _setTop = GetDelegate<DelegateSetTop>(luaModule, "lua_settop");
            _getGlobal = GetDelegate<DelegateGetGlobal>(luaModule, "lua_getglobal");
            _setGlobal = GetDelegate<DelegateSetGlobal>(luaModule, "lua_setglobal");
            _pushString = GetDelegate<DelegatePushString>(luaModule, "lua_pushstring");
            _pushInteger = GetDelegate<DelegatePushInteger>(luaModule, "lua_pushinteger");
            _pushNumber = GetDelegate<DelegatePushNumber>(luaModule, "lua_pushnumber");
            _pushBoolean = GetDelegate<DelegatePushBoolean>(luaModule, "lua_pushboolean");
            _createTable = GetDelegate<DelegateCreateTable>(luaModule, "lua_createtable");
            _type = GetDelegate<DelegateType>(luaModule, "lua_type");
            _isString = GetDelegate<DelegateIsString>(luaModule, "lua_isstring");
            _isNumber = GetDelegate<DelegateIsNumber>(luaModule, "lua_isnumber");
            _toString = GetDelegate<DelegateToString>(luaModule, "lua_tolstring");
            _toInteger = GetDelegate<DelegateToInteger>(luaModule, "lua_tointegerx");
            _toNumber = GetDelegate<DelegateToNumber>(luaModule, "lua_tonumberx");
            _toBoolean = GetDelegate<DelegateToBoolean>(luaModule, "lua_toboolean");
            _pcall = GetDelegate<DelegatePCall>(luaModule, "lua_pcallk");
            _loadString = GetDelegate<DelegateLoadString>(luaModule, "luaL_loadstring");
            _getTable = GetDelegate<DelegateGetTable>(luaModule, "lua_gettable");
            _getField = GetDelegate<DelegateGetField>(luaModule, "lua_getfield");
            _setTable = GetDelegate<DelegateSetTable>(luaModule, "lua_settable");
            _pushValue = GetDelegate<DelegatePushValue>(luaModule, "lua_pushvalue");
            _pushNil = GetDelegate<DelegatePushNil>(luaModule, "lua_pushnil");
            _rotate = GetDelegate<DelegateRotate>(luaModule, "lua_rotate");
            _copy = GetDelegate<DelegateCopy>(luaModule, "lua_copy");
            _isInteger = GetDelegate<DelegateIsInteger>(luaModule, "lua_isinteger");
            _isCFunction = GetDelegate<DelegateIsCFunction>(luaModule, "lua_iscfunction");
            _isUserData = GetDelegate<DelegateIsUserData>(luaModule, "lua_isuserdata");
            _rawLen = GetDelegate<DelegateRawLen>(luaModule, "lua_rawlen");
            _call = GetDelegate<DelegateCall>(luaModule, "lua_callk");
            _toUserData = GetDelegate<DelegateToUserData>(luaModule, "lua_touserdata");
            _next = GetDelegate<DelegateNext>(luaModule, "lua_next");
        }

        private T GetDelegate<T>(IntPtr module, string functionName) where T : class
        {
            var address = GetProcAddress(module, functionName);
            if (address == IntPtr.Zero)
                throw new InvalidOperationException($"Could not find function: {functionName}");
            return Marshal.GetDelegateForFunctionPointer<T>(address);
        }

        #region Public API
        
        /// <summary>
        /// Gets the number of elements currently on the Lua stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <returns>The number of elements on the stack (0 means empty stack).</returns>
        /// <remarks>
        /// This is equivalent to the index of the top element. A stack with one element has top = 1.
        /// </remarks>
        public int GetTop(IntPtr state) => _getTop(state);
        
        /// <summary>
        /// Sets the stack top to the specified index, effectively resizing the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The new top index. Use negative values to count from current top.</param>
        /// <remarks>
        /// <para>If the new top is larger than the old one, new nil values are pushed.</para>
        /// <para>If the new top is smaller, elements above the new top are discarded.</para>
        /// <para>Use index 0 to clear the entire stack.</para>
        /// </remarks>
        public void SetTop(IntPtr state, int index) => _setTop(state, index);
        
        /// <summary>
        /// Pops (removes) the specified number of elements from the top of the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="count">The number of elements to remove from the stack.</param>
        /// <remarks>
        /// This is equivalent to calling SetTop with -count-1.
        /// </remarks>
        public void Pop(IntPtr state, int count) => SetTop(state, -count - 1);

        /// <summary>
        /// Pushes the value of the global variable onto the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="name">The name of the global variable to retrieve.</param>
        /// <returns>The type of the value that was pushed onto the stack.</returns>
        /// <remarks>
        /// If the global variable doesn't exist, nil is pushed onto the stack.
        /// </remarks>
        public int GetGlobal(IntPtr state, string name) => _getGlobal(state, name);
        
        /// <summary>
        /// Sets the value at the top of the stack as a global variable.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="name">The name of the global variable to set.</param>
        /// <remarks>
        /// The value at the top of the stack is popped and assigned to the global variable.
        /// </remarks>
        public void SetGlobal(IntPtr state, string name) => _setGlobal(state, name);

        /// <summary>
        /// Pushes a string value onto the Lua stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="value">The string value to push.</param>
        /// <remarks>
        /// Lua creates an internal copy of the string, so the original can be safely modified or freed.
        /// </remarks>
        public void PushString(IntPtr state, string value) => _pushString(state, value);
        
        /// <summary>
        /// Pushes an integer value onto the Lua stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="value">The integer value to push.</param>
        public void PushInteger(IntPtr state, long value) => _pushInteger(state, value);
        
        /// <summary>
        /// Pushes a floating-point number onto the Lua stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="value">The floating-point value to push.</param>
        public void PushNumber(IntPtr state, double value) => _pushNumber(state, value);
        
        /// <summary>
        /// Pushes a boolean value onto the Lua stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="value">The boolean value to push.</param>
        public void PushBoolean(IntPtr state, bool value) => _pushBoolean(state, value);
        
        /// <summary>
        /// Pushes a nil value onto the Lua stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        public void PushNil(IntPtr state) => _pushNil(state);
        
        /// <summary>
        /// Pushes a copy of the value at the specified index onto the top of the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the value to copy. Can be negative to count from top.</param>
        /// <remarks>
        /// The original value remains at its position - this creates a copy on top of the stack.
        /// </remarks>
        public void PushValue(IntPtr state, int index) => _pushValue(state, index);
        
        /// <summary>
        /// Creates a new empty table and pushes it onto the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="arraySize">Expected number of array elements (optimization hint).</param>
        /// <param name="recordSize">Expected number of non-array elements (optimization hint).</param>
        /// <remarks>
        /// The size parameters are optimization hints to pre-allocate the table structure.
        /// </remarks>
        public void CreateTable(IntPtr state, int arraySize = 0, int recordSize = 0) => _createTable(state, arraySize, recordSize);

        /// <summary>
        /// Gets the type of the value at the specified stack index.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>A Lua type constant (LUA_T* values).</returns>
        public int Type(IntPtr state, int index) => _type(state, index);
        
        /// <summary>
        /// Checks if the value at the specified index is a string (or convertible to string).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a string or number (numbers are convertible to strings).</returns>
        public bool IsString(IntPtr state, int index) => _isString(state, index);
        
        /// <summary>
        /// Checks if the value at the specified index is a number (or convertible to number).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a number or numeric string.</returns>
        public bool IsNumber(IntPtr state, int index) => _isNumber(state, index);
        
        /// <summary>
        /// Checks if the value at the specified index is an integer.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is an integer number.</returns>
        public bool IsInteger(IntPtr state, int index) => _isInteger(state, index);
        
        /// <summary>
        /// Checks if the value at the specified index is a boolean.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a boolean.</returns>
        public bool IsBoolean(IntPtr state, int index) => Type(state, index) == LUA_TBOOLEAN;
        
        /// <summary>
        /// Checks if the value at the specified index is a function (Lua or C).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is any kind of function.</returns>
        public bool IsFunction(IntPtr state, int index) => Type(state, index) == LUA_TFUNCTION;
        
        /// <summary>
        /// Checks if the value at the specified index is a C function.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a C function (not a Lua function).</returns>
        public bool IsCFunction(IntPtr state, int index) => _isCFunction(state, index);
        
        /// <summary>
        /// Checks if the value at the specified index is userdata.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is userdata (light or heavy).</returns>
        public bool IsUserData(IntPtr state, int index) => _isUserData(state, index);
        
        /// <summary>
        /// Checks if the value at the specified index is a table.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a table.</returns>
        public bool IsTable(IntPtr state, int index) => Type(state, index) == LUA_TTABLE;
        
        /// <summary>
        /// Checks if the value at the specified index is nil.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is nil.</returns>
        public bool IsNil(IntPtr state, int index) => Type(state, index) == LUA_TNIL;
        
        /// <summary>
        /// Checks if the value at the specified index is a thread (coroutine).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a thread.</returns>
        public bool IsThread(IntPtr state, int index) => Type(state, index) == LUA_TTHREAD;
        
        /// <summary>
        /// Checks if the specified index is not valid (beyond the stack).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the index refers to a non-existent stack position.</returns>
        public bool IsNone(IntPtr state, int index) => Type(state, index) == LUA_TNONE;
        
        /// <summary>
        /// Checks if the value at the specified index is nil or the index is invalid.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is nil or the index is invalid.</returns>
        public bool IsNoneOrNil(IntPtr state, int index) => Type(state, index) <= 0;

        /// <summary>
        /// Converts the value at the specified index to a string.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the value to convert.</param>
        /// <returns>The string representation of the value, or null if conversion fails.</returns>
        /// <remarks>
        /// <para>Numbers and strings are converted directly. Other types may not be convertible.</para>
        /// <para>This function may change the actual value on the stack (numbers become strings).</para>
        /// </remarks>
        public string? ToString(IntPtr state, int index)
        {
            var ptr = _toString(state, index);
            // S2225: Intentionally returning null to match legacy behavior - empty string would be incorrect
#pragma warning disable S2225
            return ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) : null;
#pragma warning restore S2225
        }

        /// <summary>
        /// Converts the value at the specified index to an integer.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the value to convert.</param>
        /// <returns>The integer value, or 0 if conversion fails.</returns>
        /// <remarks>
        /// Numbers and numeric strings are converted to integers. Other types return 0.
        /// </remarks>
        public long ToInteger(IntPtr state, int index) => _toInteger(state, index);
        
        /// <summary>
        /// Converts the value at the specified index to a floating-point number.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the value to convert.</param>
        /// <returns>The floating-point value, or 0.0 if conversion fails.</returns>
        /// <remarks>
        /// Numbers and numeric strings are converted to doubles. Other types return 0.0.
        /// </remarks>
        public double ToNumber(IntPtr state, int index) => _toNumber(state, index);
        
        /// <summary>
        /// Converts the value at the specified index to a boolean.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the value to convert.</param>
        /// <returns>The boolean value according to Lua's truthiness rules.</returns>
        /// <remarks>
        /// In Lua, only nil and false are considered false. All other values (including 0) are true.
        /// </remarks>
        public bool ToBoolean(IntPtr state, int index) => _toBoolean(state, index);
        
        /// <summary>
        /// Gets the raw userdata pointer at the specified index.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the userdata value.</param>
        /// <returns>A pointer to the userdata, or IntPtr.Zero if the value is not userdata.</returns>
        /// <remarks>
        /// This returns the actual memory address of the userdata block, not its contents.
        /// </remarks>
        public IntPtr ToUserData(IntPtr state, int index) => _toUserData(state, index);

        /// <summary>
        /// Calls a function in protected mode (with error handling).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="nargs">Number of arguments to pass to the function.</param>
        /// <param name="nresults">Number of results expected from the function.</param>
        /// <returns>0 if successful, or an error code if the call failed.</returns>
        /// <remarks>
        /// <para>The function and its arguments should be on the stack before calling.</para>
        /// <para>On success, arguments and function are removed, and results are pushed.</para>
        /// <para>On error, the error message is pushed onto the stack.</para>
        /// </remarks>
        public int PCall(IntPtr state, int nargs, int nresults) => _pcall(state, nargs, nresults, 0, IntPtr.Zero, IntPtr.Zero);
        /// <summary>
        /// Calls a function without error protection.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="nargs">Number of arguments to pass to the function.</param>
        /// <param name="nresults">Number of results expected from the function.</param>
        /// <remarks>
        /// <para>Unlike PCall, this will terminate the program if an error occurs.</para>
        /// <para>Use PCall for safer function calls with error handling.</para>
        /// </remarks>
        public void Call(IntPtr state, int nargs, int nresults) => _call(state, nargs, nresults, IntPtr.Zero, IntPtr.Zero);
        /// <summary>
        /// Loads and compiles a Lua script from a string.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="script">The Lua script source code to compile.</param>
        /// <returns>0 if successful, or an error code if compilation failed.</returns>
        /// <remarks>
        /// <para>On success, the compiled function is pushed onto the stack.</para>
        /// <para>On error, an error message is pushed onto the stack.</para>
        /// <para>Use Call or PCall to execute the loaded function.</para>
        /// </remarks>
        public int LoadString(IntPtr state, string script) => _loadString(state, script);

        /// <summary>
        /// Gets a value from a table using the key at the top of the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the table to read from.</param>
        /// <returns>The type of the value that was pushed onto the stack.</returns>
        /// <remarks>
        /// <para>The key should be on top of the stack before calling this function.</para>
        /// <para>The key is popped from the stack, and the corresponding value is pushed.</para>
        /// <para>If the key doesn't exist, nil is pushed onto the stack.</para>
        /// </remarks>
        public int GetTable(IntPtr state, int index) => _getTable(state, index);

        /// <summary>
        /// Gets a field from a table and pushes its value onto the stack.
        /// </summary>
        /// <param name="state">The Lua state.</param>
        /// <param name="index">The stack index of the table.</param>
        /// <param name="key">The field name to retrieve.</param>
        /// <returns>The type of the pushed value.</returns>
        /// <remarks>
        /// <para>This function gets the value t[key] where t is the table at the given index.</para>
        /// <para>The retrieved value is pushed onto the stack.</para>
        /// </remarks>
        public int GetField(IntPtr state, int index, string key) => _getField(state, index, key);
        
        /// <summary>
        /// Sets a value in a table using the key and value at the top of the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the table to modify.</param>
        /// <remarks>
        /// <para>The value should be at the top of the stack, and the key just below it.</para>
        /// <para>Both the key and value are popped from the stack during this operation.</para>
        /// </remarks>
        public void SetTable(IntPtr state, int index) => _setTable(state, index);

        /// <summary>
        /// Rotates stack elements between the valid index and the top of the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The starting index for rotation (must be valid).</param>
        /// <param name="n">Number of positions to rotate. Positive rotates towards the top.</param>
        /// <remarks>
        /// <para>Elements are rotated n positions in the direction of the top.</para>
        /// <para>For n=1, the top element moves down and others move up one position.</para>
        /// </remarks>
        public void Rotate(IntPtr state, int index, int n) => _rotate(state, index, n);
        
        /// <summary>
        /// Copies a value from one stack position to another.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="fromIndex">The source stack index to copy from.</param>
        /// <param name="toIndex">The destination stack index to copy to.</param>
        /// <remarks>
        /// The value at the destination is overwritten without affecting other stack positions.
        /// </remarks>
        public void Copy(IntPtr state, int fromIndex, int toIndex) => _copy(state, fromIndex, toIndex);
        
        /// <summary>
        /// Gets the raw length of a value (for strings, tables, and userdata).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the value to measure.</param>
        /// <returns>The raw length: string length, table array size, or userdata size in bytes.</returns>
        /// <remarks>
        /// <para>For tables, this only counts the array part, not the hash part.</para>
        /// <para>This is equivalent to the # operator in Lua for most types.</para>
        /// </remarks>
        public int RawLen(IntPtr state, int index) => _rawLen(state, index);

        /// <summary>
        /// Removes the element at the specified index, shifting other elements down.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the element to remove.</param>
        /// <remarks>
        /// Elements above the removed index are shifted down to fill the gap.
        /// </remarks>
        public void Remove(IntPtr state, int index) { Rotate(state, index, -1); Pop(state, 1); }
        
        /// <summary>
        /// Moves the top element to the specified position, shifting other elements up.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index where the top element should be inserted.</param>
        /// <remarks>
        /// The top element is moved to the specified position, and elements at or above that position are shifted up.
        /// </remarks>
        public void Insert(IntPtr state, int index) => Rotate(state, index, 1);
        
        /// <summary>
        /// Replaces the value at the specified index with the value at the top of the stack.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the value to replace.</param>
        /// <remarks>
        /// The top element is copied to the specified index and then removed from the top.
        /// </remarks>
        public void Replace(IntPtr state, int index) { Copy(state, -1, index); Pop(state, 1); }

        /// <summary>
        /// Checks if the value at the specified index is heavy userdata (full userdata with metatable).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is heavy userdata.</returns>
        /// <remarks>
        /// Heavy userdata is allocated by Lua and can have metatables and finalizers.
        /// </remarks>
        public bool IsHeavyUserData(IntPtr state, int index) => Type(state, index) == LUA_TUSERDATA;
        
        /// <summary>
        /// Checks if the value at the specified index is light userdata (just a pointer).
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is light userdata.</returns>
        /// <remarks>
        /// Light userdata is just a C pointer value and cannot have metatables.
        /// </remarks>
        public bool IsLightUserData(IntPtr state, int index) => Type(state, index) == LUA_TLIGHTUSERDATA;
        
        /// <summary>
        /// Checks if the value at the specified index is a Cheat Engine object.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a CE object (heavy userdata).</returns>
        /// <remarks>
        /// CE objects are represented as heavy userdata containing pointers to native CE objects.
        /// </remarks>
        public bool IsCEObject(IntPtr state, int index) => IsHeavyUserData(state, index);

        /// <summary>
        /// Converts a userdata value to a CE object pointer.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the userdata containing the CE object.</param>
        /// <returns>A pointer to the native CE object, or IntPtr.Zero if conversion fails.</returns>
        /// <remarks>
        /// <para>This extracts the CE object from Lua userdata by reading the pointer.</para>
        /// <para>CE objects are stored as pointers inside userdata blocks.</para>
        /// </remarks>
        public IntPtr ToCEObject(IntPtr state, int index)
        {
            var userData = ToUserData(state, index);
            if (userData == IntPtr.Zero)
                return IntPtr.Zero;
            
            // CE objects are stored as pointers inside userdata
            return Marshal.ReadIntPtr(userData);
        }

        /// <summary>
        /// Pops a key from the stack and pushes a key-value pair from the table at the given index.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <param name="index">The stack index of the table to traverse.</param>
        /// <returns>Non-zero if there are more keys to iterate, zero if iteration is complete.</returns>
        /// <remarks>
        /// <para>This function is used for table iteration. The key should be at the top of the stack.</para>
        /// <para>If the function returns non-zero, both key and value are left on the stack (key at -2, value at -1).</para>
        /// <para>If the function returns zero, nothing is pushed and the stack is unchanged.</para>
        /// <para>To start iteration, push nil onto the stack before the first call.</para>
        /// <para>During iteration, do not call ToString on the key unless you know it is a string.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Table iteration example
        /// native.PushNil(state);  // First key
        /// while (native.Next(state, -2))  // -2 is table index
        /// {
        ///     // Key is at -2, value is at -1
        ///     var key = native.ToString(state, -2);
        ///     var value = native.ToString(state, -1);
        ///     Console.WriteLine($"{key} = {value}");
        ///     
        ///     native.Pop(state, 1);  // Remove value, keep key for next iteration
        /// }
        /// </code>
        /// </example>
        public int Next(IntPtr state, int index) => _next(state, index);
        #endregion
    }
}