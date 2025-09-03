using System;
using CESDK.Lua;
using CESDK.Utils;

namespace CESDK.Classes
{
    public class MemScanException : Exception
    {
        public MemScanException(string message) : base(message) { }
        public MemScanException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Memory scanning class that wraps Cheat Engine's MemScan Lua object
    /// </summary>
    public class MemScan
    {
        private readonly LuaNative lua;
        private bool _memScanObjectCreated = false;

        public MemScan()
        {
            lua = PluginContext.Lua;
            CreateMemScanObject();
        }

        private void CreateMemScanObject()
        {
            try
            {
                LuaUtils.CallLuaFunction("createMemScan", "create MemScan object", () => { /* MemScan object left on stack */ return true; });
                _memScanObjectCreated = true;
            }
            catch (InvalidOperationException ex)
            {
                throw new MemScanException(ex.Message, ex);
            }
        }

        private void EnsureMemScanObject()
        {
            if (!_memScanObjectCreated)
                CreateMemScanObject();
        }

        /// <summary>
        /// Calls a method on the MemScan Lua object with parameters
        /// </summary>
        private void CallMemScanMethod(string methodName, string operationName, params object[] parameters)
        {
            try
            {
                EnsureMemScanObject();

                // Push MemScan object
                lua.PushValue(-1);

                // Get method
                lua.GetField(-1, methodName);
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new InvalidOperationException($"{methodName} method not available on MemScan object");
                }

                // Push self (MemScan object)
                lua.PushValue(-2);

                // Push parameters
                foreach (var param in parameters)
                {
                    PushParameter(param);
                }

                // Call method
                var result = lua.PCall(1 + parameters.Length, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new InvalidOperationException($"{methodName}() call failed: {error}");
                }

                lua.Pop(1); // Pop MemScan object copy
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MemScanException($"Failed to {operationName}", ex);
            }
        }

        /// <summary>
        /// Calls a method on the MemScan Lua object that returns a value
        /// </summary>
        private T CallMemScanMethod<T>(string methodName, string operationName, Func<T> valueExtractor, params object[] parameters)
        {
            try
            {
                EnsureMemScanObject();

                // Push MemScan object
                lua.PushValue(-1);

                // Get method
                lua.GetField(-1, methodName);
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new InvalidOperationException($"{methodName} method not available on MemScan object");
                }

                // Push self (MemScan object)
                lua.PushValue(-2);

                // Push parameters
                foreach (var param in parameters)
                {
                    PushParameter(param);
                }

                // Call method
                var result = lua.PCall(1 + parameters.Length, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new InvalidOperationException($"{methodName}() call failed: {error}");
                }

                var value = valueExtractor();
                lua.Pop(1); // Pop MemScan object copy
                return value;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MemScanException($"Failed to {operationName}", ex);
            }
        }

        private void PushParameter(object parameter)
        {
            switch (parameter)
            {
                case int intValue: lua.PushInteger(intValue); break;
                case long longValue: lua.PushInteger(longValue); break;
                case ulong ulongValue: lua.PushInteger((long)ulongValue); break;
                case string stringValue: lua.PushString(stringValue); break;
                case bool boolValue: lua.PushBoolean(boolValue); break;
                default: throw new ArgumentException($"Unsupported parameter type: {parameter.GetType()}");
            }
        }

        /// <summary>
        /// Performs an initial memory scan
        /// </summary>
        /// <param name="parameters">Scan parameters</param>
        public void FirstScan(ScanParameters parameters)
        {
            CallMemScanMethod("firstScan", "perform first scan",
                (int)parameters.ScanOption,
                (int)(parameters.VarType ?? VariableType.vtDword),
                (int)parameters.RoundingType,
                parameters.Input1,
                parameters.Input2,
                (long)parameters.StartAddress,
                (long)parameters.StopAddress,
                parameters.ProtectionFlags,
                (int)parameters.AlignmentType,
                parameters.AlignmentParam,
                parameters.IsHexadecimalInput,
                parameters.IsNotABinaryString,
                parameters.IsUnicodeScan,
                parameters.IsCaseSensitive);
        }

        /// <summary>
        /// Performs a next scan based on previous scan results
        /// </summary>
        /// <param name="parameters">Scan parameters</param>
        public void NextScan(ScanParameters parameters)
        {
            var args = new object[]
            {
                (int)parameters.ScanOption,
                (int)parameters.RoundingType,
                parameters.Input1,
                parameters.Input2,
                parameters.IsHexadecimalInput,
                parameters.IsNotABinaryString,
                parameters.IsUnicodeScan,
                parameters.IsCaseSensitive,
                parameters.IsPercentageScan
            };

            if (!string.IsNullOrEmpty(parameters.SavedResultName))
            {
                var argsWithName = new object[args.Length + 1];
                Array.Copy(args, argsWithName, args.Length);
                argsWithName[args.Length] = parameters.SavedResultName;
                args = argsWithName;
            }

            try
            {
                EnsureMemScanObject();

                // Push MemScan object
                lua.PushValue(-1);

                // Get method
                lua.GetField(-1, "nextScan");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new InvalidOperationException("nextScan method not available on MemScan object");
                }

                // Push self (MemScan object)
                lua.PushValue(-2);

                // Push parameters
                foreach (var param in args)
                {
                    PushParameter(param);
                }

                // Call method
                var result = lua.PCall(1 + args.Length, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new InvalidOperationException($"nextScan() call failed: {error}");
                }

                lua.Pop(1); // Pop MemScan object copy
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MemScanException("Failed to perform next scan", ex);
            }
        }

        /// <summary>
        /// Waits for the scan to complete
        /// </summary>
        public void WaitTillDone()
        {
            CallMemScanMethod("waitTillDone", "wait for scan completion");
        }

        /// <summary>
        /// Saves current scan results with the given name
        /// </summary>
        /// <param name="name">Name to save results under</param>
        public void SaveCurrentResults(string name)
        {
            CallMemScanMethod("saveCurrentResults", "save current results", name);
        }

        /// <summary>
        /// Gets the attached FoundList for reading scan results
        /// </summary>
        /// <returns>FoundList object or null if none attached</returns>
        public FoundList? GetAttachedFoundList()
        {
            return CallMemScanMethod("getAttachedFoundlist", "get attached found list", () =>
            {
                // Check if result is nil
                if (lua.IsNil(-1))
                {
                    return null;
                }

                // Create FoundList wrapper and initialize it with the Lua object on the stack
                var foundList = new FoundList();
                foundList.InitializeWithLuaObject();
                return foundList;
            });
        }
    }


    // Enums from celua.txt
    public enum ScanOption
    {
        soUnknownValue = 0,
        soExactValue = 1,
        soValueBetween = 2,
        soBiggerThan = 3,
        soSmallerThan = 4,
        soIncreasedValue = 5,
        soIncreasedValueBy = 6,
        soDecreasedValue = 7,
        soDecreasedValueBy = 8,
        soChanged = 9,
        soUnchanged = 10
    }

    public enum VariableType
    {
        vtByte = 0,
        vtWord = 1,
        vtDword = 2,
        vtQword = 3,
        vtSingle = 4,
        vtDouble = 5,
        vtString = 6,
        vtByteArray = 7,
        vtGrouped = 8,
        vtBinary = 9,
        vtAll = 10
    }

    public enum RoundingType
    {
        rtRounded = 0,
        rtExtremerounded = 1,
        rtTruncated = 2
    }

    public enum AlignmentType
    {
        fsmNotAligned = 0,
        fsmAligned = 1,
        fsmLastDigits = 2
    }

    public class ScanParameters
    {
        public ScanOption ScanOption { get; set; }
        public VariableType? VarType { get; set; }
        public RoundingType RoundingType { get; set; }
        public string Input1 { get; set; } = "";
        public string Input2 { get; set; } = "";
        public ulong StartAddress { get; set; } = 0;
        public ulong StopAddress { get; set; } = 0x7FFFFFFFFFFFFFFF;
        public string ProtectionFlags { get; set; } = "+W-C";
        public AlignmentType AlignmentType { get; set; } = AlignmentType.fsmAligned;
        public string AlignmentParam { get; set; } = "4";
        public bool IsHexadecimalInput { get; set; } = false;
        public bool IsNotABinaryString { get; set; } = false;
        public bool IsUnicodeScan { get; set; } = false;
        public bool IsCaseSensitive { get; set; } = false;
        public bool IsPercentageScan { get; set; } = false;
        public string SavedResultName { get; set; } = "";
    }
}