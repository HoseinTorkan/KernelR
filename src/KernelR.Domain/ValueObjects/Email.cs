using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a valid email address as an immutable value object.
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
/// validation for an email is actual delivery.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;Email&gt; result = Email.Create("User@Example.com");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value); // Outputs: user@example.com
/// }
/// </code>
/// </example>
public readonly partial record struct Email :
    IValueObject,
    IParsable<Email>
{
    #region ═══ Constants & Fields                                       ═══

    /// <summary>
    /// The maximum allowed length for an email address according to RFC 5321.
    /// </summary>
    private const int MaxLength = 254;

    /// <summary>
    /// The generated regex pattern used to validate the email format.
    /// </summary>
    [GeneratedRegex(
        @"^[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*" +
        @"@" +
        @"([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}$")]
    private static partial Regex AllowedPattern();

    #endregion ═══ Constants & Fields                                       ═══

    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the normalized (lowercase) string representation of the email address.
    /// </summary>
    /// <value>A string containing the normalized email address.</value>
    public string Value { get; }

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> struct.
    /// </summary>
    /// <param name="value">The normalized email address.</param>
    private Email(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="Email"/> instance from the specified string.
    /// </summary>
    /// <param name="value">The raw email address to validate and normalize.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the normalized <see cref="Email"/> on success, 
    /// or a failure <see cref="Error"/> with one of the following codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Email.EMPTY</c>: The input is null, empty, or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Email.TOO_LONG</c>: The input exceeds 254 characters.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Email.INVALID_FORMAT</c>: The input does not match the expected pattern.</description></item>
    /// </list>
    /// </returns>
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

        return Result<Email>.Success(new(normalized));
    }

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <summary>
    /// Parses the specified string into an <see cref="Email"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>An <see cref="Email"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, too long, or does not match 
    /// the expected email pattern. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static Email Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into an <see cref="Email"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="Email"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
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

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Helpers & Overrides                                        ═══

    /// <summary>
    /// Returns the normalized email address as a string.
    /// </summary>
    /// <returns>The normalized email address.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Explicitly converts an <see cref="Email"/> to its normalized string value.
    /// </summary>
    /// <param name="email">The email instance to convert.</param>
    /// <returns>The normalized string representation of the email.</returns>
    public static explicit operator string(Email email) => email.Value;

    #endregion ═══ Helpers & Overrides                                        ═══
}
