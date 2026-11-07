using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a valid email address.
/// </summary>
/// <remarks>
/// <para>
/// The value is normalized to <b>lowercase</b> before storage. Email addresses are
/// case-insensitive in practice for the domain part, and treating the whole address
/// as lowercase avoids duplicate registrations that differ only in case.
/// </para>
/// <para>
/// Validation uses a practical subset of RFC 5322, sufficient for the vast majority of
/// real-world addresses. It is intentionally not a full RFC implementation: fully
/// RFC-compliant addresses can be arbitrarily complex, and the only authoritative
/// validation for an email is delivery.
/// </para>
/// </remarks>
public readonly partial record struct Email : IParsable<Email>
{
    private const int MaxLength = 254;

    [GeneratedRegex(
        @"^[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*" +
        @"@" +
        @"([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}$")]
    private static partial Regex AllowedPattern();

    /// <summary>
    /// Gets the normalized (lowercase) email address.
    /// </summary>
    public string Value { get; }

    private Email(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="Email"/> from the given string.
    /// </summary>
    /// <param name="value">The raw email address.</param>
    /// <returns>
    /// A successful result carrying the normalized email, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Email.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Email.TOO_LONG</c> when the input exceeds 254 characters.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Email.INVALID_FORMAT</c> when the input does not match the expected pattern.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// Email.Create("User@Example.com");  // → Success, Value = "user@example.com"
    /// Email.Create("invalid");           // → Failure, Code = "KernelR.ValueObjects.Email.INVALID_FORMAT"
    /// </code>
    /// </example>
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Email>.Failure(
                new Error(
                    "KernelR.ValueObjects.Email.EMPTY",
                    "Email cannot be empty.",
                    ErrorType.ValidationError));
        }

        string normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
        {
            return Result<Email>.Failure(
                new Error(
                    "KernelR.ValueObjects.Email.TOO_LONG",
                    "Email is too long.",
                    ErrorType.ValidationError));
        }

        if (!AllowedPattern().IsMatch(normalized))
        {
            return Result<Email>.Failure(
                new Error(
                    "KernelR.ValueObjects.Email.INVALID_FORMAT",
                    "Email format is invalid.",
                    ErrorType.ValidationError));
        }

        return Result<Email>.Success(new Email(normalized));
    }

    /// <inheritdoc />
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/>, empty, whitespace, too long, or
    /// does not match the expected email pattern. The exception message contains the
    /// JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static Email Parse(string s, IFormatProvider? provider)
    {
        Result<Email> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, ErrorSerialization.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Email result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<Email> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    /// <summary>
    /// Returns the normalized email address as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts an <see cref="Email"/> to its normalized string value.
    /// </summary>
    public static implicit operator string(Email email) => email.Value;
}
