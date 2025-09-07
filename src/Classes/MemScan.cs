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
    public class MemScan : CEObjectWrapper
    {
        public IntPtr obj { get { return CEObject; } }

        public MemScan()
        {
            try
            {
                lua.GetGlobal("createMemScan");
                if (lua.IsNil(-1))
                    throw new MemScanException("You have no createMemScan (WTF)");
                               
                lua.PCall(0, 1);

                if (lua.IsCEObject(-1))
                {
                    CEObject = lua.ToCEObject(-1);
                }
                else
                    throw new MemScanException("No idea what createMemScan returned");                
            }
            finally
            {
                lua.SetTop(0);
            }
        }

        /// <summary>
        /// Calls a method on the MemScan Lua object with parameters
        /// </summary>
        private void CallMemScanMethod(string methodName, string operationName, params object[] parameters)
        {
            try
            {
                PushCEObject();

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

                lua.Pop(1); // Pop MemScan object
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
            try
            {
                lua.PushCEObject(CEObject);

                lua.PushString("firstScan");
                lua.GetTable(-2);
                
                if (!lua.IsFunction(-1)) 
                    throw new MemScanException("memscan object without a firstScan method");

                lua.PushInteger((long)parameters.ScanOption);
                lua.PushInteger((long)(parameters.VarType ?? VariableType.vtDword));
                lua.PushInteger((long)parameters.RoundingType);
                lua.PushString(parameters.Input1 ?? "");
                lua.PushString(parameters.Input2 ?? "");
                lua.PushInteger((long)parameters.StartAddress);
                lua.PushInteger((long)parameters.StopAddress);
                lua.PushString(parameters.ProtectionFlags ?? "");
                lua.PushInteger((long)parameters.AlignmentType);
                lua.PushString(parameters.AlignmentParam ?? "4");
                lua.PushBoolean(parameters.IsHexadecimalInput);
                lua.PushBoolean(!parameters.IsNotABinaryString); // isnotabinarystring
                lua.PushBoolean(parameters.IsUnicodeScan);
                lua.PushBoolean(parameters.IsCaseSensitive);
                lua.PCall(14, 0);
            }
            finally
            {
                lua.SetTop(0);
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
                lua.PushCEObject(CEObject);

                lua.PushString("nextScan");
                lua.GetTable(-2);

                if (!lua.IsFunction(-1))
                    throw new MemScanException("memscan object without a nextScan method");

                lua.PushInteger((long)parameters.ScanOption);
                lua.PushInteger((long)parameters.RoundingType);
                lua.PushString(parameters.Input1 ?? "");
                lua.PushString(parameters.Input2 ?? "");
                lua.PushBoolean(parameters.IsHexadecimalInput);
                lua.PushBoolean(!parameters.IsNotABinaryString);
                lua.PushBoolean(parameters.IsUnicodeScan);
                lua.PushBoolean(parameters.IsCaseSensitive);
                lua.PushBoolean(parameters.IsPercentageScan);
                lua.PCall(9, 0);
            }
            finally
            {
                lua.SetTop(0);
            }
        }

        /// <summary>
        /// Waits for the scan to complete
        /// </summary>
        public void WaitTillDone()
        {
            try
            {
                lua.PushCEObject(CEObject);
                lua.PushString("waitTillDone");
                lua.GetTable(-2);

                if (!lua.IsFunction(-1)) 
                    throw new MemScanException("memscan object without a waitTillDone method");

                lua.PCall(0, 0);
            }
            finally
            {
                lua.SetTop(0);
            }
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
            try
            {
                PushCEObject();

                // Try to get FoundList property first
                lua.GetField(-1, "FoundList");
                if (!lua.IsNil(-1))
                {
                    var foundList = new FoundList();
                    foundList.SetCEObjectFromFoundListOnStack();
                    lua.Pop(2); // Pop FoundList and MemScan object
                    return foundList;
                }
                lua.Pop(1); // Pop nil FoundList

                // Fallback to getAttachedFoundlist() method
                lua.GetField(-1, "getAttachedFoundlist");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(2); // Pop non-function and MemScan object
                    return null;
                }

                // Push self (MemScan object)
                lua.PushValue(-2);

                // Call getAttachedFoundlist()
                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    lua.Pop(2); // Pop error and MemScan object
                    return null;
                }

                if (lua.IsNil(-1))
                {
                    lua.Pop(2); // Pop nil and MemScan object
                    return null;
                }

                var attachedFoundList = new FoundList();
                attachedFoundList.SetCEObjectFromFoundListOnStack();
                lua.Pop(2); // Pop FoundList and MemScan object
                return attachedFoundList;
            }
            catch (Exception ex)
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