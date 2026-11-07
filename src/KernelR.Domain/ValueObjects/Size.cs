using KernelR.Domain.Enums;
using KernelR.Patterns.Results;
using KernelR.Patterns.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a physical size (length, width, height, or diameter) with a unit,
/// convertible between millimeters, centimeters, meters, inches, and feet.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees the following invariants:
/// </para>
/// <list type="bullet">
///   <item><description>The <see cref="Value"/> is always strictly positive.</description></item>
///   <item><description>
///     The <see cref="Value"/> never exceeds <c>10,000</c> in the unit supplied to
///     <see cref="Create(decimal, SizeUnit)"/>.
///   </description></item>
///   <item><description>
///     The <see cref="Value"/> is always rounded to two decimal places using
///     <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// </remarks>
public readonly record struct Size : IParsable<Size>
{
    private const decimal MaxValue = 10_000m;
    private const int Precision = 2;

    /// <summary>Gets the numeric value, rounded to two decimal places.</summary>
    public decimal Value { get; }

    /// <summary>Gets the unit of measurement.</summary>
    public SizeUnit Unit { get; }

    private Size(decimal value, SizeUnit unit)
    {
        Value = Math.Round(value, Precision, MidpointRounding.AwayFromZero);
        Unit = unit;
    }

    /// <summary>
    /// Creates a new <see cref="Size"/> from a numeric value and a unit.
    /// </summary>
    public static Result<Size> Create(decimal value, SizeUnit unit)
    {
        if (value <= 0)
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.INVALID_VALUE",
                    "Size value must be greater than zero.",
                    ErrorType.ValidationError));
        }

        if (value > MaxValue)
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.TOO_LARGE",
                    $"Size value must not exceed {MaxValue}.",
                    ErrorType.ValidationError));
        }

        if (!Enum.IsDefined(unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<SizeUnit>());
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.INVALID_UNIT",
                    $"Size unit '{(int)unit}' is not defined. Valid units are: {validUnits}.",
                    ErrorType.ValidationError));
        }

        return Result<Size>.Success(new Size(value, unit));
    }

    /// <summary>
    /// Creates a new <see cref="Size"/> from a string in the form
    /// <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    public static Result<Size> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.EMPTY",
                    "Size cannot be empty.",
                    ErrorType.ValidationError));
        }

        string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.INVALID_FORMAT",
                    "Size must be formatted as '<value> <unit>', for example '5 cm'.",
                    ErrorType.ValidationError));
        }

        if (!decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.INVALID_VALUE",
                    "Size value is not a valid number.",
                    ErrorType.ValidationError));
        }

        if (!TryParseUnit(parts[1], out SizeUnit unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<SizeUnit>());
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.INVALID_UNIT",
                    $"Size unit '{parts[1]}' is not recognized. Valid units are: {validUnits}.",
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
    public static Size Parse(string s, IFormatProvider? provider)
    {
        Result<Size> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, KernelRJsonOptions.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Size result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<Size> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    /// <summary>Converts this size to millimeters.</summary>
    public Size ToMillimeter() => Convert(SizeUnit.Millimeter);

    /// <summary>Converts this size to centimeters.</summary>
    public Size ToCentimeter() => Convert(SizeUnit.Centimeter);

    /// <summary>Converts this size to meters.</summary>
    public Size ToMeter() => Convert(SizeUnit.Meter);

    /// <summary>Converts this size to inches.</summary>
    public Size ToInch() => Convert(SizeUnit.Inch);

    /// <summary>Converts this size to feet.</summary>
    public Size ToFoot() => Convert(SizeUnit.Foot);

    private Size Convert(SizeUnit target)
    {
        if (Unit == target)
        {
            return this;
        }

        decimal inMillimeters = Value * ToMillimeterFactor(Unit);
        decimal result = inMillimeters / ToMillimeterFactor(target);

        return new Size(result, target);
    }

    private static decimal ToMillimeterFactor(SizeUnit unit) => unit switch
    {
        SizeUnit.Millimeter => 1m,
        SizeUnit.Centimeter => 10m,
        SizeUnit.Meter => 1000m,
        SizeUnit.Inch => 25.4m,
        SizeUnit.Foot => 304.8m,
        _ => throw new InvalidOperationException($"Unsupported size unit: {unit}"),
    };

    private static bool TryParseUnit(string text, out SizeUnit unit)
    {
        switch (text.ToLowerInvariant())
        {
            case "mm": unit = SizeUnit.Millimeter; return true;
            case "cm": unit = SizeUnit.Centimeter; return true;
            case "m": unit = SizeUnit.Meter; return true;
            case "in": unit = SizeUnit.Inch; return true;
            case "ft": unit = SizeUnit.Foot; return true;
        }

        if (Enum.TryParse(text, ignoreCase: true, out unit) && Enum.IsDefined(unit))
        {
            return true;
        }

        unit = default;
        return false;
    }

    /// <summary>
    /// Returns a human-readable representation such as <c>5.5 Centimeter</c>.
    /// </summary>
    public override string ToString() => $"{Value} {Unit}";

    /// <summary>
    /// Implicitly converts a <see cref="Size"/> to its string representation.
    /// </summary>
    public static implicit operator string(Size size) => size.ToString();
}
