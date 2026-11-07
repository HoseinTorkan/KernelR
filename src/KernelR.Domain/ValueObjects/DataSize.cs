using KernelR.Domain.Enums;
using KernelR.Patterns.Results;
using KernelR.Patterns.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

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
/// <b>Zero is valid</b> because empty files and unused storage are meaningful values.
/// Use <see cref="Create(decimal, DataSizeUnit)"/> with any non-negative amount.
/// </para>
/// </remarks>
public readonly record struct DataSize : IParsable<DataSize>
{
    private const decimal MaxValue = 1_000_000m;
    private const int Precision = 2;

    /// <summary>
    /// Gets the numeric value, rounded to two decimal places.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Gets the unit of measurement.
    /// </summary>
    public DataSizeUnit Unit { get; }

    private DataSize(decimal value, DataSizeUnit unit)
    {
        Value = Math.Round(value, Precision, MidpointRounding.AwayFromZero);
        Unit = unit;
    }

    /// <summary>
    /// Creates a new <see cref="DataSize"/> from a numeric value and a unit.
    /// </summary>
    /// <param name="value">
    /// The numeric value. Must be non-negative and no greater than <c>1,000,000</c>.
    /// </param>
    /// <param name="unit">The unit of measurement. Must be a defined <see cref="DataSizeUnit"/>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_VALUE</c> when <paramref name="value"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.TOO_LARGE</c> when <paramref name="value"/> exceeds <c>1,000,000</c>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_UNIT</c> when <paramref name="unit"/> is not a defined value.</description></item>
    /// </list>
    /// </returns>
    public static Result<DataSize> Create(decimal value, DataSizeUnit unit)
    {
        if (value < 0)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.INVALID_VALUE",
                    "Data size cannot be negative.",
                    ErrorType.ValidationError));
        }

        if (value > MaxValue)
        {
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.TOO_LARGE",
                    $"Data size value must not exceed {MaxValue}.",
                    ErrorType.ValidationError));
        }

        if (!Enum.IsDefined(unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<DataSizeUnit>());
            return Result<DataSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.DataSize.INVALID_UNIT",
                    $"Data size unit '{(int)unit}' is not defined. Valid units are: {validUnits}.",
                    ErrorType.ValidationError));
        }

        return Result<DataSize>.Success(new DataSize(value, unit));
    }

    /// <summary>
    /// Creates a new <see cref="DataSize"/> from a string in the form
    /// <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"1.5 GB"</c>, <c>"500 MB"</c>.
    /// The value uses invariant culture; the unit accepts both full names and
    /// short forms (case-insensitive).
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="DataSize"/>, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.DataSize.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.DataSize.INVALID_VALUE</c> when the numeric part is not a valid number.</description></item>
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

        string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
                    "KernelR.ValueObjects.DataSize.INVALID_VALUE",
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

    /// <inheritdoc />
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/>, empty, whitespace, not in the
    /// form <c>"&lt;value&gt; &lt;unit&gt;"</c>, or contains an invalid value or unit.
    /// The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static DataSize Parse(string s, IFormatProvider? provider)
    {
        Result<DataSize> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, KernelRJsonOptions.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
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

    /// <summary>
    /// Creates a zero amount in the given unit.
    /// </summary>
    /// <param name="unit">The unit of measurement.</param>
    public static Result<DataSize> Zero(DataSizeUnit unit) => Create(0m, unit);

    /// <summary>
    /// Converts this data size to bytes.
    /// </summary>
    public DataSize ToByte() => Convert(DataSizeUnit.Byte);

    /// <summary>
    /// Converts this data size to kilobytes.
    /// </summary>
    public DataSize ToKilobyte() => Convert(DataSizeUnit.Kilobyte);

    /// <summary>
    /// Converts this data size to megabytes.
    /// </summary>
    public DataSize ToMegabyte() => Convert(DataSizeUnit.Megabyte);

    /// <summary>
    /// Converts this data size to gigabytes.
    /// </summary>
    public DataSize ToGigabyte() => Convert(DataSizeUnit.Gigabyte);

    /// <summary>
    /// Converts this data size to terabytes.
    /// </summary>
    public DataSize ToTerabyte() => Convert(DataSizeUnit.Terabyte);

    /// <summary>
    /// Converts this data size to the given target unit.
    /// </summary>
    /// <param name="target">The target unit.</param>
    /// <returns>
    /// A new <see cref="DataSize"/> expressed in <paramref name="target"/>, or <c>this</c>
    /// if already in that unit.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="target"/> is not a supported unit.
    /// </exception>
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
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="unit"/> is not a supported unit.
    /// </exception>
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

        if (Enum.TryParse(text, ignoreCase: true, out unit) && Enum.IsDefined(unit))
        {
            return true;
        }

        unit = default;
        return false;
    }

    /// <summary>
    /// Returns a human-readable representation such as <c>1.5 Gigabyte</c>.
    /// </summary>
    public override string ToString() => $"{Value} {Unit}";

    /// <summary>
    /// Implicitly converts a <see cref="DataSize"/> to its string representation.
    /// </summary>
    public static implicit operator string(DataSize size) => size.ToString();
}
