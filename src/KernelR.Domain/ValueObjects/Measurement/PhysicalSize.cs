using KernelR.Domain.Enums;
using KernelR.Domain.Results;
using KernelR.Domain.Results.Extensions;
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
///     Fractional sizes supplied via <c>FromInches</c>, <c>FromFeet</c>, <c>FromYards</c>, etc. are
///     rounded to the nearest micrometer using <see cref="MidpointRounding.AwayFromZero"/>.
///   </description></item>
/// </list>
/// <para>
/// <b>No precision loss:</b> Unlike weight (where 1 oz has a fractional microgram part),
/// all supported size conversions are exact integers in micrometers:
/// <c>1 in = 25,400 μm</c>, <c>1 ft = 304,800 μm</c>, <c>1 yd = 914,400 μm</c>, <c>1 m = 1,000,000 μm</c>.
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
/// <c>size.ToString(CultureInfo.GetCultureInfo("fa-IR"))</c>.
/// This keeps the decision of "how to display" in the presentation layer, not in the library.
/// </para>
/// <para>
/// <b>Mathematical operations:</b> Fallible operations (<c>Add</c>, <c>Subtract</c>, <c>Multiply</c>,
/// <c>Divide</c>, <c>Ratio</c>) return <see cref="Result{T}"/> and have corresponding operators
/// that throw <see cref="InvalidOperationException"/> on failure. Comparison uses the standard
/// operators (<c>&gt;</c>, <c>&lt;</c>, <c>&gt;=</c>, <c>&lt;=</c>) and <see cref="CompareTo"/> —
/// no separate comparison methods exist, because comparing two <see cref="PhysicalSize"/>
/// instances cannot fail.
/// </para>
/// <para>
/// <b>Format specifiers (IFormattable):</b> The format string passed to
/// <see cref="ToString(string?, IFormatProvider?)"/> may be one of:
/// </para>
/// <list type="bullet">
///   <item><description><c>null</c> or <c>""</c>: auto-select the largest applicable metric unit.</description></item>
///   <item><description>A unit specifier (case-insensitive): <c>"μm"</c>, <c>"um"</c>, <c>"mm"</c>, <c>"cm"</c>, <c>"m"</c>, <c>"in"</c>, <c>"ft"</c>, <c>"yd"</c>.</description></item>
///   <item><description>A .NET numeric format string (e.g., <c>"N2"</c>, <c>"F3"</c>): auto-select the unit, then apply the numeric format.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;PhysicalSize&gt; result = PhysicalSize.FromInches(2.5m);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.ToCentimeters()); // Outputs: 6.35
///     Console.WriteLine(result.Value);                  // Outputs: 6.35 cm
///     Console.WriteLine($"{result.Value:in}");          // Outputs: 2.5 in
///     Console.WriteLine($"{result.Value:N2}");          // Outputs: 6.35 cm
///     Console.WriteLine(result.Value > PhysicalSize.Zero); // Outputs: True
/// }
/// </code>
/// </example>
public readonly record struct PhysicalSize :
    IValueObject,
    IParsable<PhysicalSize>,
    IComparable<PhysicalSize>,
    IComparable,
    IFormattable
{
    /// <summary>The number of micrometers in one millimeter (1,000).</summary>
    public const long MicrometersPerMillimeter = 1_000L;

    /// <summary>The number of micrometers in one centimeter (10,000).</summary>
    public const long MicrometersPerCentimeter = 10_000L;

    /// <summary>The number of micrometers in one meter (1,000,000).</summary>
    public const long MicrometersPerMeter = 1_000_000L;

    /// <summary>The number of micrometers in one inch (25,400), exact by the international definition (1 in = 2.54 cm).</summary>
    public const long MicrometersPerInch = 25_400L;

    /// <summary>The number of micrometers in one foot (304,800), exact by the international definition (1 ft = 12 in).</summary>
    public const long MicrometersPerFoot = 304_800L;

    /// <summary>The number of micrometers in one yard (914,400), exact by the international definition (1 yd = 3 ft = 36 in).</summary>
    public const long MicrometersPerYard = 914_400L;

    /// <summary>The number of millimeters in one centimeter (10).</summary>
    public const long MillimetersPerCentimeter = 10L;

    /// <summary>The number of millimeters in one meter (1,000).</summary>
    public const long MillimetersPerMeter = 1_000L;

    /// <summary>The number of centimeters in one meter (100).</summary>
    public const long CentimetersPerMeter = 100L;

    /// <summary>The number of inches in one foot (12).</summary>
    public const long InchesPerFoot = 12L;

    /// <summary>The number of inches in one yard (36).</summary>
    public const long InchesPerYard = 36L;

    /// <summary>The number of feet in one yard (3).</summary>
    public const long FeetPerYard = 3L;

    /// <summary>The number of millimeters in one inch (25.4), exact by the international definition.</summary>
    public const decimal MillimetersPerInch = 25.4m;

    /// <summary>The number of millimeters in one foot (304.8), exact by the international definition.</summary>
    public const decimal MillimetersPerFoot = 304.8m;

    /// <summary>The number of millimeters in one yard (914.4), exact by the international definition.</summary>
    public const decimal MillimetersPerYard = 914.4m;

    /// <summary>The number of centimeters in one inch (2.54), exact by the international definition.</summary>
    public const decimal CentimetersPerInch = 2.54m;

    /// <summary>The number of centimeters in one foot (30.48), exact by the international definition.</summary>
    public const decimal CentimetersPerFoot = 30.48m;

    /// <summary>The number of centimeters in one yard (91.44), exact by the international definition.</summary>
    public const decimal CentimetersPerYard = 91.44m;

    /// <summary>The number of meters in one inch (0.0254), exact by the international definition.</summary>
    public const decimal MetersPerInch = 0.0254m;

    /// <summary>The number of meters in one foot (0.3048), exact by the international definition.</summary>
    public const decimal MetersPerFoot = 0.3048m;

    /// <summary>The number of meters in one yard (0.9144), exact by the international definition.</summary>
    public const decimal MetersPerYard = 0.9144m;

    /// <summary>
    /// Gets the canonical size, in micrometers (μm).
    /// </summary>
    /// <value>A non-negative number of micrometers.</value>
    /// <remarks>
    /// This is the single source of truth for the value object. All conversions
    /// (to mm, cm, m, in, ft, yd) are derived from this value.
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
    /// Creates a <see cref="PhysicalSize"/> from a number of yards.
    /// </summary>
    /// <param name="yards">The number of yards. Must be non-negative.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="PhysicalSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.NEGATIVE</c> when <paramref name="yards"/> is negative.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the result exceeds <see cref="long.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// 1 yard = 914,400 micrometers (exact by the international definition: 1 yd = 3 ft = 36 in).
    /// There is no precision loss.
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalSize.FromYards(1m); // → 0.9144 m (exact, no rounding error)
    /// </code>
    /// </example>
    public static Result<PhysicalSize> FromYards(decimal yards) => FromDecimal(yards, MicrometersPerYard);

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
                PhysicalSizeUnit.Yard => FromYards(amount),
                _ => new Error(
                    "KernelR.ValueObjects.PhysicalSize.INVALID_UNIT",
                    FormattableString.Invariant($"Physical size unit '{(int)unit}' is not defined."),
                    ErrorType.ValidationError),
            };
    }

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a string in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>.
    /// </summary>
    /// <param name="value">
    /// The string representation, for example <c>"5 cm"</c>, <c>"2.5 ft"</c>, <c>"1000 μm"</c>.
    /// The numeric value is parsed using <see cref="CultureInfo.InvariantCulture"/>; the unit accepts
    /// both full names and short forms (case-insensitive).
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
    /// PhysicalSize.Create("3 yd");    // → PhysicalSize(3 yd)
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
                FormattableString.Invariant($"Physical size unit '{parts[1]}' is not recognized."),
                ErrorType.ValidationError)

            : FromDecimal(amount, factor);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="PhysicalSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <returns>A <see cref="PhysicalSize"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not in the form <c>"&lt;value&gt; &lt;unit&gt;"</c>,
    /// or contains an invalid value or unit. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which requires
    /// an <see cref="IFormatProvider"/> parameter. Because <see cref="PhysicalSize"/> is a
    /// machine-to-machine contract, parsing is <b>always</b> performed using
    /// <see cref="CultureInfo.InvariantCulture"/> regardless of the provider passed.
    /// </para>
    /// <para>
    /// In other words: <c>PhysicalSize.Parse("5.5 cm", CultureInfo.GetCultureInfo("de-DE"))</c>
    /// and <c>PhysicalSize.Parse("5.5 cm", null)</c> produce identical results. A German caller
    /// that wants to accept the German decimal separator (<c>"5,5 cm"</c>) must normalize the
    /// input to invariant form (<c>"5.5 cm"</c>) before calling this method.
    /// </para>
    /// <para>
    /// For consistency, the same policy applies to
    /// <see cref="TryParse(string?, IFormatProvider?, out PhysicalSize)"/>.
    /// </para>
    /// </remarks>
    public static PhysicalSize Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="PhysicalSize"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="PhysicalSize"/> instance, if parsing
    /// succeeded, or the default value if it failed.
    /// </param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which requires
    /// an <see cref="IFormatProvider"/> parameter. Because <see cref="PhysicalSize"/> is a
    /// machine-to-machine contract, parsing is <b>always</b> performed using
    /// <see cref="CultureInfo.InvariantCulture"/> regardless of the provider passed.
    /// </para>
    /// <para>
    /// When only a validity check is needed and the resulting instance is not used, callers
    /// can discard it via <c>TryParse(value, null, out _)</c>.
    /// </para>
    /// </remarks>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PhysicalSize result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Converts a raw decimal value from one unit to another, without creating a <see cref="PhysicalSize"/>.
    /// </summary>
    /// <param name="value">The raw value to convert.</param>
    /// <param name="fromUnit">The source unit of <paramref name="value"/>.</param>
    /// <param name="toUnit">The target unit for the result.</param>
    /// <returns>
    /// A successful result carrying the converted value, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.INVALID_UNIT</c> when either unit is not defined.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhysicalSize.OVERFLOW</c> when the intermediate calculation exceeds <see cref="decimal.MaxValue"/>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is intended for callers who only need the numeric conversion and do not
    /// want to instantiate a <see cref="PhysicalSize"/>. The conversion is performed in
    /// canonical micrometers, so it is exact for all supported units.
    /// </para>
    /// <para>
    /// <b>Do not combine this with a <c>FromXxx</c> factory.</b> If the goal is to build a
    /// <see cref="PhysicalSize"/>, call <c>FromXxx</c> directly on the source value. For example,
    /// <c>PhysicalSize.FromInches(2.5m)</c> and
    /// <c>PhysicalSize.FromCentimeters(PhysicalSize.Convert(2.5m, PhysicalSizeUnit.Inch, PhysicalSizeUnit.Centimeter).OrThrow())</c>
    /// produce the same result — the former is shorter and avoids the extra round-trip.
    /// </para>
    /// <para>
    /// <b>Why the return type is <see cref="decimal"/> and not <see cref="PhysicalSize"/>:</b>
    /// a conversion between units can produce a fractional value that is not representable
    /// as whole micrometers. For example, converting <c>1 μm</c> to yards yields
    /// <c>0.000001093613298... yd</c>, which rounds to <c>0 yd</c> if coerced into a
    /// <see cref="PhysicalSize"/> — silently losing the entire value. The <see cref="decimal"/>
    /// return type preserves the full precision. If the goal is to obtain a precise numeric
    /// result, prefer this method (or <see cref="ToUnit"/>) over converting to a
    /// <see cref="PhysicalSize"/> first.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// decimal inches = 100m;
    /// decimal millimeters = PhysicalSize.Convert(inches, PhysicalSizeUnit.Inch, PhysicalSizeUnit.Millimeter).OrThrow();
    /// // millimeters = 2540
    /// </code>
    /// </example>
    public static Result<decimal> Convert(decimal value, PhysicalSizeUnit fromUnit, PhysicalSizeUnit toUnit)
    {
        if (fromUnit == toUnit)
            return value;

        if (!TryGetMicrometerFactor(fromUnit, out long fromFactor))
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_UNIT",
                FormattableString.Invariant($"Source unit '{(int)fromUnit}' is not defined."),
                ErrorType.ValidationError);

        if (!TryGetMicrometerFactor(toUnit, out long toFactor))
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.INVALID_UNIT",
                FormattableString.Invariant($"Target unit '{(int)toUnit}' is not defined."),
                ErrorType.ValidationError);

        try
        {
            return value * fromFactor / toFactor;
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.OVERFLOW",
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
    public static bool TryConvert(decimal value, PhysicalSizeUnit fromUnit, PhysicalSizeUnit toUnit, out decimal result)
    {
        result = default;
        return Convert(value, fromUnit, toUnit).TryGetValue(out result);
    }

    /// <summary>Converts this size to micrometers.</summary>
    /// <returns>The canonical size in micrometers.</returns>
    public long ToMicrometers() => Micrometers;

    /// <summary>Converts this size to millimeters.</summary>
    /// <returns>The size in millimeters (may have a fractional part).</returns>
    public decimal ToMillimeters() => Micrometers / (decimal)MicrometersPerMillimeter;

    /// <summary>Converts this size to centimeters.</summary>
    /// <returns>The size in centimeters (may have a fractional part).</returns>
    public decimal ToCentimeters() => Micrometers / (decimal)MicrometersPerCentimeter;

    /// <summary>Converts this size to meters.</summary>
    /// <returns>The size in meters (may have a fractional part).</returns>
    public decimal ToMeters() => Micrometers / (decimal)MicrometersPerMeter;

    /// <summary>Converts this size to inches.</summary>
    /// <returns>The size in inches (may have a fractional part).</returns>
    public decimal ToInches() => Micrometers / (decimal)MicrometersPerInch;

    /// <summary>Converts this size to feet.</summary>
    /// <returns>The size in feet (may have a fractional part).</returns>
    public decimal ToFeet() => Micrometers / (decimal)MicrometersPerFoot;

    /// <summary>Converts this size to yards.</summary>
    /// <returns>The size in yards (may have a fractional part).</returns>
    public decimal ToYards() => Micrometers / (decimal)MicrometersPerYard;

    /// <summary>
    /// Converts this size to the specified unit.
    /// </summary>
    /// <param name="unit">The target unit.</param>
    /// <returns>The size expressed in <paramref name="unit"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unit"/> is not a defined <see cref="PhysicalSizeUnit"/>.</exception>
    /// <remarks>
    /// This method is a convenience wrapper around the individual <c>ToXxx</c> methods,
    /// useful when the target unit is determined at runtime.
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalSizeUnit target = (PhysicalSizeUnit)request.TargetUnit;
    /// decimal value = size.ToUnit(target);
    /// </code>
    /// </example>
    public decimal ToUnit(PhysicalSizeUnit unit)
    {
        return unit switch
        {
            PhysicalSizeUnit.Micrometer => ToMicrometers(),
            PhysicalSizeUnit.Millimeter => ToMillimeters(),
            PhysicalSizeUnit.Centimeter => ToCentimeters(),
            PhysicalSizeUnit.Meter => ToMeters(),
            PhysicalSizeUnit.Inch => ToInches(),
            PhysicalSizeUnit.Foot => ToFeet(),
            PhysicalSizeUnit.Yard => ToYards(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(unit),
                FormattableString.Invariant($"Unsupported physical size unit: '{(int)unit}'."))
        };
    }

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
        => other.Micrometers > Micrometers
            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.NEGATIVE_RESULT",
                "Subtraction resulted in a negative physical size.",
                ErrorType.ValidationError)
            : new PhysicalSize(Micrometers - other.Micrometers);


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
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.NEGATIVE_MULTIPLIER",
                "Multiplier cannot be negative.",
                ErrorType.ValidationError);
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
            ? new Error(
                "KernelR.ValueObjects.PhysicalSize.DIVIDE_BY_ZERO",
                "Cannot divide by zero or a negative number.",
                ErrorType.ValidationError)
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
            : Micrometers / (decimal)other.Micrometers;
    }

    /// <summary>Adds two physical sizes.</summary>
    /// <param name="left">The first physical size.</param>
    /// <param name="right">The second physical size.</param>
    /// <returns>The sum of the two physical sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sum overflows <see cref="long.MaxValue"/>.</exception>
    public static PhysicalSize operator +(PhysicalSize left, PhysicalSize right) => left.Add(right).OrThrowInvalidOperationException();

    /// <summary>Subtracts one physical size from another.</summary>
    /// <param name="left">The first physical size.</param>
    /// <param name="right">The physical size to subtract.</param>
    /// <returns>The difference between the two physical sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is negative.</exception>
    public static PhysicalSize operator -(PhysicalSize left, PhysicalSize right) => left.Subtract(right).OrThrowInvalidOperationException();

    /// <summary>Multiplies a physical size by a scalar factor.</summary>
    /// <param name="size">The physical size.</param>
    /// <param name="multiplier">The scalar factor.</param>
    /// <returns>The product of the size and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static PhysicalSize operator *(PhysicalSize size, decimal multiplier) => size.Multiply(multiplier).OrThrowInvalidOperationException();

    /// <summary>Multiplies a scalar factor by a physical size (commutative support).</summary>
    /// <param name="multiplier">The scalar factor.</param>
    /// <param name="size">The physical size.</param>
    /// <returns>The product of the size and the factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the multiplier is negative or the product overflows.</exception>
    public static PhysicalSize operator *(decimal multiplier, PhysicalSize size) => size.Multiply(multiplier).OrThrowInvalidOperationException();

    /// <summary>Divides a physical size by a scalar divisor.</summary>
    /// <param name="size">The physical size.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The quotient of the size and the divisor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the divisor is zero or negative.</exception>
    public static PhysicalSize operator /(PhysicalSize size, decimal divisor) => size.Divide(divisor).OrThrowInvalidOperationException();

    /// <summary>Returns the ratio of two physical sizes as a <see cref="decimal"/>.</summary>
    /// <param name="left">The numerator.</param>
    /// <param name="right">The denominator.</param>
    /// <returns>The ratio of the two sizes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="right"/> is zero.</exception>
    public static decimal operator /(PhysicalSize left, PhysicalSize right) => left.Ratio(right).OrThrowInvalidOperationException();

    /// <summary>Determines whether the left size is greater than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="PhysicalSize"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(PhysicalSize)"/>.
    /// </remarks>
    public static bool operator >(PhysicalSize left, PhysicalSize right) => left.Micrometers > right.Micrometers;

    /// <summary>Determines whether the left size is less than the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="PhysicalSize"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(PhysicalSize)"/>.
    /// </remarks>
    public static bool operator <(PhysicalSize left, PhysicalSize right) => left.Micrometers < right.Micrometers;

    /// <summary>Determines whether the left size is greater than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is greater or equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="PhysicalSize"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(PhysicalSize)"/>.
    /// </remarks>
    public static bool operator >=(PhysicalSize left, PhysicalSize right) => left.Micrometers >= right.Micrometers;

    /// <summary>Determines whether the left size is less than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is less or equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Comparison operators are provided directly (not via a <see cref="Result{T}"/>-returning
    /// method) because comparing two <see cref="PhysicalSize"/> instances is a total operation
    /// that cannot fail. For sorting scenarios, use <see cref="CompareTo(PhysicalSize)"/>.
    /// </remarks>
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
            _ => throw new ArgumentException(
                FormattableString.Invariant($"Object must be of type {nameof(PhysicalSize)}."),
                nameof(obj))
        };
    }

    /// <summary>
    /// Returns a human-readable representation of the size, automatically selecting
    /// the largest applicable metric unit (μm, mm, cm, or m).
    /// </summary>
    /// <returns>
    /// A culture-invariant string (e.g., <c>"6.35 cm"</c>, <c>"500 μm"</c>).
    /// </returns>
    /// <remarks>
    /// Imperial units (inches, feet, yards) are not used by this overload because
    /// the canonical representation is metric. Use <see cref="ToString(PhysicalSizeUnit, IFormatProvider?)"/>
    /// or the format specifier <c>"in"</c> / <c>"ft"</c> / <c>"yd"</c> to display in imperial units.
    /// <para>
    /// To produce localized output (e.g., Persian digits), call
    /// <see cref="ToString(IFormatProvider?)"/> with the desired culture explicitly.
    /// </para>
    /// </remarks>
    public override string ToString() => FormatAutoSelected("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns a human-readable representation of the size, using the specified culture.
    /// </summary>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information, or
    /// <see langword="null"/> to use <see cref="CultureInfo.InvariantCulture"/>.
    /// </param>
    /// <returns>A string such as <c>"6.35 cm"</c> or <c>"500 μm"</c>.</returns>
    /// <remarks>
    /// The unit is chosen automatically (μm, mm, cm, or m). The threshold check is
    /// rounding-aware: if rounding to two decimals would produce <c>100</c> or more of
    /// the current unit, the next larger unit is used instead (e.g., <c>999,999 μm</c>
    /// is shown as <c>"1 m"</c>, not <c>"100 cm"</c>).
    /// </remarks>
    public string ToString(IFormatProvider? provider)
        => FormatAutoSelected("0.##", provider ?? CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns a representation of the size in the specified unit.
    /// </summary>
    /// <param name="unit">The unit to express the size in.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information, or
    /// <see langword="null"/> to use <see cref="CultureInfo.InvariantCulture"/>.
    /// </param>
    /// <returns>A string such as <c>"2.5 in"</c> or <c>"1500 mm"</c>.</returns>
    /// <remarks>
    /// This overload allows the caller to choose the unit explicitly. It supports both
    /// metric and imperial units. Use this when you need to force a specific unit
    /// (e.g., for user-facing display in a region that uses imperial units).
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalSize size = PhysicalSize.FromInches(2.5m).OrThrow();
    /// Console.WriteLine(size.ToString(PhysicalSizeUnit.Millimeter)); // "63.5 mm"
    /// Console.WriteLine(size.ToString(PhysicalSizeUnit.Inch));       // "2.5 in"
    /// </code>
    /// </example>
    public string ToString(PhysicalSizeUnit unit, IFormatProvider? provider = null)
    {
        IFormatProvider culture = provider ?? CultureInfo.InvariantCulture;
        decimal value = ToUnit(unit);

        return unit switch
        {
            PhysicalSizeUnit.Micrometer => string.Format(culture, "{0} μm", value),
            PhysicalSizeUnit.Millimeter => string.Format(culture, "{0:0.##} mm", value),
            PhysicalSizeUnit.Centimeter => string.Format(culture, "{0:0.##} cm", value),
            PhysicalSizeUnit.Meter => string.Format(culture, "{0:0.##} m", value),
            PhysicalSizeUnit.Inch => string.Format(culture, "{0:0.##} in", value),
            PhysicalSizeUnit.Foot => string.Format(culture, "{0:0.##} ft", value),
            PhysicalSizeUnit.Yard => string.Format(culture, "{0:0.##} yd", value),
            _ => throw new ArgumentOutOfRangeException(
                nameof(unit),
                FormattableString.Invariant($"Unsupported physical size unit: '{(int)unit}'."))
        };
    }

    /// <summary>
    /// Formats the size using the specified format string and culture.
    /// </summary>
    /// <param name="format">
    /// The format string. May be:
    /// <list type="bullet">
    ///   <item><description><c>null</c> or <c>""</c> to auto-select the unit.</description></item>
    ///   <item><description>A unit specifier (case-insensitive): <c>"μm"</c>, <c>"um"</c>, <c>"mm"</c>, <c>"cm"</c>, <c>"m"</c>, <c>"in"</c>, <c>"ft"</c>, <c>"yd"</c>.</description></item>
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
    /// string interpolation (e.g., <c>$"{size:in}"</c>) and by <see cref="string.Format(IFormatProvider, string, object[])"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// PhysicalSize size = PhysicalSize.FromInches(2.5m).OrThrow();
    /// string a = $"{size}";      // "6.35 cm"   (auto-select)
    /// string b = $"{size:in}";   // "2.5 in"    (unit specifier)
    /// string c = $"{size:N2}";   // "6.35 cm"   (numeric format, auto-select)
    /// </code>
    /// </example>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        IFormatProvider culture = formatProvider ?? CultureInfo.InvariantCulture;

        return string.IsNullOrEmpty(format)
            ? FormatAutoSelected("0.##", culture)

            : TryParseUnit(format, out PhysicalSizeUnit unit)
            ? ToString(unit, culture)

            : FormatAutoSelected(format, culture);
    }

    /// <summary>Explicitly converts a <see cref="PhysicalSize"/> to its underlying micrometer count.</summary>
    /// <param name="size">The physical size to convert.</param>
    /// <returns>The canonical size in micrometers.</returns>
    public static explicit operator long(PhysicalSize size) => size.Micrometers;

    /// <summary>Explicitly converts a <see cref="PhysicalSize"/> to its human-readable string representation.</summary>
    /// <param name="size">The physical size to convert.</param>
    /// <returns>The string representation of the physical size, formatted using <see cref="CultureInfo.InvariantCulture"/>.</returns>
    public static explicit operator string(PhysicalSize size) => size.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Creates a <see cref="PhysicalSize"/> from a decimal value multiplied by a micrometer factor.
    /// </summary>
    /// <param name="value">The decimal value. Must be non-negative.</param>
    /// <param name="factor">The number of micrometers per unit.</param>
    /// <returns>A successful result carrying the new <see cref="PhysicalSize"/>, or a failure.</returns>
    private static Result<PhysicalSize> FromDecimal(decimal value, long factor)
    {
        if (value < 0m)
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.NEGATIVE",
                "Physical size cannot be negative.",
                ErrorType.ValidationError);

        try
        {
            decimal micrometersDecimal = value * factor;
            return micrometersDecimal > long.MaxValue
                ? new Error(
                    "KernelR.ValueObjects.PhysicalSize.OVERFLOW",
                    "Physical size is too large.",
                    ErrorType.ValidationError)
                : new PhysicalSize((long)Math.Round(micrometersDecimal, MidpointRounding.AwayFromZero));
        }
        catch (OverflowException)
        {
            return new Error(
                "KernelR.ValueObjects.PhysicalSize.OVERFLOW",
                "Physical size is too large.",
                ErrorType.ValidationError);
        }
    }

    /// <summary>
    /// Gets the micrometer factor for a given unit.
    /// </summary>
    /// <param name="unit">The unit to look up.</param>
    /// <param name="factor">When this method returns, contains the number of micrometers per unit.</param>
    /// <returns><see langword="true"/> if the unit is defined; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetMicrometerFactor(PhysicalSizeUnit unit, out long factor)
    {
        switch (unit)
        {
            case PhysicalSizeUnit.Micrometer: factor = 1L; return true;
            case PhysicalSizeUnit.Millimeter: factor = MicrometersPerMillimeter; return true;
            case PhysicalSizeUnit.Centimeter: factor = MicrometersPerCentimeter; return true;
            case PhysicalSizeUnit.Meter: factor = MicrometersPerMeter; return true;
            case PhysicalSizeUnit.Inch: factor = MicrometersPerInch; return true;
            case PhysicalSizeUnit.Foot: factor = MicrometersPerFoot; return true;
            case PhysicalSizeUnit.Yard: factor = MicrometersPerYard; return true;
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
    private static bool TryParseUnit(string text, out PhysicalSizeUnit unit)
    {
        switch (text.ToLowerInvariant())
        {
            case "μm" or "um" or "micrometer" or "micrometers": unit = PhysicalSizeUnit.Micrometer; return true;
            case "mm" or "millimeter" or "millimeters": unit = PhysicalSizeUnit.Millimeter; return true;
            case "cm" or "centimeter" or "centimeters": unit = PhysicalSizeUnit.Centimeter; return true;
            case "m" or "meter" or "meters": unit = PhysicalSizeUnit.Meter; return true;
            case "in" or "inch" or "inches": unit = PhysicalSizeUnit.Inch; return true;
            case "ft" or "foot" or "feet": unit = PhysicalSizeUnit.Foot; return true;
            case "yd" or "yard" or "yards": unit = PhysicalSizeUnit.Yard; return true;
            default: unit = default; return false;
        }
    }

    /// <summary>
    /// Parses a unit name from a string and returns its micrometer factor.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="factor">When this method returns, contains the number of micrometers per unit.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseUnitFactor(string text, out long factor)
    {
        factor = 0L;
        return TryParseUnit(text, out PhysicalSizeUnit unit) && TryGetMicrometerFactor(unit, out factor);
    }

    /// <summary>
    /// Formats the size using the auto-selected metric unit (μm, mm, cm, or m) with the
    /// specified numeric format string.
    /// </summary>
    /// <param name="numericFormat">The .NET numeric format string (e.g., "0.##", "N2", "F3").</param>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <returns>A formatted string such as <c>"6.35 cm"</c>.</returns>
    /// <remarks>
    /// <para>
    /// The unit is chosen by comparing the rounded value in each unit against the threshold
    /// for the next larger unit. This avoids ambiguities like <c>"100 cm"</c> vs <c>"1 m"</c>
    /// or <c>"10 mm"</c> vs <c>"1 cm"</c> that would occur if the raw (unrounded) value were
    /// compared directly.
    /// </para>
    /// <para>
    /// Examples of the rounding-aware threshold behavior:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><c>999,999 μm</c> → <c>"1 m"</c> (not <c>"100 cm"</c>)</description></item>
    ///   <item><description><c>9,999 μm</c> → <c>"1 cm"</c> (not <c>"10 mm"</c>)</description></item>
    ///   <item><description><c>999,500 μm</c> → <c>"99.95 cm"</c> (not rounded to 100 cm)</description></item>
    /// </list>
    /// </remarks>
    private string FormatAutoSelected(string numericFormat, IFormatProvider culture)
    {
        switch (Micrometers)
        {
            case >= MicrometersPerMeter:
                return string.Format(culture, "{0:" + numericFormat + "} m", ToMeters());
            case >= MicrometersPerCentimeter:
                {
                    decimal cm = ToCentimeters();
                    return Math.Round(cm, 2, MidpointRounding.AwayFromZero) >= 100m
                        ? string.Format(culture, "{0:" + numericFormat + "} m", ToMeters())
                        : string.Format(culture, "{0:" + numericFormat + "} cm", cm);
                }

            case >= MicrometersPerMillimeter:
                {
                    decimal mm = ToMillimeters();
                    return Math.Round(mm, 2, MidpointRounding.AwayFromZero) >= 10m
                        ? string.Format(culture, "{0:" + numericFormat + "} cm", ToCentimeters())
                        : string.Format(culture, "{0:" + numericFormat + "} mm", mm);
                }

            default:
                return string.Format(culture, "{0:" + numericFormat + "} μm", Micrometers);
        }
    }
}
