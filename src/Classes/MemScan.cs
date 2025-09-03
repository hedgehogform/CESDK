using System;
using CESDK.Lua;

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
                lua.GetGlobal("createMemScan");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new MemScanException("createMemScan function not available in this CE version");
                }

                var result = lua.PCall(0, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemScanException($"createMemScan() call failed: {error}");
                }

                // MemScan object is now on top of stack - keep it there for future use
                _memScanObjectCreated = true;
            }
            catch (Exception ex) when (ex is not MemScanException)
            {
                throw new MemScanException("Failed to create MemScan object", ex);
            }
        }

        private void EnsureMemScanObject()
        {
            if (!_memScanObjectCreated)
                CreateMemScanObject();
        }

        /// <summary>
        /// Performs an initial memory scan
        /// </summary>
        /// <param name="parameters">Scan parameters</param>
        public void FirstScan(ScanParameters parameters)
        {
            try
            {
                EnsureMemScanObject();

                // Push MemScan object
                lua.PushValue(-1);

                // Get firstScan method
                lua.GetField(-1, "firstScan");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new MemScanException("firstScan method not available on MemScan object");
                }

                // Push self (MemScan object)
                lua.PushValue(-2);

                // Push all parameters
                lua.PushInteger((int)parameters.ScanOption);
                lua.PushInteger((int)(parameters.VarType ?? VariableType.vtDword));
                lua.PushInteger((int)parameters.RoundingType);
                lua.PushString(parameters.Input1);
                lua.PushString(parameters.Input2);
                lua.PushInteger((long)parameters.StartAddress);
                lua.PushInteger((long)parameters.StopAddress);
                lua.PushString(parameters.ProtectionFlags);
                lua.PushInteger((int)parameters.AlignmentType);
                lua.PushString(parameters.AlignmentParam);
                lua.PushBoolean(parameters.IsHexadecimalInput);
                lua.PushBoolean(parameters.IsNotABinaryString);
                lua.PushBoolean(parameters.IsUnicodeScan);
                lua.PushBoolean(parameters.IsCaseSensitive);

                // Call firstScan(self, scanOption, varType, roundingType, input1, input2, startAddress, stopAddress, protectionFlags, alignmentType, alignmentParam, isHexadecimalInput, isNotABinaryString, isUnicodeScan, isCaseSensitive)
                var result = lua.PCall(15, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemScanException($"firstScan() call failed: {error}");
                }

                lua.Pop(1); // Pop MemScan object copy
            }
            catch (Exception ex) when (ex is not MemScanException)
            {
                throw new MemScanException("Failed to perform first scan", ex);
            }
        }

        /// <summary>
        /// Performs a next scan based on previous scan results
        /// </summary>
        /// <param name="parameters">Scan parameters</param>
        public void NextScan(ScanParameters parameters)
        {
            try
            {
                EnsureMemScanObject();

                // Push MemScan object
                lua.PushValue(-1);

                // Get nextScan method
                lua.GetField(-1, "nextScan");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new MemScanException("nextScan method not available on MemScan object");
                }

                // Push self (MemScan object)
                lua.PushValue(-2);

                // Push all parameters
                lua.PushInteger((int)parameters.ScanOption);
                lua.PushInteger((int)parameters.RoundingType);
                lua.PushString(parameters.Input1);
                lua.PushString(parameters.Input2);
                lua.PushBoolean(parameters.IsHexadecimalInput);
                lua.PushBoolean(parameters.IsNotABinaryString);
                lua.PushBoolean(parameters.IsUnicodeScan);
                lua.PushBoolean(parameters.IsCaseSensitive);
                lua.PushBoolean(parameters.IsPercentageScan);
                if (!string.IsNullOrEmpty(parameters.SavedResultName))
                    lua.PushString(parameters.SavedResultName);

                // Call nextScan
                var paramCount = string.IsNullOrEmpty(parameters.SavedResultName) ? 10 : 11;
                var result = lua.PCall(paramCount, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemScanException($"nextScan() call failed: {error}");
                }

                lua.Pop(1); // Pop MemScan object copy
            }
            catch (Exception ex) when (ex is not MemScanException)
            {
                throw new MemScanException("Failed to perform next scan", ex);
            }
        }

        /// <summary>
        /// Waits for the scan to complete
        /// </summary>
        public void WaitTillDone()
        {
            try
            {
                EnsureMemScanObject();

                // Push MemScan object
                lua.PushValue(-1);

                // Get waitTillDone method
                lua.GetField(-1, "waitTillDone");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new MemScanException("waitTillDone method not available on MemScan object");
                }

                // Push self (MemScan object)
                lua.PushValue(-2);

                // Call waitTillDone(self)
                var result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemScanException($"waitTillDone() call failed: {error}");
                }

                lua.Pop(1); // Pop MemScan object copy
            }
            catch (Exception ex) when (ex is not MemScanException)
            {
                throw new MemScanException("Failed to wait for scan completion", ex);
            }
        }

        /// <summary>
        /// Saves current scan results with the given name
        /// </summary>
        /// <param name="name">Name to save results under</param>
        public void SaveCurrentResults(string name)
        {
            try
            {
                EnsureMemScanObject();

                // Push MemScan object
                lua.PushValue(-1);

                // Get saveCurrentResults method
                lua.GetField(-1, "saveCurrentResults");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new MemScanException("saveCurrentResults method not available on MemScan object");
                }

                // Push self (MemScan object)
                lua.PushValue(-2);
                lua.PushString(name);

                // Call saveCurrentResults(self, name)
                var result = lua.PCall(2, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemScanException($"saveCurrentResults() call failed: {error}");
                }

                lua.Pop(1); // Pop MemScan object copy
            }
            catch (Exception ex) when (ex is not MemScanException)
            {
                throw new MemScanException("Failed to save current results", ex);
            }
        }

        /// <summary>
        /// Gets the attached FoundList for reading scan results
        /// </summary>
        /// <returns>FoundList object or null if none attached</returns>
        public FoundList GetAttachedFoundList()
        {
            try
            {
                EnsureMemScanObject();

                // Push MemScan object
                lua.PushValue(-1);

                // Get getAttachedFoundlist method
                lua.GetField(-1, "getAttachedFoundlist");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2);
                    throw new MemScanException("getAttachedFoundlist method not available on MemScan object");
                }

                // Push self (MemScan object)
                lua.PushValue(-2);

                // Call getAttachedFoundlist(self)
                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new MemScanException($"getAttachedFoundlist() call failed: {error}");
                }

                // Check if result is nil
                if (lua.IsNil(-1))
                {
                    lua.Pop(2); // Pop nil and MemScan object copy
                    return null;
                }

                // Create FoundList wrapper and initialize it with the Lua object on the stack
                var foundList = new FoundList();
                foundList.InitializeWithLuaObject();
                lua.Pop(1); // Pop MemScan object copy, leave FoundList object on stack for foundList to use

                return foundList;
            }
            catch (Exception ex) when (ex is not MemScanException)
            {
                throw new MemScanException("Failed to get attached found list", ex);
            }
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