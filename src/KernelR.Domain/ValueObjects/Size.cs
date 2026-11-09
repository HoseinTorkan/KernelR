using KernelR.Domain.Enums;
using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
///     <see cref="Create(decimal, SizeUnit)"/>. Conversions may produce values above
///     this bound, because the bound applies to the input, not to derived values.
///   </description></item>
///   <item><description>
///     The <see cref="Value"/> is always rounded to two decimal places using
///     <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// <para>
/// <b>Mathematical Operations:</b> When adding or subtracting two <see cref="Size"/> instances 
/// with different units, the second operand is automatically converted to the unit of the first operand 
/// before the operation is performed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;Size&gt; result = Size.Create("5.5 cm");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToInch()); // Outputs: 2.17 Inch
/// }
/// </code>
/// </example>
public readonly record struct Size :
    IValueObject,
    IParsable<Size>,
    IComparable<Size>,
    IComparable
{
    #region ═══ Constants & Fields                                       ═══

    /// <summary>
    /// The maximum allowed value before conversion.
    /// </summary>
    private const decimal MaxValue = 10_000m;

    /// <summary>
    /// The number of decimal places to round the value to.
    /// </summary>
    private const int Precision = 2;

    #endregion ═══ Constants & Fields                                       ═══

    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the numeric value, rounded to two decimal places.
    /// </summary>
    /// <value>The numeric value of the size.</value>
    public decimal Value { get; }

    /// <summary>
    /// Gets the unit of measurement.
    /// </summary>
    /// <value>The <see cref="SizeUnit"/> representing the measurement unit.</value>
    public SizeUnit Unit { get; }

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> struct.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="unit">The unit of measurement.</param>
    private Size(decimal value, SizeUnit unit)
    {
        Value = Math.Round(value, Precision, MidpointRounding.AwayFromZero);
        Unit = unit;
    }

    /// <summary>
    /// Creates a new <see cref="Size"/> from a numeric value and a unit.
    /// </summary>
    /// <param name="value">The numeric value. Must be strictly positive and no greater than <c>10,000</c>.</param>
    /// <param name="unit">The unit of measurement. Must be a defined <see cref="SizeUnit"/>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="Size"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Size.INVALID_VALUE</c> when <paramref name="value"/> is less than or equal to zero.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Size.TOO_LARGE</c> when <paramref name="value"/> exceeds <c>10,000</c>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Size.INVALID_UNIT</c> when <paramref name="unit"/> is not defined.</description></item>
    /// </list>
    /// </returns>
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

        if (!Enum.IsDefined<SizeUnit>(unit))
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
    /// Creates a new <see cref="Size"/> from a string in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"5 cm"</c>, <c>"2.5 ft"</c>.
    /// The value uses invariant culture; the unit accepts both full names and short forms (case-insensitive).
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="Size"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Size.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Size.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Size.INVALID_VALUE</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Size.INVALID_UNIT</c> when the unit part is not recognized.</description></item>
    /// </list>
    /// </returns>
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

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

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

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <summary>
    /// Parses the specified string into a <see cref="Size"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="Size"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>, 
    /// or contains an invalid value or unit. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static Size Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="Size"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="Size"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
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

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Safe Functional Operations (Math & Comparison)              ═══

    // ─── Math Operations (Returns Result) ───

    /// <summary>
    /// Adds the specified size to the current size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The size to add. It will be automatically converted to the current unit.</param>
    /// <returns>
    /// A successful result carrying the sum, or a failure with <c>KernelR.ValueObjects.Size.OVERFLOW</c> 
    /// if the result exceeds the maximum capacity.
    /// </returns>
    public Result<Size> Add(Size other)
    {
        try
        {
            Size convertedOther = other.Convert(Unit);
            return Result<Size>.Success(new(Value + convertedOther.Value, Unit));
        }
        catch (OverflowException)
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.OVERFLOW",
                    "The addition resulted in a value that is too large.",
                    ErrorType.ValidationError));
        }
    }

    /// <summary>
    /// Subtracts the specified size from the current size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The size to subtract. It will be automatically converted to the current unit.</param>
    /// <returns>
    /// A successful result carrying the difference, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Size.NEGATIVE_OR_ZERO_RESULT</c> when the subtraction would result in a value less than or equal to zero.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Size.OVERFLOW</c> when the result exceeds the bounds.</description></item>
    /// </list>
    /// </returns>
    public Result<Size> Subtract(Size other)
    {
        try
        {
            Size convertedOther = other.Convert(Unit);
            decimal newValue = Value - convertedOther.Value;

            if (newValue <= 0)
            {
                return Result<Size>.Failure(
                    new Error(
                        "KernelR.ValueObjects.Size.NEGATIVE_OR_ZERO_RESULT",
                        "Subtraction resulted in a size that is zero or negative.",
                        ErrorType.ValidationError));
            }

            return Result<Size>.Success(new(newValue, Unit));
        }
        catch (OverflowException)
        {
            return Result<Size>.Failure(
                new Error(
                    "KernelR.ValueObjects.Size.OVERFLOW",
                    "The subtraction resulted in a value that is out of bounds.",
                    ErrorType.ValidationError));
        }
    }

    // ─── Comparison Operations (Returns Result) ───

    /// <summary>
    /// Determines whether the current size is greater than the specified size.
    /// </summary>
    public Result<bool> GreaterThan(Size other)
    {
        Size convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value > convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current size is less than the specified size.
    /// </summary>
    public Result<bool> LessThan(Size other)
    {
        Size convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value < convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current size is greater than or equal to the specified size.
    /// </summary>
    public Result<bool> GreaterThanOrEqual(Size other)
    {
        Size convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value >= convertedOther.Value);
    }

    /// <summary>
    /// Determines whether the current size is less than or equal to the specified size.
    /// </summary>
    public Result<bool> LessThanOrEqual(Size other)
    {
        Size convertedOther = other.Convert(Unit);
        return Result<bool>.Success(Value <= convertedOther.Value);
    }

    #endregion ═══ Safe Functional Operations (Math & Comparison)              ═══

    #region ═══ Arithmetic Operators                                       ═══

    /// <summary>Adds two sizes.</summary>
    public static Size operator +(Size left, Size right)
    {
        return left.Add(right).OrThrowInvalidOperationException();
    }

    /// <summary>Subtracts one size from another.</summary>
    public static Size operator -(Size left, Size right)
    {
        return left.Subtract(right).OrThrowInvalidOperationException();
    }

    #endregion ═══ Arithmetic Operators                                       ═══

    #region ═══ Comparison Operators (Size vs Size)                        ═══

    /// <summary>Determines whether the left size is greater than the right size.</summary>
    public static bool operator >(Size left, Size right)
    {
        return left.GreaterThan(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left size is less than the right size.</summary>
    public static bool operator <(Size left, Size right)
    {
        return left.LessThan(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left size is greater than or equal to the right size.</summary>
    public static bool operator >=(Size left, Size right)
    {
        return left.GreaterThanOrEqual(right).OrThrowInvalidOperationException();
    }

    /// <summary>Determines whether the left size is less than or equal to the right size.</summary>
    public static bool operator <=(Size left, Size right)
    {
        return left.LessThanOrEqual(right).OrThrowInvalidOperationException();
    }

    #endregion ═══ Comparison Operators (Size vs Size)                        ═══

    #region ═══ IComparable Implementation                                 ═══

    /// <inheritdoc />
    public int CompareTo(Size other)
    {
        Size convertedOther = other.Convert(Unit);
        return Value.CompareTo(convertedOther.Value);
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1; // Any instance is greater than null.
        }

        if (obj is not Size other)
        {
            throw new ArgumentException($"Object must be of type {nameof(Size)}.", nameof(obj));
        }

        return CompareTo(other);
    }

    #endregion ═══ IComparable Implementation                                 ═══

    #region ═══ Helpers & Overrides                                        ═══

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

    /// <summary>
    /// Converts this size to the given target unit.
    /// </summary>
    /// <param name="target">The target unit.</param>
    /// <returns>A new <see cref="Size"/> expressed in <paramref name="target"/>, or <c>this</c> if already in that unit.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="target"/> is not a supported unit.</exception>
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

    /// <summary>
    /// Returns the multiplicative factor that converts one unit into millimeters.
    /// </summary>
    /// <param name="unit">The source unit.</param>
    /// <returns>The conversion factor as a decimal.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="unit"/> is not a supported unit.</exception>
    private static decimal ToMillimeterFactor(SizeUnit unit) => unit switch
    {
        SizeUnit.Millimeter => 1m,
        SizeUnit.Centimeter => 10m,
        SizeUnit.Meter => 1000m,
        SizeUnit.Inch => 25.4m,
        SizeUnit.Foot => 304.8m,
        _ => throw new InvalidOperationException($"Unsupported size unit: {unit}"),
    };

    /// <summary>
    /// Parses a unit name, accepting both full names and short forms.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="unit">When this method returns, contains the parsed unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
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

        if (Enum.TryParse(text, ignoreCase: true, out unit) && Enum.IsDefined<SizeUnit>(unit))
        {
            return true;
        }

        unit = default;
        return false;
    }

    /// <summary>
    /// Returns a human-readable representation of the size.
    /// </summary>
    /// <returns>A string formatted as <c>"&lt;value&gt; &lt;unit&gt;"</c>, for example <c>"5.5 Centimeter"</c>.</returns>
    public override string ToString() => $"{Value} {Unit}";

    /// <summary>
    /// Explicitly converts a <see cref="Size"/> to its string representation.
    /// </summary>
    /// <param name="size">The size instance to convert.</param>
    /// <returns>The string representation of the size.</returns>
    public static explicit operator string(Size size) => size.ToString();

    #endregion ═══ Helpers & Overrides                                        ═══
}
