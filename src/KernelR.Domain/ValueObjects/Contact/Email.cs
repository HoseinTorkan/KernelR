using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KernelR.Domain.ValueObjects.Contact;

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
/// <para>
/// The maximum total length is <see cref="MaxLength"/> characters (RFC 5321), and the
/// local part (before the <c>@</c>) is limited to <see cref="MaxLocalPartLength"/>
/// characters (RFC 5321 §4.5.3.1.1).
/// </para>
/// <para>
/// <b>Culture policy:</b> The email is an ASCII machine-to-machine contract.
/// Normalization uses <see cref="string.ToLowerInvariant"/> — never
/// <see cref="CultureInfo.CurrentCulture"/> — so the type is not affected by the
/// host application's culture. This also avoids the Turkish-I problem, where
/// <c>"I".ToLower(tr-TR)</c> yields a dotless <c>"ı"</c>.
/// </para>
/// <para>
/// <b>Struct design (per Microsoft guidelines):</b> A default-constructed
/// <see cref="Email"/> (i.e., <c>default(Email)</c>) represents an <b>empty email
/// address</b> and is a valid state. All property getters return
/// <see cref="string.Empty"/> on such instances and <b>never throw</b>, in accordance
/// with CA1065 and the <see cref="object.ToString"/> contract. This ensures the struct
/// is safe to use in arrays, DTOs, and generic containers without unexpected exceptions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;Email&gt; result = Email.Create("User@Example.com");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value);           // user@example.com
///     Console.WriteLine(result.Value.LocalPart); // user
///     Console.WriteLine(result.Value.Domain);    // example.com
///     Console.WriteLine(result.Value.Masked);    // u***@example.com
/// }
/// </code>
/// </example>
/// <seealso cref="IValueObject"/>
public readonly partial record struct Email :
    IValueObject,
    IParsable<Email>
{
    /// <summary>
    /// The backing field for <see cref="Value"/>. <see langword="null"/> for a
    /// default-constructed <see cref="Email"/>.
    /// </summary>
    private readonly string? _value;

    /// <summary>
    /// The maximum allowed total length for an email address, according to RFC 5321.
    /// </summary>
    /// <value>254 characters.</value>
    /// <remarks>
    /// <para>
    /// This is a protocol limit from RFC 5321 §4.5.3.1.3. Exposed publicly so that
    /// consumers can pre-validate input (e.g., set <c>maxlength</c> on a UI input)
    /// without hard-coding the magic number.
    /// </para>
    /// <para>
    /// Enforcing this limit before sending an email to a remote server avoids the
    /// common <c>550 5.1.1</c> rejection that many SMTP servers produce for oversized
    /// addresses.
    /// </para>
    /// </remarks>
    public const int MaxLength = 254;

    /// <summary>
    /// The maximum allowed length for the local part (the portion before the <c>@</c>),
    /// according to RFC 5321 §4.5.3.1.1.
    /// </summary>
    /// <value>64 characters.</value>
    /// <remarks>
    /// <para>
    /// This is a protocol limit from RFC 5321 §4.5.3.1.1. Exposed publicly for the same
    /// reason as <see cref="MaxLength"/>.
    /// </para>
    /// <para>
    /// Note that this limit applies only to the local part; the domain part has its own
    /// per-label and total limits, which are validated by the underlying
    /// <see cref="AllowedPattern"/> regex.
    /// </para>
    /// </remarks>
    public const int MaxLocalPartLength = 64;

    /// <summary>
    /// The generated regex pattern used to validate the email format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Private because the regex is an implementation detail. Consumers should use
    /// <see cref="Create"/> or <see cref="TryParse"/> instead; the underlying regex may
    /// be refined in future versions without this being a breaking change.
    /// </para>
    /// <para>
    /// The pattern follows a practical subset of RFC 5322:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Local part:</b> one or more characters from the RFC 5322 atom set, optionally
    ///     separated by dots. Consecutive dots and leading/trailing dots are rejected.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Domain:</b> one or more labels separated by dots, each label starting and
    ///     ending with an alphanumeric character, with a total label length of at most 63.
    ///     The TLD must be at least 2 alphabetic characters.
    ///   </description></item>
    /// </list>
    /// </remarks>
    [GeneratedRegex(
        @"^[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*" +
        @"@" +
        @"([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}$")]
    private static partial Regex AllowedPattern();

    /// <summary>
    /// Gets the normalized (lowercase) string representation of the email address,
    /// or <see cref="string.Empty"/> for a default-constructed instance.
    /// </summary>
    /// <value>
    /// A non-null string containing the normalized email address, or
    /// <see cref="string.Empty"/> when this is a default-constructed instance.
    /// </value>
    /// <remarks>
    /// <para>
    /// The value is guaranteed non-null: it either contains the normalized email
    /// (when the instance was constructed via <see cref="Create"/>,
    /// <see cref="Parse"/>, or <see cref="TryParse"/>) or <see cref="string.Empty"/>
    /// (when the instance is <c>default(Email)</c>). This is required by the
    /// struct design guideline that a struct's default state must be valid.
    /// </para>
    /// <para>
    /// This getter <b>never throws</b>, in accordance with CA1065. Callers that
    /// need to distinguish between "empty" and "valid email" can check
    /// <c>email.Value.Length == 0</c>.
    /// </para>
    /// </remarks>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets the local part of the email (the portion before the <c>@</c>).
    /// </summary>
    /// <value>
    /// The local part of the email address, or <see cref="string.Empty"/> for a
    /// default-constructed instance.
    /// </value>
    /// <remarks>
    /// This getter <b>never throws</b>, in accordance with CA1065.
    /// </remarks>
    /// <example>
    /// <code>
    /// Email.Create("user@example.com").OrThrow().LocalPart; // "user"
    /// default(Email).LocalPart;                             // ""
    /// </code>
    /// </example>
    public string LocalPart =>
        Value.Length == 0 ? string.Empty : Value[..Value.IndexOf('@')];

    /// <summary>
    /// Gets the domain part of the email (the portion after the <c>@</c>).
    /// </summary>
    /// <value>
    /// The domain part of the email address, or <see cref="string.Empty"/> for a
    /// default-constructed instance.
    /// </value>
    /// <remarks>
    /// This getter <b>never throws</b>, in accordance with CA1065.
    /// </remarks>
    /// <example>
    /// <code>
    /// Email.Create("user@example.com").OrThrow().Domain; // "example.com"
    /// default(Email).Domain;                              // ""
    /// </code>
    /// </example>
    public string Domain =>
        Value.Length == 0 ? string.Empty : Value[(Value.IndexOf('@') + 1)..];

    /// <summary>
    /// Gets a masked representation of the email suitable for logging.
    /// </summary>
    /// <value>
    /// A string of the form <c>"u***@example.com"</c>, or <see cref="string.Empty"/>
    /// for a default-constructed instance.
    /// </value>
    /// <remarks>
    /// <para>
    /// Use this property when logging or emitting telemetry: <b>never log the raw email</b>.
    /// The local part is masked down to its first character, while the domain is kept
    /// intact so that operators can still identify the provider (e.g., <c>gmail.com</c>)
    /// without exposing PII.
    /// </para>
    /// <para>
    /// For stricter anonymization, callers can further strip the domain themselves.
    /// </para>
    /// <para>
    /// This getter <b>never throws</b>, in accordance with CA1065.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Email.Create("user@example.com").OrThrow().Masked; // "u***@example.com"
    /// default(Email).Masked;                              // ""
    /// </code>
    /// </example>
    public string Masked
    {
        get
        {
            if (Value.Length == 0) return string.Empty;
            int at = Value.IndexOf('@');
            return string.Concat(Value.AsSpan(0, 1), "***", Value.AsSpan(at));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> struct.
    /// </summary>
    /// <param name="value">The normalized email address.</param>
    private Email(string value) => _value = value;

    /// <summary>
    /// Creates a new <see cref="Email"/> instance from the specified string.
    /// </summary>
    /// <param name="value">The raw email address to validate and normalize.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the normalized <see cref="Email"/> on success,
    /// or a failure <see cref="Error"/> with one of the following codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.Email.EMPTY</c> when the input is null, empty, or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Email.TOO_LONG</c> when the input exceeds <see cref="MaxLength"/> characters.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Email.LOCAL_PART_TOO_LONG</c> when the local part exceeds <see cref="MaxLocalPartLength"/> characters.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.Email.INVALID_FORMAT</c> when the input does not match the expected RFC 5322 subset pattern.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validation is performed in a deterministic order (empty → length → local part
    /// length → format), so the returned error code is always the first applicable one.
    /// This makes error handling predictable for callers.
    /// </para>
    /// <para>
    /// The input is trimmed before normalization, so leading and trailing whitespace
    /// is silently removed. Internal separators are <b>not</b> accepted: the string
    /// must be a complete email address as-is.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Email.Create("User@Example.com");   // → Success, normalized to user@example.com
    /// Email.Create("  a@b.co  ");         // → Success, trimmed and normalized
    /// Email.Create("");                   // → Failure (EMPTY)
    /// Email.Create("not-an-email");       // → Failure (INVALID_FORMAT)
    /// </code>
    /// </example>
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.Email.EMPTY",
                "Email cannot be empty.",
                ErrorType.ValidationError);

        string normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            return new Error(
                "KernelR.ValueObjects.Email.TOO_LONG",
                "Email is too long.",
                ErrorType.ValidationError);

        int at = normalized.IndexOf('@');
        if (at > MaxLocalPartLength)
            return new Error(
                "KernelR.ValueObjects.Email.LOCAL_PART_TOO_LONG",
                "Email local part exceeds 64 characters.",
                ErrorType.ValidationError);

        return !AllowedPattern().IsMatch(normalized)
            ? new Error(
                "KernelR.ValueObjects.Email.INVALID_FORMAT",
                "Email format is invalid.",
                ErrorType.ValidationError)
            : new Email(normalized);
    }

    /// <summary>
    /// Parses the specified string into an <see cref="Email"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <returns>An <see cref="Email"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, too long, or does not
    /// match the expected email pattern. The exception message contains the JSON-serialized
    /// domain <see cref="Error"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which
    /// requires an <see cref="IFormatProvider"/> parameter. Because <see cref="Email"/> is
    /// an ASCII, machine-to-machine contract, parsing is <b>always</b> performed using
    /// <see cref="CultureInfo.InvariantCulture"/> regardless of the provider passed.
    /// </para>
    /// <para>
    /// In other words:
    /// <c>Email.Parse("user@example.com", CultureInfo.GetCultureInfo("fa-IR"))</c> and
    /// <c>Email.Parse("user@example.com", null)</c> produce identical results. If you were
    /// expecting culture-sensitive behavior, this is not the type for that.
    /// </para>
    /// <para>
    /// For consistency, the same policy applies to
    /// <see cref="TryParse(string?, IFormatProvider?, out Email)"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var email = Email.Parse("User@Example.com", null);
    /// Console.WriteLine(email.Value); // "user@example.com"
    /// </code>
    /// </example>
    public static Email Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into an <see cref="Email"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Email"/> instance, if parsing
    /// succeeded, or the default value if it failed.
    /// </param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which
    /// requires an <see cref="IFormatProvider"/> parameter. Because <see cref="Email"/> is
    /// an ASCII, machine-to-machine contract, parsing is <b>always</b> performed using
    /// <see cref="CultureInfo.InvariantCulture"/> regardless of the provider passed.
    /// </para>
    /// <para>
    /// When only a validity check is needed and the resulting instance is not used, callers
    /// can discard it via <c>TryParse(value, null, out _)</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (Email.TryParse("user@example.com", null, out var email))
    ///     Console.WriteLine(email.Value);
    /// </code>
    /// </example>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Email result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Returns the normalized email address as a string, or <c>"&lt;empty&gt;"</c> for a
    /// default-constructed instance.
    /// </summary>
    /// <returns>
    /// The normalized email address, or <c>"&lt;empty&gt;"</c> when this is a
    /// default-constructed instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <c>Object.ToString()</c> is contractually required to never return <see langword="null"/>
    /// and never return an empty string, and is called implicitly by the debugger, logging
    /// frameworks, string interpolation, and exception messages. For this reason, this method
    /// does <b>not</b> throw on a default-constructed instance — it returns the placeholder
    /// <c>"&lt;empty&gt;"</c> instead.
    /// </para>
    /// <para>
    /// The distinction from <see cref="Value"/> is intentional:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="Value"/> returns <see cref="string.Empty"/> for a default instance —
    ///     safe for programmatic use (concatenation, comparison with <see cref="string.Empty"/>).
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ToString"/> returns <c>"&lt;empty&gt;"</c> for a default instance —
    ///     <b>never returns an empty string</b>, so debugger and log output is unambiguous
    ///     and the reader can distinguish "empty value" from "missing output".
    ///   </description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// Email.Create("user@example.com").OrThrow().ToString(); // "user@example.com"
    /// default(Email).ToString();                              // "&lt;empty&gt;"
    /// </code>
    /// </example>
    public override string ToString() =>
        Value.Length == 0 ? "<empty>" : Value;

    /// <summary>
    /// Explicitly converts an <see cref="Email"/> to its normalized string value,
    /// or <see cref="string.Empty"/> for a default-constructed instance.
    /// </summary>
    /// <param name="email">The email instance to convert.</param>
    /// <returns>
    /// The normalized string representation of the email, or <see cref="string.Empty"/>
    /// when <paramref name="email"/> is a default-constructed instance.
    /// </returns>
    /// <remarks>
    /// Mirrors <see cref="Value"/> (not <see cref="ToString"/>): the conversion returns an
    /// empty string (not <c>"&lt;empty&gt;"</c>), so that <c>(string)email</c> is safe to use
    /// in string concatenation and comparison without surprising the caller.
    /// </remarks>
    public static explicit operator string(Email email) => email.Value;
}
