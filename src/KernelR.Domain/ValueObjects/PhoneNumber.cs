using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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
public readonly partial record struct PhoneNumber : IParsable<PhoneNumber>
{
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

    /// <summary>
    /// Gets the normalized phone number in the format <c>09XXXXXXXXX</c>.
    /// </summary>
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="PhoneNumber"/> from the given string.
    /// </summary>
    /// <param name="value">
    /// The raw phone number string (for example <c>"+989121234567"</c>,
    /// <c>"0912-123-4567"</c>, <c>"00989121234567"</c>).
    /// </param>
    /// <returns>
    /// A successful result carrying the normalized number, or a failure with one of:
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

    private static string Normalize(string value)
    {
        string withoutSeparators = Separators().Replace(value, string.Empty);
        return CountryCodePrefix().Replace(withoutSeparators, "0");
    }

    /// <inheritdoc />
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/>, empty, whitespace, or does not
    /// match the expected format. The exception message contains the JSON-serialized
    /// domain <see cref="Error"/>.
    /// </exception>
    public static PhoneNumber Parse(string s, IFormatProvider? provider)
    {
        Result<PhoneNumber> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, ErrorSerialization.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
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

    /// <summary>
    /// Returns the normalized phone number as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a <see cref="PhoneNumber"/> to its normalized string value.
    /// </summary>
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
