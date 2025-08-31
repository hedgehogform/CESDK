using System;
using System.Runtime.InteropServices;

namespace CESDK.Lua
{
    /// <summary>
    /// Provides a modern, clean interface to Cheat Engine's Lua scripting environment.
    /// This class offers high-level methods for executing Lua scripts, registering C# functions,
    /// and managing global variables while maintaining access to low-level Lua operations.
    /// </summary>
    /// <remarks>
    /// <para>The LuaEngine automatically handles Lua state management, error handling, and memory cleanup.</para>
    /// <para>For advanced scenarios requiring direct Lua stack manipulation, use the <see cref="Native"/> property.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Execute simple Lua code
    /// Lua.Execute("print('Hello World!')");
    /// 
    /// // Register a C# function
    /// Lua.RegisterFunction("myFunction", () => {
    ///     Console.WriteLine("Called from Lua!");
    ///     return 1;
    /// });
    /// 
    /// // Work with global variables
    /// Lua.SetGlobalString("playerName", "John");
    /// var name = Lua.GetGlobalString("playerName");
    /// </code>
    /// </example>
    public class LuaEngine
    {
        private readonly Core.TExportedFunctions _exports;
        private readonly LuaNative _native;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr DelegateGetLuaState();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr DelegateLuaRegister(IntPtr state, [MarshalAs(UnmanagedType.LPStr)] string name, LuaFunctionRaw function);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void DelegateLuaPushClassInstance(IntPtr state, IntPtr instance);

        private readonly DelegateGetLuaState _getLuaState;
        private readonly DelegateLuaRegister _luaRegister;
        private readonly DelegateLuaPushClassInstance _luaPushClassInstance;

        /// <summary>
        /// Represents a C# function that can be called from Lua with access to parameters.
        /// </summary>
        /// <param name="parameters">Parameters passed from Lua as strings.</param>
        /// <returns>The return value to pass back to Lua, or null for no return value.</returns>
        public delegate string? LuaFunction(string[] parameters);

        /// <summary>
        /// Represents a raw C# function that can be called from Lua with direct state access.
        /// </summary>
        /// <param name="state">The Lua state pointer.</param>
        /// <returns>The number of return values pushed onto the Lua stack.</returns>
        public delegate int LuaFunctionRaw(IntPtr state);

        internal LuaEngine(Core.TExportedFunctions exports)
        {
            _exports = exports;
            _getLuaState = Marshal.GetDelegateForFunctionPointer<DelegateGetLuaState>(exports.GetLuaState);
            _luaRegister = Marshal.GetDelegateForFunctionPointer<DelegateLuaRegister>(exports.LuaRegister);
            _luaPushClassInstance = Marshal.GetDelegateForFunctionPointer<DelegateLuaPushClassInstance>(exports.LuaPushClassInstance);
            _native = new LuaNative();
        }

        /// <summary>
        /// Gets the current Lua state pointer for advanced operations.
        /// </summary>
        /// <value>A pointer to the current Lua state managed by Cheat Engine.</value>
        /// <remarks>
        /// <para>This property provides direct access to the Lua state for advanced users who need
        /// to perform operations not covered by the high-level API.</para>
        /// <para>Use this carefully as improper manipulation can crash Cheat Engine.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// IntPtr state = Lua.State;
        /// // Use with Native methods for direct Lua API access
        /// Lua.Native.PushString(state, "direct access");
        /// </code>
        /// </example>
        public IntPtr State => _getLuaState();

        /// <summary>
        /// Executes a Lua script in the Cheat Engine environment.
        /// </summary>
        /// <param name="script">The Lua script code to execute. Cannot be null or empty.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="script"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the script fails to parse or execute.</exception>
        /// <remarks>
        /// <para>This method compiles and executes the provided Lua script in CE's environment.</para>
        /// <para>You have access to all of Cheat Engine's Lua functions and objects (e.g., MainForm, addresslist, etc.).</para>
        /// <para>The script executes in the same context as CE's built-in Lua console.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Simple script execution
        /// Lua.Execute("print('Hello from C# plugin!')");
        /// 
        /// // Multi-line script with CE objects
        /// Lua.Execute(@"
        ///     local addressList = getAddressList()
        ///     local entry = addressList.createMemoryRecord()
        ///     entry.Description = 'Health'
        ///     entry.Address = 0x12345678
        ///     entry.Type = vtDword
        /// ");
        /// 
        /// // Execute script that calls registered C# functions
        /// Lua.Execute("local result = myCustomFunction(42, 'test')");
        /// </code>
        /// </example>
        public void Execute(string script)
        {
            if (string.IsNullOrEmpty(script))
                throw new ArgumentException("Script cannot be null or empty", nameof(script));

            var state = State;
            try
            {
                var result = _native.LoadString(state, script);
                if (result == 0)
                {
                    result = _native.PCall(state, 0, -1);
                    if (result != 0)
                    {
                        var error = _native.ToString(state, -1);
                        _native.Pop(state, 1);
                        throw new InvalidOperationException($"Lua execution failed: {error}");
                    }
                }
                else
                {
                    var error = _native.ToString(state, -1);
                    _native.Pop(state, 1);
                    throw new InvalidOperationException($"Lua parse failed: {error}");
                }
            }
            catch
            {
                _native.SetTop(state, 0);
                throw;
            }
        }

        /// <summary>
        /// Registers a simple C# function to be callable from Lua scripts.
        /// </summary>
        /// <param name="name">The name to register the function as in Lua. Cannot be null or empty.</param>
        /// <param name="function">The C# function to register. Called with no parameters.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null.</exception>
        /// <remarks>
        /// <para>Use this overload for simple functions that don't need parameters from Lua.</para>
        /// <para>The function can perform any action but cannot return values to Lua directly.</para>
        /// <para>To return values to Lua, use <see cref="SetGlobalString"/> or <see cref="SetGlobalInteger"/>.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Register a simple logging function
        /// Lua.RegisterFunction("log_message", () => {
        ///     Console.WriteLine("Message logged from Lua");
        /// });
        /// 
        /// // Register a function that sets a global variable for return
        /// Lua.RegisterFunction("get_player_health", () => {
        ///     var health = GetCurrentPlayerHealth();
        ///     Lua.SetGlobalString("result", health.ToString());
        /// });
        /// 
        /// // Call from Lua:
        /// // log_message()
        /// // get_player_health()
        /// // print(result)
        /// </code>
        /// </example>
        public void RegisterFunction(string name, Action function)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Function name cannot be null or empty", nameof(name));
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            LuaFunctionRaw wrapper = (state) =>
            {
                function();
                return 0;
            };
            _luaRegister(State, name, wrapper);
        }

        /// <summary>
        /// Registers a C# function that can receive parameters and return values from Lua scripts.
        /// </summary>
        /// <param name="name">The name to register the function as in Lua. Cannot be null or empty.</param>
        /// <param name="function">The C# function that receives parameters as strings and can return a string value.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null.</exception>
        /// <remarks>
        /// <para>Parameters from Lua are automatically converted to strings and passed to your function.</para>
        /// <para>Return a string value to pass it back to Lua, or null for no return value.</para>
        /// <para>All Lua types (numbers, booleans, tables) are converted to their string representations.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Register a function that takes parameters and returns values
        /// Lua.RegisterFunction("add_numbers", (parameters) => {
        ///     if (parameters.Length >= 2) {
        ///         if (int.TryParse(parameters[0], out int a) &&
        ///             int.TryParse(parameters[1], out int b)) {
        ///             return (a + b).ToString();
        ///         }
        ///     }
        ///     return "0"; // Default return value
        /// });
        /// 
        /// // Register a string manipulation function
        /// Lua.RegisterFunction("reverse_string", (parameters) => {
        ///     if (parameters.Length > 0) {
        ///         return new string(parameters[0].Reverse().ToArray());
        ///     }
        ///     return null; // No return value
        /// });
        /// 
        /// // Call from Lua:
        /// // local sum = add_numbers(10, 20) -- Returns "30"
        /// // local reversed = reverse_string("hello") -- Returns "olleh"
        /// </code>
        /// </example>
        public void RegisterFunction(string name, LuaFunction function)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Function name cannot be null or empty", nameof(name));
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            LuaFunctionRaw wrapper = (state) =>
            {
                var paramCount = _native.GetTop(state);
                var parameters = new string[paramCount];
                
                // Convert all parameters to strings
                for (int i = 1; i <= paramCount; i++)
                {
                    parameters[i - 1] = _native.ToString(state, i) ?? "";
                }

                var result = function(parameters);
                
                if (result != null)
                {
                    _native.PushString(state, result);
                    return 1; // One return value
                }
                
                return 0; // No return value
            };
            
            _luaRegister(State, name, wrapper);
        }

        /// <summary>
        /// Registers a C# function with full Lua state access to be callable from Lua scripts.
        /// </summary>
        /// <param name="name">The name to register the function as in Lua. Cannot be null or empty.</param>
        /// <param name="function">The C# function that receives the Lua state pointer. Must return the number of values pushed to Lua stack.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null.</exception>
        /// <remarks>
        /// <para>Use this method for advanced functions that need direct access to the Lua stack.</para>
        /// <para>The function receives the Lua state pointer and can use Native methods to manipulate the stack.</para>
        /// <para>Parameters passed from Lua are available on the stack, with the first parameter at index 1.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Register a function that takes parameters and returns values
        /// Lua.RegisterRawFunction("add_numbers", (IntPtr state) => {
        ///     var native = Lua.Native;
        ///     
        ///     // Read parameters from Lua stack
        ///     if (native.GetTop(state) >= 2) {
        ///         var a = native.ToInteger(state, 1);
        ///         var b = native.ToInteger(state, 2);
        ///         var result = a + b;
        ///         
        ///         // Push result back to Lua
        ///         native.PushInteger(state, result);
        ///         return 1; // One return value
        ///     }
        ///     
        ///     return 0; // No return value if insufficient parameters
        /// });
        /// 
        /// // Call from Lua:
        /// // local sum = add_numbers(10, 20) -- Returns 30
        /// </code>
        /// </example>
        public void RegisterRawFunction(string name, LuaFunctionRaw function)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Function name cannot be null or empty", nameof(name));
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _luaRegister(State, name, function);
        }

        /// <summary>
        /// Retrieves the value of a global Lua variable as a string.
        /// </summary>
        /// <param name="variableName">The name of the global variable to retrieve. Cannot be null or empty.</param>
        /// <returns>The string value of the variable, or <see langword="null"/> if the variable doesn't exist or is not a string.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="variableName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a Lua error occurs during retrieval.</exception>
        /// <remarks>
        /// <para>This method safely retrieves global variables without affecting the Lua stack state.</para>
        /// <para>If the variable exists but is not a string type, this method returns <see langword="null"/>.</para>
        /// <para>Use this method to read configuration values, process names, or other string data from Lua.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Set a value in Lua and read it back
        /// Lua.Execute("targetProcess = 'notepad.exe'");
        /// var processName = Lua.GetGlobalString("targetProcess"); // Returns "notepad.exe"
        /// 
        /// // Check if variable exists
        /// var nonExistent = Lua.GetGlobalString("doesNotExist"); // Returns null
        /// 
        /// // Reading CE built-in variables
        /// Lua.Execute("currentFile = MainForm.OpenDialog1.FileName");
        /// var fileName = Lua.GetGlobalString("currentFile");
        /// </code>
        /// </example>
        public string GetGlobalString(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(variableName));

            var state = State;
            try
            {
                _native.GetGlobal(state, variableName);
                if (_native.IsString(state, -1))
                {
                    var result = _native.ToString(state, -1);
                    _native.Pop(state, 1);
                    return result;
                }
                _native.Pop(state, 1);
                return null;
            }
            catch
            {
                _native.SetTop(state, 0);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the value of a global Lua variable as an integer.
        /// </summary>
        /// <param name="variableName">The name of the global variable to retrieve. Cannot be null or empty.</param>
        /// <returns>The integer value of the variable, or <see langword="null"/> if the variable doesn't exist or is not a number.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="variableName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a Lua error occurs during retrieval.</exception>
        /// <remarks>
        /// <para>This method safely retrieves global variables without affecting the Lua stack state.</para>
        /// <para>If the variable exists but is not a number type, this method returns <see langword="null"/>.</para>
        /// <para>Lua numbers (both integer and floating-point) are converted to 64-bit integers.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Set values in Lua and read them back
        /// Lua.Execute("playerLevel = 42");
        /// Lua.Execute("playerHealth = 100.5"); // Will be truncated to 100
        /// 
        /// var level = Lua.GetGlobalInteger("playerLevel"); // Returns 42
        /// var health = Lua.GetGlobalInteger("playerHealth"); // Returns 100
        /// 
        /// // Check if variable exists or is numeric
        /// var missing = Lua.GetGlobalInteger("doesNotExist"); // Returns null
        /// 
        /// // Reading memory addresses
        /// Lua.Execute("baseAddress = getAddress('player_base')");
        /// var address = Lua.GetGlobalInteger("baseAddress");
        /// </code>
        /// </example>
        public long? GetGlobalInteger(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(variableName));

            var state = State;
            try
            {
                _native.GetGlobal(state, variableName);
                if (_native.IsNumber(state, -1))
                {
                    var result = _native.ToInteger(state, -1);
                    _native.Pop(state, 1);
                    return result;
                }
                _native.Pop(state, 1);
                return null;
            }
            catch
            {
                _native.SetTop(state, 0);
                throw;
            }
        }

        /// <summary>
        /// Sets a global Lua variable to a string value.
        /// </summary>
        /// <param name="variableName">The name of the global variable to set. Cannot be null or empty.</param>
        /// <param name="value">The string value to assign. Null values are converted to empty strings.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="variableName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a Lua error occurs during assignment.</exception>
        /// <remarks>
        /// <para>This method creates or updates a global variable in the Lua environment.</para>
        /// <para>The variable becomes accessible from all Lua scripts and the CE console.</para>
        /// <para>Null values are automatically converted to empty strings for safety.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Set configuration values
        /// Lua.SetGlobalString("pluginVersion", "1.2.3");
        /// Lua.SetGlobalString("authorName", "John Doe");
        /// 
        /// // Use in Lua scripts
        /// Lua.Execute(@"
        ///     print('Plugin: ' .. pluginVersion)
        ///     print('Author: ' .. authorName)
        /// ");
        /// 
        /// // Set values that other functions can read
        /// Lua.SetGlobalString("lastScanResult", "Found 42 matches");
        /// </code>
        /// </example>
        public void SetGlobalString(string variableName, string value)
        {
            if (string.IsNullOrEmpty(variableName))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(variableName));

            var state = State;
            try
            {
                _native.PushString(state, value ?? "");
                _native.SetGlobal(state, variableName);
            }
            catch
            {
                _native.SetTop(state, 0);
                throw;
            }
        }

        /// <summary>
        /// Sets a global Lua variable to an integer value.
        /// </summary>
        /// <param name="variableName">The name of the global variable to set. Cannot be null or empty.</param>
        /// <param name="value">The integer value to assign.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="variableName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a Lua error occurs during assignment.</exception>
        /// <remarks>
        /// <para>This method creates or updates a global variable in the Lua environment.</para>
        /// <para>The variable becomes accessible from all Lua scripts and the CE console as a Lua number.</para>
        /// <para>Large integers maintain their precision in Lua's number system.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Set numeric configuration
        /// Lua.SetGlobalString("maxRetries", 5);
        /// Lua.SetGlobalString("baseAddress", 0x12345678L);
        /// 
        /// // Use in calculations
        /// Lua.Execute(@"
        ///     local retries = 0
        ///     while retries < maxRetries do
        ///         -- Attempt operation
        ///         retries = retries + 1
        ///     end
        /// ");
        /// 
        /// // Store scan results
        /// var matchCount = PerformMemoryScan();
        /// Lua.SetGlobalString("matchCount", matchCount);
        /// </code>
        /// </example>
        public void SetGlobalInteger(string variableName, long value)
        {
            if (string.IsNullOrEmpty(variableName))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(variableName));

            var state = State;
            try
            {
                _native.PushInteger(state, value);
                _native.SetGlobal(state, variableName);
            }
            catch
            {
                _native.SetTop(state, 0);
                throw;
            }
        }


        /// <summary>
        /// Gets access to low-level Lua API functions for advanced scenarios.
        /// </summary>
        /// <value>A <see cref="LuaNative"/> instance providing direct access to Lua C API functions.</value>
        /// <remarks>
        /// <para>Use this property when you need direct control over the Lua stack or want to perform
        /// operations not covered by the high-level LuaEngine methods.</para>
        /// <para>Direct stack manipulation requires careful management to avoid corrupting the Lua state.</para>
        /// <para>Always ensure proper stack cleanup after using native functions.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var native = Lua.Native;
        /// var state = Lua.State;
        /// 
        /// // Direct stack manipulation
        /// native.PushString(state, "key");
        /// native.PushInteger(state, 42);
        /// native.SetTable(state, -3);
        /// 
        /// // Type checking
        /// if (native.IsString(state, 1)) {
        ///     var str = native.ToString(state, 1);
        ///     Console.WriteLine($"Parameter: {str}");
        /// }
        /// 
        /// // Stack management
        /// var stackSize = native.GetTop(state);
        /// native.Pop(state, stackSize); // Clean up
        /// </code>
        /// </example>
        public LuaNative Native => _native;

        /// <summary>
        /// Creates a new Lua table and pushes it onto the stack.
        /// </summary>
        /// <param name="arraySize">Expected number of sequential integer-indexed elements (optimization hint). Default is 0.</param>
        /// <param name="recordSize">Expected number of key-value pairs (optimization hint). Default is 0.</param>
        /// <remarks>
        /// <para>This method creates an empty table and places it on top of the Lua stack.</para>
        /// <para>The size parameters are optimization hints to pre-allocate table space and improve performance for large tables.</para>
        /// <para>Use Native methods to populate the table or assign it to variables.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Create a simple empty table
        /// Lua.CreateTable();
        /// Lua.Native.SetGlobal(Lua.State, "myTable");
        /// 
        /// // Create optimized tables
        /// Lua.CreateTable(arraySize: 100); // For arrays
        /// Lua.CreateTable(recordSize: 10);  // For dictionaries
        /// Lua.CreateTable(50, 20);          // Mixed usage
        /// 
        /// // Use in combination with other stack operations
        /// Lua.CreateTable();
        /// var state = Lua.State;
        /// Lua.Native.PushString(state, "name");
        /// Lua.Native.PushString(state, "MyPlugin");
        /// Lua.Native.SetTable(state, -3); // table["name"] = "MyPlugin"
        /// </code>
        /// </example>
        public void CreateTable(int arraySize = 0, int recordSize = 0)
        {
            _native.CreateTable(State, arraySize, recordSize);
        }

        /// <summary>
        /// Pushes a Cheat Engine object onto the Lua stack.
        /// </summary>
        /// <param name="ceObject">Pointer to the CE object to push.</param>
        /// <remarks>
        /// <para>This method pushes CE objects (like MemoryRecord, AddressList, etc.) onto the Lua stack
        /// so they can be accessed from Lua scripts as their respective types.</para>
        /// <para>CE objects are wrapped in Lua userdata with appropriate metatable methods.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Push a CE object and make it available globally
        /// IntPtr memoryRecord = GetMemoryRecord();
        /// Lua.PushCEObject(memoryRecord);
        /// Lua.Native.SetGlobal(Lua.State, "myRecord");
        /// 
        /// // Now accessible from Lua as: myRecord.Address = 0x12345678
        /// </code>
        /// </example>
        public void PushCEObject(IntPtr ceObject)
        {
            _luaPushClassInstance(State, ceObject);
        }
    }
}