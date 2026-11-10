using KernelR.Patterns;
using KernelR.Patterns.Results;
using KernelR.Patterns.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a valid Iranian mobile phone number.
/// </summary>
/// <remarks>
/// <para>
/// This type contains <b>only</b> domain logic: normalization and validation.
/// It has <b>no</b> dependency on ASP.NET Core, System.Text.Json, or any
/// presentation-layer concern. Serialization, model binding, and Minimal API
/// integration are provided by <c>KernelR.AspNetCore</c>.
/// </para>
/// <para>
/// Input is normalized before validation:
/// </para>
/// <list type="bullet">
///   <item><description>Whitespace, dashes, and parentheses are removed.</description></item>
///   <item><description>Country-code prefixes <c>+98</c>, <c>0098</c>, and <c>98</c> are replaced with a leading <c>0</c>.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;PhoneNumber&gt; result = PhoneNumber.Create("+989121234567");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value); // Outputs: 09121234567
/// }
/// </code>
/// </example>
public readonly partial record struct PhoneNumber :
    IValueObject,
    IParsable<PhoneNumber>
{
    #region ═══ Constants & Fields                                       ═══

    /// <summary>
    /// Matches a normalized Iranian mobile number: a leading <c>0</c>, a <c>9</c>, and nine digits.
    /// </summary>
    [GeneratedRegex(@"^09\d{9}$")]
    private static partial Regex AllowedPattern();

    /// <summary>
    /// Matches characters treated as separators during normalization.
    /// </summary>
    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex Separators();

    /// <summary>
    /// Matches a country-code prefix at the start of the input.
    /// </summary>
    [GeneratedRegex(@"^(\+98|0098|98)")]
    private static partial Regex CountryCodePrefix();

    #endregion ═══ Constants & Fields                                       ═══

    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the normalized phone number in the format <c>09XXXXXXXXX</c>.
    /// </summary>
    /// <value>A string containing the normalized 11-digit phone number.</value>
    public string Value { get; }

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneNumber"/> struct.
    /// </summary>
    /// <param name="value">The normalized phone number.</param>
    private PhoneNumber(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="PhoneNumber"/> from the given string.
    /// </summary>
    /// <param name="value">
    /// The raw phone number string (for example <c>"+989121234567"</c>,
    /// <c>"0912-123-4567"</c>, <c>"00989121234567"</c>).
    /// </param>
    /// <returns>
    /// A successful result carrying the normalized number, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PhoneNumber.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PhoneNumber.INVALID_FORMAT</c> when the normalized input does not match the expected pattern.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// PhoneNumber.Create("09121234567");     // → Success
    /// PhoneNumber.Create("+989121234567");   // → Success (normalized to 09121234567)
    /// PhoneNumber.Create("0912-123-4567");   // → Success (separators removed)
    /// PhoneNumber.Create("123");             // → Failure
    /// </code>
    /// </example>
    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<PhoneNumber>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhoneNumber.EMPTY",
                    "Phone number cannot be empty.",
                    ErrorType.ValidationError));
        }

        string normalized = Normalize(value);

        return !AllowedPattern().IsMatch(normalized)
            ? Result<PhoneNumber>.Failure(
                new Error(
                    "KernelR.ValueObjects.PhoneNumber.INVALID_FORMAT",
                    "Phone number format is invalid.",
                    ErrorType.ValidationError))
            : Result<PhoneNumber>.Success(new PhoneNumber(normalized));
    }

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <summary>
    /// Parses the specified string into a <see cref="PhoneNumber"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="PhoneNumber"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, or does not match the expected format. 
    /// The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static PhoneNumber Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="PhoneNumber"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="PhoneNumber"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PhoneNumber result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<PhoneNumber> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Helpers & Overrides                                        ═══

    /// <summary>
    /// Normalizes the raw phone number by removing separators and standardizing the country code.
    /// </summary>
    /// <param name="value">The raw phone number string.</param>
    /// <returns>The normalized phone number string.</returns>
    private static string Normalize(string value)
    {
        string withoutSeparators = Separators().Replace(value, string.Empty);
        return CountryCodePrefix().Replace(withoutSeparators, "0");
    }

    /// <summary>
    /// Returns the normalized phone number as a string.
    /// </summary>
    /// <returns>The normalized 11-digit phone number.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Explicitly converts a <see cref="PhoneNumber"/> to its normalized string value.
    /// </summary>
    /// <param name="phoneNumber">The phone number instance to convert.</param>
    /// <returns>The normalized string representation of the phone number.</returns>
    public static explicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    #endregion ═══ Helpers & Overrides                                        ═══
}
