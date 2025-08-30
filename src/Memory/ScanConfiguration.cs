using System;

namespace CESDK.Memory
{
    /// <summary>
    /// Configuration class for memory scanning operations.
    /// Provides a modern, fluent API for configuring scan parameters.
    /// </summary>
    /// <example>
    /// <code>
    /// var config = new ScanConfiguration()
    ///     .ForValue("100")
    ///     .AsInteger()
    ///     .InRange(0x1000000, 0x2000000)
    ///     .WithAlignment(AlignmentType.Aligned);
    /// </code>
    /// </example>
    public class ScanConfiguration
    {
        /// <summary>
        /// Gets or sets the primary value to search for.
        /// </summary>
        /// <value>The value as a string representation.</value>
        public string Value { get; set; } = "";

        /// <summary>
        /// Gets or sets the secondary value for range-based scans.
        /// </summary>
        /// <value>The second value for "between" scans.</value>
        public string Value2 { get; set; } = "";

        /// <summary>
        /// Gets or sets the type of scan operation to perform.
        /// </summary>
        /// <value>The scan type from the <see cref="ScanType"/> enumeration.</value>
        public ScanType ScanType { get; set; } = ScanType.ExactValue;

        /// <summary>
        /// Gets or sets the data type to scan for in memory.
        /// </summary>
        /// <value>The value type from the <see cref="ValueType"/> enumeration.</value>
        public ValueType ValueType { get; set; } = ValueType.Dword;

        /// <summary>
        /// Gets or sets the rounding type for floating-point comparisons.
        /// </summary>
        /// <value>The rounding type from the <see cref="RoundingType"/> enumeration.</value>
        public RoundingType RoundingType { get; set; } = RoundingType.ExtremeRounded;

        /// <summary>
        /// Gets or sets the starting address for the memory scan.
        /// </summary>
        /// <value>The starting memory address. Default is 0.</value>
        public ulong StartAddress { get; set; } = 0;

        /// <summary>
        /// Gets or sets the ending address for the memory scan.
        /// </summary>
        /// <value>The ending memory address. Default is maximum value.</value>
        public ulong EndAddress { get; set; } = ulong.MaxValue;

        /// <summary>
        /// Gets or sets the memory protection flags filter.
        /// </summary>
        /// <value>Memory protection flags as a string (e.g., "+W-C" for writable, not copy-on-write).</value>
        /// <example>
        /// Common protection flags:
        /// - "+W" = writable
        /// - "-C" = not copy-on-write  
        /// - "+X" = executable
        /// - "+R" = readable
        /// </example>
        public string ProtectionFlags { get; set; } = "+W-C";

        /// <summary>
        /// Gets or sets the memory alignment type for optimization.
        /// </summary>
        /// <value>The alignment type from the <see cref="AlignmentType"/> enumeration.</value>
        public AlignmentType AlignmentType { get; set; } = AlignmentType.Aligned;

        /// <summary>
        /// Gets or sets the alignment value as a string.
        /// </summary>
        /// <value>The alignment value (e.g., "4" for 4-byte alignment).</value>
        public string AlignmentValue { get; set; } = "4";

        /// <summary>
        /// Gets or sets whether the input value should be interpreted as hexadecimal.
        /// </summary>
        /// <value><see langword="true"/> if the value is hexadecimal; otherwise, <see langword="false"/>.</value>
        public bool IsHexadecimal { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to scan for UTF-16 encoded strings.
        /// </summary>
        /// <value><see langword="true"/> to scan for UTF-16 strings; otherwise, <see langword="false"/>.</value>
        public bool IsUtf16 { get; set; } = false;

        /// <summary>
        /// Gets or sets whether string comparisons should be case-sensitive.
        /// </summary>
        /// <value><see langword="true"/> for case-sensitive comparisons; otherwise, <see langword="false"/>.</value>
        public bool IsCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use percentage-based scanning for next scans.
        /// </summary>
        /// <value><see langword="true"/> to use percentage scanning; otherwise, <see langword="false"/>.</value>
        public bool IsPercentageScan { get; set; } = false;

        #region Fluent API Methods

        /// <summary>
        /// Sets the value to search for.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>This configuration instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// config.ForValue("100");
        /// config.ForValue("0x1234"); // Hexadecimal value
        /// </code>
        /// </example>
        public ScanConfiguration ForValue(string value)
        {
            Value = value ?? "";
            return this;
        }

        /// <summary>
        /// Sets the value range for "between" scans.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>This configuration instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// config.ForValueBetween("100", "200");
        /// </code>
        /// </example>
        public ScanConfiguration ForValueBetween(string minValue, string maxValue)
        {
            Value = minValue ?? "";
            Value2 = maxValue ?? "";
            ScanType = ScanType.ValueBetween;
            return this;
        }

        /// <summary>
        /// Sets the scan type to search for exact values.
        /// </summary>
        /// <returns>This configuration instance for method chaining.</returns>
        public ScanConfiguration ForExactValue()
        {
            ScanType = ScanType.ExactValue;
            return this;
        }

        /// <summary>
        /// Sets the scan type to search for changed values.
        /// </summary>
        /// <returns>This configuration instance for method chaining.</returns>
        public ScanConfiguration ForChangedValues()
        {
            ScanType = ScanType.Changed;
            return this;
        }

        /// <summary>
        /// Sets the scan type to search for unchanged values.
        /// </summary>
        /// <returns>This configuration instance for method chaining.</returns>
        public ScanConfiguration ForUnchangedValues()
        {
            ScanType = ScanType.Unchanged;
            return this;
        }

        /// <summary>
        /// Sets the value type to 4-byte integer.
        /// </summary>
        /// <returns>This configuration instance for method chaining.</returns>
        public ScanConfiguration AsInteger()
        {
            ValueType = ValueType.Dword;
            AlignmentValue = "4";
            return this;
        }

        /// <summary>
        /// Sets the value type to 8-byte integer.
        /// </summary>
        /// <returns>This configuration instance for method chaining.</returns>
        public ScanConfiguration AsLongInteger()
        {
            ValueType = ValueType.Qword;
            AlignmentValue = "8";
            return this;
        }

        /// <summary>
        /// Sets the value type to floating-point number.
        /// </summary>
        /// <returns>This configuration instance for method chaining.</returns>
        public ScanConfiguration AsFloat()
        {
            ValueType = ValueType.Single;
            AlignmentValue = "4";
            return this;
        }

        /// <summary>
        /// Sets the value type to double-precision floating-point.
        /// </summary>
        /// <returns>This configuration instance for method chaining.</returns>
        public ScanConfiguration AsDouble()
        {
            ValueType = ValueType.Double;
            AlignmentValue = "8";
            return this;
        }

        /// <summary>
        /// Sets the value type to string.
        /// </summary>
        /// <param name="caseSensitive">Whether string comparison should be case-sensitive.</param>
        /// <returns>This configuration instance for method chaining.</returns>
        public ScanConfiguration AsString(bool caseSensitive = false)
        {
            ValueType = ValueType.String;
            IsCaseSensitive = caseSensitive;
            AlignmentType = AlignmentType.NotAligned;
            return this;
        }

        /// <summary>
        /// Sets the memory address range to scan.
        /// </summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="endAddress">The ending address.</param>
        /// <returns>This configuration instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// config.InRange(0x1000000, 0x2000000); // Scan specific memory range
        /// </code>
        /// </example>
        public ScanConfiguration InRange(ulong startAddress, ulong endAddress)
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
            return this;
        }

        /// <summary>
        /// Sets the memory protection flags filter.
        /// </summary>
        /// <param name="flags">Protection flags string.</param>
        /// <returns>This configuration instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// config.WithProtection("+W-C"); // Writable, not copy-on-write
        /// config.WithProtection("+R+W+X"); // Readable, writable, executable
        /// </code>
        /// </example>
        public ScanConfiguration WithProtection(string flags)
        {
            ProtectionFlags = flags ?? "+W-C";
            return this;
        }

        /// <summary>
        /// Sets the memory alignment for optimized scanning.
        /// </summary>
        /// <param name="alignmentType">The alignment type.</param>
        /// <param name="alignmentValue">The alignment value (optional).</param>
        /// <returns>This configuration instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// config.WithAlignment(AlignmentType.Aligned); // Use default alignment for value type
        /// config.WithAlignment(AlignmentType.Aligned, "8"); // 8-byte alignment
        /// </code>
        /// </example>
        public ScanConfiguration WithAlignment(AlignmentType alignmentType, string? alignmentValue = null)
        {
            AlignmentType = alignmentType;
            if (alignmentValue != null)
                AlignmentValue = alignmentValue;
            return this;
        }

        /// <summary>
        /// Sets whether input values should be interpreted as hexadecimal.
        /// </summary>
        /// <param name="isHex">True for hexadecimal interpretation.</param>
        /// <returns>This configuration instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// config.AsHexadecimal().ForValue("DEADBEEF");
        /// </code>
        /// </example>
        public ScanConfiguration AsHexadecimal(bool isHex = true)
        {
            IsHexadecimal = isHex;
            return this;
        }

        #endregion
    }
}