using KernelR.Domain.Enums;
using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Measurement;

/// <summary>
/// Represents a physical size (length, width, height, or diameter) as an immutable value object
/// with a canonical representation in micrometers.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees the following invariants:
/// </para>
/// <list type="bullet">
///   <item><description>The <see cref="Micrometers"/> is always non-negative (zero is allowed).</description></item>
///   <item><description>
///     The value is stored canonically in micrometers. This guarantees that two logically equal values
///     (e.g., <c>1 inch</c> and <c>25,400 μm</c>) are always exactly equal, and that EF Core and
///     JSON serialization can use a single column/property.
///   </description></item>
///   <item><description>
///     Fractional sizes supplied via <c>FromInches</c>, <c>FromFeet</c>, etc. are
///     rounded to the nearest micrometer using <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// <para>
/// <b>No precision loss:</b> Unlike weight (where 1 oz has a fractional microgram part),
/// all supported size conversions are exact integers in micrometers:
/// <c>1 in = 25,400 μm</c>, <c>1 ft = 304,800 μm</c>, <c>1 m = 1,000,000 μm</c>.
/// </para>
/// <para>
/// <b>Mathematical Operations:</b> When adding or subtracting two <see cref="PhysicalSize"/> instances
/// with different units, the operation is performed directly on the canonical micrometer counts,
/// so no unit conversion or precision loss is ever needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;PhysicalSize&gt; result = PhysicalSize.FromInches(2.5m);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToCentimeters()); // Outputs: 6.35
///     Console.WriteLine(result.Value);                  // Outputs: 6.35 cm
/// }
/// </code>
/// </example>
public readonly record struct PhysicalSize :
    IValueObject,
    IParsable<PhysicalSize>,
    IComparable<PhysicalSize>,
    IComparable
{
    /// <summary>
    /// The number of micrometers in one millimeter (1,000).
    /// </summary>
    private const long MicrometersPerMillimeter = 1_000L;

    /// <summary>
    /// The number of micrometers in one centimeter (10,000).
    /// </summary>
    private const long MicrometersPerCentimeter = 10_000L;

    /// <summary>
    /// The number of micrometers in one meter (1,000,000).
    /// </summary>
    private const long MicrometersPerMeter = 1_000_000L;

    /// <summary>
    /// The number of micrometers in one inch (25,400), exact by the international definition (1 in = 2.54 cm).
    /// </summary>
    private const long MicrometersPerInch = 25_400L;

    /// <summary>
    /// The number of micrometers in one foot (304,800), exact by the international definition (1 ft = 12 in).
    /// </summary>
    private const long MicrometersPerFoot = 304_800L;

    /// <summary>
    /// Gets the canonical size, in micrometers (μm).
    /// </summary>
    /// <value>A non-negative number of micrometers.</value>
    /// <remarks>
    /// This is the single source of truth for the value object. All conversions
    /// (to mm, cm, m, in, ft) are derived from this value.
    /// </remarks>
    public long Micrometers { get; }

    /// <summary>
    /// Gets a value indicating whether the size is zero.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Micrometers"/> is exactly zero; otherwise, <see langword="false"/>.</value>
    public bool IsZero => Micrometers == 0L;

    /// <summary>
    /// Gets a value indicating whether the size is strictly positive.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Micrometers"/> is greater than zero; otherwise, <see langword="false"/>.</value>
    public bool IsPositive => Micrometers > 0L;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalSize"/> struct.
    /// </summary>
    /// <param name="micrometers">The canonical size in micrometers. Must be non-negative.</param>
    private PhysicalSize(long micrometers) => Micrometers = micrometers;

    /// <summary>
    /// Gets a zero-sized <see cref="PhysicalSize"/>.
    /// </summary>
    /// <value>A new <see cref="PhysicalSize"/> instance with <see cref="Micrometers"/> equal to zero.</value>
    public static PhysicalSize Zero => new(0L);

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a number of micrometers.
    /// </summary>
    /// <param name="micrometers">The number of micrometers. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE</c> when <paramref name="micrometers"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalSize> FromMicrometers(decimal micrometers) => FromDecimal(micrometers, 1L);

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a number of millimeters.
    /// </summary>
    /// <param name="millimeters">The number of millimeters. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE</c> when <paramref name="millimeters"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalSize> FromMillimeters(decimal millimeters) => FromDecimal(millimeters, MicrometersPerMillimeter);

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a number of centimeters.
    /// </summary>
    /// <param name="centimeters">The number of centimeters. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE</c> when <paramref name="centimeters"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalSize> FromCentimeters(decimal centimeters) => FromDecimal(centimeters, MicrometersPerCentimeter);

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a number of meters.
    /// </summary>
    /// <param name="meters">The number of meters. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE</c> when <paramref name="meters"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalSize> FromMeters(decimal meters) => FromDecimal(meters, MicrometersPerMeter);

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a number of inches.
    /// </summary>
    /// <param name="inches">The number of inches. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE</c> when <paramref name="inches"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// 1 inch = 25,400 micrometers (exact by the international definition: 1 in = 2.54 cm).
    /// There is no precision loss.
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalSize.FromInches(2.5m); // → 6.35 cm (exact, no rounding error)
    /// </code>
    /// </example>
    public static Result<PhysicalSize> FromInches(decimal inches) => FromDecimal(inches, MicrometersPerInch);

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a number of feet.
    /// </summary>
    /// <param name="feet">The number of feet. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE</c> when <paramref name="feet"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// 1 foot = 304,800 micrometers (exact by the international definition: 1 ft = 12 in).
    /// There is no precision loss.
    /// </remarks>
    public static Result<PhysicalSize> FromFeet(decimal feet) => FromDecimal(feet, MicrometersPerFoot);

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a numeric value and a unit.
    /// </summary>
    /// <param name="amount">The numeric value. Must be non-negative.</param>
    /// <param name="unit">The unit of measurement. Must be a defined <see cref="PhysicalSizeUnit"/>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE</c> when <paramref name="amount"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_UNIT</c> when <paramref name="unit"/> is not defined.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <b>When to use this method:</b> Use this overload when the unit is determined at runtime
    /// (e.g., from an API request, a database row with a separate unit column, or a UI dropdown).
    /// When the unit is known at compile time, prefer the type-safe <c>FromXxx</c> factories.
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalSizeUnit unit = (PhysicalSizeUnit)request.Unit;
    /// Result&lt;PhysicalSize&gt; result = PhysicalSize.Create(request.Amount, unit);
    /// </code>
    /// </example>
    public static Result<PhysicalSize> Create(decimal amount, PhysicalSizeUnit unit)
    {
        return amount < 0m
            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.NEGATIVE",
                "Physical size cannot be negative.",
                ErrorType.ValidationError)
            : unit switch
            {
                PhysicalSizeUnit.Micrometer => FromMicrometers(amount),
                PhysicalSizeUnit.Millimeter => FromMillimeters(amount),
                PhysicalSizeUnit.Centimeter => FromCentimeters(amount),
                PhysicalSizeUnit.Meter => FromMeters(amount),
                PhysicalSizeUnit.Inch => FromInches(amount),
                PhysicalSizeUnit.Foot => FromFeet(amount),
                _ => new Error(
                    "KernelR.ValueObjects.PhysicalSize.INVALID_UNIT",
                    $"Physical size unit '{(int)unit}' is not defined.",
                    ErrorType.ValidationError),
            };
    }

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a string in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"5 cm"</c>, <c>"2.5 ft"</c>, <c>"1000 μm"</c>.
    /// The value uses invariant culture; the unit accepts both full names and short forms (case-insensitive).
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_AMOUNT</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_UNIT</c> when the unit part is not recognized.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// PhysicalSize.Create("5 cm");    // → PhysicalSize(5 cm)
    /// PhysicalSize.Create("2.5 ft");  // → PhysicalSize(2.5 ft)
    /// PhysicalSize.Create("0 μm");    // → PhysicalSize(0 μm)
    /// </code>
    /// </example>
    public static Result<PhysicalSize> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.EMPTY",
                "Physical size cannot be empty.",
                ErrorType.ValidationError);

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length != 2
            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_FORMAT",
                "Physical size must be formatted as '<value> <unit>', for example '5 cm'.",
                ErrorType.ValidationError)

            : !decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount)
            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_AMOUNT",
                "Physical size value is not a valid number.",
                ErrorType.ValidationError)
            : !TryParseUnitFactor(parts[1], out long factor)

            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_UNIT",
                $"Physical size unit '{parts[1]}' is not recognized.",
                ErrorType.ValidationError)
            : FromDecimal(amount, factor);
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
    /// Converts this size to micrometers.
    /// </summary>
    /// <returns>The canonical size in micrometers.</returns>
    public long ToMicrometers() => Micrometers;

    /// <summary>
    /// Converts this size to millimeters.
    /// </summary>
    /// <returns>The size in millimeters (may have a fractional part).</returns>
    public decimal ToMillimeters() => Micrometers / (decimal)MicrometersPerMillimeter;

    /// <summary>
    /// Converts this size to centimeters.
    /// </summary>
    /// <returns>The size in centimeters (may have a fractional part).</returns>
    public decimal ToCentimeters() => Micrometers / (decimal)MicrometersPerCentimeter;

    /// <summary>
    /// Converts this size to meters.
    /// </summary>
    /// <returns>The size in meters (may have a fractional part).</returns>
    public decimal ToMeters() => Micrometers / (decimal)MicrometersPerMeter;

    /// <summary>
    /// Converts this size to inches.
    /// </summary>
    /// <returns>The size in inches (may have a fractional part).</returns>
    public decimal ToInches() => Micrometers / (decimal)MicrometersPerInch;

    /// <summary>
    /// Converts this size to feet.
    /// </summary>
    /// <returns>The size in feet (may have a fractional part).</returns>
    public decimal ToFeet() => Micrometers / (decimal)MicrometersPerFoot;

    /// <summary>
    /// Adds the specified physical size to the current size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The physical size to add.</param>
    /// <returns>
    /// A successful result carrying the sum, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the sum exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalSize> Add(PhysicalSize other)
    {
        try
        {
            long sum = checked(Micrometers + other.Micrometers);
            return new PhysicalSize(sum);
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.OVERFLOW",
                "The addition resulted in a value that is too large.",
                ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Subtracts the specified physical size from the current size, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The physical size to subtract.</param>
    /// <returns>
    /// A successful result carrying the difference, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE_RESULT</c> when the subtraction would result in a negative value.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalSize> Subtract(PhysicalSize other)
    {
        return other.Micrometers > Micrometers
            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.NEGATIVE_RESULT",
                "Subtraction resulted in a negative physical size.",
                ErrorType.ValidationError)
            : new PhysicalSize(Micrometers - other.Micrometers);
    }

    /// <summary>
    /// Multiplies the physical size by a scalar factor, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="multiplier">The scalar factor. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the product, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE_MULTIPLIER</c> when <paramref name="multiplier"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the product exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalSize> Multiply(decimal multiplier)
    {
        if (multiplier < 0m)
            return new Error("KernelR.ValueObjects.PhysicalSize.NEGATIVE_MULTIPLIER", "Multiplier cannot be negative.", ErrorType.ValidationError);
        try
        {
            decimal result = Micrometers * multiplier;
            return result > long.MaxValue
                ? new Error(
                    "KernelR.ValueObjects.PhysicalSize.OVERFLOW",
                    "The multiplication resulted in a value that is too large.",
                    ErrorType.ValidationError)
                : new PhysicalSize((long)Math.Round(result, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.OVERFLOW",
                "The multiplication resulted in a value that is too large.",
                ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Divides the physical size by a scalar divisor, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="divisor">The scalar divisor. Must be strictly positive.</param>
    /// <returns>
    /// A successful result carrying the quotient, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.DIVIDE_BY_ZERO</c> when <paramref name="divisor"/> is zero or negative.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalSize> Divide(decimal divisor)
    {
        return divisor <= 0m
            ? new Error("KernelR.ValueObjects.PhysicalSize.DIVIDE_BY_ZERO", "Cannot divide by zero or a negative number.", ErrorType.ValidationError)
            : new PhysicalSize((long)Math.Round(Micrometers / divisor, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Returns the ratio of this size to another size.
    /// </summary>
    /// <param name="other">The other size.</param>
    /// <returns>
    /// A successful result carrying the ratio (e.g., <c>2.5m</c> means "this is 2.5 times larger"), or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.DIVIDE_BY_ZERO</c> when <paramref name="other"/> is zero.</description></item>
    /// </list>
    /// </returns>
    public Result<decimal> Ratio(PhysicalSize other)
    {
        return other.Micrometers == 0L
            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.DIVIDE_BY_ZERO",
                "Cannot compute ratio against zero.",
                ErrorType.ValidationError)
            : Result<decimal>.Success(Micrometers / (decimal)other.Micrometers);
    }

    /// <summary>
    /// Returns the smaller of two sizes.
    /// </summary>
    /// <param name="other">The other size.</param>
    /// <returns>The size with the smaller <see cref="Micrometers"/> value.</returns>
    public PhysicalSize Min(PhysicalSize other) => Micrometers <= other.Micrometers ? this : other;

    /// <summary>
    /// Returns the larger of two sizes.
    /// </summary>
    /// <param name="other">The other size.</param>
    /// <returns>The size with the larger <see cref="Micrometers"/> value.</returns>
    public PhysicalSize Max(PhysicalSize other) => Micrometers >= other.Micrometers ? this : other;

    /// <summary>Determines whether the current size is greater than the specified size.</summary>
    /// <param name="other">The physical size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> GreaterThan(PhysicalSize other) => Result<bool>.Success(Micrometers > other.Micrometers);

    /// <summary>Determines whether the current size is less than the specified size.</summary>
    /// <param name="other">The physical size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> LessThan(PhysicalSize other) => Result<bool>.Success(Micrometers < other.Micrometers);

    /// <summary>Determines whether the current size is greater than or equal to the specified size.</summary>
    /// <param name="other">The physical size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> GreaterThanOrEqual(PhysicalSize other) => Result<bool>.Success(Micrometers >= other.Micrometers);

    /// <summary>Determines whether the current size is less than or equal to the specified size.</summary>
    /// <param name="other">The physical size to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> LessThanOrEqual(PhysicalSize other) => Result<bool>.Success(Micrometers <= other.Micrometers);

    /// <summary>Adds two physical sizes.</summary>
    /// <param name="left">The first physical size.</param>
    /// <param name="right">The second physical size.</param>
    /// <returns>The sum of the two physical sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sum overflows <see cref="long.MaxValue"/>.</exception>
    public static PhysicalSize operator +(PhysicalSize left, PhysicalSize right) => left.Add(right).OrThrow();

    /// <summary>Subtracts one physical size from another.</summary>
    /// <param name="left">The first physical size.</param>
    /// <param name="right">The physical size to subtract.</param>
    /// <returns>The difference between the two physical sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is negative.</exception>
    public static PhysicalSize operator -(PhysicalSize left, PhysicalSize right) => left.Subtract(right).OrThrow();

    /// <summary>Multiplies a physical size by a scalar factor.</summary>
    /// <param name="size">The physical size.</param>
    /// <param name="multiplier">The scalar factor.</param>
    /// <returns>The product of the size and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static PhysicalSize operator *(PhysicalSize size, decimal multiplier) => size.Multiply(multiplier).OrThrow();

    /// <summary>Multiplies a scalar factor by a physical size (commutative support).</summary>
    /// <param name="multiplier">The scalar factor.</param>
    /// <param name="size">The physical size.</param>
    /// <returns>The product of the size and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static PhysicalSize operator *(decimal multiplier, PhysicalSize size) => size.Multiply(multiplier).OrThrow();

    /// <summary>Divides a physical size by a scalar divisor.</summary>
    /// <param name="size">The physical size.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The quotient of the size and the divisor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the divisor is zero or negative.</exception>
    public static PhysicalSize operator /(PhysicalSize size, decimal divisor) => size.Divide(divisor).OrThrow();

    /// <summary>Returns the ratio of two physical sizes as a <see cref="decimal"/>.</summary>
    /// <param name="left">The numerator.</param>
    /// <param name="right">The denominator.</param>
    /// <returns>The ratio of the two sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="right"/> is zero.</exception>
    public static decimal operator /(PhysicalSize left, PhysicalSize right) => left.Ratio(right).OrThrow();

    /// <summary>Determines whether the left size is greater than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(PhysicalSize left, PhysicalSize right) => left.Micrometers > right.Micrometers;

    /// <summary>Determines whether the left size is less than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(PhysicalSize left, PhysicalSize right) => left.Micrometers < right.Micrometers;

    /// <summary>Determines whether the left size is greater than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(PhysicalSize left, PhysicalSize right) => left.Micrometers >= right.Micrometers;

    /// <summary>Determines whether the left size is less than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(PhysicalSize left, PhysicalSize right) => left.Micrometers <= right.Micrometers;

    /// <inheritdoc />
    public int CompareTo(PhysicalSize other) => Micrometers.CompareTo(other.Micrometers);

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

    /// <summary>
    /// Returns a human-readable representation of the size, automatically selecting
    /// the largest applicable metric unit (μm, mm, cm, or m).
    /// </summary>
    /// <returns>A culture-invariant string (e.g., <c>"6.35 cm"</c>, <c>"500 μm"</c>).</returns>
    /// <remarks>
    /// Imperial units (inches, feet) are not used by <see cref="ToString"/> because
    /// the canonical representation is metric. Use <see cref="ToInches"/> or
    /// <see cref="ToFeet"/> for explicit imperial conversion.
    /// </remarks>
    public override string ToString()
    {
        return Micrometers switch
        {
            >= MicrometersPerMeter => string.Format(CultureInfo.InvariantCulture, "{0:0.##} m", ToMeters()),
            >= MicrometersPerCentimeter => string.Format(CultureInfo.InvariantCulture, "{0:0.##} cm", ToCentimeters()),
            >= MicrometersPerMillimeter => string.Format(CultureInfo.InvariantCulture, "{0:0.##} mm", ToMillimeters()),
            _ => string.Format(CultureInfo.InvariantCulture, "{0} μm", Micrometers)
        };
    }

    /// <summary>Explicitly converts a <see cref="PhysicalSize"/> to its underlying micrometer count.</summary>
    /// <param name="size">The physical size to convert.</param>
    /// <returns>The canonical size in micrometers.</returns>
    public static explicit operator long(PhysicalSize size) => size.Micrometers;

    /// <summary>Explicitly converts a <see cref="PhysicalSize"/> to its human-readable string representation.</summary>
    /// <param name="size">The physical size to convert.</param>
    /// <returns>The string representation of the physical size.</returns>
    public static explicit operator string(PhysicalSize size) => size.ToString();

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a decimal value multiplied by a micrometer factor.
    /// </summary>
    /// <param name="value">The decimal value. Must be non-negative.</param>
    /// <param name="factor">The number of micrometers per unit.</param>
    /// <returns>A successful result carrying the new <see cref="PhysicalSize"/>, or a failure.</returns>
    private static Result<PhysicalSize> FromDecimal(decimal value, long factor)
    {
        if (value < 0m)
            return new Error("KernelR.ValueObjects.PhysicalSize.NEGATIVE", "Physical size cannot be negative.", ErrorType.ValidationError);

        try
        {
            decimal micrometersDecimal = value * factor;
            return micrometersDecimal > long.MaxValue
                ? new Error("KernelR.ValueObjects.PhysicalSize.OVERFLOW", "Physical size is too large.", ErrorType.ValidationError)
                : new PhysicalSize((long)Math.Round(micrometersDecimal, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error("KernelR.ValueObjects.PhysicalSize.OVERFLOW", "Physical size is too large.", ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Parses a unit name, accepting both full names and short forms, and returns
    /// the number of micrometers per unit.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="factor">When this method returns, contains the number of micrometers per unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnitFactor(string text, out long factor)
    {
        switch (text.ToLowerInvariant())
        {
            case "μm" or "um" or "micrometer" or "micrometers": factor = 1L; return true;
            case "mm" or "millimeter" or "millimeters": factor = MicrometersPerMillimeter; return true;
            case "cm" or "centimeter" or "centimeters": factor = MicrometersPerCentimeter; return true;
            case "m" or "meter" or "meters": factor = MicrometersPerMeter; return true;
            case "in" or "inch" or "inches": factor = MicrometersPerInch; return true;
            case "ft" or "foot" or "feet": factor = MicrometersPerFoot; return true;
            default: factor = 0L; return false;
        }
    }
}
