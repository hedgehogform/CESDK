using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CESDK.Lua
{
    /// <summary>
    /// Low-level Lua API bindings - only the essential functions
    /// </summary>
    public class LuaNative
    {
        #region DLL Imports
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibraryA([MarshalAs(UnmanagedType.LPStr)] string fileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr module, [MarshalAs(UnmanagedType.LPStr)] string procedureName);
        #endregion

        /* Basic types: https://www.lua.org/source/5.3/lua.h.html

        #define LUA_TNONE               (-1)

        #define LUA_TNIL                0
        #define LUA_TBOOLEAN            1
        #define LUA_TLIGHTUSERDATA      2
        #define LUA_TNUMBER             3
        #define LUA_TSTRING             4
        #define LUA_TTABLE              5
        #define LUA_TFUNCTION           6
        #define LUA_TUSERDATA           7
        #define LUA_TTHREAD             8
        #define LUA_NUMTAGS             9

        */

        #region Delegates
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DelegateNewState();

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
        private delegate void DelegateSetField(IntPtr state, int index, [MarshalAs(UnmanagedType.LPStr)] string key);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateCreateTable(IntPtr state, int arraySize, int recordSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateType(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsString(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsNumber(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DelegateToString(IntPtr state, int index, IntPtr len);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long DelegateToInteger(IntPtr state, int index, IntPtr isnum);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate double DelegateToNumber(IntPtr state, int index, IntPtr isnum);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateToBoolean(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegatePCall(IntPtr state, int nargs, int nresults, int errfunc);

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
        private delegate bool DelegateIsBoolean(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsFunction(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int LuaCFunction(IntPtr state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegatePushCFunction(IntPtr L, LuaCFunction f);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsCFunction(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsTable(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsNil(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsThread(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsNone(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsNoneOrNil(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelegateIsUserData(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateRawLen(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateCall(IntPtr state, int nargs, int nresults);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DelegateToUserData(IntPtr state, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DelegateNext(IntPtr state, int index);
        #endregion

        #region Function Pointers
        private readonly IntPtr _luaState;
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
        private readonly DelegateSetField _setField;
        private readonly DelegateSetTable _setTable;
        private readonly DelegatePushValue _pushValue;
        private readonly DelegatePushNil _pushNil;
        private readonly DelegateRotate _rotate;
        private readonly DelegateCopy _copy;
        private readonly DelegateIsInteger _isInteger;
        private readonly DelegateIsBoolean _isBoolean;
        private readonly DelegateIsFunction _isFunction;
        private readonly DelegateIsCFunction _isCFunction;
        private readonly DelegatePushCFunction _pushCFunction;
        private readonly DelegateIsTable _isTable;
        private readonly DelegateIsNil _isNil;
        private readonly DelegateIsThread _isThread;
        private readonly DelegateIsNone _isNone;
        private readonly DelegateIsNoneOrNil _isNoneOrNil;
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
            var _newState = GetDelegate<DelegateNewState>(luaModule, "luaL_newstate");
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
            _pcall = GetDelegate<DelegatePCall>(luaModule, "lua_pcall");
            _loadString = GetDelegate<DelegateLoadString>(luaModule, "luaL_loadstring");
            _getTable = GetDelegate<DelegateGetTable>(luaModule, "lua_gettable");
            _getField = GetDelegate<DelegateGetField>(luaModule, "lua_getfield");
            _setField = GetDelegate<DelegateSetField>(luaModule, "lua_setfield");
            _setTable = GetDelegate<DelegateSetTable>(luaModule, "lua_settable");
            _pushValue = GetDelegate<DelegatePushValue>(luaModule, "lua_pushvalue");
            _pushNil = GetDelegate<DelegatePushNil>(luaModule, "lua_pushnil");
            _rotate = GetDelegate<DelegateRotate>(luaModule, "lua_rotate");
            _copy = GetDelegate<DelegateCopy>(luaModule, "lua_copy");
            _isInteger = GetDelegate<DelegateIsInteger>(luaModule, "lua_isinteger");
            _isBoolean = GetDelegate<DelegateIsBoolean>(luaModule, "lua_isboolean");
            _isFunction = GetDelegate<DelegateIsFunction>(luaModule, "lua_isfunction");
            _isCFunction = GetDelegate<DelegateIsCFunction>(luaModule, "lua_iscfunction");
            _pushCFunction = GetDelegate<DelegatePushCFunction>(luaModule, "lua_pushcfunction");
            _isTable = GetDelegate<DelegateIsTable>(luaModule, "lua_istable");
            _isNil = GetDelegate<DelegateIsNil>(luaModule, "lua_isnil");
            _isThread = GetDelegate<DelegateIsThread>(luaModule, "lua_isthread");
            _isNone = GetDelegate<DelegateIsNone>(luaModule, "lua_isnone");
            _isNoneOrNil = GetDelegate<DelegateIsNoneOrNil>(luaModule, "lua_isnoneornil");
            _isUserData = GetDelegate<DelegateIsUserData>(luaModule, "lua_isuserdata");
            _rawLen = GetDelegate<DelegateRawLen>(luaModule, "lua_rawlen");
            _call = GetDelegate<DelegateCall>(luaModule, "lua_call");
            _toUserData = GetDelegate<DelegateToUserData>(luaModule, "lua_touserdata");
            _next = GetDelegate<DelegateNext>(luaModule, "lua_next");

            _luaState = _newState();  // Create a new Lua state
            if (_luaState == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create Lua state");
        }

        /// <summary>
        /// Gets a delegate for a function in the Lua module.
        /// </summary>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        /// <param name="module">The Lua module handle.</param>
        /// <param name="functionName">The name of the function to retrieve.</param>
        /// <returns>A delegate for the specified function.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static T GetDelegate<T>(IntPtr module, string functionName) where T : class
        {
            var address = GetProcAddress(module, functionName);
            if (address == IntPtr.Zero)
                throw new InvalidOperationException($"Could not find function: {functionName}");
            return Marshal.GetDelegateForFunctionPointer<T>(address);
        }

        private readonly List<LuaCFunction> _keepAlive = [];

        #region Public API

        /// <summary>
        /// Gets the number of elements currently on the Lua stack.
        /// </summary>
        /// <returns>The number of elements on the stack (0 means empty stack).</returns>
        /// <remarks>
        /// This is equivalent to the index of the top element. A stack with one element has top = 1.
        /// </remarks>
        public int GetTop() => _getTop(_luaState);

        /// <summary>
        /// Sets the stack top to the specified index, effectively resizing the stack.
        /// </summary>
        /// <param name="index">The new top index. Use negative values to count from current top.</param>
        /// <remarks>
        /// <para>If the new top is larger than the old one, new nil values are pushed.</para>
        /// <para>If the new top is smaller, elements above the new top are discarded.</para>
        /// <para>Use index 0 to clear the entire stack.</para>
        /// </remarks>
        public void SetTop(int index) => _setTop(_luaState, index);

        /// <summary>
        /// Pops (removes) the specified number of elements from the top of the stack.
        /// </summary>
        /// <param name="count">The number of elements to remove from the stack.</param>
        /// <remarks>
        /// This is equivalent to calling SetTop with -count-1.
        /// </remarks>
        public void Pop(int count) => SetTop(-count - 1);

        /// <summary>
        /// Pushes the value of the global variable onto the stack.
        /// </summary>
        /// <param name="name">The name of the global variable to retrieve.</param>
        /// <returns>The type of the value that was pushed onto the stack.</returns>
        /// <remarks>
        /// If the global variable doesn't exist, nil is pushed onto the stack.
        /// </remarks>
        public int GetGlobal(string name) => _getGlobal(_luaState, name);

        /// <summary>
        /// Sets the value at the top of the stack as a global variable.
        /// </summary>
        /// <param name="name">The name of the global variable to set.</param>
        /// <remarks>
        /// The value at the top of the stack is popped and assigned to the global variable.
        /// </remarks>
        public void SetGlobal(string name) => _setGlobal(_luaState, name);

        /// <summary>
        /// Pushes a string value onto the Lua stack.
        /// </summary>
        /// <param name="value">The string value to push.</param>
        /// <remarks>
        /// Lua creates an internal copy of the string, so the original can be safely modified or freed.
        /// </remarks>
        public void PushString(string value) => _pushString(_luaState, value);

        /// <summary>
        /// Pushes an integer value onto the Lua stack.
        /// </summary>
        /// <param name="value">The integer value to push.</param>
        public void PushInteger(long value) => _pushInteger(_luaState, value);

        /// <summary>
        /// Pushes a floating-point number onto the Lua stack.
        /// </summary>
        /// <param name="value">The floating-point value to push.</param>
        public void PushNumber(double value) => _pushNumber(_luaState, value);

        /// <summary>
        /// Pushes a boolean value onto the Lua stack.
        /// </summary>
        /// <param name="value">The boolean value to push.</param>
        public void PushBoolean(bool value) => _pushBoolean(_luaState, value);

        /// <summary>
        /// Pushes a nil value onto the Lua stack.
        /// </summary>
        public void PushNil() => _pushNil(_luaState);

        /// <summary>
        /// Pushes a copy of the value at the specified index onto the top of the stack.
        /// </summary>
        /// <param name="index">The stack index of the value to copy. Can be negative to count from top.</param>
        /// <remarks>
        /// The original value remains at its position - this creates a copy on top of the stack.
        /// </remarks>
        public void PushValue(int index) => _pushValue(_luaState, index);

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack.
        /// </summary>
        /// <param name="arraySize">Expected number of array elements (optimization hint).</param>
        /// <param name="recordSize">Expected number of non-array elements (optimization hint).</param>
        /// <remarks>
        /// The size parameters are optimization hints to pre-allocate the table structure.
        /// </remarks>
        public void CreateTable(int arraySize = 0, int recordSize = 0) => _createTable(_luaState, arraySize, recordSize);

        /// <summary>
        /// Gets the type of the value at the specified stack index.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>A Lua type constant (LUA_T* values).</returns>
        public int Type(int index) => _type(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is a string (or convertible to string).
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a string or number (numbers are convertible to strings).</returns>
        public bool IsString(int index) => _isString(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is a number (or convertible to number).
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a number or numeric string.</returns>
        public bool IsNumber(int index) => _isNumber(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is an integer.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is an integer number.</returns>
        public bool IsInteger(int index) => _isInteger(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is a boolean.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a boolean.</returns>
        public bool IsBoolean(int index) => _isBoolean(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is a function (Lua or C).
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is any kind of function.</returns>
        public bool IsFunction(int index) => _isFunction(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is a C function.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a C function (not a Lua function).</returns>
        public bool IsCFunction(int index) => _isCFunction(_luaState, index);

        public void PushCFunction(LuaCFunction function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _pushCFunction(_luaState, function);
        }

        /// <summary>
        /// Checks if the value at the specified index is userdata.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is userdata (light or heavy).</returns>
        public bool IsUserData(int index) => _isUserData(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is a table.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a table.</returns>
        public bool IsTable(int index) => _isTable(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is nil.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is nil.</returns>
        public bool IsNil(int index) => _isNil(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is a thread (coroutine).
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a thread.</returns>
        public bool IsThread(int index) => _isThread(_luaState, index);

        /// <summary>
        /// Checks if the specified index is not valid (beyond the stack).
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the index refers to a non-existent stack position.</returns>
        public bool IsNone(int index) => _isNone(_luaState, index);

        /// <summary>
        /// Checks if the value at the specified index is nil or the index is invalid.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is nil or the index is invalid.</returns>
        public bool IsNoneOrNil(int index) => _isNoneOrNil(_luaState, index);

        /// <summary>
        /// Converts the value at the specified index to a string.
        /// </summary>
        /// <param name="index">The stack index of the value to convert.</param>
        /// <returns>The string representation of the value, or null if conversion fails.</returns>
        /// <remarks>
        /// <para>Numbers and strings are converted directly. Other types may not be convertible.</para>
        /// <para>This function may change the actual value on the stack (numbers become strings).</para>
        /// </remarks>
        public string ToString(int index)
        {
            var ptr = _toString(_luaState, index, IntPtr.Zero);
            return ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) : string.Empty;
        }

        /// <summary>
        /// Converts the value at the specified index to an integer.
        /// </summary>
        /// <param name="index">The stack index of the value to convert.</param>
        /// <returns>The integer value, or 0 if conversion fails.</returns>
        /// <remarks>
        /// Numbers and numeric strings are converted to integers. Other types return 0.
        /// </remarks>
        public int ToInteger(int index) => (int)_toInteger(_luaState, index, IntPtr.Zero);

        /// <summary>
        /// Converts the value at the specified index to a floating-point number.
        /// </summary>
        /// <param name="index">The stack index of the value to convert.</param>
        /// <returns>The floating-point value, or 0.0 if conversion fails.</returns>
        /// <remarks>
        /// Numbers and numeric strings are converted to doubles. Other types return 0.0.
        /// </remarks>
        public double ToNumber(int index) => _toNumber(_luaState, index, IntPtr.Zero);

        /// <summary>
        /// Converts the value at the specified index to a boolean.
        /// </summary>
        /// <param name="index">The stack index of the value to convert.</param>
        /// <returns>The boolean value according to Lua's truthiness rules.</returns>
        /// <remarks>
        /// In Lua, only nil and false are considered false. All other values (including 0) are true.
        /// </remarks>
        public bool ToBoolean(int index) => _toBoolean(_luaState, index);

        /// <summary>
        /// Gets the raw userdata pointer at the specified index.
        /// </summary>
        /// <param name="index">The stack index of the userdata value.</param>
        /// <returns>A pointer to the userdata, or IntPtr.Zero if the value is not userdata.</returns>
        /// <remarks>
        /// This returns the actual memory address of the userdata block, not its contents.
        /// </remarks>
        public IntPtr ToUserData(int index) => _toUserData(_luaState, index);

        /// <summary>
        /// Calls a function in protected mode (with error handling).
        /// </summary>
        /// <param name="nargs">Number of arguments to pass to the function.</param>
        /// <param name="nresults">Number of results expected from the function.</param>
        /// <returns>0 if successful, or an error code if the call failed.</returns>
        /// <remarks>
        /// <para>The function and its arguments should be on the stack before calling.</para>
        /// <para>On success, arguments and function are removed, and results are pushed.</para>
        /// <para>On error, the error message is pushed onto the stack.</para>
        /// </remarks>
        public int PCall(int nargs, int nresults) => _pcall(_luaState, nargs, nresults, 0);
        /// <summary>
        /// Calls a function without error protection.
        /// </summary>
        /// <param name="nargs">Number of arguments to pass to the function.</param>
        /// <param name="nresults">Number of results expected from the function.</param>
        /// <remarks>
        /// <para>Unlike PCall, this will terminate the program if an error occurs.</para>
        /// <para>Use PCall for safer function calls with error handling.</para>
        /// </remarks>
        public void Call(int nargs, int nresults) => _call(_luaState, nargs, nresults);
        /// <summary>
        /// Loads and compiles a Lua script from a string.
        /// </summary>
        /// <param name="script">The Lua script source code to compile.</param>
        /// <returns>0 if successful, or an error code if compilation failed.</returns>
        /// <remarks>
        /// <para>On success, the compiled function is pushed onto the stack.</para>
        /// <para>On error, an error message is pushed onto the stack.</para>
        /// <para>Use Call or PCall to execute the loaded function.</para>
        /// </remarks>
        public int LoadString(string script) => _loadString(_luaState, script);

        /// <summary>
        /// Gets a value from a table using the key at the top of the stack.
        /// </summary>
        /// <param name="index">The stack index of the table to read from.</param>
        /// <returns>The type of the value that was pushed onto the stack.</returns>
        /// <remarks>
        /// <para>The key should be on top of the stack before calling this function.</para>
        /// <para>The key is popped from the stack, and the corresponding value is pushed.</para>
        /// <para>If the key doesn't exist, nil is pushed onto the stack.</para>
        /// </remarks>
        public int GetTable(int index) => _getTable(_luaState, index);

        /// <summary>
        /// Gets a field from a table and pushes its value onto the stack.
        /// </summary>
        /// <param name="index">The stack index of the table.</param>
        /// <param name="key">The field name to retrieve.</param>
        /// <returns>The type of the pushed value.</returns>
        /// <remarks>
        /// <para>This function gets the value t[key] where t is the table at the given index.</para>
        /// <para>The retrieved value is pushed onto the stack.</para>
        /// </remarks>
        public int GetField(int index, string key) => _getField(_luaState, index, key);

        /// <summary>
        /// Sets a field in a table.
        /// </summary>
        /// <param name="index">The stack index of the table.</param>
        /// <param name="key">The field name to set.</param>
        public void SetField(int index, string key) => _setField(_luaState, index, key);
        /// <summary>
        /// Sets a value in a table using the key and value at the top of the stack.
        /// </summary>
        /// <param name="index">The stack index of the table to modify.</param>
        /// <remarks>
        /// <para>The value should be at the top of the stack, and the key just below it.</para>
        /// <para>Both the key and value are popped from the stack during this operation.</para>
        /// </remarks>
        public void SetTable(int index) => _setTable(_luaState, index);

        /// <summary>
        /// Rotates stack elements between the valid index and the top of the stack.
        /// </summary>
        /// <param name="index">The starting index for rotation (must be valid).</param>
        /// <param name="n">Number of positions to rotate. Positive rotates towards the top.</param>
        /// <remarks>
        /// <para>Elements are rotated n positions in the direction of the top.</para>
        /// <para>For n=1, the top element moves down and others move up one position.</para>
        /// </remarks>
        public void Rotate(int index, int n) => _rotate(_luaState, index, n);

        /// <summary>
        /// Copies a value from one stack position to another.
        /// </summary>
        /// <param name="fromIndex">The source stack index to copy from.</param>
        /// <param name="toIndex">The destination stack index to copy to.</param>
        /// <remarks>
        /// The value at the destination is overwritten without affecting other stack positions.
        /// </remarks>
        public void Copy(int fromIndex, int toIndex) => _copy(_luaState, fromIndex, toIndex);

        /// <summary>
        /// Gets the raw length of a value (for strings, tables, and userdata).
        /// </summary>
        /// <param name="index">The stack index of the value to measure.</param>
        /// <returns>The raw length: string length, table array size, or userdata size in bytes.</returns>
        /// <remarks>
        /// <para>For tables, this only counts the array part, not the hash part.</para>
        /// <para>This is equivalent to the # operator in Lua for most types.</para>
        /// </remarks>
        public int RawLen(int index) => _rawLen(_luaState, index);

        /// <summary>
        /// Removes the element at the specified index, shifting other elements down.
        /// </summary>
        /// <param name="index">The stack index of the element to remove.</param>
        /// <remarks>
        /// Elements above the removed index are shifted down to fill the gap.
        /// </remarks>
        public void Remove(int index) { Rotate(index, -1); Pop(1); }

        /// <summary>
        /// Moves the top element to the specified position, shifting other elements up.
        /// </summary>
        /// <param name="index">The stack index where the top element should be inserted.</param>
        /// <remarks>
        /// The top element is moved to the specified position, and elements at or above that position are shifted up.
        /// </remarks>
        public void Insert(int index) => Rotate(index, 1);

        /// <summary>
        /// Replaces the value at the specified index with the value at the top of the stack.
        /// </summary>
        /// <param name="index">The stack index of the value to replace.</param>
        /// <remarks>
        /// The top element is copied to the specified index and then removed from the top.
        /// </remarks>
        public void Replace(int index) { Copy(-1, index); Pop(1); }

        /// <summary>
        /// Checks if the value at the specified index is heavy userdata (full userdata with metatable).
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is heavy userdata.</returns>
        /// <remarks>
        /// Heavy userdata is allocated by Lua and can have metatables and finalizers.
        /// </remarks>
        public bool IsHeavyUserData(int index) => Type(index) == 7; // LUA_TUSERDATA is 7

        /// <summary>
        /// Checks if the value at the specified index is light userdata (just a pointer).
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is light userdata.</returns>
        /// <remarks>
        /// Light userdata is just a C pointer value and cannot have metatables.
        /// </remarks>
        public bool IsLightUserData(int index) => Type(index) == 2; // LUA_TLIGHTUSERDATA is 2

        /// <summary>
        /// Checks if the value at the specified index is a Cheat Engine object.
        /// </summary>
        /// <param name="index">The stack index to check.</param>
        /// <returns>True if the value is a CE object (heavy userdata).</returns>
        /// <remarks>
        /// CE objects are represented as heavy userdata containing pointers to native CE objects.
        /// </remarks>
        public bool IsCEObject(int index) => IsHeavyUserData(index);

        /// <summary>
        /// Converts a userdata value to a CE object pointer.
        /// </summary>
        /// <param name="index">The stack index of the userdata containing the CE object.</param>
        /// <returns>A pointer to the native CE object, or IntPtr.Zero if conversion fails.</returns>
        /// <remarks>
        /// <para>This extracts the CE object from Lua userdata by reading the pointer.</para>
        /// <para>CE objects are stored as pointers inside userdata blocks.</para>
        /// </remarks>
        public IntPtr ToCEObject(int index)
        {
            var userData = ToUserData(index);
            if (userData == IntPtr.Zero)
                return IntPtr.Zero;

            // CE objects are stored as pointers inside userdata
            return Marshal.ReadIntPtr(userData);
        }

        /// <summary>
        /// Pops a key from the stack and pushes a key-value pair from the table at the given index.
        /// </summary>
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
        /// while (native.Next(-2))  // -2 is table index
        /// {
        ///     // Key is at -2, value is at -1
        ///     var key = native.ToString(-2);
        ///     var value = native.ToString(-1);
        ///     Console.WriteLine($"{key} = {value}");
        ///
        ///     native.Pop(1);  // Remove value, keep key for next iteration
        /// }
        /// </code>
        /// </example>
        public int Next(int index) => _next(_luaState, index);
        #endregion

        #region Convenience Methods

        /// <summary>
        /// Calls a Lua function that returns a boolean value.
        /// </summary>
        /// <param name="functionName">Name of the Lua function to call</param>
        /// <returns>The boolean value returned by the function</returns>
        /// <exception cref="InvalidOperationException">Thrown when function doesn't exist or call fails</exception>
        public bool CallBooleanFunction(string functionName)
        {
            GetGlobal(functionName);
            if (!IsFunction(-1))
            {
                Pop(1);
                throw new InvalidOperationException($"{functionName} function not available");
            }

            var result = PCall(0, 1);
            if (result != 0)
            {
                var error = ToString(-1);
                Pop(1);
                throw new InvalidOperationException($"{functionName}() call failed: {error}");
            }

            var returnValue = ToBoolean(-1);
            Pop(1);
            return returnValue;
        }

        /// <summary>
        /// Calls a Lua function that returns an integer value.
        /// </summary>
        /// <param name="functionName">Name of the Lua function to call</param>
        /// <returns>The integer value returned by the function</returns>
        /// <exception cref="InvalidOperationException">Thrown when function doesn't exist or call fails</exception>
        public int CallIntegerFunction(string functionName)
        {
            GetGlobal(functionName);
            if (!IsFunction(-1))
            {
                Pop(1);
                throw new InvalidOperationException($"{functionName} function not available");
            }

            var result = PCall(0, 1);
            if (result != 0)
            {
                var error = ToString(-1);
                Pop(1);
                throw new InvalidOperationException($"{functionName}() call failed: {error}");
            }

            var returnValue = ToInteger(-1);
            Pop(1);
            return returnValue;
        }

        /// <summary>
        /// Loads and executes a Lua script string in one step.
        /// </summary>
        /// <param name="script">The Lua code to execute.</param>
        /// <exception cref="InvalidOperationException">Thrown if compilation or execution fails.</exception>
        public void DoString(string script)
        {
            // Load the Lua string
            int loadResult = LoadString(script);
            if (loadResult != 0) // non-zero means error
            {
                var error = ToString(-1);
                Pop(1);
                throw new InvalidOperationException($"Failed to load Lua script: {error}");
            }

            // Execute the loaded chunk
            int callResult = PCall(0, -1); // 0 args, multiple results (-1)
            if (callResult != 0) // non-zero means runtime error
            {
                var error = ToString(-1);
                Pop(1);
                throw new InvalidOperationException($"Error running Lua script: {error}");
            }
        }


        /// <summary>
        /// Registers a C# function as a global Lua function.
        /// </summary>
        public void RegisterFunction(string name, Action action)
        {
            // wrap user action into a LuaCFunction
            int wrapper(IntPtr state)
            {
                action(); // call the user’s void method
                return 0; // no return values pushed
            }

            _keepAlive.Add(wrapper); // prevent GC

            // equivalent to lua_register(L,name,f)
            PushCFunction(wrapper);
            SetGlobal(name);
        }

        #endregion
    }
}