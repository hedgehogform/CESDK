using System;

namespace CESDK.Memory
{
    /// <summary>
    /// Defines the type of scan operation to perform.
    /// </summary>
    public enum ScanType
    {
        /// <summary>Unknown value - scan for any value</summary>
        UnknownValue = 0,
        /// <summary>Exact value - scan for specific value</summary>
        ExactValue = 1,
        /// <summary>Value between - scan for values in range</summary>
        ValueBetween = 2,
        /// <summary>Bigger than - scan for values greater than specified</summary>
        BiggerThan = 3,
        /// <summary>Smaller than - scan for values less than specified</summary>
        SmallerThan = 4,
        /// <summary>Increased value - scan for values that have increased</summary>
        IncreasedValue = 5,
        /// <summary>Increased by - scan for values increased by specific amount</summary>
        IncreasedValueBy = 6,
        /// <summary>Decreased value - scan for values that have decreased</summary>
        DecreasedValue = 7,
        /// <summary>Decreased by - scan for values decreased by specific amount</summary>
        DecreasedValueBy = 8,
        /// <summary>Changed - scan for values that have changed</summary>
        Changed = 9,
        /// <summary>Unchanged - scan for values that haven't changed</summary>
        Unchanged = 10
    }

    /// <summary>
    /// Defines the data type to scan for in memory.
    /// </summary>
    public enum ValueType
    {
        /// <summary>1 byte integer (0-255)</summary>
        Byte = 0,
        /// <summary>2 byte integer (0-65535)</summary>
        Word = 1,
        /// <summary>4 byte integer</summary>
        Dword = 2,
        /// <summary>8 byte integer</summary>
        Qword = 3,
        /// <summary>4 byte floating point</summary>
        Single = 4,
        /// <summary>8 byte floating point</summary>
        Double = 5,
        /// <summary>ASCII string</summary>
        String = 6,
        /// <summary>Unicode string</summary>
        UnicodeString = 7,
        /// <summary>Wide string (same as Unicode)</summary>
        WideString = 7,
        /// <summary>Array of bytes</summary>
        ByteArray = 8,
        /// <summary>Binary pattern</summary>
        Binary = 9,
        /// <summary>All types</summary>
        All = 10,
        /// <summary>Auto assembler script</summary>
        AutoAssembler = 11,
        /// <summary>Pointer value</summary>
        Pointer = 12,
        /// <summary>Custom type</summary>
        Custom = 13,
        /// <summary>Grouped values</summary>
        Grouped = 14
    }

    /// <summary>
    /// Defines how floating point values should be rounded during scanning.
    /// </summary>
    public enum RoundingType
    {
        /// <summary>Standard rounding</summary>
        Rounded = 0,
        /// <summary>Extreme rounding (more tolerance)</summary>
        ExtremeRounded = 1,
        /// <summary>Truncated (no rounding)</summary>
        Truncated = 2
    }

    /// <summary>
    /// Defines memory alignment options for faster scanning.
    /// </summary>
    public enum AlignmentType
    {
        /// <summary>No alignment requirements</summary>
        NotAligned = 0,
        /// <summary>Values must be aligned to their size</summary>
        Aligned = 1,
        /// <summary>Use last digits pattern matching</summary>
        LastDigits = 2
    }
}