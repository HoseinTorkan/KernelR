using KernelR.Domain.Enums;
using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Measurement;

/// <summary>
/// Represents a physical weight or mass as an immutable value object with a canonical
/// representation in micrograms.
/// </summary>
/// <remarks>
/// <para>
/// This value object guarantees the following invariants:
/// </para>
/// <list type="bullet">
///   <item><description>The <see cref="Micrograms"/> is always non-negative (zero is allowed).</description></item>
///   <item><description>
///     The value is stored canonically in micrograms. This guarantees that two logically equal values
///     (e.g., <c>1 kg</c> and <c>1,000,000,000 μg</c>) are always exactly equal, and that EF Core and
///     JSON serialization can use a single column/property.
///   </description></item>
///   <item><description>
///     Fractional weights supplied via <c>FromOunces</c>, <c>FromPounds</c>, etc. are
///     rounded to the nearest microgram using <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// <para>
/// <b>Imperial units precision:</b> The pound is exact (<c>1 lb = 453,592,370 μg</c>),
/// but the ounce has an inherent fractional part in micrograms
/// (<c>1 oz = 28,349,523.125 μg</c>) and is rounded to <c>28,349,523 μg</c>.
/// The resulting error is 0.125 μg per ounce, which is smaller than 1/800 of a grain of salt.
/// This error is far below the precision of any commercial or laboratory scale and is negligible
/// for all practical applications.
/// </para>
/// <para>
/// <b>Mathematical Operations:</b> When adding or subtracting two <see cref="PhysicalWeight"/> instances
/// with different units, the operation is performed directly on the canonical microgram counts,
/// so no unit conversion or precision loss is ever needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;PhysicalWeight&gt; result = PhysicalWeight.FromKilograms(2.5m);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToPounds()); // Outputs: 5.51
///     Console.WriteLine(result.Value);            // Outputs: 2.5 kg
/// }
/// </code>
/// </example>
public readonly record struct PhysicalWeight : IValueObject, IParsable<PhysicalWeight>, IComparable<PhysicalWeight>, IComparable
{
    /// <summary>
    /// The number of micrograms in one milligram (1,000).
    /// </summary>
    private const long MicrogramsPerMilligram = 1_000L;

    /// <summary>
    /// The number of micrograms in one gram (1,000,000).
    /// </summary>
    private const long MicrogramsPerGram = 1_000_000L;

    /// <summary>
    /// The number of micrograms in one kilogram (1,000,000,000).
    /// </summary>
    private const long MicrogramsPerKilogram = 1_000_000_000L;

    /// <summary>
    /// The number of micrograms in one metric ton (1,000,000,000,000).
    /// </summary>
    private const long MicrogramsPerTon = 1_000_000_000_000L;

    /// <summary>
    /// The number of micrograms in one pound (453,592,370), exact by the international avoirdupois definition.
    /// </summary>
    private const long MicrogramsPerPound = 453_592_370L;

    /// <summary>
    /// The number of micrograms in one ounce (28,349,523), rounded from 28,349,523.125 μg.
    /// </summary>
    private const long MicrogramsPerOunce = 28_349_523L;

    /// <summary>
    /// Gets the canonical weight, in micrograms (μg).
    /// </summary>
    /// <value>A non-negative number of micrograms.</value>
    /// <remarks>
    /// This is the single source of truth for the value object. All conversions
    /// (to mg, g, kg, t, lb, oz) are derived from this value.
    /// </remarks>
    public long Micrograms { get; }

    /// <summary>
    /// Gets a value indicating whether the weight is zero.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Micrograms"/> is exactly zero; otherwise, <see langword="false"/>.</value>
    public bool IsZero => Micrograms == 0L;

    /// <summary>
    /// Gets a value indicating whether the weight is strictly positive.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Micrograms"/> is greater than zero; otherwise, <see langword="false"/>.</value>
    public bool IsPositive => Micrograms > 0L;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalWeight"/> struct.
    /// </summary>
    /// <param name="micrograms">The canonical weight in micrograms. Must be non-negative.</param>
    private PhysicalWeight(long micrograms) => Micrograms = micrograms;

    /// <summary>
    /// Gets a zero-sized <see cref="PhysicalWeight"/>.
    /// </summary>
    /// <value>A new <see cref="PhysicalWeight"/> instance with <see cref="Micrograms"/> equal to zero.</value>
    public static PhysicalWeight Zero => new(0L);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a number of micrograms.
    /// </summary>
    /// <param name="micrograms">The number of micrograms. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE</c> when <paramref name="micrograms"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalWeight> FromMicrograms(decimal micrograms) => FromDecimal(micrograms, 1L);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a number of milligrams.
    /// </summary>
    /// <param name="milligrams">The number of milligrams. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE</c> when <paramref name="milligrams"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalWeight> FromMilligrams(decimal milligrams) => FromDecimal(milligrams, MicrogramsPerMilligram);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a number of grams.
    /// </summary>
    /// <param name="grams">The number of grams. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE</c> when <paramref name="grams"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalWeight> FromGrams(decimal grams) => FromDecimal(grams, MicrogramsPerGram);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a number of kilograms.
    /// </summary>
    /// <param name="kilograms">The number of kilograms. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE</c> when <paramref name="kilograms"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalWeight> FromKilograms(decimal kilograms) => FromDecimal(kilograms, MicrogramsPerKilogram);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a number of metric tons.
    /// </summary>
    /// <param name="tons">The number of metric tons. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE</c> when <paramref name="tons"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public static Result<PhysicalWeight> FromTons(decimal tons) => FromDecimal(tons, MicrogramsPerTon);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a number of pounds.
    /// </summary>
    /// <param name="pounds">The number of pounds. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE</c> when <paramref name="pounds"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// 1 pound = 453,592,370 micrograms (exact by the international avoirdupois definition).
    /// There is no precision loss.
    /// </remarks>
    public static Result<PhysicalWeight> FromPounds(decimal pounds) => FromDecimal(pounds, MicrogramsPerPound);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a number of ounces.
    /// </summary>
    /// <param name="ounces">The number of ounces. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE</c> when <paramref name="ounces"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// 1 oz = 453,592,370 / 16 = 28,349,523.125 μg, which is not an integer. The value is
    /// rounded to <c>28,349,523 μg</c>, introducing a relative error of about 4.4 × 10⁻⁹
    /// (0.00000044%) per ounce.
    /// </para>
    /// <para>
    /// For context: the error per ounce is 0.125 μg, which is smaller than 1/800 of a
    /// grain of salt. On a 1,000,000-ounce shipment, the total error would be 125 mg.
    /// This is far below the precision of any commercial scale and is negligible for
    /// all practical applications.
    /// </para>
    /// </remarks>
    public static Result<PhysicalWeight> FromOunces(decimal ounces) => FromDecimal(ounces, MicrogramsPerOunce);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a numeric value and a unit.
    /// </summary>
    /// <param name="amount">The numeric value. Must be non-negative.</param>
    /// <param name="unit">The unit of measurement. Must be a defined <see cref="PhysicalWeightUnit"/>.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE</c> when <paramref name="amount"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT</c> when <paramref name="unit"/> is not defined.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <b>When to use this method:</b> Use this overload when the unit is determined at runtime
    /// (e.g., from an API request, a database row with a separate unit column, or a UI dropdown).
    /// When the unit is known at compile time, prefer the type-safe <c>FromXxx</c> factories.
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalWeightUnit unit = (PhysicalWeightUnit)request.Unit;
    /// Result&lt;PhysicalWeight&gt; result = PhysicalWeight.Create(request.Amount, unit);
    /// </code>
    /// </example>
    public static Result<PhysicalWeight> Create(decimal amount, PhysicalWeightUnit unit)
    {
        if (amount < 0m)
            return new Error("KernelR.ValueObjects.PhysicalWeight.NEGATIVE", "Physical weight cannot be negative.", ErrorType.ValidationError);

        return unit switch
        {
            PhysicalWeightUnit.Microgram => FromMicrograms(amount),
            PhysicalWeightUnit.Milligram => FromMilligrams(amount),
            PhysicalWeightUnit.Gram => FromGrams(amount),
            PhysicalWeightUnit.Kilogram => FromKilograms(amount),
            PhysicalWeightUnit.Ton => FromTons(amount),
            PhysicalWeightUnit.Pound => FromPounds(amount),
            PhysicalWeightUnit.Ounce => FromOunces(amount),
            _ => new Error("KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT", $"Physical weight unit '{(int)unit}' is not defined.", ErrorType.ValidationError),
        };
    }

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a string in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"2.5 kg"</c>, <c>"500 g"</c>, <c>"10 lb"</c>.
    /// The value uses invariant culture; the unit accepts both full names and short forms (case-insensitive).
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_FORMAT</c> when the input does not contain exactly two parts.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_AMOUNT</c> when the numeric part is not a valid number.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT</c> when the unit part is not recognized.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// PhysicalWeight.Create("2.5 kg");  // → PhysicalWeight(2.5 kg)
    /// PhysicalWeight.Create("500 g");   // → PhysicalWeight(500 g)
    /// PhysicalWeight.Create("0 μg");    // → PhysicalWeight(0 μg)
    /// </code>
    /// </example>
    public static Result<PhysicalWeight> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error("KernelR.ValueObjects.PhysicalWeight.EMPTY", "Physical weight cannot be empty.", ErrorType.ValidationError);

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return new Error("KernelR.ValueObjects.PhysicalWeight.INVALID_FORMAT", "Physical weight must be formatted as '<value> <unit>', for example '2.5 kg'.", ErrorType.ValidationError);

        if (!decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            return new Error("KernelR.ValueObjects.PhysicalWeight.INVALID_AMOUNT", "Physical weight value is not a valid number.", ErrorType.ValidationError);

        if (!TryParseUnitFactor(parts[1], out long factor))
            return new Error("KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT", $"Physical weight unit '{parts[1]}' is not recognized.", ErrorType.ValidationError);

        return FromDecimal(amount, factor);
    }

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
    public static PhysicalWeight Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="PhysicalWeight"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="PhysicalWeight"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PhysicalWeight result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Converts this weight to micrograms.
    /// </summary>
    /// <returns>The canonical weight in micrograms.</returns>
    public long ToMicrograms() => Micrograms;

    /// <summary>
    /// Converts this weight to milligrams.
    /// </summary>
    /// <returns>The weight in milligrams (may have a fractional part).</returns>
    public decimal ToMilligrams() => Micrograms / (decimal)MicrogramsPerMilligram;

    /// <summary>
    /// Converts this weight to grams.
    /// </summary>
    /// <returns>The weight in grams (may have a fractional part).</returns>
    public decimal ToGrams() => Micrograms / (decimal)MicrogramsPerGram;

    /// <summary>
    /// Converts this weight to kilograms.
    /// </summary>
    /// <returns>The weight in kilograms (may have a fractional part).</returns>
    public decimal ToKilograms() => Micrograms / (decimal)MicrogramsPerKilogram;

    /// <summary>
    /// Converts this weight to metric tons.
    /// </summary>
    /// <returns>The weight in metric tons (may have a fractional part).</returns>
    public decimal ToTons() => Micrograms / (decimal)MicrogramsPerTon;

    /// <summary>
    /// Converts this weight to pounds.
    /// </summary>
    /// <returns>The weight in pounds (may have a fractional part).</returns>
    public decimal ToPounds() => Micrograms / (decimal)MicrogramsPerPound;

    /// <summary>
    /// Converts this weight to ounces.
    /// </summary>
    /// <returns>The weight in ounces (may have a fractional part).</returns>
    /// <remarks>
    /// The ounce conversion uses the exact definition (1 oz = 28,349,523.125 μg) conceptually,
    /// but the stored value is in whole micrograms, so a tiny rounding error (0.125 μg per ounce)
    /// may be present. See <see cref="FromOunces"/> for details.
    /// </remarks>
    public decimal ToOunces() => Micrograms / (decimal)MicrogramsPerOunce;

    /// <summary>
    /// Adds the specified physical weight to the current weight, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The physical weight to add.</param>
    /// <returns>
    /// A successful result carrying the sum, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the sum exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalWeight> Add(PhysicalWeight other)
    {
        try
        {
            long sum = checked(Micrograms + other.Micrograms);
            return new PhysicalWeight(sum);
        }
        catch (OverflowException)
        {
            return new Error("KernelR.ValueObjects.PhysicalWeight.OVERFLOW", "The addition resulted in a value that is too large.", ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Subtracts the specified physical weight from the current weight, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="other">The physical weight to subtract.</param>
    /// <returns>
    /// A successful result carrying the difference, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE_RESULT</c> when the subtraction would result in a negative value.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalWeight> Subtract(PhysicalWeight other)
    {
        if (other.Micrograms > Micrograms)
            return new Error("KernelR.ValueObjects.PhysicalWeight.NEGATIVE_RESULT", "Subtraction resulted in a negative weight.", ErrorType.ValidationError);
        return new PhysicalWeight(Micrograms - other.Micrograms);
    }

    /// <summary>
    /// Multiplies the physical weight by a scalar factor, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="multiplier">The scalar factor. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the product, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.NEGATIVE_MULTIPLIER</c> when <paramref name="multiplier"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the product exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalWeight> Multiply(decimal multiplier)
    {
        if (multiplier < 0m)
            return new Error("KernelR.ValueObjects.PhysicalWeight.NEGATIVE_MULTIPLIER", "Multiplier cannot be negative.", ErrorType.ValidationError);
        try
        {
            decimal result = Micrograms * multiplier;
            if (result > long.MaxValue)
                return new Error("KernelR.ValueObjects.PhysicalWeight.OVERFLOW", "The multiplication resulted in a value that is too large.", ErrorType.ValidationError);
            return new PhysicalWeight((long)Math.Round(result, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error("KernelR.ValueObjects.PhysicalWeight.OVERFLOW", "The multiplication resulted in a value that is too large.", ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Divides the physical weight by a scalar divisor, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="divisor">The scalar divisor. Must be strictly positive.</param>
    /// <returns>
    /// A successful result carrying the quotient, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.DIVIDE_BY_ZERO</c> when <paramref name="divisor"/> is zero or negative.</description></item>
    /// </list>
    /// </returns>
    public Result<PhysicalWeight> Divide(decimal divisor)
    {
        if (divisor <= 0m)
            return new Error("KernelR.ValueObjects.PhysicalWeight.DIVIDE_BY_ZERO", "Cannot divide by zero or a negative number.", ErrorType.ValidationError);
        return new PhysicalWeight((long)Math.Round(Micrograms / divisor, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Returns the ratio of this weight to another weight.
    /// </summary>
    /// <param name="other">The other weight.</param>
    /// <returns>
    /// A successful result carrying the ratio (e.g., <c>2.5m</c> means "this is 2.5 times heavier"), or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.DIVIDE_BY_ZERO</c> when <paramref name="other"/> is zero.</description></item>
    /// </list>
    /// </returns>
    public Result<decimal> Ratio(PhysicalWeight other)
    {
        if (other.Micrograms == 0L)
            return new Error("KernelR.ValueObjects.PhysicalWeight.DIVIDE_BY_ZERO", "Cannot compute ratio against zero.", ErrorType.ValidationError);
        return Result<decimal>.Success(Micrograms / (decimal)other.Micrograms);
    }

    /// <summary>
    /// Returns the smaller of two weights.
    /// </summary>
    /// <param name="other">The other weight.</param>
    /// <returns>The weight with the smaller <see cref="Micrograms"/> value.</returns>
    public PhysicalWeight Min(PhysicalWeight other) => Micrograms <= other.Micrograms ? this : other;

    /// <summary>
    /// Returns the larger of two weights.
    /// </summary>
    /// <param name="other">The other weight.</param>
    /// <returns>The weight with the larger <see cref="Micrograms"/> value.</returns>
    public PhysicalWeight Max(PhysicalWeight other) => Micrograms >= other.Micrograms ? this : other;

    /// <summary>Determines whether the current weight is greater than the specified weight.</summary>
    /// <param name="other">The physical weight to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> GreaterThan(PhysicalWeight other) => Result<bool>.Success(Micrograms > other.Micrograms);

    /// <summary>Determines whether the current weight is less than the specified weight.</summary>
    /// <param name="other">The physical weight to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> LessThan(PhysicalWeight other) => Result<bool>.Success(Micrograms < other.Micrograms);

    /// <summary>Determines whether the current weight is greater than or equal to the specified weight.</summary>
    /// <param name="other">The physical weight to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> GreaterThanOrEqual(PhysicalWeight other) => Result<bool>.Success(Micrograms >= other.Micrograms);

    /// <summary>Determines whether the current weight is less than or equal to the specified weight.</summary>
    /// <param name="other">The physical weight to compare.</param>
    /// <returns>A successful result carrying the comparison result.</returns>
    public Result<bool> LessThanOrEqual(PhysicalWeight other) => Result<bool>.Success(Micrograms <= other.Micrograms);

    /// <summary>Adds two physical weights.</summary>
    /// <param name="left">The first physical weight.</param>
    /// <param name="right">The second physical weight.</param>
    /// <returns>The sum of the two physical weights.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sum overflows <see cref="long.MaxValue"/>.</exception>
    public static PhysicalWeight operator +(PhysicalWeight left, PhysicalWeight right) => left.Add(right).OrThrow();

    /// <summary>Subtracts one physical weight from another.</summary>
    /// <param name="left">The first physical weight.</param>
    /// <param name="right">The physical weight to subtract.</param>
    /// <returns>The difference between the two physical weights.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is negative.</exception>
    public static PhysicalWeight operator -(PhysicalWeight left, PhysicalWeight right) => left.Subtract(right).OrThrow();

    /// <summary>Multiplies a physical weight by a scalar factor.</summary>
    /// <param name="weight">The physical weight.</param>
    /// <param name="multiplier">The scalar factor.</param>
    /// <returns>The product of the weight and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static PhysicalWeight operator *(PhysicalWeight weight, decimal multiplier) => weight.Multiply(multiplier).OrThrow();

    /// <summary>Multiplies a scalar factor by a physical weight (commutative support).</summary>
    /// <param name="multiplier">The scalar factor.</param>
    /// <param name="weight">The physical weight.</param>
    /// <returns>The product of the weight and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static PhysicalWeight operator *(decimal multiplier, PhysicalWeight weight) => weight.Multiply(multiplier).OrThrow();

    /// <summary>Divides a physical weight by a scalar divisor.</summary>
    /// <param name="weight">The physical weight.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The quotient of the weight and the divisor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the divisor is zero or negative.</exception>
    public static PhysicalWeight operator /(PhysicalWeight weight, decimal divisor) => weight.Divide(divisor).OrThrow();

    /// <summary>Returns the ratio of two physical weights as a <see cref="decimal"/>.</summary>
    /// <param name="left">The numerator.</param>
    /// <param name="right">The denominator.</param>
    /// <returns>The ratio of the two weights.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="right"/> is zero.</exception>
    public static decimal operator /(PhysicalWeight left, PhysicalWeight right) => left.Ratio(right).OrThrow();

    /// <summary>Determines whether the left weight is greater than the right weight.</summary>
    /// <param name="left">The left weight.</param>
    /// <param name="right">The right weight.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(PhysicalWeight left, PhysicalWeight right) => left.Micrograms > right.Micrograms;

    /// <summary>Determines whether the left weight is less than the right weight.</summary>
    /// <param name="left">The left weight.</param>
    /// <param name="right">The right weight.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(PhysicalWeight left, PhysicalWeight right) => left.Micrograms < right.Micrograms;

    /// <summary>Determines whether the left weight is greater than or equal to the right weight.</summary>
    /// <param name="left">The left weight.</param>
    /// <param name="right">The right weight.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(PhysicalWeight left, PhysicalWeight right) => left.Micrograms >= right.Micrograms;

    /// <summary>Determines whether the left weight is less than or equal to the right weight.</summary>
    /// <param name="left">The left weight.</param>
    /// <param name="right">The right weight.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(PhysicalWeight left, PhysicalWeight right) => left.Micrograms <= right.Micrograms;

    /// <inheritdoc />
    public int CompareTo(PhysicalWeight other) => Micrograms.CompareTo(other.Micrograms);

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            PhysicalWeight other => CompareTo(other),
            _ => throw new ArgumentException($"Object must be of type {nameof(PhysicalWeight)}.", nameof(obj))
        };
    }

    /// <summary>
    /// Returns a human-readable representation of the weight, automatically selecting
    /// the largest applicable metric unit (μg, mg, g, kg, or t).
    /// </summary>
    /// <returns>A culture-invariant string (e.g., <c>"2.5 kg"</c>, <c>"500 mg"</c>).</returns>
    /// <remarks>
    /// Imperial units (pounds, ounces) are not used by <see cref="ToString"/> because
    /// the canonical representation is metric. Use <see cref="ToPounds"/> or
    /// <see cref="ToOunces"/> for explicit imperial conversion.
    /// </remarks>
    public override string ToString()
    {
        if (Micrograms >= MicrogramsPerTon)
            return string.Format(CultureInfo.InvariantCulture, "{0:0.##} t", ToTons());
        if (Micrograms >= MicrogramsPerKilogram)
            return string.Format(CultureInfo.InvariantCulture, "{0:0.##} kg", ToKilograms());
        if (Micrograms >= MicrogramsPerGram)
            return string.Format(CultureInfo.InvariantCulture, "{0:0.##} g", ToGrams());
        if (Micrograms >= MicrogramsPerMilligram)
            return string.Format(CultureInfo.InvariantCulture, "{0:0.##} mg", ToMilligrams());
        return string.Format(CultureInfo.InvariantCulture, "{0} μg", Micrograms);
    }

    /// <summary>Explicitly converts a <see cref="PhysicalWeight"/> to its underlying microgram count.</summary>
    /// <param name="weight">The physical weight to convert.</param>
    /// <returns>The canonical weight in micrograms.</returns>
    public static explicit operator long(PhysicalWeight weight) => weight.Micrograms;

    /// <summary>Explicitly converts a <see cref="PhysicalWeight"/> to its human-readable string representation.</summary>
    /// <param name="weight">The physical weight to convert.</param>
    /// <returns>The string representation of the physical weight.</returns>
    public static explicit operator string(PhysicalWeight weight) => weight.ToString();

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a decimal value multiplied by a microgram factor.
    /// </summary>
    /// <param name="value">The decimal value. Must be non-negative.</param>
    /// <param name="factor">The number of micrograms per unit.</param>
    /// <returns>A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure.</returns>
    private static Result<PhysicalWeight> FromDecimal(decimal value, long factor)
    {
        if (value < 0m)
            return new Error("KernelR.ValueObjects.PhysicalWeight.NEGATIVE", "Physical weight cannot be negative.", ErrorType.ValidationError);

        try
        {
            decimal microgramsDecimal = value * factor;
            if (microgramsDecimal > long.MaxValue)
                return new Error("KernelR.ValueObjects.PhysicalWeight.OVERFLOW", "Physical weight is too large.", ErrorType.ValidationError);
            return new PhysicalWeight((long)Math.Round(microgramsDecimal, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error("KernelR.ValueObjects.PhysicalWeight.OVERFLOW", "Physical weight is too large.", ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Parses a unit name, accepting both full names and short forms, and returns
    /// the number of micrograms per unit.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="factor">When this method returns, contains the number of micrograms per unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnitFactor(string text, out long factor)
    {
        switch (text.ToLowerInvariant())
        {
            case "μg" or "ug" or "microgram" or "micrograms": factor = 1L; return true;
            case "mg" or "milligram" or "milligrams": factor = MicrogramsPerMilligram; return true;
            case "g" or "gram" or "grams": factor = MicrogramsPerGram; return true;
            case "kg" or "kilogram" or "kilograms": factor = MicrogramsPerKilogram; return true;
            case "t" or "ton" or "tons" or "tonne" or "tonnes": factor = MicrogramsPerTon; return true;
            case "lb" or "lbs" or "pound" or "pounds": factor = MicrogramsPerPound; return true;
            case "oz" or "ounce" or "ounces": factor = MicrogramsPerOunce; return true;
            default: factor = 0L; return false;
        }
    }
}
