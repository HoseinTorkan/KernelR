using KernelR.Domain.Enums;
using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Measurement;

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
///     <see cref="Create(decimal, PhysicalSizeUnit)"/>. Conversions may produce values above
///     this bound, because the bound applies to the input, not to derived values.
///   </description></item>
///   <item><description>
///     The <see cref="Value"/> is always rounded to two decimal places using
///     <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// <para>
/// <b>Mathematical Operations:</b> When adding or subtracting two <see cref="PhysicalSize"/> instances 
/// with different units, the second operand is automatically converted to the unit of the first operand 
/// before the operation is performed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;PhysicalSize&gt; result = PhysicalSize.Create("5.5 cm");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToInch()); // Outputs: 2.17 Inch
/// }
/// </code>
/// </example>
public readonly record struct PhysicalSize : IValueObject, IParsable<PhysicalSize>, IComparable<PhysicalSize>, IComparable
{
    /// <summary>
    /// The maximum allowed value before conversion.
    /// </summary>
    private const decimal MaxValue = 10_000m;

    /// <summary>
    /// The number of decimal places to round the value to.
    /// </summary>
    private const int Precision = 2;

    /// <summary>
    /// Gets the numeric value, rounded to two decimal places.
    /// </summary>
    /// <value>The numeric value of the physical size.</value>
    public decimal Value { get; }

    /// <summary>
    /// Gets the unit of measurement.
    /// </summary>
    /// <value>The <see cref="PhysicalSizeUnit"/> representing the measurement unit.</value>
    public PhysicalSizeUnit Unit { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalSize"/> struct.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="unit">The unit of measurement.</param>
    private PhysicalSize(decimal value, PhysicalSizeUnit unit)
    {
        Value = Math.Round(value, Precision, MidpointRounding.AwayFromZero);
        Unit = unit;
    }

    /// <summary>
    /// Creates a new <see cref="PhysicalSize"/> from a numeric value and a unit.
    /// </summary>
    /// <param name="value">The numeric value. Must be strictly positive and no greater than <c>10,000</c>.</param>
    /// <param name="unit">The unit of measurement. Must be a defined <see cref="PhysicalSizeUnit"/>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_VALUE</c> when <paramref name="value"/> is less than or equal to zero.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.TOO_LARGE</c> when <paramref name="value"/> exceeds <c>10,000</c>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_UNIT</c> when <paramref name="unit"/> is not defined.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalSize> Create(decimal value, PhysicalSizeUnit unit)
    {
        if (value <= 0)
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_VALUE",
                "Physical size value must be greater than zero.",
                ErrorType.ValidationError);

        if (value > MaxValue)
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.TOO_LARGE",
                $"Physical size value must not exceed {MaxValue}.",
                ErrorType.ValidationError);

        if (!Enum.IsDefined(unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<PhysicalSizeUnit>());
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_UNIT",
                $"Physical size unit '{(int)unit}' is not defined. Valid units are: {validUnits}.",
                ErrorType.ValidationError);
        }

        return new PhysicalSize(value, unit);
    }

    /// <summary>
    /// Creates a new <see cref="PhysicalSize"/> from a string in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"5 cm"</c>, <c>"2.5 ft"</c>.
    /// The value uses invariant culture; the unit accepts both full names and short forms (case-insensitive).
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_VALUE</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_UNIT</c> when the unit part is not recognized.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalSize> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.EMPTY",
                "Physical size cannot be empty.",
                ErrorType.ValidationError);

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_FORMAT",
                "Physical size must be formatted as '<value> <unit>', for example '5 cm'.",
                ErrorType.ValidationError);

        if (!decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_VALUE",
                "Physical size value is not a valid number.",
                ErrorType.ValidationError);

        if (!TryParseUnit(parts[1], out PhysicalSizeUnit unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<PhysicalSizeUnit>());
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_UNIT",
                $"Physical size unit '{parts[1]}' is not recognized. Valid units are: {validUnits}.",
                ErrorType.ValidationError);
        }

        return Create(amount, unit);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="PhysicalSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="PhysicalSize"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>, 
    /// or contains an invalid value or unit. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static PhysicalSize Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="PhysicalSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="PhysicalSize"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PhysicalSize result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Adds the specified physical size to the current size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The physical size to add. It will be automatically converted to the current unit.</param>
    /// <returns>
    /// A successful result carrying the sum.
    /// </returns>
    public Result<PhysicalSize> Add(PhysicalSize other)
    {
        PhysicalSize convertedOther = other.Convert(Unit);
        return new PhysicalSize(Value + convertedOther.Value, Unit);
    }

    /// <summary>
    /// Subtracts the specified physical size from the current size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The physical size to subtract. It will be automatically converted to the current unit.</param>
    /// <returns>
    /// A successful result carrying the difference, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE_OR_ZERO_RESULT</c> when the subtraction would result in a value less than or equal to zero.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalSize> Subtract(PhysicalSize other)
    {
        PhysicalSize convertedOther = other.Convert(Unit);
        decimal newValue = Value - convertedOther.Value;

        return newValue <= 0
            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.NEGATIVE_OR_ZERO_RESULT",
                "Subtraction resulted in a size that is zero or negative.",
                ErrorType.ValidationError)
            : new PhysicalSize(newValue, Unit);
    }

    /// <summary>
    /// Determines whether the current size is greater than the specified size.
    /// </summary>
    /// <param name="other">The physical size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> GreaterThan(PhysicalSize other) => Result<bool>.Success(Value > other.Convert(Unit).Value);

    /// <summary>
    /// Determines whether the current size is less than the specified size.
    /// </summary>
    /// <param name="other">The physical size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> LessThan(PhysicalSize other) => Result<bool>.Success(Value < other.Convert(Unit).Value);

    /// <summary>
    /// Determines whether the current size is greater than or equal to the specified size.
    /// </summary>
    /// <param name="other">The physical size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> GreaterThanOrEqual(PhysicalSize other) => Result<bool>.Success(Value >= other.Convert(Unit).Value);

    /// <summary>
    /// Determines whether the current size is less than or equal to the specified size.
    /// </summary>
    /// <param name="other">The physical size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> LessThanOrEqual(PhysicalSize other) => Result<bool>.Success(Value <= other.Convert(Unit).Value);

    /// <summary>Adds two physical sizes.</summary>
    /// <param name="left">The first physical size.</param>
    /// <param name="right">The second physical size.</param>
    /// <returns>The sum of the two physical sizes.</returns>
    public static PhysicalSize operator +(PhysicalSize left, PhysicalSize right) => left.Add(right).OrThrow();

    /// <summary>Subtracts one physical size from another.</summary>
    /// <param name="left">The first physical size.</param>
    /// <param name="right">The physical size to subtract.</param>
    /// <returns>The difference between the two physical sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is zero or negative.</exception>
    public static PhysicalSize operator -(PhysicalSize left, PhysicalSize right) => left.Subtract(right).OrThrow();

    /// <summary>Determines whether the left size is greater than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(PhysicalSize left, PhysicalSize right) => left.GreaterThan(right).OrThrow();

    /// <summary>Determines whether the left size is less than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(PhysicalSize left, PhysicalSize right) => left.LessThan(right).OrThrow();

    /// <summary>Determines whether the left size is greater than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(PhysicalSize left, PhysicalSize right) => left.GreaterThanOrEqual(right).OrThrow();

    /// <summary>Determines whether the left size is less than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(PhysicalSize left, PhysicalSize right) => left.LessThanOrEqual(right).OrThrow();

    /// <inheritdoc />
    public int CompareTo(PhysicalSize other) => Value.CompareTo(other.Convert(Unit).Value);

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            PhysicalSize other => CompareTo(other),
            _ => throw new ArgumentException($"Object must be of type {nameof(PhysicalSize)}.", nameof(obj))
        };
    }

    /// <summary>Converts this physical size to millimeters.</summary>
    /// <returns>A new <see cref="PhysicalSize"/> expressed in millimeters.</returns>
    public PhysicalSize ToMillimeter() => Convert(PhysicalSizeUnit.Millimeter);

    /// <summary>Converts this physical size to centimeters.</summary>
    /// <returns>A new <see cref="PhysicalSize"/> expressed in centimeters.</returns>
    public PhysicalSize ToCentimeter() => Convert(PhysicalSizeUnit.Centimeter);

    /// <summary>Converts this physical size to meters.</summary>
    /// <returns>A new <see cref="PhysicalSize"/> expressed in meters.</returns>
    public PhysicalSize ToMeter() => Convert(PhysicalSizeUnit.Meter);

    /// <summary>Converts this physical size to inches.</summary>
    /// <returns>A new <see cref="PhysicalSize"/> expressed in inches.</returns>
    public PhysicalSize ToInch() => Convert(PhysicalSizeUnit.Inch);

    /// <summary>Converts this physical size to feet.</summary>
    /// <returns>A new <see cref="PhysicalSize"/> expressed in feet.</returns>
    public PhysicalSize ToFoot() => Convert(PhysicalSizeUnit.Foot);

    /// <summary>
    /// Converts this physical size to the given target unit.
    /// </summary>
    /// <param name="target">The target unit.</param>
    /// <returns>A new <see cref="PhysicalSize"/> expressed in <paramref name="target"/>, or <c>this</c> if already in that unit.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="target"/> is not a supported unit.</exception>
    private PhysicalSize Convert(PhysicalSizeUnit target)
    {
        if (Unit == target) return this;
        decimal inMillimeters = Value * ToMillimeterFactor(Unit);
        return new PhysicalSize(inMillimeters / ToMillimeterFactor(target), target);
    }

    /// <summary>
    /// Returns the multiplicative factor that converts one unit into millimeters.
    /// </summary>
    /// <param name="unit">The source unit.</param>
    /// <returns>The conversion factor as a decimal.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="unit"/> is not a supported unit.</exception>
    private static decimal ToMillimeterFactor(PhysicalSizeUnit unit) => unit switch
    {
        PhysicalSizeUnit.Millimeter => 1m,
        PhysicalSizeUnit.Centimeter => 10m,
        PhysicalSizeUnit.Meter => 1000m,
        PhysicalSizeUnit.Inch => 25.4m,
        PhysicalSizeUnit.Foot => 304.8m,
        _ => throw new InvalidOperationException($"Unsupported physical size unit: {unit}"),
    };

    /// <summary>
    /// Parses a unit name, accepting both full names and short forms.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="unit">When this method returns, contains the parsed unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnit(string text, out PhysicalSizeUnit unit)
    {
        switch (text.ToLowerInvariant())
        {
            case "mm": unit = PhysicalSizeUnit.Millimeter; return true;
            case "cm": unit = PhysicalSizeUnit.Centimeter; return true;
            case "m": unit = PhysicalSizeUnit.Meter; return true;
            case "in": unit = PhysicalSizeUnit.Inch; return true;
            case "ft": unit = PhysicalSizeUnit.Foot; return true;
        }

        if (Enum.TryParse(text, ignoreCase: true, out unit) && Enum.IsDefined(unit))
            return true;

        unit = default;
        return false;
    }

    /// <summary>
    /// Returns a human-readable representation of the physical size.
    /// </summary>
    /// <returns>A string formatted as <c>"&lt;value&gt; &lt;unit&gt;"</c>, for example <c>"5.5 Centimeter"</c>.</returns>
    public override string ToString() => $"{Value} {Unit}";

    /// <summary>
    /// Explicitly converts a <see cref="PhysicalSize"/> to its string representation.
    /// </summary>
    /// <param name="size">The physical size instance to convert.</param>
    /// <returns>The string representation of the physical size.</returns>
    public static explicit operator string(PhysicalSize size) => size.ToString();
}
