using KernelR.Domain.Enums;
using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a quantity of digital information (storage capacity, file size,
/// memory usage) with a unit, convertible between bytes, KB, MB, GB, and TB.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees the following invariants:
/// </para>
/// <list type="bullet">
///   <item><description>The <see cref="Value"/> is always non-negative (zero is allowed).</description></item>
///   <item><description>
///     The <see cref="Value"/> never exceeds <c>1,000,000</c> in the unit supplied to
///     <see cref="Create(decimal, DataSizeUnit)"/>. Conversions may produce values above
///     this bound, because the bound applies to the input, not to derived values.
///   </description></item>
///   <item><description>
///     The <see cref="Value"/> is always rounded to two decimal places using
///     <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// <para>
/// <b>Binary units:</b> This type uses base-1024 prefixes (1 KB = 1024 B), matching
/// what operating systems and file managers display. See <see cref="DataSizeUnit"/>
/// for the rationale.
/// </para>
/// <para>
/// <b>Mathematical Operations:</b> When adding or subtracting two <see cref="DataSize"/> instances 
/// with different units, the second operand is automatically converted to the unit of the first operand 
/// before the operation is performed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;DataSize&gt; result = DataSize.Create("1.5 GB");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToMegabyte()); // Outputs: 1536 Megabyte
/// }
/// </code>
/// </example>
public readonly record struct DataSize :
    IValueObject,
    IParsable<DataSize>,
    IComparable<DataSize>,
    IComparable
{
    #region ═══ Constants                                                  ═══

    /// <summary>
    /// The maximum allowed value before conversion.
    /// </summary>
    private const decimal MaxValue = 1_000_000m;

    /// <summary>
    /// The number of decimal places to round the value to.
    /// </summary>
    private const int Precision = 2;

    #endregion ═══ Constants                                                  ═══

    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the numeric value, rounded to two decimal places.
    /// </summary>
    /// <value>The numeric value of the data size.</value>
    public decimal Value { get; }

    /// <summary>
    /// Gets the unit of measurement.
    /// </summary>
    /// <value>The <see cref="DataSizeUnit"/> representing the measurement unit.</value>
    public DataSizeUnit Unit { get; }

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSize"/> struct.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="unit">The unit of measurement.</param>
    private DataSize(decimal value, DataSizeUnit unit)
    {
        Value = Math.Round(value, Precision, MidpointRounding.AwayFromZero);
        Unit = unit;
    }

    /// <summary>
    /// Creates a new <see cref="DataSize"/> from a numeric value and a unit.
    /// </summary>
    /// <param name="amount">The numeric value. Must be non-negative and no greater than <c>1,000,000</c>.</param>
    /// <param name="unit">The unit of measurement. Must be a defined <see cref="DataSizeUnit"/>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_AMOUNT</c> when <paramref name="amount"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.TOO_LARGE</c> when <paramref name="amount"/> exceeds <c>1,000,000</c>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_UNIT</c> when <paramref name="unit"/> is not defined.</description></item>
    /// </list>
    /// </returns>
    public static Result<DataSize> Create(decimal amount, DataSizeUnit unit)
    {
        if (amount < 0)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.INVALID_AMOUNT",
                    "Data size cannot be negative.",
                    ErrorType.ValidationError));
        }

        if (amount > MaxValue)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.TOO_LARGE",
                    $"Data size value must not exceed {MaxValue}.",
                    ErrorType.ValidationError));
        }

        if (!Enum.IsDefined<DataSizeUnit>(unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<DataSizeUnit>());
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.INVALID_UNIT",
                    $"Data size unit '{(int)unit}' is not defined. Valid units are: {validUnits}.",
                    ErrorType.ValidationError));
        }

        return Result<DataSize>.Success(new DataSize(amount, unit));
    }

    /// <summary>
    /// Creates a new <see cref="DataSize"/> from a string in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"1.5 GB"</c>, <c>"500 MB"</c>.
    /// The value uses invariant culture; the unit accepts both full names and
    /// short forms (case-insensitive).
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_AMOUNT</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_UNIT</c> when the unit part is not recognized.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// DataSize.Create("1.5 GB");  // → DataSize(1.5, Gigabyte)
    /// DataSize.Create("500 MB");  // → DataSize(500, Megabyte)
    /// DataSize.Create("0 Byte");  // → DataSize(0, Byte)
    /// </code>
    /// </example>
    public static Result<DataSize> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.EMPTY",
                    "Data size cannot be empty.",
                    ErrorType.ValidationError));
        }

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.INVALID_FORMAT",
                    "Data size must be formatted as '<value> <unit>', for example '1.5 GB'.",
                    ErrorType.ValidationError));
        }

        if (!decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.INVALID_AMOUNT",
                    "Data size value is not a valid number.",
                    ErrorType.ValidationError));
        }

        if (!TryParseUnit(parts[1], out DataSizeUnit unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<DataSizeUnit>());
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.INVALID_UNIT",
                    $"Data size unit '{parts[1]}' is not recognized. Valid units are: {validUnits}.",
                    ErrorType.ValidationError));
        }

        return Create(amount, unit);
    }

    /// <summary>
    /// Creates a zero amount in the given unit.
    /// </summary>
    /// <param name="unit">The unit of measurement.</param>
    /// <returns>A <see cref="Result{T}"/> containing a new <see cref="DataSize"/> with a value of zero.</returns>
    public static Result<DataSize> Zero(DataSizeUnit unit) => Create(0m, unit);

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

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
    public static DataSize Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="DataSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="DataSize"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out DataSize result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<DataSize> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Safe Functional Operations (Math & Comparison)           ═══

    // ─── Math Operations (Returns Result) ───

    /// <summary>
    /// Adds the specified data size to the current data size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The data size to add. It will be automatically converted to the current unit.</param>
    /// <returns>
    /// A successful result carrying the sum, or a failure with <c>KernelR.ValueObjects.DataSize.OVERFLOW</c> 
    /// if the result exceeds the maximum capacity.
    /// </returns>
    public Result<DataSize> Add(DataSize other)
    {
        try
        {
            DataSize convertedOther = other.Convert(Unit);
            return Result<DataSize>.Success(new(Value + convertedOther.Value, Unit));
        }
        catch (OverflowException)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.OVERFLOW",
                    "The addition resulted in a value that is too large.",
                    ErrorType.ValidationError));
        }
    }

    /// <summary>
    /// Subtracts the specified data size from the current data size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The data size to subtract. It will be automatically converted to the current unit.</param>
    /// <returns>
    /// A successful result carrying the difference, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.NEGATIVE_RESULT</c> when the subtraction would result in a negative value.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.OVERFLOW</c> when the result exceeds the bounds.</description></item>
    /// </list>
    /// </returns>
    public Result<DataSize> Subtract(DataSize other)
    {
        try
        {
            DataSize convertedOther = other.Convert(Unit);
            decimal newValue = Value - convertedOther.Value;

            if (newValue < 0)
            {
                return Result<DataSize>.Failure(
                    new Error(
                        "KernelR.ValueObjects.DataSize.NEGATIVE_RESULT",
                        "Subtraction resulted in a negative data size.",
                        ErrorType.ValidationError));
            }

            return Result<DataSize>.Success(new(newValue, Unit));
        }
        catch (OverflowException)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.OVERFLOW",
                    "The subtraction resulted in a value that is out of bounds.",
                    ErrorType.ValidationError));
        }
    }

    // ─── Comparison Operations (Returns Result) ───

    /// <summary>
    /// Determines whether the current data size is greater than the specified data size.
    /// </summary>
    public Result<bool> GreaterThan(DataSize other)
    {
        DataSize convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value > convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current data size is less than the specified data size.
    /// </summary>
    public Result<bool> LessThan(DataSize other)
    {
        DataSize convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value < convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current data size is greater than or equal to the specified data size.
    /// </summary>
    public Result<bool> GreaterThanOrEqual(DataSize other)
    {
        DataSize convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value >= convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current data size is less than or equal to the specified data size.
    /// </summary>
    public Result<bool> LessThanOrEqual(DataSize other)
    {
        DataSize convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value <= convertedOther.Value);
    }

    #endregion ═══ Safe Functional Operations (Math & Comparison)              ═══

    #region ═══ Arithmetic Operators                                       ═══

    /// <summary>
    /// Adds two data sizes.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the addition overflows. The message contains JSON-serialized <see cref="Error"/>.</exception>
    public static DataSize operator +(DataSize left, DataSize right)
    {
        return left.Add(right).OrThrowInvalidOperationException();
    }

    /// <summary>
    /// Subtracts one data size from another.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is negative or overflows. The message contains JSON-serialized <see cref="Error"/>.</exception>
    public static DataSize operator -(DataSize left, DataSize right)
    {
        return left.Subtract(right).OrThrowInvalidOperationException();
    }

    #endregion ═══ Arithmetic Operators                                       ═══

    #region ═══ Comparison Operators (DataSize vs DataSize)                  ═══

    /// <summary>Determines whether the left size is greater than the right size.</summary>
    public static bool operator >(DataSize left, DataSize right)
    {
        return left.GreaterThan(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left size is less than the right size.</summary>
    public static bool operator <(DataSize left, DataSize right)
    {
        return left.LessThan(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left size is greater than or equal to the right size.</summary>
    public static bool operator >=(DataSize left, DataSize right)
    {
        return left.GreaterThanOrEqual(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left size is less than or equal to the right size.</summary>
    public static bool operator <=(DataSize left, DataSize right)
    {
        return left.LessThanOrEqual(right).OrThrowInvalidOperationException();
    }

    #endregion ═══ Comparison Operators (DataSize vs DataSize)                 ═══

    #region ═══ IComparable Implementation                                 ═══

    /// <inheritdoc />
    public int CompareTo(DataSize other)
    {
        DataSize convertedOther = other.Convert(Unit);
        return Value.CompareTo(convertedOther.Value);
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1; // Any instance is greater than null.
        }

        if (obj is not DataSize other)
        {
            throw new ArgumentException($"Object must be of type {nameof(DataSize)}.", nameof(obj));
        }

        return CompareTo(other);
    }

    #endregion ═══ IComparable Implementation                                 ═══

    #region ═══ Helpers & Overrides                                        ═══

    /// <summary>
    /// Converts this data size to bytes.
    /// </summary>
    /// <returns>A new <see cref="DataSize"/> expressed in bytes.</returns>
    public DataSize ToByte() => Convert(DataSizeUnit.Byte);

    /// <summary>
    /// Converts this data size to kilobytes.
    /// </summary>
    /// <returns>A new <see cref="DataSize"/> expressed in kilobytes.</returns>
    public DataSize ToKilobyte() => Convert(DataSizeUnit.Kilobyte);

    /// <summary>
    /// Converts this data size to megabytes.
    /// </summary>
    /// <returns>A new <see cref="DataSize"/> expressed in megabytes.</returns>
    public DataSize ToMegabyte() => Convert(DataSizeUnit.Megabyte);

    /// <summary>
    /// Converts this data size to gigabytes.
    /// </summary>
    /// <returns>A new <see cref="DataSize"/> expressed in gigabytes.</returns>
    public DataSize ToGigabyte() => Convert(DataSizeUnit.Gigabyte);

    /// <summary>
    /// Converts this data size to terabytes.
    /// </summary>
    /// <returns>A new <see cref="DataSize"/> expressed in terabytes.</returns>
    public DataSize ToTerabyte() => Convert(DataSizeUnit.Terabyte);

    /// <summary>
    /// Converts this data size to the given target unit.
    /// </summary>
    /// <param name="target">The target unit.</param>
    /// <returns>A new <see cref="DataSize"/> expressed in <paramref name="target"/>, or <c>this</c> if already in that unit.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="target"/> is not a supported unit.</exception>
    private DataSize Convert(DataSizeUnit target)
    {
        if (Unit == target)
        {
            return this;
        }

        decimal inBytes = Value * ToByteFactor(Unit);
        decimal result = inBytes / ToByteFactor(target);

        return new DataSize(result, target);
    }

    /// <summary>
    /// Returns the multiplicative factor that converts one unit into bytes.
    /// </summary>
    /// <param name="unit">The source unit.</param>
    /// <returns>The conversion factor as a decimal.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="unit"/> is not a supported unit.</exception>
    private static decimal ToByteFactor(DataSizeUnit unit) => unit switch
    {
        DataSizeUnit.Byte => 1m,
        DataSizeUnit.Kilobyte => 1024m,
        DataSizeUnit.Megabyte => 1_048_576m,
        DataSizeUnit.Gigabyte => 1_073_741_824m,
        DataSizeUnit.Terabyte => 1_099_511_627_776m,
        _ => throw new InvalidOperationException($"Unsupported data size unit: {unit}"),
    };

    /// <summary>
    /// Parses a unit name, accepting both full names and short forms.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="unit">When this method returns, contains the parsed unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnit(string text, out DataSizeUnit unit)
    {
        switch (text.ToUpperInvariant())
        {
            case "B": unit = DataSizeUnit.Byte; return true;
            case "KB": unit = DataSizeUnit.Kilobyte; return true;
            case "MB": unit = DataSizeUnit.Megabyte; return true;
            case "GB": unit = DataSizeUnit.Gigabyte; return true;
            case "TB": unit = DataSizeUnit.Terabyte; return true;
        }

        if (Enum.TryParse(text, ignoreCase: true, out unit) && Enum.IsDefined<DataSizeUnit>(unit))
        {
            return true;
        }

        unit = default;
        return false;
    }

    /// <summary>
    /// Returns a human-readable representation of the data size.
    /// </summary>
    /// <returns>A string formatted as <c>"&lt;value&gt; &lt;unit&gt;"</c>, for example <c>"1.5 Gigabyte"</c>.</returns>
    public override string ToString() => $"{Value} {Unit}";

    /// <summary>
    /// Explicitly converts a <see cref="DataSize"/> to its string representation.
    /// </summary>
    /// <param name="size">The data size instance to convert.</param>
    /// <returns>The string representation of the data size.</returns>
    public static explicit operator string(DataSize size) => size.ToString();

    #endregion ═══ Helpers & Overrides                                        ═══
}
