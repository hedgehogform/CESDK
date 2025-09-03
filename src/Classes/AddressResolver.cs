using System;
using System.Globalization;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class AddressResolutionException : Exception
    {
        public AddressResolutionException(string message) : base(message) { }
        public AddressResolutionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class AddressResolver
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static ulong GetAddress(string symbolName, bool searchLocal = false)
        {
            try
            {
                lua.GetGlobal("getAddress");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new AddressResolutionException("getAddress function not available in this CE version");
                }

                lua.PushString(symbolName);
                int paramCount = 1;

                if (searchLocal)
                {
                    lua.PushBoolean(true);
                    paramCount++;
                }

                var result = lua.PCall(paramCount, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new AddressResolutionException($"getAddress() call failed: {error}");
                }

                ulong address;
                if (lua.IsNumber(-1))
                {
                    address = (ulong)lua.ToInteger(-1);
                }
                else if (lua.IsString(-1))
                {
                    var addressStr = lua.ToString(-1);
                    if (!ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out address))
                    {
                        if (!ulong.TryParse(addressStr, out address))
                        {
                            lua.Pop(1);
                            throw new AddressResolutionException($"Invalid address format returned: {addressStr}");
                        }
                    }
                }
                else
                {
                    lua.Pop(1);
                    throw new AddressResolutionException("Symbol not found or address could not be resolved");
                }

                lua.Pop(1);
                return address;
            }
            catch (Exception ex) when (ex is not AddressResolutionException)
            {
                throw new AddressResolutionException($"Failed to resolve address for '{symbolName}'", ex);
            }
        }

        public static ulong? GetAddressSafe(string symbolName, bool searchLocal = false)
        {
            try
            {
                lua.GetGlobal("getAddressSafe");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new AddressResolutionException("getAddressSafe function not available in this CE version");
                }

                lua.PushString(symbolName);
                int paramCount = 1;

                if (searchLocal)
                {
                    lua.PushBoolean(true);
                    paramCount++;
                }

                var result = lua.PCall(paramCount, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new AddressResolutionException($"getAddressSafe() call failed: {error}");
                }

                ulong? address = null;
                if (lua.IsNumber(-1))
                {
                    address = (ulong)lua.ToInteger(-1);
                }
                else if (lua.IsString(-1))
                {
                    var addressStr = lua.ToString(-1);
                    if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var hexAddr))
                    {
                        address = hexAddr;
                    }
                    else if (ulong.TryParse(addressStr, out var decAddr))
                    {
                        address = decAddr;
                    }
                }

                lua.Pop(1);
                return address;
            }
            catch (Exception ex) when (ex is not AddressResolutionException)
            {
                throw new AddressResolutionException($"Failed to safely resolve address for '{symbolName}'", ex);
            }
        }

        public static string GetNameFromAddress(ulong address, bool moduleNames = true, bool symbols = true, bool sections = false)
        {
            try
            {
                lua.GetGlobal("getNameFromAddress");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new AddressResolutionException("getNameFromAddress function not available in this CE version");
                }

                lua.PushInteger((long)address);
                lua.PushBoolean(moduleNames);
                lua.PushBoolean(symbols);
                lua.PushBoolean(sections);

                var result = lua.PCall(4, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new AddressResolutionException($"getNameFromAddress() call failed: {error}");
                }

                var name = lua.ToString(-1) ?? "";
                lua.Pop(1);
                return name;
            }
            catch (Exception ex) when (ex is not AddressResolutionException)
            {
                throw new AddressResolutionException($"Failed to get name from address 0x{address:X}", ex);
            }
        }

        public static bool InModule(ulong address)
        {
            try
            {
                lua.GetGlobal("inModule");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new AddressResolutionException("inModule function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new AddressResolutionException($"inModule() call failed: {error}");
                }

                var isInModule = lua.ToBoolean(-1);
                lua.Pop(1);
                return isInModule;
            }
            catch (Exception ex) when (ex is not AddressResolutionException)
            {
                throw new AddressResolutionException($"Failed to check if address 0x{address:X} is in module", ex);
            }
        }

        public static bool InSystemModule(ulong address)
        {
            try
            {
                lua.GetGlobal("inSystemModule");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new AddressResolutionException("inSystemModule function not available in this CE version");
                }

                lua.PushInteger((long)address);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new AddressResolutionException($"inSystemModule() call failed: {error}");
                }

                var isInSystemModule = lua.ToBoolean(-1);
                lua.Pop(1);
                return isInSystemModule;
            }
            catch (Exception ex) when (ex is not AddressResolutionException)
            {
                throw new AddressResolutionException($"Failed to check if address 0x{address:X} is in system module", ex);
            }
        }
    }
}