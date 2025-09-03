using System;
using System.Collections.Generic;
using System.Globalization;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class AOBScanException : Exception
    {
        public AOBScanException(string message) : base(message) { }
        public AOBScanException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class AOBScanner
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static List<ulong> Scan(string pattern, string? protectionFlags = null, int alignmentType = 0, string? alignmentParam = null)
        {
            try
            {
                CallAOBScanFunction(pattern, protectionFlags, alignmentType, alignmentParam);
                return ProcessScanResults();
            }
            catch (Exception ex) when (ex is not AOBScanException)
            {
                throw new AOBScanException("Failed to perform AOB scan", ex);
            }
        }

        private static void CallAOBScanFunction(string pattern, string? protectionFlags, int alignmentType, string? alignmentParam)
        {
            lua.GetGlobal("AOBScan");
            if (!lua.IsFunction(-1))
            {
                lua.Pop(1);
                throw new AOBScanException("AOBScan function not available in this CE version");
            }

            var paramCount = PushScanParameters(pattern, protectionFlags, alignmentType, alignmentParam);

            var result = lua.PCall(paramCount, 1);
            if (result != 0)
            {
                var error = lua.ToString(-1);
                lua.Pop(1);
                throw new AOBScanException($"AOBScan() call failed: {error}");
            }
        }

        private static int PushScanParameters(string pattern, string? protectionFlags, int alignmentType, string? alignmentParam)
        {
            lua.PushString(pattern);
            int paramCount = 1;

            if (!string.IsNullOrEmpty(protectionFlags))
            {
                lua.PushString(protectionFlags);
                paramCount++;

                if (alignmentType != 0)
                {
                    lua.PushInteger(alignmentType);
                    paramCount++;

                    if (!string.IsNullOrEmpty(alignmentParam))
                    {
                        lua.PushString(alignmentParam);
                        paramCount++;
                    }
                }
            }

            return paramCount;
        }

        private static List<ulong> ProcessScanResults()
        {
            var addresses = new List<ulong>();

            if (!lua.IsUserData(-1))
            {
                lua.Pop(1);
                return addresses;
            }

            lua.GetField(-1, "Count");
            if (!lua.IsNumber(-1))
            {
                lua.Pop(2);
                return addresses;
            }

            var count = lua.ToInteger(-1);
            lua.Pop(1);

            for (int i = 0; i < count; i++)
            {
                ProcessSingleResult(addresses, i);
            }

            lua.Pop(1);
            return addresses;
        }

        private static void ProcessSingleResult(List<ulong> addresses, int index)
        {
            lua.PushInteger(index);
            lua.GetTable(-2);

            if (lua.IsString(-1))
            {
                var addressStr = lua.ToString(-1);
                if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var address))
                {
                    addresses.Add(address);
                }
            }
            lua.Pop(1);
        }

        public static ulong? ScanUnique(string pattern, string? protectionFlags = null, int alignmentType = 0, string? alignmentParam = null)
        {
            try
            {
                lua.GetGlobal("AOBScanUnique");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new AOBScanException("AOBScanUnique function not available in this CE version");
                }

                lua.PushString(pattern);
                int paramCount = 1;

                if (!string.IsNullOrEmpty(protectionFlags))
                {
                    lua.PushString(protectionFlags);
                    paramCount++;

                    if (alignmentType != 0)
                    {
                        lua.PushInteger(alignmentType);
                        paramCount++;

                        if (!string.IsNullOrEmpty(alignmentParam))
                        {
                            lua.PushString(alignmentParam);
                            paramCount++;
                        }
                    }
                }

                var result = lua.PCall(paramCount, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new AOBScanException($"AOBScanUnique() call failed: {error}");
                }

                ulong? address = null;
                if (lua.IsNumber(-1))
                {
                    address = (ulong)lua.ToInteger(-1);
                }
                else if (lua.IsString(-1))
                {
                    var addressStr = lua.ToString(-1);
                    if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var parsedAddress))
                    {
                        address = parsedAddress;
                    }
                }

                lua.Pop(1);
                return address;
            }
            catch (Exception ex) when (ex is not AOBScanException)
            {
                throw new AOBScanException("Failed to perform unique AOB scan", ex);
            }
        }

        public static ulong? ScanModuleUnique(string moduleName, string pattern, string? protectionFlags = null, int alignmentType = 0, string? alignmentParam = null)
        {
            try
            {
                lua.GetGlobal("AOBScanModuleUnique");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new AOBScanException("AOBScanModuleUnique function not available in this CE version");
                }

                lua.PushString(moduleName);
                lua.PushString(pattern);
                int paramCount = 2;

                if (!string.IsNullOrEmpty(protectionFlags))
                {
                    lua.PushString(protectionFlags);
                    paramCount++;

                    if (alignmentType != 0)
                    {
                        lua.PushInteger(alignmentType);
                        paramCount++;

                        if (!string.IsNullOrEmpty(alignmentParam))
                        {
                            lua.PushString(alignmentParam);
                            paramCount++;
                        }
                    }
                }

                var result = lua.PCall(paramCount, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new AOBScanException($"AOBScanModuleUnique() call failed: {error}");
                }

                ulong? address = null;
                if (lua.IsNumber(-1))
                {
                    address = (ulong)lua.ToInteger(-1);
                }
                else if (lua.IsString(-1))
                {
                    var addressStr = lua.ToString(-1);
                    if (ulong.TryParse(addressStr, NumberStyles.HexNumber, null, out var parsedAddress))
                    {
                        address = parsedAddress;
                    }
                }

                lua.Pop(1);
                return address;
            }
            catch (Exception ex) when (ex is not AOBScanException)
            {
                throw new AOBScanException("Failed to perform module unique AOB scan", ex);
            }
        }
    }
}