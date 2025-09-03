using System;
using System.Collections.Generic;
using CESDK.Utils;
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
        public static List<ulong> Scan(string pattern, string? protectionFlags = null, int alignmentType = 0, string? alignmentParam = null) =>
            WrapException(() =>
            {
                LuaUtils.CallVoidLuaFunctionWithOptionalParams("AOBScan", "perform AOB scan", pattern, protectionFlags, alignmentType, alignmentParam);
                return ProcessScanResults();
            });

        public static ulong? ScanUnique(string pattern, string? protectionFlags = null, int alignmentType = 0, string? alignmentParam = null) =>
            WrapException(() => LuaUtils.CallLuaFunctionWithOptionalParams("AOBScanUnique", "perform unique AOB scan", LuaUtils.ParseAddressFromStack, pattern, protectionFlags, alignmentType, alignmentParam));

        public static ulong? ScanModuleUnique(string moduleName, string pattern, string? protectionFlags = null, int alignmentType = 0, string? alignmentParam = null) =>
            WrapException(() => LuaUtils.CallLuaFunction("AOBScanModuleUnique", "perform module unique AOB scan", LuaUtils.ParseAddressFromStack, moduleName, pattern, protectionFlags, alignmentType, alignmentParam));

        private static List<ulong> ProcessScanResults()
        {
            var addresses = new List<ulong>();
            var lua = PluginContext.Lua;

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
                ProcessSingleResult(addresses, i, lua);
            }

            lua.Pop(1);
            return addresses;
        }

        private static void ProcessSingleResult(List<ulong> addresses, int index, LuaNative lua)
        {
            lua.PushInteger(index);
            lua.GetTable(-2);

            if (lua.IsString(-1))
            {
                var addressStr = lua.ToString(-1);
                if (ulong.TryParse(addressStr, System.Globalization.NumberStyles.HexNumber, null, out var address))
                {
                    addresses.Add(address);
                }
            }
            lua.Pop(1);
        }

        private static T WrapException<T>(Func<T> operation)
        {
            try
            {
                return operation();
            }
            catch (InvalidOperationException ex)
            {
                throw new AOBScanException(ex.Message, ex);
            }
        }
    }
}