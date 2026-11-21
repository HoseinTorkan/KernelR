using KernelR.Domain.Enums;
using KernelR.Domain.Results;
using KernelR.Domain.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Measurement;

/// <summary>
/// Represents a quantity of digital information (storage capacity, file size,
/// memory usage) as an immutable value object with a canonical representation in bytes.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees the following invariants:
/// </para>
/// <list type="bullet">
///   <item><description>The <see cref="Bytes"/> is always non-negative (zero is allowed).</description></item>
///   <item><description>
///     The value is stored canonically in bytes. This guarantees that two logically equal values
///     (e.g., <c>1 KB</c> and <c>1024 B</c>) are always exactly equal, and that EF Core and
///     JSON serialization can use a single column/property.
///   </description></item>
///   <item><description>
///     Fractional sizes supplied via <c>FromKilobytes</c>, <c>FromMegabytes</c>, etc. are
///     rounded to the nearest byte using <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// <para>
/// <b>Binary units:</b> This type uses base-1024 prefixes (1 KB = 1024 B), matching
/// what operating systems and file managers display. See <see cref="DataSizeUnit"/>
/// for the rationale.
/// </para>
/// <para>
/// <b>Culture policy:</b> This value object is a domain primitive and a machine-to-machine contract.
/// All internal parsing, error messages, and numeric conversion use
/// <see cref="CultureInfo.InvariantCulture"/> explicitly, so the type behaves identically
/// regardless of the host application's <see cref="CultureInfo.CurrentCulture"/>.
/// </para>
/// <para>
/// <b>Display responsibility:</b> For display, the <c>ToString</c> overloads accept an optional
/// <see cref="IFormatProvider"/>. Callers who want localized output (e.g., Persian digits)
/// should pass the desired culture explicitly:
/// <c>size.ToString(CultureInfo.GetCultureInfo("fa-IR"))</c>.
/// This keeps the decision of "how to display" in the presentation layer, not in the library.
/// </para>
/// <para>
/// <b>Mathematical operations:</b> Fallible operations (<c>Add</c>, <c>Subtract</c>, <c>Multiply</c>,
/// <c>Divide</c>, <c>Percentage</c>, <c>Ratio</c>) return <see cref="Result{T}"/> and have
/// corresponding operators that throw <see cref="InvalidOperationException"/> on failure.
/// Comparison uses the standard operators (<c>&gt;</c>, <c>&lt;</c>, <c>&gt;=</c>, <c>&lt;=</c>)
/// and <see cref="CompareTo"/> — no separate comparison methods exist, because comparing two
/// <see cref="DataSize"/> instances cannot fail.
/// </para>
/// <para>
/// <b>Format specifiers (IFormattable):</b> The format string passed to
/// <see cref="ToString(string?, IFormatProvider?)"/> may be one of:
/// </para>
/// <list type="bullet">
///   <item><description><c>null</c> or <c>""</c>: auto-select the largest applicable binary unit.</description></item>
///   <item><description>A unit specifier (case-insensitive): <c>"B"</c>, <c>"KB"</c>, <c>"MB"</c>, <c>"GB"</c>, <c>"TB"</c>.</description></item>
///   <item><description>A .NET numeric format string (e.g., <c>"N2"</c>, <c>"F3"</c>): auto-select the unit, then apply the numeric format.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;DataSize&gt; result = DataSize.FromGigabytes(1.5m);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToMegabytes()); // Outputs: 1536
///     Console.WriteLine(result.Value);               // Outputs: 1.5 GB
///     Console.WriteLine($"{result.Value:MB}");        // Outputs: 1536 MB
///     Console.WriteLine($"{result.Value:N0}");        // Outputs: 2 GB
///     Console.WriteLine(result.Value > DataSize.Zero); // Outputs: True
/// }
/// </code>
/// </example>
public readonly record struct DataSize :
    IValueObject,
    IParsable<DataSize>,
    IComparable<DataSize>,
    IComparable,
    IFormattable
{
    /// <summary>The number of bits in one byte (8).</summary>
    public const long BitsPerByte = 8L;

    /// <summary>The number of bytes in one kilobyte (1024).</summary>
    public const long BytesPerKilobyte = 1024L;

    /// <summary>The number of bytes in one megabyte (1024²).</summary>
    public const long BytesPerMegabyte = BytesPerKilobyte * 1024L;

    /// <summary>The number of bytes in one gigabyte (1024³).</summary>
    public const long BytesPerGigabyte = BytesPerMegabyte * 1024L;

    /// <summary>The number of bytes in one terabyte (1024⁴).</summary>
    public const long BytesPerTerabyte = BytesPerGigabyte * 1024L;

    /// <summary>The number of kilobytes in one megabyte (1024).</summary>
    public const long KilobytesPerMegabyte = 1024L;

    /// <summary>The number of kilobytes in one gigabyte (1024²).</summary>
    public const long KilobytesPerGigabyte = KilobytesPerMegabyte * 1024L;

    /// <summary>The number of kilobytes in one terabyte (1024³).</summary>
    public const long KilobytesPerTerabyte = KilobytesPerGigabyte * 1024L;

    /// <summary>The number of megabytes in one gigabyte (1024).</summary>
    public const long MegabytesPerGigabyte = 1024L;

    /// <summary>The number of megabytes in one terabyte (1024²).</summary>
    public const long MegabytesPerTerabyte = MegabytesPerGigabyte * 1024L;

    /// <summary>The number of gigabytes in one terabyte (1024).</summary>
    public const long GigabytesPerTerabyte = 1024L;

    /// <summary>
    /// Gets the canonical size, in bytes.
    /// </summary>
    /// <value>A non-negative number of bytes.</value>
    /// <remarks>
    /// This is the single source of truth for the value object. All conversions
    /// (to KB, MB, GB, TB) are derived from this value.
    /// </remarks>
    public long Bytes { get; }

    /// <summary>
    /// Gets a value indicating whether the size is zero.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Bytes"/> is exactly zero; otherwise, <see langword="false"/>.</value>
    public bool IsZero => Bytes == 0L;

    /// <summary>
    /// Gets a value indicating whether the size is strictly positive.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Bytes"/> is greater than zero; otherwise, <see langword="false"/>.</value>
    public bool IsPositive => Bytes > 0L;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSize"/> struct.
    /// </summary>
    /// <param name="bytes">The canonical size in bytes. Must be non-negative.</param>
    private DataSize(long bytes) => Bytes = bytes;

    /// <summary>
    /// Gets a zero-sized <see cref="DataSize"/>.
    /// </summary>
    /// <value>A new <see cref="DataSize"/> instance with <see cref="Bytes"/> equal to zero.</value>
    public static DataSize Zero => new(0L);

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a number of bytes.
    /// </summary>
    /// <param name="bytes">The number of bytes. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE</c> when <paramref name="bytes"/> is negative.</description></item>
    /// </list>
    /// </returns>
    public static Result<DataSize> FromBytes(long bytes)
        => bytes < 0L
            ? new Error(
                "KernelR.ValueObjects.DataSize.NEGATIVE",
                "Data size cannot be negative.",
                ErrorType.ValidationError)
            : new DataSize(bytes);

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a number of bits (rounded to the nearest byte).
    /// </summary>
    /// <param name="bits">The number of bits. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE</c> when <paramref name="bits"/> is negative.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The conversion divides <paramref name="bits"/> by <see cref="BitsPerByte"/> and rounds
    /// to the nearest byte using <see cref="MidpointRounding.AwayFromZero"/>. The division
    /// uses <see cref="decimal"/> internally so that no precision is lost for any value of <see cref="long"/>.
    /// </remarks>
    public static Result<DataSize> FromBits(long bits)
    {
        return bits < 0L
            ? new Error(
                "KernelR.ValueObjects.DataSize.NEGATIVE",
                "Data size cannot be negative.",
                ErrorType.ValidationError)
            : FromBytes((long)Math.Round(bits / (decimal)BitsPerByte, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a number of kilobytes.
    /// </summary>
    /// <param name="kilobytes">The number of kilobytes. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE</c> when <paramref name="kilobytes"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<DataSize> FromKilobytes(decimal kilobytes) => FromDecimal(kilobytes, BytesPerKilobyte);

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a number of megabytes.
    /// </summary>
    /// <param name="megabytes">The number of megabytes. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE</c> when <paramref name="megabytes"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<DataSize> FromMegabytes(decimal megabytes) => FromDecimal(megabytes, BytesPerMegabyte);

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a number of gigabytes.
    /// </summary>
    /// <param name="gigabytes">The number of gigabytes. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE</c> when <paramref name="gigabytes"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<DataSize> FromGigabytes(decimal gigabytes) => FromDecimal(gigabytes, BytesPerGigabyte);

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a number of terabytes.
    /// </summary>
    /// <param name="terabytes">The number of terabytes. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE</c> when <paramref name="terabytes"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<DataSize> FromTerabytes(decimal terabytes) => FromDecimal(terabytes, BytesPerTerabyte);

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a numeric value and a unit.
    /// </summary>
    /// <param name="amount">The numeric value. Must be non-negative.</param>
    /// <param name="unit">The unit of measurement. Must be a defined <see cref="DataSizeUnit"/>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE</c> when <paramref name="amount"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_UNIT</c> when <paramref name="unit"/> is not defined.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <b>When to use this method:</b> Use this overload when the unit is determined at runtime
    /// (e.g., from an API request, a database row with a separate unit column, or a UI dropdown).
    /// When the unit is known at compile time, prefer the type-safe <c>FromXxx</c> factories.
    /// </remarks>
    /// <example>
    /// <code>
    /// DataSizeUnit unit = (DataSizeUnit)request.Unit;
    /// Result&lt;DataSize&gt; result = DataSize.Create(request.Amount, unit);
    /// </code>
    /// </example>
    public static Result<DataSize> Create(decimal amount, DataSizeUnit unit)
    {
        return amount < 0m
            ? new Error(
                "KernelR.ValueObjects.DataSize.NEGATIVE",
                "Data size cannot be negative.",
                ErrorType.ValidationError)

            : unit switch
            {
                DataSizeUnit.Byte => FromDecimal(amount, 1L),
                DataSizeUnit.Kilobyte => FromKilobytes(amount),
                DataSizeUnit.Megabyte => FromMegabytes(amount),
                DataSizeUnit.Gigabyte => FromGigabytes(amount),
                DataSizeUnit.Terabyte => FromTerabytes(amount),
                _ => new Error(
                    "KernelR.ValueObjects.DataSize.INVALID_UNIT",
                    FormattableString.Invariant($"Data size unit '{(int)unit}' is not defined."),
                    ErrorType.ValidationError),
            };
    }

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a string in the form <c>"&lt;value&gt; &lt;unit&gt;"</c> or a raw byte count.
    /// </summary>
    /// <param name="value">
    /// The string representation (e.g., <c>"1.5 GB"</c>, <c>"500 MB"</c>, or <c>"1610612736"</c>).
    /// The value uses invariant culture; the unit accepts both full names and short forms (case-insensitive).
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_FORMAT</c> when the input does not match either a raw byte count or the form <c>"&lt;value&gt; &lt;unit&gt;"</c>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_AMOUNT</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_UNIT</c> when the unit part is not recognized.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// DataSize.Create("1.5 GB");      // → DataSize(1.5 GB)
    /// DataSize.Create("500 MB");      // → DataSize(500 MB)
    /// DataSize.Create("0 Byte");      // → DataSize(0 B)
    /// DataSize.Create("1610612736");  // → DataSize(1.5 GB)
    /// DataSize.Create("2 gigabytes"); // → DataSize(2 GB)
    /// </code>
    /// </example>
    public static Result<DataSize> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.DataSize.EMPTY",
                "Data size cannot be empty.",
                ErrorType.ValidationError);

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rawBytes))
            return FromBytes(rawBytes);

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length != 2
            ? new Error(
                "KernelR.ValueObjects.DataSize.INVALID_FORMAT",
                "Data size must be formatted as '<value> <unit>' or a raw byte count.",
                ErrorType.ValidationError)

            : !decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount)
            ? new Error(
                "KernelR.ValueObjects.DataSize.INVALID_AMOUNT",
                "Data size value is not a valid number.",
                ErrorType.ValidationError)

            : !TryParseUnitFactor(parts[1], out long factor)
            ? new Error(
                "KernelR.ValueObjects.DataSize.INVALID_UNIT",
                FormattableString.Invariant($"Data size unit '{parts[1]}' is not recognized."),
                ErrorType.ValidationError)

            : FromDecimal(amount, factor);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="DataSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <returns>A <see cref="DataSize"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>,
    /// or contains an invalid value or unit. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which requires
    /// an <see cref="IFormatProvider"/> parameter. Because <see cref="DataSize"/> is a
    /// machine-to-machine contract, parsing is <b>always</b> performed using
    /// <see cref="CultureInfo.InvariantCulture"/> regardless of the provider passed.
    /// </para>
    /// <para>
    /// In other words: <c>DataSize.Parse("1.5 GB", CultureInfo.GetCultureInfo("de-DE"))</c>
    /// and <c>DataSize.Parse("1.5 GB", null)</c> produce identical results. A German caller
    /// that wants to accept the German decimal separator (<c>"1,5 GB"</c>) must normalize the
    /// input to invariant form (<c>"1.5 GB"</c>) before calling this method.
    /// </para>
    /// <para>
    /// For consistency, the same policy applies to
    /// <see cref="TryParse(string?, IFormatProvider?, out DataSize)"/>.
    /// </para>
    /// </remarks>
    public static DataSize Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="DataSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="DataSize"/> instance, if parsing
    /// succeeded, or the default value if it failed.
    /// </param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which requires
    /// an <see cref="IFormatProvider"/> parameter. Because <see cref="DataSize"/> is a
    /// machine-to-machine contract, parsing is <b>always</b> performed using
    /// <see cref="CultureInfo.InvariantCulture"/> regardless of the provider passed.
    /// </para>
    /// <para>
    /// When only a validity check is needed and the resulting instance is not used, callers
    /// can discard it via <c>TryParse(value, null, out _)</c>.
    /// </para>
    /// </remarks>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out DataSize result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Converts a raw decimal value from one unit to another, without creating a <see cref="DataSize"/>.
    /// </summary>
    /// <param name="value">The raw value to convert.</param>
    /// <param name="fromUnit">The source unit of <paramref name="value"/>.</param>
    /// <param name="toUnit">The target unit for the result.</param>
    /// <returns>
    /// A successful result carrying the converted value, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_UNIT</c> when either unit is not defined.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the intermediate calculation exceeds <see cref="decimal.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is intended for callers who only need the numeric conversion and do not
    /// want to instantiate a <see cref="DataSize"/>. The conversion is performed in
    /// canonical bytes, so it is exact for all supported units.
    /// </para>
    /// <para>
    /// <b>Do not combine this with a <c>FromXxx</c> factory.</b> If the goal is to build a
    /// <see cref="DataSize"/>, call <c>FromXxx</c> directly on the source value. For example,
    /// <c>DataSize.FromMegabytes(1536m)</c> and
    /// <c>DataSize.FromGigabytes(DataSize.Convert(1536m, DataSizeUnit.Megabyte, DataSizeUnit.Gigabyte).OrThrow())</c>
    /// produce the same result — the former is shorter and avoids the extra round-trip.
    /// </para>
    /// <para>
    /// <b>Why the return type is <see cref="decimal"/> and not <see cref="DataSize"/>:</b>
    /// a conversion between units can produce a fractional value that is not representable
    /// as whole bytes. For example, <c>1 MB</c> equals <c>0.00000095367431640625 TB</c>;
    /// returning a <see cref="DataSize"/> would round this to <c>0 TB</c>, silently losing
    /// the entire value. The <see cref="decimal"/> return type preserves the full precision.
    /// If the goal is to obtain a precise numeric result, prefer this method (or
    /// <see cref="ToUnit"/>) over converting to a <see cref="DataSize"/> first.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// decimal megabytes = 1536m;
    /// decimal gigabytes = DataSize.Convert(megabytes, DataSizeUnit.Megabyte, DataSizeUnit.Gigabyte).OrThrow();
    /// // gigabytes = 1.5
    /// </code>
    /// </example>
    public static Result<decimal> Convert(decimal value, DataSizeUnit fromUnit, DataSizeUnit toUnit)
    {
        if (fromUnit == toUnit)
            return value;

        if (!TryGetByteFactor(fromUnit, out long fromFactor))
            return new Error(
                "KernelR.ValueObjects.DataSize.INVALID_UNIT",
                FormattableString.Invariant($"Source unit '{(int)fromUnit}' is not defined."),
                ErrorType.ValidationError);

        if (!TryGetByteFactor(toUnit, out long toFactor))
            return new Error(
                "KernelR.ValueObjects.DataSize.INVALID_UNIT",
                FormattableString.Invariant($"Target unit '{(int)toUnit}' is not defined."),
                ErrorType.ValidationError);

        try
        {
            return value * fromFactor / toFactor;
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.DataSize.OVERFLOW",
                "The conversion resulted in a value that is too large.",
                ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Attempts to convert a raw decimal value from one unit to another.
    /// </summary>
    /// <param name="value">The raw value to convert.</param>
    /// <param name="fromUnit">The source unit of <paramref name="value"/>.</param>
    /// <param name="toUnit">The target unit for the result.</param>
    /// <param name="result">When this method returns, contains the converted value on success, or <c>0</c> on failure.</param>
    /// <returns><see langword="true"/> if the conversion succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryConvert(decimal value, DataSizeUnit fromUnit, DataSizeUnit toUnit, out decimal result)
    {
        result = default;
        return Convert(value, fromUnit, toUnit).TryGetValue(out result);
    }

    /// <summary>Converts this data size to bits.</summary>
    /// <returns>The size in bits (exact for sizes up to <c>long.MaxValue / 8</c>).</returns>
    public decimal ToBits() => (decimal)Bytes * BitsPerByte;

    /// <summary>Converts this data size to bytes.</summary>
    /// <returns>The canonical size in bytes.</returns>
    public long ToBytes() => Bytes;

    /// <summary>Converts this data size to kilobytes.</summary>
    /// <returns>The size in kilobytes (may have a fractional part).</returns>
    public decimal ToKilobytes() => Bytes / (decimal)BytesPerKilobyte;

    /// <summary>Converts this data size to megabytes.</summary>
    /// <returns>The size in megabytes (may have a fractional part).</returns>
    public decimal ToMegabytes() => Bytes / (decimal)BytesPerMegabyte;

    /// <summary>Converts this data size to gigabytes.</summary>
    /// <returns>The size in gigabytes (may have a fractional part).</returns>
    public decimal ToGigabytes() => Bytes / (decimal)BytesPerGigabyte;

    /// <summary>Converts this data size to terabytes.</summary>
    /// <returns>The size in terabytes (may have a fractional part).</returns>
    public decimal ToTerabytes() => Bytes / (decimal)BytesPerTerabyte;

    /// <summary>
    /// Converts this size to the specified unit.
    /// </summary>
    /// <param name="unit">The target unit.</param>
    /// <returns>The size expressed in <paramref name="unit"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unit"/> is not a defined <see cref="DataSizeUnit"/>.</exception>
    /// <remarks>
    /// This method is a convenience wrapper around the individual <c>ToXxx</c> methods,
    /// useful when the target unit is determined at runtime.
    /// </remarks>
    /// <example>
    /// <code>
    /// DataSizeUnit target = (DataSizeUnit)request.TargetUnit;
    /// decimal value = size.ToUnit(target);
    /// </code>
    /// </example>
    public decimal ToUnit(DataSizeUnit unit)
    {
        return unit switch
        {
            DataSizeUnit.Byte => ToBytes(),
            DataSizeUnit.Kilobyte => ToKilobytes(),
            DataSizeUnit.Megabyte => ToMegabytes(),
            DataSizeUnit.Gigabyte => ToGigabytes(),
            DataSizeUnit.Terabyte => ToTerabytes(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(unit),
                FormattableString.Invariant($"Unsupported data size unit: '{(int)unit}'."))
        };
    }

    /// <summary>
    /// Adds the specified data size to the current data size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The data size to add.</param>
    /// <returns>
    /// A successful result carrying the sum, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the sum exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public Result<DataSize> Add(DataSize other)
    {
        try
        {
            long sum = checked(Bytes + other.Bytes);
            return new DataSize(sum);
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.DataSize.OVERFLOW",
                "The addition resulted in a value that is too large.",
                ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Subtracts the specified data size from the current data size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The data size to subtract.</param>
    /// <returns>
    /// A successful result carrying the difference, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE_RESULT</c> when the subtraction would result in a negative value.</description></item>
    /// </list>
    /// </returns>
    public Result<DataSize> Subtract(DataSize other)
    {
        return other.Bytes > Bytes
            ? new Error(
                "KernelR.ValueObjects.DataSize.NEGATIVE_RESULT",
                "Subtraction resulted in a negative data size.",
                ErrorType.ValidationError)
            : new DataSize(Bytes - other.Bytes);
    }

    /// <summary>
    /// Multiplies the data size by a scalar factor, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="multiplier">The scalar factor. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the product, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE_MULTIPLIER</c> when <paramref name="multiplier"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the product exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public Result<DataSize> Multiply(decimal multiplier)
    {
        if (multiplier < 0m)
            return new Error(
                "KernelR.ValueObjects.DataSize.NEGATIVE_MULTIPLIER",
                "Multiplier cannot be negative.",
                ErrorType.ValidationError);

        try
        {
            decimal bytesDecimal = Bytes * multiplier;
            return bytesDecimal > long.MaxValue
                ? new Error(
                    "KernelR.ValueObjects.DataSize.OVERFLOW",
                    "The multiplication resulted in a value that is too large.",
                    ErrorType.ValidationError)
                : new DataSize((long)Math.Round(bytesDecimal, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.DataSize.OVERFLOW",
                "The multiplication resulted in a value that is too large.",
                ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Divides the data size by a scalar divisor, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="divisor">The scalar divisor. Must be strictly positive.</param>
    /// <returns>
    /// A successful result carrying the quotient, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.DIVIDE_BY_ZERO</c> when <paramref name="divisor"/> is zero or negative.</description></item>
    /// </list>
    /// </returns>
    public Result<DataSize> Divide(decimal divisor)
    {
        return divisor <= 0m
            ? new Error(
                "KernelR.ValueObjects.DataSize.DIVIDE_BY_ZERO",
                "Cannot divide by zero or a negative number.",
                ErrorType.ValidationError)
            : new DataSize((long)Math.Round(Bytes / divisor, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Returns a new data size representing the specified percentage of this size.
    /// </summary>
    /// <param name="percent">The percentage (e.g., <c>50</c> for 50%). Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the percentage of the size, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE_PERCENT</c> when <paramref name="percent"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public Result<DataSize> Percentage(decimal percent)
    {
        if (percent < 0m)
            return new Error(
                "KernelR.ValueObjects.DataSize.NEGATIVE_PERCENT",
                "Percentage cannot be negative.",
                ErrorType.ValidationError);

        decimal bytesDecimal = Bytes * percent / 100m;
        return bytesDecimal > long.MaxValue
            ? new Error(
                "KernelR.ValueObjects.DataSize.OVERFLOW",
                "The percentage calculation resulted in a value that is too large.",
                ErrorType.ValidationError)
            : new DataSize((long)Math.Round(bytesDecimal, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Returns the ratio of this size to another size.
    /// </summary>
    /// <param name="other">The other size.</param>
    /// <returns>
    /// A successful result carrying the ratio (e.g., <c>2.5m</c> means "this is 2.5 times larger"), or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.DIVIDE_BY_ZERO</c> when <paramref name="other"/> is zero.</description></item>
    /// </list>
    /// </returns>
    public Result<decimal> Ratio(DataSize other)
    {
        return other.Bytes == 0L
            ? new Error(
                "KernelR.ValueObjects.DataSize.DIVIDE_BY_ZERO",
                "Cannot compute ratio against zero.",
                ErrorType.ValidationError)
            : Bytes / (decimal)other.Bytes;
    }

    /// <summary>Adds two data sizes.</summary>
    /// <param name="left">The first data size.</param>
    /// <param name="right">The second data size.</param>
    /// <returns>The sum of the two data sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sum overflows <see cref="long.MaxValue"/>.</exception>
    public static DataSize operator +(DataSize left, DataSize right) => left.Add(right).OrThrowInvalidOperationException();

    /// <summary>Subtracts one data size from another.</summary>
    /// <param name="left">The first data size.</param>
    /// <param name="right">The data size to subtract.</param>
    /// <returns>The difference between the two data sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is negative.</exception>
    public static DataSize operator -(DataSize left, DataSize right) => left.Subtract(right).OrThrowInvalidOperationException();

    /// <summary>Multiplies a data size by a scalar factor.</summary>
    /// <param name="size">The data size.</param>
    /// <param name="multiplier">The scalar factor.</param>
    /// <returns>The product of the size and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static DataSize operator *(DataSize size, decimal multiplier) => size.Multiply(multiplier).OrThrowInvalidOperationException();

    /// <summary>Multiplies a scalar factor by a data size (commutative support).</summary>
    /// <param name="multiplier">The scalar factor.</param>
    /// <param name="size">The data size.</param>
    /// <returns>The product of the size and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static DataSize operator *(decimal multiplier, DataSize size) => size.Multiply(multiplier).OrThrowInvalidOperationException();

    /// <summary>Divides a data size by a scalar divisor.</summary>
    /// <param name="size">The data size.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The quotient of the size and the divisor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the divisor is zero or negative.</exception>
    public static DataSize operator /(DataSize size, decimal divisor) => size.Divide(divisor).OrThrowInvalidOperationException();

    /// <summary>Returns the ratio of two data sizes as a <see cref="decimal"/>.</summary>
    /// <param name="left">The numerator.</param>
    /// <param name="right">The denominator.</param>
    /// <returns>The ratio of the two sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="right"/> is zero.</exception>
    public static decimal operator /(DataSize left, DataSize right) => left.Ratio(right).OrThrowInvalidOperationException();

    /// <summary>Determines whether the left size is greater than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="DataSize"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(DataSize)"/>.
    /// </remarks>
    public static bool operator >(DataSize left, DataSize right) => left.Bytes > right.Bytes;

    /// <summary>Determines whether the left size is less than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="DataSize"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(DataSize)"/>.
    /// </remarks>
    public static bool operator <(DataSize left, DataSize right) => left.Bytes < right.Bytes;

    /// <summary>Determines whether the left size is greater than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="DataSize"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(DataSize)"/>.
    /// </remarks>
    public static bool operator >=(DataSize left, DataSize right) => left.Bytes >= right.Bytes;

    /// <summary>Determines whether the left size is less than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="DataSize"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(DataSize)"/>.
    /// </remarks>
    public static bool operator <=(DataSize left, DataSize right) => left.Bytes <= right.Bytes;

    /// <inheritdoc />
    public int CompareTo(DataSize other) => Bytes.CompareTo(other.Bytes);

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            DataSize other => CompareTo(other),
            _ => throw new ArgumentException(
                FormattableString.Invariant($"Object must be of type {nameof(DataSize)}."),
                nameof(obj))
        };
    }

    /// <summary>
    /// Returns a human-readable representation of the size, automatically selecting
    /// the largest applicable binary unit (B, KB, MB, GB, or TB).
    /// </summary>
    /// <returns>A culture-invariant string (e.g., <c>"1.5 GB"</c>, <c>"512 B"</c>).</returns>
    /// <remarks>
    /// The threshold check is rounding-aware: if rounding to two decimals would produce
    /// <c>1024</c> or more of the current unit, the next larger unit is used instead
    /// (e.g., <c>1,048,575 B</c> is shown as <c>"1 MB"</c>, not <c>"1024 KB"</c>).
    /// <para>
    /// To produce localized output (e.g., Persian digits), call
    /// <see cref="ToString(IFormatProvider?)"/> with the desired culture explicitly.
    /// </para>
    /// </remarks>
    public override string ToString() => FormatAutoSelected("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns a human-readable representation of the size, using the specified culture.
    /// </summary>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information, or
    /// <see langword="null"/> to use <see cref="CultureInfo.InvariantCulture"/>.
    /// </param>
    /// <returns>A string such as <c>"1.5 GB"</c> or <c>"512 B"</c>.</returns>
    /// <remarks>
    /// The unit is chosen automatically (B, KB, MB, GB, or TB). The threshold check is
    /// rounding-aware (see <see cref="ToString()"/> for details).
    /// </remarks>
    public string ToString(IFormatProvider? provider)
        => FormatAutoSelected("0.##", provider ?? CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns a representation of the size in the specified unit.
    /// </summary>
    /// <param name="unit">The unit to express the size in.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information, or
    /// <see langword="null"/> to use <see cref="CultureInfo.InvariantCulture"/>.
    /// </param>
    /// <returns>A string such as <c>"1536 MB"</c> or <c>"1.5 GB"</c>.</returns>
    /// <remarks>
    /// This overload allows the caller to choose the unit explicitly. Use this when you need
    /// to force a specific unit (e.g., for API responses that always use a fixed unit).
    /// </remarks>
    /// <example>
    /// <code>
    /// DataSize size = DataSize.FromGigabytes(1.5m).OrThrow();
    /// Console.WriteLine(size.ToString(DataSizeUnit.Megabyte)); // "1536 MB"
    /// Console.WriteLine(size.ToString(DataSizeUnit.Byte));     // "1610612736 B"
    /// </code>
    /// </example>
    public string ToString(DataSizeUnit unit, IFormatProvider? provider = null)
    {
        IFormatProvider culture = provider ?? CultureInfo.InvariantCulture;
        decimal value = ToUnit(unit);

        return unit switch
        {
            DataSizeUnit.Byte => string.Format(culture, "{0} B", value),
            DataSizeUnit.Kilobyte => string.Format(culture, "{0:0.##} KB", value),
            DataSizeUnit.Megabyte => string.Format(culture, "{0:0.##} MB", value),
            DataSizeUnit.Gigabyte => string.Format(culture, "{0:0.##} GB", value),
            DataSizeUnit.Terabyte => string.Format(culture, "{0:0.##} TB", value),
            _ => throw new ArgumentOutOfRangeException(
                nameof(unit),
                FormattableString.Invariant($"Unsupported data size unit: '{(int)unit}'."))
        };
    }

    /// <summary>
    /// Formats the size using the specified format string and culture.
    /// </summary>
    /// <param name="format">
    /// The format string. May be:
    /// <list type="bullet">
    ///   <item><description><c>null</c> or <c>""</c> to auto-select the unit.</description></item>
    ///   <item><description>A unit specifier (case-insensitive): <c>"B"</c>, <c>"KB"</c>, <c>"MB"</c>, <c>"GB"</c>, <c>"TB"</c>, or their full forms (e.g., <c>"kilobyte"</c>).</description></item>
    ///   <item><description>A .NET numeric format string (e.g., <c>"N2"</c>, <c>"F3"</c>) to apply to the auto-selected unit.</description></item>
    /// </list>
    /// </param>
    /// <param name="formatProvider">
    /// An object that supplies culture-specific formatting information, or
    /// <see langword="null"/> to use <see cref="CultureInfo.InvariantCulture"/>.
    /// </param>
    /// <returns>The formatted string.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="format"/> is neither a recognized unit specifier
    /// nor a valid .NET numeric format string. This matches the <see cref="IFormattable"/>
    /// contract and the behavior of built-in numeric types.
    /// </exception>
    /// <remarks>
    /// This method implements <see cref="IFormattable"/>, so it is invoked by
    /// string interpolation (e.g., <c>$"{size:MB}"</c>) and by <see cref="string.Format(IFormatProvider, string, object[])"/>.
    /// </remarks>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        IFormatProvider culture = formatProvider ?? CultureInfo.InvariantCulture;

        return string.IsNullOrEmpty(format)
            ? FormatAutoSelected("0.##", culture)

            : TryParseUnit(format, out DataSizeUnit unit)
            ? ToString(unit, culture)

            : FormatAutoSelected(format, culture);
    }

    /// <summary>Explicitly converts a <see cref="DataSize"/> to its underlying byte count.</summary>
    /// <param name="size">The data size to convert.</param>
    /// <returns>The canonical size in bytes.</returns>
    public static explicit operator long(DataSize size) => size.Bytes;

    /// <summary>Explicitly converts a <see cref="DataSize"/> to its human-readable string representation.</summary>
    /// <param name="size">The data size to convert.</param>
    /// <returns>The string representation of the data size, formatted using <see cref="CultureInfo.InvariantCulture"/>.</returns>
    public static explicit operator string(DataSize size) => size.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Creates a <see cref="DataSize"/> from a decimal value multiplied by a byte factor.
    /// </summary>
    /// <param name="value">The decimal value. Must be non-negative.</param>
    /// <param name="factor">The number of bytes per unit.</param>
    /// <returns>A successful result carrying the new <see cref="DataSize"/>, or a failure.</returns>
    private static Result<DataSize> FromDecimal(decimal value, long factor)
    {
        if (value < 0m)
            return new Error(
                "KernelR.ValueObjects.DataSize.NEGATIVE",
                "Data size cannot be negative.",
                ErrorType.ValidationError);

        try
        {
            decimal bytesDecimal = value * factor;
            return bytesDecimal > long.MaxValue
                ? new Error(
                    "KernelR.ValueObjects.DataSize.OVERFLOW",
                    "Data size is too large.",
                    ErrorType.ValidationError)
                : new DataSize((long)Math.Round(bytesDecimal, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.DataSize.OVERFLOW",
                "Data size is too large.",
                ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Gets the byte factor for a given unit.
    /// </summary>
    /// <param name="unit">The unit to look up.</param>
    /// <param name="factor">When this method returns, contains the number of bytes per unit.</param>
    /// <returns><see langword="true"/> if the unit is defined; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetByteFactor(DataSizeUnit unit, out long factor)
    {
        switch (unit)
        {
            case DataSizeUnit.Byte: factor = 1L; return true;
            case DataSizeUnit.Kilobyte: factor = BytesPerKilobyte; return true;
            case DataSizeUnit.Megabyte: factor = BytesPerMegabyte; return true;
            case DataSizeUnit.Gigabyte: factor = BytesPerGigabyte; return true;
            case DataSizeUnit.Terabyte: factor = BytesPerTerabyte; return true;
            default: factor = 0L; return false;
        }
    }

    /// <summary>
    /// Parses a unit name from a string, accepting both full names and short forms.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="unit">When this method returns, contains the parsed unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method intentionally uses <see cref="string.ToUpperInvariant"/> so that unit parsing
    /// is not affected by the current UI culture (e.g., the Turkish-I problem).
    /// </remarks>
    private static bool TryParseUnit(string text, out DataSizeUnit unit)
    {
        switch (text.ToUpperInvariant())
        {
            case "B" or "BYTE" or "BYTES": unit = DataSizeUnit.Byte; return true;
            case "KB" or "KILOBYTE" or "KILOBYTES": unit = DataSizeUnit.Kilobyte; return true;
            case "MB" or "MEGABYTE" or "MEGABYTES": unit = DataSizeUnit.Megabyte; return true;
            case "GB" or "GIGABYTE" or "GIGABYTES": unit = DataSizeUnit.Gigabyte; return true;
            case "TB" or "TERABYTE" or "TERABYTES": unit = DataSizeUnit.Terabyte; return true;
            default: unit = default; return false;
        }
    }

    /// <summary>
    /// Parses a unit name from a string and returns its byte factor.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="factor">When this method returns, contains the number of bytes per unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnitFactor(string text, out long factor)
    {
        factor = 0L;
        return TryParseUnit(text, out DataSizeUnit unit) && TryGetByteFactor(unit, out factor);
    }

    /// <summary>
    /// Formats the size using the auto-selected binary unit (B, KB, MB, GB, or TB) with the
    /// specified numeric format string.
    /// </summary>
    /// <param name="numericFormat">The .NET numeric format string (e.g., "0.##", "N2", "F3").</param>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <returns>A formatted string such as <c>"1.5 GB"</c>.</returns>
    /// <remarks>
    /// <para>
    /// The unit is chosen by comparing the rounded value in each unit against the threshold
    /// for the next larger unit. This avoids the <c>"1024 KB"</c> vs <c>"1 MB"</c> ambiguity
    /// that would occur if the raw (unrounded) value were compared directly.
    /// </para>
    /// <para>
    /// Specifically, <c>1,048,575 B</c> (one byte less than 1 MB) is displayed as
    /// <c>"1 MB"</c>, not <c>"1024 KB"</c>, because rounding 1023.999 KB to two decimals
    /// yields 1024 KB, which is the threshold for one megabyte.
    /// </para>
    /// </remarks>
    private string FormatAutoSelected(string numericFormat, IFormatProvider culture)
    {
        switch (Bytes)
        {
            case < BytesPerKilobyte:
                return string.Format(culture, "{0:" + numericFormat + "} B", Bytes);
            case < BytesPerMegabyte:
                {
                    decimal kb = ToKilobytes();
                    return Math.Round(kb, 2, MidpointRounding.AwayFromZero) >= 1024m
                        ? string.Format(culture, "{0:" + numericFormat + "} MB", ToMegabytes())
                        : string.Format(culture, "{0:" + numericFormat + "} KB", kb);
                }

            case < BytesPerGigabyte:
                {
                    decimal mb = ToMegabytes();
                    return Math.Round(mb, 2, MidpointRounding.AwayFromZero) >= 1024m
                        ? string.Format(culture, "{0:" + numericFormat + "} GB", ToGigabytes())
                        : string.Format(culture, "{0:" + numericFormat + "} MB", mb);
                }

            case < BytesPerTerabyte:
                {
                    decimal gb = ToGigabytes();
                    return Math.Round(gb, 2, MidpointRounding.AwayFromZero) >= 1024m
                        ? string.Format(culture, "{0:" + numericFormat + "} TB", ToTerabytes())
                        : string.Format(culture, "{0:" + numericFormat + "} GB", gb);
                }

            default:
                return string.Format(culture, "{0:" + numericFormat + "} TB", ToTerabytes());
        }
    }
}
