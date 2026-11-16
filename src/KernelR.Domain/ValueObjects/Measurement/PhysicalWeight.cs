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
/// <b>Culture policy:</b> This value object is a domain primitive and a machine-to-machine contract.
/// All internal parsing, error messages, and numeric conversion use
/// <see cref="CultureInfo.InvariantCulture"/> explicitly, so the type behaves identically
/// regardless of the host application's <see cref="CultureInfo.CurrentCulture"/>.
/// </para>
/// <para>
/// <b>Display responsibility:</b> For display, the <c>ToString</c> overloads accept an optional
/// <see cref="IFormatProvider"/>. Callers who want localized output (e.g., Persian digits)
/// should pass the desired culture explicitly:
/// <c>weight.ToString(CultureInfo.GetCultureInfo("fa-IR"))</c>.
/// This keeps the decision of "how to display" in the presentation layer, not in the library.
/// </para>
/// <para>
/// <b>Mathematical operations:</b> Fallible operations (<c>Add</c>, <c>Subtract</c>, <c>Multiply</c>,
/// <c>Divide</c>, <c>Ratio</c>) return <see cref="Result{T}"/> and have corresponding operators
/// that throw <see cref="InvalidOperationException"/> on failure. Comparison uses the standard
/// operators (<c>&gt;</c>, <c>&lt;</c>, <c>&gt;=</c>, <c>&lt;=</c>) and <see cref="CompareTo"/> —
/// no separate comparison methods exist, because comparing two <see cref="PhysicalWeight"/>
/// instances cannot fail.
/// </para>
/// <para>
/// <b>Format specifiers (IFormattable):</b> The format string passed to
/// <see cref="ToString(string?, IFormatProvider?)"/> may be one of:
/// </para>
/// <list type="bullet">
///   <item><description><c>null</c> or <c>""</c>: auto-select the largest applicable metric unit (μg, mg, g, kg, or t).</description></item>
///   <item><description>A unit specifier (case-insensitive): <c>"μg"</c>, <c>"ug"</c>, <c>"mg"</c>, <c>"g"</c>, <c>"kg"</c>, <c>"t"</c>, <c>"lb"</c>, <c>"oz"</c>.</description></item>
///   <item><description>A .NET numeric format string (e.g., <c>"N2"</c>, <c>"F3"</c>): auto-select the unit, then apply the numeric format.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;PhysicalWeight&gt; result = PhysicalWeight.FromKilograms(2.5m);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToPounds()); // Outputs: 5.51
///     Console.WriteLine(result.Value);            // Outputs: 2.5 kg
///     Console.WriteLine($"{result.Value:lb}");    // Outputs: 5.51 lb
///     Console.WriteLine($"{result.Value:N3}");    // Outputs: 2.500 kg
///     Console.WriteLine(result.Value > PhysicalWeight.Zero); // Outputs: True
/// }
/// </code>
/// </example>
public readonly record struct PhysicalWeight :
    IValueObject,
    IParsable<PhysicalWeight>,
    IComparable<PhysicalWeight>,
    IComparable,
    IFormattable
{
    /// <summary>The number of micrograms in one milligram (1,000).</summary>
    public const long MicrogramsPerMilligram = 1_000L;

    /// <summary>The number of micrograms in one gram (1,000,000).</summary>
    public const long MicrogramsPerGram = 1_000_000L;

    /// <summary>The number of micrograms in one kilogram (1,000,000,000).</summary>
    public const long MicrogramsPerKilogram = 1_000_000_000L;

    /// <summary>The number of micrograms in one metric ton (1,000,000,000,000).</summary>
    public const long MicrogramsPerTon = 1_000_000_000_000L;

    /// <summary>The number of micrograms in one pound (453,592,370), exact by the international avoirdupois definition.</summary>
    public const long MicrogramsPerPound = 453_592_370L;

    /// <summary>
    /// The number of micrograms in one ounce (28,349,523), rounded from the exact value
    /// 28,349,523.125 μg. The rounding error is 0.125 μg per ounce (see class-level remarks).
    /// </summary>
    public const long MicrogramsPerOunce = 28_349_523L;

    /// <summary>The number of milligrams in one gram (1,000).</summary>
    public const long MilligramsPerGram = 1_000L;

    /// <summary>The number of milligrams in one kilogram (1,000,000).</summary>
    public const long MilligramsPerKilogram = 1_000_000L;

    /// <summary>The number of milligrams in one metric ton (1,000,000,000).</summary>
    public const long MilligramsPerTon = 1_000_000_000L;

    /// <summary>The number of grams in one kilogram (1,000).</summary>
    public const long GramsPerKilogram = 1_000L;

    /// <summary>The number of grams in one metric ton (1,000,000).</summary>
    public const long GramsPerTon = 1_000_000L;

    /// <summary>The number of kilograms in one metric ton (1,000).</summary>
    public const long KilogramsPerTon = 1_000L;

    /// <summary>The number of ounces in one pound (16), exact by the international avoirdupois definition.</summary>
    public const long OuncesPerPound = 16L;

    /// <summary>The number of milligrams in one pound (453,592.37), exact by the international avoirdupois definition.</summary>
    public const decimal MilligramsPerPound = 453_592.37m;

    /// <summary>The number of grams in one pound (453.59237), exact by the international avoirdupois definition.</summary>
    public const decimal GramsPerPound = 453.59237m;

    /// <summary>The number of kilograms in one pound (0.45359237), exact by the international avoirdupois definition.</summary>
    public const decimal KilogramsPerPound = 0.45359237m;

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
        return amount < 0m
            ? new Error(
                "KernelR.ValueObjects.PhysicalWeight.NEGATIVE",
                "Physical weight cannot be negative.",
                ErrorType.ValidationError)
            : unit switch
            {
                PhysicalWeightUnit.Microgram => FromMicrograms(amount),
                PhysicalWeightUnit.Milligram => FromMilligrams(amount),
                PhysicalWeightUnit.Gram => FromGrams(amount),
                PhysicalWeightUnit.Kilogram => FromKilograms(amount),
                PhysicalWeightUnit.Ton => FromTons(amount),
                PhysicalWeightUnit.Pound => FromPounds(amount),
                PhysicalWeightUnit.Ounce => FromOunces(amount),
                _ => new Error(
                    "KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT",
                    FormattableString.Invariant($"Physical weight unit '{(int)unit}' is not defined."),
                    ErrorType.ValidationError),
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
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.EMPTY",
                "Physical weight cannot be empty.",
                ErrorType.ValidationError);

        string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length != 2
            ? new Error(
                "KernelR.ValueObjects.PhysicalWeight.INVALID_FORMAT",
                "Physical weight must be formatted as '<value> <unit>', for example '2.5 kg'.",
                ErrorType.ValidationError)

            : !decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount)
            ? new Error(
                "KernelR.ValueObjects.PhysicalWeight.INVALID_AMOUNT",
                "Physical weight value is not a valid number.",
                ErrorType.ValidationError)

            : !TryParseUnitFactor(parts[1], out long factor)
            ? new Error(
                "KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT",
                FormattableString.Invariant($"Physical weight unit '{parts[1]}' is not recognized."),
                ErrorType.ValidationError)

            : FromDecimal(amount, factor);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="PhysicalWeight"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <returns>A <see cref="PhysicalWeight"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>,
    /// or contains an invalid value or unit. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which requires
    /// an <see cref="IFormatProvider"/> parameter. Because <see cref="PhysicalWeight"/> is a
    /// machine-to-machine contract, parsing is <b>always</b> performed using
    /// <see cref="CultureInfo.InvariantCulture"/> regardless of the provider passed.
    /// </para>
    /// <para>
    /// In other words: <c>PhysicalWeight.Parse("2.5 kg", CultureInfo.GetCultureInfo("de-DE"))</c>
    /// and <c>PhysicalWeight.Parse("2.5 kg", null)</c> produce identical results. A German caller
    /// that wants to accept the German decimal separator (<c>"2,5 kg"</c>) must normalize the
    /// input to invariant form (<c>"2.5 kg"</c>) before calling this method.
    /// </para>
    /// <para>
    /// For consistency, the same policy applies to
    /// <see cref="TryParse(string?, IFormatProvider?, out PhysicalWeight)"/>.
    /// </para>
    /// </remarks>
    public static PhysicalWeight Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="PhysicalWeight"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="PhysicalWeight"/> instance, if parsing
    /// succeeded, or the default value if it failed.
    /// </param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which requires
    /// an <see cref="IFormatProvider"/> parameter. Because <see cref="PhysicalWeight"/> is a
    /// machine-to-machine contract, parsing is <b>always</b> performed using
    /// <see cref="CultureInfo.InvariantCulture"/> regardless of the provider passed.
    /// </para>
    /// <para>
    /// When only a validity check is needed and the resulting instance is not used, callers
    /// can discard it via <c>TryParse(value, null, out _)</c>.
    /// </para>
    /// </remarks>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PhysicalWeight result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Converts a raw decimal value from one unit to another, without creating a <see cref="PhysicalWeight"/>.
    /// </summary>
    /// <param name="value">The raw value to convert.</param>
    /// <param name="fromUnit">The source unit of <paramref name="value"/>.</param>
    /// <param name="toUnit">The target unit for the result.</param>
    /// <returns>
    /// A successful result carrying the converted value, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT</c> when either unit is not defined.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalWeight.OVERFLOW</c> when the intermediate calculation exceeds <see cref="decimal.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is intended for callers who only need the numeric conversion and do not
    /// want to instantiate a <see cref="PhysicalWeight"/>. The conversion is performed in
    /// canonical micrograms, so it is exact for all metric units and for pounds; conversions
    /// involving ounces carry the same 0.125 μg per ounce rounding described on <see cref="FromOunces"/>.
    /// </para>
    /// <para>
    /// <b>Do not combine this with a <c>FromXxx</c> factory.</b> If the goal is to build a
    /// <see cref="PhysicalWeight"/>, call <c>FromXxx</c> directly on the source value. For example,
    /// <c>PhysicalWeight.FromKilograms(2.5m)</c> and
    /// <c>PhysicalWeight.FromPounds(PhysicalWeight.Convert(2.5m, PhysicalWeightUnit.Kilogram, PhysicalWeightUnit.Pound).OrThrow())</c>
    /// produce the same result — the former is shorter and avoids the extra round-trip.
    /// </para>
    /// <para>
    /// <b>Why the return type is <see cref="decimal"/> and not <see cref="PhysicalWeight"/>:</b>
    /// a conversion between units can produce a fractional value that is not representable
    /// as whole micrograms. For example, converting <c>1 μg</c> to pounds yields a value far
    /// below <c>1 lb</c>, which rounds to <c>0 lb</c> if coerced into a <see cref="PhysicalWeight"/> —
    /// silently losing the entire value. The <see cref="decimal"/> return type preserves the
    /// full precision. If the goal is to obtain a precise numeric result, prefer this method
    /// (or <see cref="ToUnit"/>) over converting to a <see cref="PhysicalWeight"/> first.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// decimal kilograms = 2.5m;
    /// decimal pounds = PhysicalWeight.Convert(kilograms, PhysicalWeightUnit.Kilogram, PhysicalWeightUnit.Pound).OrThrow();
    /// // pounds = 5.51155655462...
    /// </code>
    /// </example>
    public static Result<decimal> Convert(decimal value, PhysicalWeightUnit fromUnit, PhysicalWeightUnit toUnit)
    {
        if (fromUnit == toUnit)
            return value;

        if (!TryGetMicrogramFactor(fromUnit, out long fromFactor))
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT",
                FormattableString.Invariant($"Source unit '{(int)fromUnit}' is not defined."),
                ErrorType.ValidationError);

        if (!TryGetMicrogramFactor(toUnit, out long toFactor))
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.INVALID_UNIT",
                FormattableString.Invariant($"Target unit '{(int)toUnit}' is not defined."),
                ErrorType.ValidationError);

        try
        {
            return value * fromFactor / toFactor;
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.OVERFLOW",
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
    public static bool TryConvert(decimal value, PhysicalWeightUnit fromUnit, PhysicalWeightUnit toUnit, out decimal result)
    {
        result = default;
        return Convert(value, fromUnit, toUnit).TryGetValue(out result);
    }

    /// <summary>Converts this weight to micrograms.</summary>
    /// <returns>The canonical weight in micrograms.</returns>
    public long ToMicrograms() => Micrograms;

    /// <summary>Converts this weight to milligrams.</summary>
    /// <returns>The weight in milligrams (may have a fractional part).</returns>
    public decimal ToMilligrams() => Micrograms / (decimal)MicrogramsPerMilligram;

    /// <summary>Converts this weight to grams.</summary>
    /// <returns>The weight in grams (may have a fractional part).</returns>
    public decimal ToGrams() => Micrograms / (decimal)MicrogramsPerGram;

    /// <summary>Converts this weight to kilograms.</summary>
    /// <returns>The weight in kilograms (may have a fractional part).</returns>
    public decimal ToKilograms() => Micrograms / (decimal)MicrogramsPerKilogram;

    /// <summary>Converts this weight to metric tons.</summary>
    /// <returns>The weight in metric tons (may have a fractional part).</returns>
    public decimal ToTons() => Micrograms / (decimal)MicrogramsPerTon;

    /// <summary>Converts this weight to pounds.</summary>
    /// <returns>The weight in pounds (may have a fractional part).</returns>
    public decimal ToPounds() => Micrograms / (decimal)MicrogramsPerPound;

    /// <summary>Converts this weight to ounces.</summary>
    /// <returns>The weight in ounces (may have a fractional part).</returns>
    /// <remarks>
    /// The ounce conversion uses the exact definition (1 oz = 28,349,523.125 μg) conceptually,
    /// but the stored value is in whole micrograms, so a tiny rounding error (0.125 μg per ounce)
    /// may be present. See <see cref="FromOunces"/> for details.
    /// </remarks>
    public decimal ToOunces() => Micrograms / (decimal)MicrogramsPerOunce;

    /// <summary>
    /// Converts this weight to the specified unit.
    /// </summary>
    /// <param name="unit">The target unit.</param>
    /// <returns>The weight expressed in <paramref name="unit"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unit"/> is not a defined <see cref="PhysicalWeightUnit"/>.</exception>
    /// <remarks>
    /// This method is a convenience wrapper around the individual <c>ToXxx</c> methods,
    /// useful when the target unit is determined at runtime.
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalWeightUnit target = (PhysicalWeightUnit)request.TargetUnit;
    /// decimal value = weight.ToUnit(target);
    /// </code>
    /// </example>
    public decimal ToUnit(PhysicalWeightUnit unit)
    {
        return unit switch
        {
            PhysicalWeightUnit.Microgram => ToMicrograms(),
            PhysicalWeightUnit.Milligram => ToMilligrams(),
            PhysicalWeightUnit.Gram => ToGrams(),
            PhysicalWeightUnit.Kilogram => ToKilograms(),
            PhysicalWeightUnit.Ton => ToTons(),
            PhysicalWeightUnit.Pound => ToPounds(),
            PhysicalWeightUnit.Ounce => ToOunces(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(unit),
                FormattableString.Invariant($"Unsupported physical weight unit: '{(int)unit}'."))
        };
    }

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
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.OVERFLOW",
                "The addition resulted in a value that is too large.",
                ErrorType.ValidationError);
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
        return other.Micrograms > Micrograms
            ? new Error(
                "KernelR.ValueObjects.PhysicalWeight.NEGATIVE_RESULT",
                "Subtraction resulted in a negative weight.",
                ErrorType.ValidationError)
            : new PhysicalWeight(Micrograms - other.Micrograms);
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
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.NEGATIVE_MULTIPLIER",
                "Multiplier cannot be negative.",
                ErrorType.ValidationError);

        try
        {
            decimal result = Micrograms * multiplier;
            return result > long.MaxValue
                ? new Error(
                    "KernelR.ValueObjects.PhysicalWeight.OVERFLOW",
                    "The multiplication resulted in a value that is too large.",
                    ErrorType.ValidationError)
                : new PhysicalWeight((long)Math.Round(result, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.OVERFLOW",
                "The multiplication resulted in a value that is too large.",
                ErrorType.ValidationError);
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
        return divisor <= 0m
            ? new Error(
                "KernelR.ValueObjects.PhysicalWeight.DIVIDE_BY_ZERO",
                "Cannot divide by zero or a negative number.",
                ErrorType.ValidationError)
            : new PhysicalWeight((long)Math.Round(Micrograms / divisor, MidpointRounding.AwayFromZero));
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
        return other.Micrograms == 0L
            ? new Error(
                "KernelR.ValueObjects.PhysicalWeight.DIVIDE_BY_ZERO",
                "Cannot compute ratio against zero.",
                ErrorType.ValidationError)
            : Micrograms / (decimal)other.Micrograms;
    }

    /// <summary>Adds two physical weights.</summary>
    /// <param name="left">The first physical weight.</param>
    /// <param name="right">The second physical weight.</param>
    /// <returns>The sum of the two physical weights.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sum overflows <see cref="long.MaxValue"/>.</exception>
    public static PhysicalWeight operator +(PhysicalWeight left, PhysicalWeight right) => left.Add(right).OrThrowInvalidOperationException();

    /// <summary>Subtracts one physical weight from another.</summary>
    /// <param name="left">The first physical weight.</param>
    /// <param name="right">The physical weight to subtract.</param>
    /// <returns>The difference between the two physical weights.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is negative.</exception>
    public static PhysicalWeight operator -(PhysicalWeight left, PhysicalWeight right) => left.Subtract(right).OrThrowInvalidOperationException();

    /// <summary>Multiplies a physical weight by a scalar factor.</summary>
    /// <param name="weight">The physical weight.</param>
    /// <param name="multiplier">The scalar factor.</param>
    /// <returns>The product of the weight and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static PhysicalWeight operator *(PhysicalWeight weight, decimal multiplier) => weight.Multiply(multiplier).OrThrowInvalidOperationException();

    /// <summary>Multiplies a scalar factor by a physical weight (commutative support).</summary>
    /// <param name="multiplier">The scalar factor.</param>
    /// <param name="weight">The physical weight.</param>
    /// <returns>The product of the weight and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static PhysicalWeight operator *(decimal multiplier, PhysicalWeight weight) => weight.Multiply(multiplier).OrThrowInvalidOperationException();

    /// <summary>Divides a physical weight by a scalar divisor.</summary>
    /// <param name="weight">The physical weight.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The quotient of the weight and the divisor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the divisor is zero or negative.</exception>
    public static PhysicalWeight operator /(PhysicalWeight weight, decimal divisor) => weight.Divide(divisor).OrThrowInvalidOperationException();

    /// <summary>Returns the ratio of two physical weights as a <see cref="decimal"/>.</summary>
    /// <param name="left">The numerator.</param>
    /// <param name="right">The denominator.</param>
    /// <returns>The ratio of the two weights.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="right"/> is zero.</exception>
    public static decimal operator /(PhysicalWeight left, PhysicalWeight right) => left.Ratio(right).OrThrowInvalidOperationException();

    /// <summary>Determines whether the left weight is greater than the right weight.</summary>
    /// <param name="left">The left weight.</param>
    /// <param name="right">The right weight.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="PhysicalWeight"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(PhysicalWeight)"/>.
    /// </remarks>
    public static bool operator >(PhysicalWeight left, PhysicalWeight right) => left.Micrograms > right.Micrograms;

    /// <summary>Determines whether the left weight is less than the right weight.</summary>
    /// <param name="left">The left weight.</param>
    /// <param name="right">The right weight.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="PhysicalWeight"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(PhysicalWeight)"/>.
    /// </remarks>
    public static bool operator <(PhysicalWeight left, PhysicalWeight right) => left.Micrograms < right.Micrograms;

    /// <summary>Determines whether the left weight is greater than or equal to the right weight.</summary>
    /// <param name="left">The left weight.</param>
    /// <param name="right">The right weight.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="PhysicalWeight"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(PhysicalWeight)"/>.
    /// </remarks>
    public static bool operator >=(PhysicalWeight left, PhysicalWeight right) => left.Micrograms >= right.Micrograms;

    /// <summary>Determines whether the left weight is less than or equal to the right weight.</summary>
    /// <param name="left">The left weight.</param>
    /// <param name="right">The right weight.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="PhysicalWeight"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(PhysicalWeight)"/>.
    /// </remarks>
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
            _ => throw new ArgumentException(
                FormattableString.Invariant($"Object must be of type {nameof(PhysicalWeight)}."),
                nameof(obj))
        };
    }

    /// <summary>
    /// Returns a human-readable representation of the weight, automatically selecting
    /// the largest applicable metric unit (μg, mg, g, kg, or t).
    /// </summary>
    /// <returns>
    /// A culture-invariant string (e.g., <c>"2.5 kg"</c>, <c>"500 mg"</c>).
    /// </returns>
    /// <remarks>
    /// Imperial units (pounds, ounces) are not used by this overload because the canonical
    /// representation is metric. Use <see cref="ToString(PhysicalWeightUnit, IFormatProvider?)"/>
    /// or the format specifier <c>"lb"</c> / <c>"oz"</c> to display in imperial units.
    /// <para>
    /// To produce localized output (e.g., Persian digits), call
    /// <see cref="ToString(IFormatProvider?)"/> with the desired culture explicitly.
    /// </para>
    /// </remarks>
    public override string ToString() => FormatAutoSelected("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns a human-readable representation of the weight, using the specified culture.
    /// </summary>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information, or
    /// <see langword="null"/> to use <see cref="CultureInfo.InvariantCulture"/>.
    /// </param>
    /// <returns>A string such as <c>"2.5 kg"</c> or <c>"500 mg"</c>.</returns>
    /// <remarks>
    /// The unit is chosen automatically (μg, mg, g, kg, or t). The threshold check is
    /// rounding-aware: if rounding to two decimals would produce <c>1000</c> or more of
    /// the current unit, the next larger unit is used instead (e.g., <c>999,999 μg</c>
    /// is shown as <c>"1 g"</c>, not <c>"1000 mg"</c>).
    /// </remarks>
    public string ToString(IFormatProvider? provider)
        => FormatAutoSelected("0.##", provider ?? CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns a representation of the weight in the specified unit.
    /// </summary>
    /// <param name="unit">The unit to express the weight in.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information, or
    /// <see langword="null"/> to use <see cref="CultureInfo.InvariantCulture"/>.
    /// </param>
    /// <returns>A string such as <c>"5.51 lb"</c> or <c>"2500 mg"</c>.</returns>
    /// <remarks>
    /// This overload allows the caller to choose the unit explicitly. It supports both
    /// metric and imperial units. Use this when you need to force a specific unit
    /// (e.g., for user-facing display in a region that uses imperial units).
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalWeight weight = PhysicalWeight.FromKilograms(2.5m).OrThrow();
    /// Console.WriteLine(weight.ToString(PhysicalWeightUnit.Pound));  // "5.51 lb"
    /// Console.WriteLine(weight.ToString(PhysicalWeightUnit.Gram));   // "2500 g"
    /// </code>
    /// </example>
    public string ToString(PhysicalWeightUnit unit, IFormatProvider? provider = null)
    {
        IFormatProvider culture = provider ?? CultureInfo.InvariantCulture;
        decimal value = ToUnit(unit);

        return unit switch
        {
            PhysicalWeightUnit.Microgram => string.Format(culture, "{0} μg", value),
            PhysicalWeightUnit.Milligram => string.Format(culture, "{0:0.##} mg", value),
            PhysicalWeightUnit.Gram => string.Format(culture, "{0:0.##} g", value),
            PhysicalWeightUnit.Kilogram => string.Format(culture, "{0:0.##} kg", value),
            PhysicalWeightUnit.Ton => string.Format(culture, "{0:0.##} t", value),
            PhysicalWeightUnit.Pound => string.Format(culture, "{0:0.##} lb", value),
            PhysicalWeightUnit.Ounce => string.Format(culture, "{0:0.##} oz", value),
            _ => throw new ArgumentOutOfRangeException(
                nameof(unit),
                FormattableString.Invariant($"Unsupported physical weight unit: '{(int)unit}'."))
        };
    }

    /// <summary>
    /// Formats the weight using the specified format string and culture.
    /// </summary>
    /// <param name="format">
    /// The format string. May be:
    /// <list type="bullet">
    ///   <item><description><c>null</c> or <c>""</c> to auto-select the unit.</description></item>
    ///   <item><description>A unit specifier (case-insensitive): <c>"μg"</c>, <c>"ug"</c>, <c>"mg"</c>, <c>"g"</c>, <c>"kg"</c>, <c>"t"</c>, <c>"lb"</c>, <c>"oz"</c>.</description></item>
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
    /// string interpolation (e.g., <c>$"{weight:lb}"</c>) and by <see cref="string.Format(IFormatProvider, string, object[])"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalWeight weight = PhysicalWeight.FromKilograms(2.5m).OrThrow();
    /// string a = $"{weight}";      // "2.5 kg"    (auto-select)
    /// string b = $"{weight:lb}";   // "5.51 lb"   (unit specifier)
    /// string c = $"{weight:N3}";   // "2.500 kg"  (numeric format, auto-select)
    /// </code>
    /// </example>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        IFormatProvider culture = formatProvider ?? CultureInfo.InvariantCulture;

        return string.IsNullOrEmpty(format)
            ? FormatAutoSelected("0.##", culture)

            : TryParseUnit(format, out PhysicalWeightUnit unit)
            ? ToString(unit, culture)

            : FormatAutoSelected(format, culture);
    }

    /// <summary>Explicitly converts a <see cref="PhysicalWeight"/> to its underlying microgram count.</summary>
    /// <param name="weight">The physical weight to convert.</param>
    /// <returns>The canonical weight in micrograms.</returns>
    public static explicit operator long(PhysicalWeight weight) => weight.Micrograms;

    /// <summary>Explicitly converts a <see cref="PhysicalWeight"/> to its human-readable string representation.</summary>
    /// <param name="weight">The physical weight to convert.</param>
    /// <returns>The string representation of the physical weight, formatted using <see cref="CultureInfo.InvariantCulture"/>.</returns>
    public static explicit operator string(PhysicalWeight weight) => weight.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Creates a <see cref="PhysicalWeight"/> from a decimal value multiplied by a microgram factor.
    /// </summary>
    /// <param name="value">The decimal value. Must be non-negative.</param>
    /// <param name="factor">The number of micrograms per unit.</param>
    /// <returns>A successful result carrying the new <see cref="PhysicalWeight"/>, or a failure.</returns>
    private static Result<PhysicalWeight> FromDecimal(decimal value, long factor)
    {
        if (value < 0m)
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.NEGATIVE",
                "Physical weight cannot be negative.",
                ErrorType.ValidationError);

        try
        {
            decimal microgramsDecimal = value * factor;
            return microgramsDecimal > long.MaxValue
                ? new Error(
                    "KernelR.ValueObjects.PhysicalWeight.OVERFLOW",
                    "Physical weight is too large.",
                    ErrorType.ValidationError)
                : new PhysicalWeight((long)Math.Round(microgramsDecimal, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.PhysicalWeight.OVERFLOW",
                "Physical weight is too large.",
                ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Gets the microgram factor for a given unit.
    /// </summary>
    /// <param name="unit">The unit to look up.</param>
    /// <param name="factor">When this method returns, contains the number of micrograms per unit.</param>
    /// <returns><see langword="true"/> if the unit is defined; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetMicrogramFactor(PhysicalWeightUnit unit, out long factor)
    {
        switch (unit)
        {
            case PhysicalWeightUnit.Microgram: factor = 1L; return true;
            case PhysicalWeightUnit.Milligram: factor = MicrogramsPerMilligram; return true;
            case PhysicalWeightUnit.Gram: factor = MicrogramsPerGram; return true;
            case PhysicalWeightUnit.Kilogram: factor = MicrogramsPerKilogram; return true;
            case PhysicalWeightUnit.Ton: factor = MicrogramsPerTon; return true;
            case PhysicalWeightUnit.Pound: factor = MicrogramsPerPound; return true;
            case PhysicalWeightUnit.Ounce: factor = MicrogramsPerOunce; return true;
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
    /// This method intentionally uses <see cref="string.ToLowerInvariant"/> so that unit parsing
    /// is not affected by the current UI culture (e.g., the Turkish-I problem).
    /// </remarks>
    private static bool TryParseUnit(string text, out PhysicalWeightUnit unit)
    {
        switch (text.ToLowerInvariant())
        {
            case "μg" or "ug" or "microgram" or "micrograms": unit = PhysicalWeightUnit.Microgram; return true;
            case "mg" or "milligram" or "milligrams": unit = PhysicalWeightUnit.Milligram; return true;
            case "g" or "gram" or "grams": unit = PhysicalWeightUnit.Gram; return true;
            case "kg" or "kilogram" or "kilograms": unit = PhysicalWeightUnit.Kilogram; return true;
            case "t" or "ton" or "tons" or "tonne" or "tonnes": unit = PhysicalWeightUnit.Ton; return true;
            case "lb" or "lbs" or "pound" or "pounds": unit = PhysicalWeightUnit.Pound; return true;
            case "oz" or "ounce" or "ounces": unit = PhysicalWeightUnit.Ounce; return true;
            default: unit = default; return false;
        }
    }

    /// <summary>
    /// Parses a unit name from a string and returns its microgram factor.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="factor">When this method returns, contains the number of micrograms per unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnitFactor(string text, out long factor)
    {
        factor = 0L;
        return TryParseUnit(text, out PhysicalWeightUnit unit) && TryGetMicrogramFactor(unit, out factor);
    }

    /// <summary>
    /// Formats the weight using the auto-selected metric unit (μg, mg, g, kg, or t) with the
    /// specified numeric format string.
    /// </summary>
    /// <param name="numericFormat">The .NET numeric format string (e.g., "0.##", "N2", "F3").</param>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <returns>A formatted string such as <c>"2.5 kg"</c>.</returns>
    /// <remarks>
    /// <para>
    /// The unit is chosen by comparing the rounded value in each unit against the threshold
    /// for the next larger unit. This avoids the <c>"1000 mg"</c> vs <c>"1 g"</c> ambiguity
    /// that would occur if the raw (unrounded) value were compared directly.
    /// </para>
    /// <para>
    /// Specifically, <c>999,999 μg</c> is displayed as <c>"1 g"</c>, not <c>"1000 mg"</c>,
    /// because rounding 999.999 mg to two decimals yields 1000 mg, which is the threshold
    /// for one gram.
    /// </para>
    /// </remarks>
    private string FormatAutoSelected(string numericFormat, IFormatProvider culture)
    {
        switch (Micrograms)
        {
            case >= MicrogramsPerTon:
                return string.Format(culture, "{0:" + numericFormat + "} t", ToTons());
            case >= MicrogramsPerKilogram:
                {
                    decimal kg = ToKilograms();
                    return Math.Round(kg, 2, MidpointRounding.AwayFromZero) >= 1000m
                        ? string.Format(culture, "{0:" + numericFormat + "} t", ToTons())
                        : string.Format(culture, "{0:" + numericFormat + "} kg", kg);
                }

            case >= MicrogramsPerGram:
                {
                    decimal g = ToGrams();
                    return Math.Round(g, 2, MidpointRounding.AwayFromZero) >= 1000m
                        ? string.Format(culture, "{0:" + numericFormat + "} kg", ToKilograms())
                        : string.Format(culture, "{0:" + numericFormat + "} g", g);
                }

            case >= MicrogramsPerMilligram:
                {
                    decimal mg = ToMilligrams();
                    return Math.Round(mg, 2, MidpointRounding.AwayFromZero) >= 1000m
                        ? string.Format(culture, "{0:" + numericFormat + "} g", ToGrams())
                        : string.Format(culture, "{0:" + numericFormat + "} mg", mg);
                }

            default:
                return string.Format(culture, "{0:" + numericFormat + "} μg", Micrograms);
        }
    }
}
