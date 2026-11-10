using KernelR.Domain.Enums;
using KernelR.Patterns;
using KernelR.Patterns.Results;
using KernelR.Patterns.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Measurement;

/// <summary>
/// Represents a physical weight or mass (e.g., product weight, shipment mass) with a unit,
/// convertible between milligrams, grams, kilograms, tons, pounds, and ounces.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees the following invariants:
/// </para>
/// <list type="bullet">
///   <item><description>The <see cref="Value"/> is always strictly positive.</description></item>
///   <item><description>
///     The <see cref="Value"/> never exceeds <c>1,000,000</c> in the unit supplied to
///     <see cref="Create(decimal, PhysicalWeightUnit)"/>. Conversions may produce values above
///     this bound, because the bound applies to the input, not to derived values.
///   </description></item>
///   <item><description>
///     The <see cref="Value"/> is always rounded to two decimal places using
///     <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// <para>
/// <b>Mathematical Operations:</b> When adding or subtracting two <see cref="PhysicalWeight"/> instances 
/// with different units, the second operand is automatically converted to the unit of the first operand 
/// before the operation is performed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;PhysicalWeight&gt; result = PhysicalWeight.Create("2.5 kg");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToPound()); // Outputs: 5.51 Pound
/// }
/// </code>
/// </example>
public readonly record struct PhysicalWeight :
    IValueObject,
    IParsable<PhysicalWeight>,
    IComparable<PhysicalWeight>,
    IComparable
{
    #region ═══ Constants & Fields                                       ═══

    /// <summary>
    /// The maximum allowed value before conversion.
    /// </summary>
    private const decimal MaxValue = 1_000_000m;

    /// <summary>
    /// The number of decimal places to round the value to.
    /// </summary>
    private const int Precision = 2;

    #endregion ═══ Constants & Fields                                       ═══

    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the numeric value, rounded to two decimal places.
    /// </summary>
    /// <value>The numeric value of the physical weight.</value>
    public decimal Value { get; }

    /// <summary>
    /// Gets the unit of measurement.
    /// </summary>
    /// <value>The <see cref="PhysicalWeightUnit"/> representing the measurement unit.</value>
    public PhysicalWeightUnit Unit { get; }

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalWeight"/> struct.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="unit">The unit of measurement.</param>
    private PhysicalWeight(decimal value, PhysicalWeightUnit unit)
    {
        Value = Math.Round(value, Precision, MidpointRounding.AwayFromZero);
        Unit = unit;
    }

    /// <summary>
    /// Creates a new <see cref="PhysicalWeight"/> from a numeric value and a unit.
    /// </summary>
    /// <param name="value">The numeric value. Must be strictly positive and no greater than <c>1,000,000</c>.</param>
    /// <param name="unit">The unit of measurement. Must be a defined <see cref="PhysicalWeightUnit"/>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_VALUE</c> when <paramref name="value"/> is less than or equal to zero.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.TOO_LARGE</c> when <paramref name="value"/> exceeds <c>1,000,000</c>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT</c> when <paramref name="unit"/> is not defined.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalWeight> Create(decimal value, PhysicalWeightUnit unit)
    {
        if (value <= 0)
        {
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.INVALID_VALUE",
                    "Physical weight value must be greater than zero.",
                    ErrorType.ValidationError));
        }

        if (value > MaxValue)
        {
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.TOO_LARGE",
                    $"Physical weight value must not exceed {MaxValue}.",
                    ErrorType.ValidationError));
        }

        if (!Enum.IsDefined<PhysicalWeightUnit>(unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<PhysicalWeightUnit>());
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT",
                    $"Physical weight unit '{(int)unit}' is not defined. Valid units are: {validUnits}.",
                    ErrorType.ValidationError));
        }

        return Result<PhysicalWeight>.Success(new PhysicalWeight(value, unit));
    }

    /// <summary>
    /// Creates a new <see cref="PhysicalWeight"/> from a string in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"2.5 kg"</c>, <c>"500 g"</c>.
    /// The value uses invariant culture; the unit accepts both full names and short forms (case-insensitive).
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_VALUE</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT</c> when the unit part is not recognized.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalWeight> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.EMPTY",
                    "Physical weight cannot be empty.",
                    ErrorType.ValidationError));
        }

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.INVALID_FORMAT",
                    "Physical weight must be formatted as '<value> <unit>', for example '2.5 kg'.",
                    ErrorType.ValidationError));
        }

        if (!decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
        {
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.INVALID_VALUE",
                    "Physical weight value is not a valid number.",
                    ErrorType.ValidationError));
        }

        if (!TryParseUnit(parts[1], out PhysicalWeightUnit unit))
        {
            string validUnits = string.Join(", ", Enum.GetNames<PhysicalWeightUnit>());
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT",
                    $"Physical weight unit '{parts[1]}' is not recognized. Valid units are: {validUnits}.",
                    ErrorType.ValidationError));
        }

        return Create(amount, unit);
    }

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <summary>
    /// Parses the specified string into a <see cref="PhysicalWeight"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="PhysicalWeight"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>, 
    /// or contains an invalid value or unit. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static PhysicalWeight Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="PhysicalWeight"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="PhysicalWeight"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PhysicalWeight result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<PhysicalWeight> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Safe Functional Operations (Math & Comparison)              ═══

    // ─── Math Operations (Returns Result) ───

    /// <summary>
    /// Adds the specified physical weight to the current weight, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The physical weight to add. It will be automatically converted to the current unit.</param>
    /// <returns>
    /// A successful result carrying the sum, or a failure with <c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> 
    /// if the result exceeds the maximum capacity.
    /// </returns>
    public Result<PhysicalWeight> Add(PhysicalWeight other)
    {
        try
        {
            PhysicalWeight convertedOther = other.Convert(Unit);
            return Result<PhysicalWeight>.Success(new(Value + convertedOther.Value, Unit));
        }
        catch (OverflowException)
        {
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.OVERFLOW",
                    "The addition resulted in a value that is too large.",
                    ErrorType.ValidationError));
        }
    }

    /// <summary>
    /// Subtracts the specified physical weight from the current weight, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The physical weight to subtract. It will be automatically converted to the current unit.</param>
    /// <returns>
    /// A successful result carrying the difference, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE_OR_ZERO_RESULT</c> when the subtraction would result in a value less than or equal to zero.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds the bounds.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalWeight> Subtract(PhysicalWeight other)
    {
        try
        {
            PhysicalWeight convertedOther = other.Convert(Unit);
            decimal newValue = Value - convertedOther.Value;

            if (newValue <= 0)
            {
                return Result<PhysicalWeight>.Failure(
                    new Error(
                        "KernelR.ValueObjects.PhysicalWeight.NEGATIVE_OR_ZERO_RESULT",
                        "Subtraction resulted in a weight that is zero or negative.",
                        ErrorType.ValidationError));
            }

            return Result<PhysicalWeight>.Success(new(newValue, Unit));
        }
        catch (OverflowException)
        {
            return Result<PhysicalWeight>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhysicalWeight.OVERFLOW",
                    "The subtraction resulted in a value that is out of bounds.",
                    ErrorType.ValidationError));
        }
    }

    // ─── Comparison Operations (Returns Result) ───

    /// <summary>
    /// Determines whether the current weight is greater than the specified weight.
    /// </summary>
    public Result<bool> GreaterThan(PhysicalWeight other)
    {
        PhysicalWeight convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value > convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current weight is less than the specified weight.
    /// </summary>
    public Result<bool> LessThan(PhysicalWeight other)
    {
        PhysicalWeight convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value < convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current weight is greater than or equal to the specified weight.
    /// </summary>
    public Result<bool> GreaterThanOrEqual(PhysicalWeight other)
    {
        PhysicalWeight convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value >= convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current weight is less than or equal to the specified weight.
    /// </summary>
    public Result<bool> LessThanOrEqual(PhysicalWeight other)
    {
        PhysicalWeight convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value <= convertedOther.Value);
    }

    #endregion ═══ Safe Functional Operations (Math & Comparison)              ═══

    #region ═══ Arithmetic Operators                                       ═══

    /// <summary>Adds two physical weights.</summary>
    public static PhysicalWeight operator +(PhysicalWeight left, PhysicalWeight right)
    {
        return left.Add(right).OrThrowInvalidOperationException();
    }

    /// <summary>Subtracts one physical weight from another.</summary>
    public static PhysicalWeight operator -(PhysicalWeight left, PhysicalWeight right)
    {
        return left.Subtract(right).OrThrowInvalidOperationException();
    }

    #endregion ═══ Arithmetic Operators                                       ═══

    #region ═══ Comparison Operators (PhysicalWeight vs PhysicalWeight)     ═══

    /// <summary>Determines whether the left weight is greater than the right weight.</summary>
    public static bool operator >(PhysicalWeight left, PhysicalWeight right)
    {
        return left.GreaterThan(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left weight is less than the right weight.</summary>
    public static bool operator <(PhysicalWeight left, PhysicalWeight right)
    {
        return left.LessThan(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left weight is greater than or equal to the right weight.</summary>
    public static bool operator >=(PhysicalWeight left, PhysicalWeight right)
    {
        return left.GreaterThanOrEqual(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left weight is less than or equal to the right weight.</summary>
    public static bool operator <=(PhysicalWeight left, PhysicalWeight right)
    {
        return left.LessThanOrEqual(right).OrThrowInvalidOperationException();
    }

    #endregion ═══ Comparison Operators (PhysicalWeight vs PhysicalWeight)     ═══

    #region ═══ IComparable Implementation                                 ═══

    /// <inheritdoc />
    public int CompareTo(PhysicalWeight other)
    {
        PhysicalWeight convertedOther = other.Convert(Unit);
        return Value.CompareTo(convertedOther.Value);
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1; // Any instance is greater than null.
        }

        if (obj is not PhysicalWeight other)
        {
            throw new ArgumentException($"Object must be of type {nameof(PhysicalWeight)}.", nameof(obj));
        }

        return CompareTo(other);
    }

    #endregion ═══ IComparable Implementation                                 ═══

    #region ═══ Helpers & Overrides                                        ═══

    /// <summary>Converts this physical weight to milligrams.</summary>
    public PhysicalWeight ToMilligram() => Convert(PhysicalWeightUnit.Milligram);

    /// <summary>Converts this physical weight to grams.</summary>
    public PhysicalWeight ToGram() => Convert(PhysicalWeightUnit.Gram);

    /// <summary>Converts this physical weight to kilograms.</summary>
    public PhysicalWeight ToKilogram() => Convert(PhysicalWeightUnit.Kilogram);

    /// <summary>Converts this physical weight to tons.</summary>
    public PhysicalWeight ToTon() => Convert(PhysicalWeightUnit.Ton);

    /// <summary>Converts this physical weight to pounds.</summary>
    public PhysicalWeight ToPound() => Convert(PhysicalWeightUnit.Pound);

    /// <summary>Converts this physical weight to ounces.</summary>
    public PhysicalWeight ToOunce() => Convert(PhysicalWeightUnit.Ounce);

    /// <summary>
    /// Converts this physical weight to the given target unit.
    /// </summary>
    /// <param name="target">The target unit.</param>
    /// <returns>A new <see cref="PhysicalWeight"/> expressed in <paramref name="target"/>, or <c>this</c> if already in that unit.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="target"/> is not a supported unit.</exception>
    private PhysicalWeight Convert(PhysicalWeightUnit target)
    {
        if (Unit == target)
        {
            return this;
        }

        decimal inGrams = Value * ToGramFactor(Unit);
        decimal result = inGrams / ToGramFactor(target);

        return new PhysicalWeight(result, target);
    }

    /// <summary>
    /// Returns the multiplicative factor that converts one unit into grams.
    /// </summary>
    /// <param name="unit">The source unit.</param>
    /// <returns>The conversion factor as a decimal.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="unit"/> is not a supported unit.</exception>
    private static decimal ToGramFactor(PhysicalWeightUnit unit) => unit switch
    {
        PhysicalWeightUnit.Milligram => 0.001m,
        PhysicalWeightUnit.Gram => 1m,
        PhysicalWeightUnit.Kilogram => 1000m,
        PhysicalWeightUnit.Ton => 1_000_000m,
        PhysicalWeightUnit.Pound => 453.59237m,
        PhysicalWeightUnit.Ounce => 28.349523125m,
        _ => throw new InvalidOperationException($"Unsupported physical weight unit: {unit}"),
    };

    /// <summary>
    /// Parses a unit name, accepting both full names and short forms.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="unit">When this method returns, contains the parsed unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnit(string text, out PhysicalWeightUnit unit)
    {
        switch (text.ToLowerInvariant())
        {
            case "mg": unit = PhysicalWeightUnit.Milligram; return true;
            case "g": unit = PhysicalWeightUnit.Gram; return true;
            case "kg": unit = PhysicalWeightUnit.Kilogram; return true;
            case "t": unit = PhysicalWeightUnit.Ton; return true;
            case "lb": unit = PhysicalWeightUnit.Pound; return true;
            case "oz": unit = PhysicalWeightUnit.Ounce; return true;
        }

        if (Enum.TryParse(text, ignoreCase: true, out unit) && Enum.IsDefined<PhysicalWeightUnit>(unit))
        {
            return true;
        }

        unit = default;
        return false;
    }

    /// <summary>
    /// Returns a human-readable representation of the physical weight.
    /// </summary>
    /// <returns>A string formatted as <c>"&lt;value&gt; &lt;unit&gt;"</c>, for example <c>"2.5 Kilogram"</c>.</returns>
    public override string ToString() => $"{Value} {Unit}";

    /// <summary>
    /// Explicitly converts a <see cref="PhysicalWeight"/> to its string representation.
    /// </summary>
    /// <param name="weight">The physical weight instance to convert.</param>
    /// <returns>The string representation of the physical weight.</returns>
    public static explicit operator string(PhysicalWeight weight) => weight.ToString();

    #endregion ═══ Helpers & Overrides                                        ═══
}
