using System;
using System.Collections.Generic;
using CESDK.Lua;

namespace CESDK.Utils
{
    public static class LuaUtils
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        /// <summary>
        /// Generic method to call Lua functions with automatic parameter handling and error management
        /// </summary>
        public static T CallLuaFunction<T>(string functionName, string operationName, Func<T> valueExtractor, params object[] args)
        {
            try
            {
                lua.GetGlobal(functionName);
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new InvalidOperationException($"{functionName} function not available in this CE version");
                }

                PushArguments(args);

                var expectedReturnValues = typeof(T) == typeof(object) ? 0 : 1;
                var result = lua.PCall(args.Length, expectedReturnValues);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new InvalidOperationException($"{functionName}() call failed: {error}");
                }

                var value = valueExtractor();
                if (expectedReturnValues > 0)
                {
                    lua.Pop(1);
                }
                return value;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Failed to {operationName}", ex);
            }
        }

        /// <summary>
        /// Calls a Lua function with optional parameters and returns the specified type
        /// </summary>
        public static T CallLuaFunctionWithOptionalParams<T>(string functionName, string operationName, Func<T> valueExtractor, params object?[] parameters)
        {
            try
            {
                lua.GetGlobal(functionName);
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new InvalidOperationException($"{functionName} function not available in this CE version");
                }

                int paramCount = PushOptionalParameters(parameters);

                var expectedReturnValues = typeof(T) == typeof(object) ? 0 : 1;
                var result = lua.PCall(paramCount, expectedReturnValues);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new InvalidOperationException($"{functionName}() call failed: {error}");
                }

                var value = valueExtractor();
                if (expectedReturnValues > 0)
                {
                    lua.Pop(1);
                }
                return value;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Failed to {operationName}", ex);
            }
        }

        /// <summary>
        /// Helper for void functions that don't return values
        /// </summary>
        public static void CallVoidLuaFunction(string functionName, string operationName, params object[] args)
        {
            CallLuaFunction(functionName, operationName, VoidExtractor, args);
        }

        /// <summary>
        /// Helper for void functions with optional parameters
        /// </summary>
        public static void CallVoidLuaFunctionWithOptionalParams(string functionName, string operationName, params object?[] parameters) =>
            CallLuaFunctionWithOptionalParams<object>(functionName, operationName, VoidExtractor, parameters);

        /// <summary>
        /// Extracts byte array from Lua table
        /// </summary>
        public static byte[] ExtractBytesFromTable()
        {
            var bytes = new List<byte>();
            if (lua.IsUserData(-1) || lua.IsTable(-1))
            {
                lua.PushNil();
                while (lua.Next(-2) != 0)
                {
                    if (lua.IsNumber(-1))
                    {
                        var byteValue = (byte)lua.ToInteger(-1);
                        bytes.Add(byteValue);
                    }
                    lua.Pop(1);
                }
            }
            return [.. bytes];
        }

        /// <summary>
        /// Parses address from Lua stack (handles both number and string formats)
        /// </summary>
        public static ulong? ParseAddressFromStack()
        {
            ulong? address = null;

            if (lua.IsNumber(-1))
            {
                address = (ulong)lua.ToInteger(-1);
            }
            else if (lua.IsString(-1))
            {
                var addressStr = lua.ToString(-1);
                if (ulong.TryParse(addressStr, System.Globalization.NumberStyles.HexNumber, null, out var parsedAddress))
                {
                    address = parsedAddress;
                }
            }

            lua.Pop(1);
            return address;
        }

        private static void PushArguments(params object[] args)
        {
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case long longValue: lua.PushInteger(longValue); break;
                    case int intValue: lua.PushInteger(intValue); break;
                    case ulong ulongValue: lua.PushInteger((long)ulongValue); break;
                    case short shortValue: lua.PushInteger(shortValue); break;
                    case byte byteValue: lua.PushInteger(byteValue); break;
                    case float floatValue: lua.PushNumber(floatValue); break;
                    case double doubleValue: lua.PushNumber(doubleValue); break;
                    case string stringValue: lua.PushString(stringValue); break;
                    case bool boolValue: lua.PushBoolean(boolValue); break;
                    case byte[] bytes:
                        lua.CreateTable();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            lua.PushInteger(i + 1);
                            lua.PushInteger(bytes[i]);
                            lua.SetTable(-3);
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unsupported argument type: {arg.GetType()}");
                }
            }
        }

        private static int PushOptionalParameters(params object?[] parameters)
        {
            int paramCount = 0;

            foreach (var param in parameters)
            {
                if (param == null) continue;

                switch (param)
                {
                    case string stringValue:
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            lua.PushString(stringValue);
                            paramCount++;
                        }
                        break;
                    case int intValue:
                        if (intValue != 0)
                        {
                            lua.PushInteger(intValue);
                            paramCount++;
                        }
                        break;
                    default:
                        // For non-optional parameters, always push
                        PushArguments(param);
                        paramCount++;
                        break;
                }
            }

            return paramCount;
        }

        private static object VoidExtractor() => null!;
    }
}