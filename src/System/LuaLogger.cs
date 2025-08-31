using System.Globalization;

namespace CESDK.System
{
    /// <summary>
    /// Exception thrown when Lua logging operations fail.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the LuaLoggerException class.
    /// </remarks>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public class LuaLoggerException(string message, Exception? innerException = null) : Exception($"Lua logging failed: {message}", innerException)
    {
    }

    /// <summary>
    /// Provides logging functionality that outputs to Cheat Engine's Lua console.
    /// Wraps CE's Lua print and logging functions with high-level, type-safe C# methods.
    /// </summary>
    /// <remarks>
    /// <para>This class allows C# plugins to output messages directly to CE's Lua console.</para>
    /// <para>All messages appear in the same console where Lua scripts output their print() statements.</para>
    /// <para>Useful for debugging, status updates, and user feedback from C# plugins.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Simple logging
    /// LuaLogger.Print("Hello from C# plugin!");
    /// 
    /// // Formatted logging
    /// LuaLogger.Print($"Loaded {count} items");
    /// LuaLogger.Printf("Player health: {0}, position: {1:F2}, {2:F2}", health, x, y);
    /// 
    /// // Object logging
    /// var data = new { Name = "John", Age = 25 };
    /// LuaLogger.PrintObject(data);
    /// 
    /// // Multiple values
    /// LuaLogger.Print("Status:", status, "Count:", itemCount, "Ready:", isReady);
    /// 
    /// // Error logging
    /// try { /* some operation */ }
    /// catch (Exception ex) { LuaLogger.PrintError(ex); }
    /// </code>
    /// </example>
    public static class LuaLogger
    {
        /// <summary>
        /// Prints a message to the Lua console using Lua's print() function.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's Lua <c>print()</c> function.</para>
        /// <para>Messages appear in CE's Lua console window.</para>
        /// <para>Null messages are converted to empty strings.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// LuaLogger.Print("Plugin loaded successfully");
        /// LuaLogger.Print($"Found {results.Count} matches");
        /// </code>
        /// </example>
        public static void Print(string? message)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                // Get the print function
                native.GetGlobal(state, "print");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new LuaLoggerException("print function not available in Lua environment");
                }

                // Push the message parameter
                native.PushString(state, message ?? "");

                // Call print(message)
                var result = native.PCall(state, 1, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new LuaLoggerException($"print() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not LuaLoggerException)
            {
                throw new LuaLoggerException($"Error calling Lua print(): {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Prints multiple values to the Lua console, separated by tabs.
        /// </summary>
        /// <param name="values">The values to print.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's Lua <c>print()</c> function with multiple parameters.</para>
        /// <para>Values are automatically converted to strings and separated by tabs.</para>
        /// <para>This mimics Lua's print() behavior with multiple arguments.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// LuaLogger.Print("Player:", playerName, "Level:", level, "HP:", currentHp);
        /// LuaLogger.Print("X:", x, "Y:", y, "Z:", z);
        /// </code>
        /// </example>
        public static void Print(params object?[] values)
        {
            if (values == null || values.Length == 0)
            {
                Print("");
                return;
            }

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                // Get the print function
                native.GetGlobal(state, "print");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new LuaLoggerException("print function not available in Lua environment");
                }

                // Push all values as parameters
                foreach (var value in values)
                {
                    var stringValue = value?.ToString() ?? "nil";
                    native.PushString(state, stringValue);
                }

                // Call print with multiple parameters
                var result = native.PCall(state, values.Length, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new LuaLoggerException($"print() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not LuaLoggerException)
            {
                throw new LuaLoggerException($"Error calling Lua print(): {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Prints a formatted message to the Lua console using C# string formatting.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <remarks>
        /// <para>Uses C# string.Format() to create the message, then prints to Lua console.</para>
        /// <para>This provides familiar C# formatting syntax while outputting to CE's Lua console.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// LuaLogger.Printf("Player {0} has {1} health and {2} mana", name, hp, mp);
        /// LuaLogger.Printf("Position: ({0:F2}, {1:F2})", x, y);
        /// LuaLogger.Printf("Progress: {0:P}", progress / 100.0);
        /// </code>
        /// </example>
        public static void Printf(string format, params object?[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            try
            {
                var message = string.Format(CultureInfo.InvariantCulture, format, args);
                Print(message);
            }
            catch (FormatException ex)
            {
                throw new LuaLoggerException($"String formatting failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Prints an object's string representation to the Lua console.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        /// <param name="prefix">Optional prefix to add before the object representation.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <remarks>
        /// <para>Uses the object's ToString() method to create the output.</para>
        /// <para>Null objects are printed as "null".</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var player = new { Name = "John", Level = 10 };
        /// LuaLogger.PrintObject(player, "Player data: ");
        /// 
        /// var list = new List&lt;string&gt; { "a", "b", "c" };
        /// LuaLogger.PrintObject(list);
        /// </code>
        /// </example>
        public static void PrintObject(object? obj, string? prefix = null)
        {
            var representation = obj?.ToString() ?? "null";
            var message = string.IsNullOrEmpty(prefix) ? representation : $"{prefix}{representation}";
            Print(message);
        }

        /// <summary>
        /// Prints exception information to the Lua console.
        /// </summary>
        /// <param name="exception">The exception to print.</param>
        /// <param name="includeStackTrace">Whether to include the stack trace. Default is true.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <remarks>
        /// <para>Formats exception information in a readable way for the Lua console.</para>
        /// <para>Includes exception type, message, and optionally the stack trace.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     // Some risky operation
        ///     RiskyOperation();
        /// }
        /// catch (Exception ex)
        /// {
        ///     LuaLogger.PrintError(ex);
        ///     // or without stack trace
        ///     LuaLogger.PrintError(ex, includeStackTrace: false);
        /// }
        /// </code>
        /// </example>
        public static void PrintError(Exception? exception, bool includeStackTrace = true)
        {
            if (exception == null)
            {
                Print("Error: (null exception)");
                return;
            }

            Print($"ERROR: {exception.GetType().Name}: {exception.Message}");

            if (includeStackTrace && !string.IsNullOrEmpty(exception.StackTrace))
            {
                Print($"Stack Trace:\n{exception.StackTrace}");
            }

            // Print inner exceptions
            var inner = exception.InnerException;
            while (inner != null)
            {
                Print($"  Inner Exception: {inner.GetType().Name}: {inner.Message}");
                inner = inner.InnerException;
            }
        }

        /// <summary>
        /// Prints a separator line to the Lua console for visual organization.
        /// </summary>
        /// <param name="character">The character to use for the separator. Default is '-'.</param>
        /// <param name="length">The length of the separator line. Default is 50.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <example>
        /// <code>
        /// LuaLogger.Print("Starting operation...");
        /// LuaLogger.PrintSeparator();
        /// DoSomeWork();
        /// LuaLogger.PrintSeparator('=', 30);
        /// LuaLogger.Print("Operation complete");
        /// </code>
        /// </example>
        public static void PrintSeparator(char character = '-', int length = 50)
        {
            if (length <= 0)
                length = 50;

            var separator = new string(character, length);
            Print(separator);
        }

        /// <summary>
        /// Prints a timestamped message to the Lua console.
        /// </summary>
        /// <param name="message">The message to print with timestamp.</param>
        /// <param name="format">The timestamp format. Default is "yyyy-MM-dd HH:mm:ss".</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <example>
        /// <code>
        /// LuaLogger.PrintTimestamped("Plugin started");
        /// LuaLogger.PrintTimestamped("Operation completed", "HH:mm:ss.fff");
        /// </code>
        /// </example>
        public static void PrintTimestamped(string? message, string format = "yyyy-MM-dd HH:mm:ss")
        {
            var timestamp = DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
            Print($"[{timestamp}] {message ?? ""}");
        }

        /// <summary>
        /// Prints a debug message with DEBUG prefix to the Lua console.
        /// </summary>
        /// <param name="message">The debug message to print.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <example>
        /// <code>
        /// LuaLogger.PrintDebug("Variable x = " + x);
        /// LuaLogger.PrintDebug($"Method entered with params: {param1}, {param2}");
        /// </code>
        /// </example>
        public static void PrintDebug(string? message)
        {
            Print($"DEBUG: {message ?? ""}");
        }

        /// <summary>
        /// Prints a warning message with WARNING prefix to the Lua console.
        /// </summary>
        /// <param name="message">The warning message to print.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <example>
        /// <code>
        /// LuaLogger.PrintWarning("This feature is experimental");
        /// LuaLogger.PrintWarning($"Low memory: {freeMemory} bytes remaining");
        /// </code>
        /// </example>
        public static void PrintWarning(string? message)
        {
            Print($"WARNING: {message ?? ""}");
        }

        /// <summary>
        /// Prints an info message with INFO prefix to the Lua console.
        /// </summary>
        /// <param name="message">The info message to print.</param>
        /// <exception cref="LuaLoggerException">Thrown when printing fails.</exception>
        /// <example>
        /// <code>
        /// LuaLogger.PrintInfo("Configuration loaded successfully");
        /// LuaLogger.PrintInfo($"Connected to process: {processName}");
        /// </code>
        /// </example>
        public static void PrintInfo(string? message)
        {
            Print($"INFO: {message ?? ""}");
        }

        /// <summary>
        /// Attempts to print a message to the Lua console without throwing exceptions.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <returns>True if printing succeeded; otherwise, false.</returns>
        /// <remarks>
        /// <para>Use this method when you need error-tolerant logging.</para>
        /// <para>Failures are silently ignored rather than throwing exceptions.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (LuaLogger.TryPrint("Optional debug message"))
        /// {
        ///     // Message was printed successfully
        /// }
        /// else
        /// {
        ///     // Fall back to alternative logging or ignore
        /// }
        /// </code>
        /// </example>
        public static bool TryPrint(string? message)
        {
            try
            {
                Print(message);
                return true;
            }
            catch (LuaLoggerException)
            {
                return false;
            }
        }

        /// <summary>
        /// Executes a Lua script that includes print statements.
        /// </summary>
        /// <param name="script">The Lua script to execute.</param>
        /// <exception cref="LuaLoggerException">Thrown when script execution fails.</exception>
        /// <remarks>
        /// <para>This allows executing arbitrary Lua code that can use print() and other Lua functions.</para>
        /// <para>Useful for complex logging scenarios that benefit from Lua's features.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Execute Lua script with complex output
        /// LuaLogger.ExecuteScript(@"
        ///     print('Starting complex operation...')
        ///     for i = 1, 5 do
        ///         print('Step ' .. i .. ' completed')
        ///     end
        ///     print('All steps finished!')
        /// ");
        /// 
        /// // Use Lua string formatting
        /// LuaLogger.ExecuteScript($"print(string.format('Value: %.2f', {value}))");
        /// </code>
        /// </example>
        public static void ExecuteScript(string script)
        {
            if (string.IsNullOrEmpty(script))
                throw new ArgumentException("Script cannot be null or empty", nameof(script));

            try
            {
                PluginContext.Lua.Execute(script);
            }
            catch (InvalidOperationException ex)
            {
                throw new LuaLoggerException($"Failed to execute Lua script: {ex.Message}", ex);
            }
        }
    }
}