using KernelR.Domain.Enums;
using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
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
/// <b>Mathematical Operations:</b> When adding or subtracting two <see cref="DataSize"/> instances
/// with different units, the operation is performed directly on the canonical byte counts,
/// so no unit conversion or precision loss is ever needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;DataSize&gt; result = DataSize.FromGigabytes(1.5m);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToMegabytes()); // Outputs: 1536
///     Console.WriteLine(result.Value);               // Outputs: 1.5 GB
/// }
/// </code>
/// </example>
public readonly record struct DataSize :
    IValueObject,
    IParsable<DataSize>,
    IComparable<DataSize>,
    IComparable
{
    /// <summary>
    /// The number of bytes in one kilobyte (1024).
    /// </summary>
    private const long BytesPerKilobyte = 1024L;

    /// <summary>
    /// The number of bytes in one megabyte (1024²).
    /// </summary>
    private const long BytesPerMegabyte = BytesPerKilobyte * 1024L;

    /// <summary>
    /// The number of bytes in one gigabyte (1024³).
    /// </summary>
    private const long BytesPerGigabyte = BytesPerMegabyte * 1024L;

    /// <summary>
    /// The number of bytes in one terabyte (1024⁴).
    /// </summary>
    private const long BytesPerTerabyte = BytesPerGigabyte * 1024L;

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
            ? new Error("KernelR.ValueObjects.DataSize.NEGATIVE", "Data size cannot be negative.", ErrorType.ValidationError)
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
    /// The conversion divides <paramref name="bits"/> by 8 and rounds to the nearest byte
    /// using <see cref="MidpointRounding.AwayFromZero"/>.
    /// </remarks>
    public static Result<DataSize> FromBits(long bits)
    {
        return bits < 0L
            ? new Error("KernelR.ValueObjects.DataSize.NEGATIVE", "Data size cannot be negative.", ErrorType.ValidationError)
            : FromBytes((long)Math.Round(bits / 8.0, MidpointRounding.AwayFromZero));
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
                    $"Data size unit '{(int)unit}' is not defined.",
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
    /// DataSize.Create("1.5 GB");     // → DataSize(1.5 GB)
    /// DataSize.Create("500 MB");     // → DataSize(500 MB)
    /// DataSize.Create("0 Byte");     // → DataSize(0 B)
    /// DataSize.Create("1610612736"); // → DataSize(1.5 GB)
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
        if (parts.Length != 2)
            return new Error(
                "KernelR.ValueObjects.DataSize.INVALID_FORMAT",
                "Data size must be formatted as '<value> <unit>' or a raw byte count.",
                ErrorType.ValidationError);

        if (!decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            return new Error(
                "KernelR.ValueObjects.DataSize.INVALID_AMOUNT",
                "Data size value is not a valid number.",
                ErrorType.ValidationError);

        return !TryParseUnitFactor(parts[1], out long factor)
            ? new Error(
                "KernelR.ValueObjects.DataSize.INVALID_UNIT",
                $"Data size unit '{parts[1]}' is not recognized.",
                ErrorType.ValidationError)
            : FromDecimal(amount, factor);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="DataSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="DataSize"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>,
    /// or contains an invalid value or unit. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static DataSize Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="DataSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="DataSize"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out DataSize result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Converts this data size to bits.
    /// </summary>
    /// <returns>The size in bits (exact for sizes up to <c>long.MaxValue / 8</c>).</returns>
    public decimal ToBits() => Bytes * 8m;

    /// <summary>
    /// Converts this data size to bytes.
    /// </summary>
    /// <returns>The canonical size in bytes.</returns>
    public long ToBytes() => Bytes;

    /// <summary>
    /// Converts this data size to kilobytes.
    /// </summary>
    /// <returns>The size in kilobytes (may have a fractional part).</returns>
    public decimal ToKilobytes() => Bytes / (decimal)BytesPerKilobyte;

    /// <summary>
    /// Converts this data size to megabytes.
    /// </summary>
    /// <returns>The size in megabytes (may have a fractional part).</returns>
    public decimal ToMegabytes() => Bytes / (decimal)BytesPerMegabyte;

    /// <summary>
    /// Converts this data size to gigabytes.
    /// </summary>
    /// <returns>The size in gigabytes (may have a fractional part).</returns>
    public decimal ToGigabytes() => Bytes / (decimal)BytesPerGigabyte;

    /// <summary>
    /// Converts this data size to terabytes.
    /// </summary>
    /// <returns>The size in terabytes (may have a fractional part).</returns>
    public decimal ToTerabytes() => Bytes / (decimal)BytesPerTerabyte;

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
            return new Error("KernelR.ValueObjects.DataSize.OVERFLOW", "The addition resulted in a value that is too large.", ErrorType.ValidationError);
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
            ? new Error("KernelR.ValueObjects.DataSize.NEGATIVE_RESULT", "Subtraction resulted in a negative data size.", ErrorType.ValidationError)
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
            return new Error("KernelR.ValueObjects.DataSize.NEGATIVE_MULTIPLIER", "Multiplier cannot be negative.", ErrorType.ValidationError);
        try
        {
            decimal bytesDecimal = Bytes * multiplier;
            return bytesDecimal > long.MaxValue
                ? (Result<DataSize>)new Error(
                    "KernelR.ValueObjects.DataSize.OVERFLOW",
                    "The multiplication resulted in a value that is too large.",
                    ErrorType.ValidationError)
                : (Result<DataSize>)new DataSize((long)Math.Round(bytesDecimal, MidpointRounding.AwayFromZero));
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
            return new Error("KernelR.ValueObjects.DataSize.NEGATIVE_PERCENT", "Percentage cannot be negative.", ErrorType.ValidationError);
        decimal bytesDecimal = Bytes * percent / 100m;
        return bytesDecimal > long.MaxValue
            ? new Error("KernelR.ValueObjects.DataSize.OVERFLOW", "The percentage calculation resulted in a value that is too large.", ErrorType.ValidationError)
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
        if (other.Bytes == 0L)
            return new Error("KernelR.ValueObjects.DataSize.DIVIDE_BY_ZERO", "Cannot compute ratio against zero.", ErrorType.ValidationError);
        return Result<decimal>.Success(Bytes / (decimal)other.Bytes);
    }

    /// <summary>
    /// Returns the smaller of two sizes.
    /// </summary>
    /// <param name="other">The other size.</param>
    /// <returns>The size with the smaller <see cref="Bytes"/> value.</returns>
    public DataSize Min(DataSize other) => Bytes <= other.Bytes ? this : other;

    /// <summary>
    /// Returns the larger of two sizes.
    /// </summary>
    /// <param name="other">The other size.</param>
    /// <returns>The size with the larger <see cref="Bytes"/> value.</returns>
    public DataSize Max(DataSize other) => Bytes >= other.Bytes ? this : other;

    /// <summary>Determines whether the current data size is greater than the specified data size.</summary>
    /// <param name="other">The data size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> GreaterThan(DataSize other) => Result<bool>.Success(Bytes > other.Bytes);

    /// <summary>Determines whether the current data size is less than the specified data size.</summary>
    /// <param name="other">The data size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> LessThan(DataSize other) => Result<bool>.Success(Bytes < other.Bytes);

    /// <summary>Determines whether the current data size is greater than or equal to the specified data size.</summary>
    /// <param name="other">The data size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> GreaterThanOrEqual(DataSize other) => Result<bool>.Success(Bytes >= other.Bytes);

    /// <summary>Determines whether the current data size is less than or equal to the specified data size.</summary>
    /// <param name="other">The data size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> LessThanOrEqual(DataSize other) => Result<bool>.Success(Bytes <= other.Bytes);

    /// <summary>Adds two data sizes.</summary>
    /// <param name="left">The first data size.</param>
    /// <param name="right">The second data size.</param>
    /// <returns>The sum of the two data sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sum overflows <see cref="long.MaxValue"/>.</exception>
    public static DataSize operator +(DataSize left, DataSize right) => left.Add(right).OrThrow();

    /// <summary>Subtracts one data size from another.</summary>
    /// <param name="left">The first data size.</param>
    /// <param name="right">The data size to subtract.</param>
    /// <returns>The difference between the two data sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is negative.</exception>
    public static DataSize operator -(DataSize left, DataSize right) => left.Subtract(right).OrThrow();

    /// <summary>Multiplies a data size by a scalar factor.</summary>
    /// <param name="size">The data size.</param>
    /// <param name="multiplier">The scalar factor.</param>
    /// <returns>The product of the size and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static DataSize operator *(DataSize size, decimal multiplier) => size.Multiply(multiplier).OrThrow();

    /// <summary>Multiplies a scalar factor by a data size (commutative support).</summary>
    /// <param name="multiplier">The scalar factor.</param>
    /// <param name="size">The data size.</param>
    /// <returns>The product of the size and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static DataSize operator *(decimal multiplier, DataSize size) => size.Multiply(multiplier).OrThrow();

    /// <summary>Divides a data size by a scalar divisor.</summary>
    /// <param name="size">The data size.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The quotient of the size and the divisor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the divisor is zero or negative.</exception>
    public static DataSize operator /(DataSize size, decimal divisor) => size.Divide(divisor).OrThrow();

    /// <summary>Returns the ratio of two data sizes as a <see cref="decimal"/>.</summary>
    /// <param name="left">The numerator.</param>
    /// <param name="right">The denominator.</param>
    /// <returns>The ratio of the two sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="right"/> is zero.</exception>
    public static decimal operator /(DataSize left, DataSize right) => left.Ratio(right).OrThrow();

    /// <summary>Determines whether the left size is greater than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(DataSize left, DataSize right) => left.Bytes > right.Bytes;

    /// <summary>Determines whether the left size is less than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(DataSize left, DataSize right) => left.Bytes < right.Bytes;

    /// <summary>Determines whether the left size is greater than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(DataSize left, DataSize right) => left.Bytes >= right.Bytes;

    /// <summary>Determines whether the left size is less than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
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
            _ => throw new ArgumentException($"Object must be of type {nameof(DataSize)}.", nameof(obj))
        };
    }

    /// <summary>
    /// Returns a human-readable representation of the size, automatically selecting
    /// the largest applicable binary unit (B, KB, MB, GB, or TB).
    /// </summary>
    /// <returns>A culture-invariant string (e.g., <c>"1.5 GB"</c>, <c>"512 B"</c>).</returns>
    public override string ToString()
    {
        return Bytes switch
        {
            >= BytesPerTerabyte => string.Format(CultureInfo.InvariantCulture, "{0:0.##} TB", ToTerabytes()),
            >= BytesPerGigabyte => string.Format(CultureInfo.InvariantCulture, "{0:0.##} GB", ToGigabytes()),
            >= BytesPerMegabyte => string.Format(CultureInfo.InvariantCulture, "{0:0.##} MB", ToMegabytes()),
            >= BytesPerKilobyte => string.Format(CultureInfo.InvariantCulture, "{0:0.##} KB", ToKilobytes()),
            _ => string.Format(CultureInfo.InvariantCulture, "{0} B", Bytes)
        };
    }

    /// <summary>Explicitly converts a <see cref="DataSize"/> to its underlying byte count.</summary>
    /// <param name="size">The data size to convert.</param>
    /// <returns>The canonical size in bytes.</returns>
    public static explicit operator long(DataSize size) => size.Bytes;

    /// <summary>Explicitly converts a <see cref="DataSize"/> to its human-readable string representation.</summary>
    /// <param name="size">The data size to convert.</param>
    /// <returns>The string representation of the data size.</returns>
    public static explicit operator string(DataSize size) => size.ToString();

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
    /// Parses a unit name, accepting both full names and short forms, and returns
    /// the number of bytes per unit.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="factor">When this method returns, contains the number of bytes per unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnitFactor(string text, out long factor)
    {
        switch (text.ToUpperInvariant())
        {
            case "B": factor = 1L; return true;
            case "KB": factor = BytesPerKilobyte; return true;
            case "MB": factor = BytesPerMegabyte; return true;
            case "GB": factor = BytesPerGigabyte; return true;
            case "TB": factor = BytesPerTerabyte; return true;
            default: factor = 0L; return false;
        }
    }
}
