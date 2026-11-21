using KernelR.Domain.Results;
using KernelR.Domain.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a valid human name (first name, last name, or full name).
/// </summary>
/// <remarks>
/// <para>
/// This value object is intended <b>only for human names</b>. It accepts Unicode letters,
/// the zero-width non-joiner (<c>U+200C</c>, used in Persian names such as
/// <c>فاطمه‌زهرا</c>), apostrophes, and hyphens. Digits, emoji, and other symbols are rejected.
/// </para>
/// <para>
/// <b>Normalization:</b> Input is normalized to Unicode Form C (NFC) before validation, so
/// that names entered in decomposed form (e.g., <c>"Jose\u0301"</c>) are treated identically
/// to their precomposed equivalents (e.g., <c>"José"</c>). Consecutive whitespace characters
/// (spaces, tabs, etc.) are then collapsed into a single space. The zero-width non-joiner
/// (ZWNJ) is preserved because it carries meaning in Persian names (e.g., <c>فاطمه‌زهرا</c>).
/// </para>
/// <para>
/// <b>Emoji and symbols:</b> Emoji (e.g., <c>😀</c>) and other symbols belong to the Unicode
/// categories <c>So</c>, <c>Sm</c>, or <c>Sk</c> — none of which is matched by the
/// <c>\p{L}</c> class used in the validation regex. They are therefore rejected as
/// <c>INVALID_CHARACTERS</c>.
/// </para>
/// <para>
/// For non-person names (product, category, brand, etc.), do <b>not</b> use this type.
/// Those concepts have their own validation rules that differ by domain; model them
/// explicitly when the need arises.
/// </para>
/// <para>
/// <b>Struct design (per Microsoft guidelines):</b> A default-constructed
/// <see cref="PersonName"/> (i.e., <c>default(PersonName)</c>) represents an
/// <b>empty name</b> and is a valid state. All property getters return
/// <see cref="string.Empty"/> on such instances and <b>never throw</b>, in accordance
/// with CA1065 and the <see cref="object.ToString"/> contract.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;PersonName&gt; result = PersonName.Create("فاطمه‌زهرا");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value); // Outputs: فاطمه‌زهرا
/// }
/// </code>
/// </example>
/// <seealso cref="IValueObject"/>
public readonly partial record struct PersonName : IValueObject, IParsable<PersonName>
{
    /// <summary>
    /// The backing field for <see cref="Value"/>. <see langword="null"/> for a
    /// default-constructed <see cref="PersonName"/>.
    /// </summary>
    private readonly string? _value;

    /// <summary>
    /// The minimum allowed length for a person's name, measured in characters
    /// (Unicode code points, not bytes).
    /// </summary>
    /// <value>2 characters.</value>
    /// <remarks>
    /// Exposed publicly so that consumers can pre-validate input
    /// (e.g., set <c>minlength</c> on a UI input) without hard-coding the magic number.
    /// </remarks>
    public const int MinLength = 2;

    /// <summary>
    /// The maximum allowed length for a person's name, measured in characters
    /// (Unicode code points, not bytes).
    /// </summary>
    /// <value>100 characters.</value>
    /// <remarks>
    /// Exposed publicly so that consumers can pre-validate input
    /// (e.g., set <c>maxlength</c> on a UI input) without hard-coding the magic number.
    /// A 100-character limit comfortably covers the longest documented personal names
    /// (including multi-part names and honorifics) without allowing abuse.
    /// </remarks>
    public const int MaxLength = 100;

    /// <summary>
    /// The generated regex pattern used to validate human names, allowing Unicode letters,
    /// ZWNJ (for Persian), apostrophes, and hyphens.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Private because the regex is an implementation detail. Consumers should use
    /// <see cref="Create"/> or <see cref="TryParse"/> instead; the underlying regex may
    /// be refined in future versions without this being a breaking change.
    /// </para>
    /// <para>
    /// The pattern enforces:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>One or more word characters (Unicode letters, ZWNJ, apostrophe, hyphen).</description></item>
    ///   <item><description>Optional additional words, separated by a space or ZWNJ.</description></item>
    ///   <item><description>No leading or trailing separators (handled by trimming and <see cref="WhitespaceCollapser"/>).</description></item>
    /// </list>
    /// <para>
    /// <b>Emoji and symbols:</b> Emoji (<c>😀</c>), math symbols, and currency signs are in
    /// Unicode categories <c>So</c>, <c>Sm</c>, and <c>Sk</c>, none of which is matched by
    /// <c>\p{L}</c>. They are therefore rejected. The ZWNJ character is admitted explicitly
    /// via <c>\u200C</c> because it is meaningful in Persian orthography.
    /// </para>
    /// </remarks>
    [GeneratedRegex(@"^[\p{L}\u200C'\-]+([\s\u200C][\p{L}\u200C'\-]+)*$")]
    private static partial Regex AllowedPattern();

    /// <summary>
    /// The generated regex pattern used to collapse consecutive whitespace characters
    /// into a single space. ZWNJ is intentionally not matched here.
    /// </summary>
    /// <remarks>
    /// Matches any Unicode whitespace (<c>\s</c>). The replacement is a single ASCII space
    /// (<c>U+0020</c>), not the matched whitespace, so tabs and newlines are normalized
    /// to plain spaces.
    /// </remarks>
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceCollapser();

    /// <summary>
    /// Gets the trimmed, normalized name value, or <see cref="string.Empty"/> for a
    /// default-constructed instance.
    /// </summary>
    /// <value>
    /// A non-null string containing the validated name, or <see cref="string.Empty"/>
    /// when this is a default-constructed instance.
    /// </value>
    /// <remarks>
    /// This getter <b>never throws</b>, in accordance with CA1065. Callers that need to
    /// distinguish between "empty" and "valid name" can check
    /// <c>name.Value.Length == 0</c>.
    /// </remarks>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonName"/> struct.
    /// </summary>
    /// <param name="value">The validated name.</param>
    private PersonName(string value) => _value = value;

    /// <summary>
    /// Creates a new <see cref="PersonName"/> from the given string.
    /// </summary>
    /// <param name="value">The raw name string.</param>
    /// <returns>
    /// A successful result carrying the trimmed name, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PersonName.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PersonName.TOO_SHORT</c> when the normalized name has fewer than <see cref="MinLength"/> characters.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PersonName.TOO_LONG</c> when the normalized name has more than <see cref="MaxLength"/> characters.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PersonName.INVALID_CHARACTERS</c> when the normalized name contains disallowed characters (digits, emoji, symbols, control characters, etc.).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validation is performed in a deterministic order (empty → normalize → length →
    /// character whitelist), so the returned error code is always the first applicable one.
    /// </para>
    /// <para>
    /// Normalization consists of three steps:
    /// </para>
    /// <list type="number">
    ///   <item><description>
    ///     <b>Trim:</b> leading and trailing whitespace is removed.
    ///   </description></item>
    ///   <item><description>
    ///     <b>NFC:</b> the string is normalized to Unicode Form C. This ensures that
    ///     canonically equivalent representations (e.g., <c>"é"</c> as one code point
    ///     vs. <c>"e"</c> + combining acute) are treated identically and can pass the
    ///     <c>\p{L}</c> whitelist.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Whitespace collapse:</b> consecutive whitespace is replaced with a single
    ///     ASCII space. ZWNJ (<c>U+200C</c>) is preserved because it is semantically
    ///     significant in Persian names such as <c>فاطمه‌زهرا</c>.
    ///   </description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// PersonName.Create("علی");           // → Success
    /// PersonName.Create("فاطمه‌زهرا");    // → Success (ZWNJ allowed)
    /// PersonName.Create("John Doe");      // → Success
    /// PersonName.Create("John  Doe");     // → Success (collapsed to "John Doe")
    /// PersonName.Create("Jose\u0301");    // → Success (NFC-normalized to "José")
    /// PersonName.Create("A");             // → Failure (too short)
    /// PersonName.Create("User123");       // → Failure (digits not allowed)
    /// PersonName.Create("John 😀");       // → Failure (emoji not allowed)
    /// </code>
    /// </example>
    public static Result<PersonName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.PersonName.EMPTY",
                "Person name cannot be empty.",
                ErrorType.ValidationError);

        string normalized = value.Trim().Normalize(NormalizationForm.FormC);
        normalized = WhitespaceCollapser().Replace(normalized, " ");

        if (normalized.Length < MinLength)
            return new Error(
                "KernelR.ValueObjects.PersonName.TOO_SHORT",
                "Person name is too short.",
                ErrorType.ValidationError);

        if (normalized.Length > MaxLength)
            return new Error(
                "KernelR.ValueObjects.PersonName.TOO_LONG",
                "Person name is too long.",
                ErrorType.ValidationError);

        return !AllowedPattern().IsMatch(normalized)
            ? new Error(
                "KernelR.ValueObjects.PersonName.INVALID_CHARACTERS",
                "Person name contains invalid characters.",
                ErrorType.ValidationError)
            : new PersonName(normalized);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="PersonName"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <returns>A <see cref="PersonName"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, too short, too long,
    /// or contains invalid characters. The exception message contains the JSON-serialized
    /// domain <see cref="Error"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which
    /// requires an <see cref="IFormatProvider"/> parameter. Because <see cref="PersonName"/>
    /// is a domain primitive with culture-invariant validation rules (the regex accepts
    /// Unicode letters from any script), parsing is <b>always</b> performed using the same
    /// rules regardless of the provider passed.
    /// </para>
    /// <para>
    /// In other words:
    /// <c>PersonName.Parse("John Doe", CultureInfo.GetCultureInfo("fa-IR"))</c> and
    /// <c>PersonName.Parse("John Doe", null)</c> produce identical results.
    /// </para>
    /// <para>
    /// For consistency, the same policy applies to
    /// <see cref="TryParse(string?, IFormatProvider?, out PersonName)"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var name = PersonName.Parse("John Doe", null);
    /// Console.WriteLine(name.Value); // "John Doe"
    /// </code>
    /// </example>
    public static PersonName Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="PersonName"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="PersonName"/> instance,
    /// if parsing succeeded, or the default value if it failed.
    /// </param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract, which
    /// requires an <see cref="IFormatProvider"/> parameter. Because <see cref="PersonName"/>
    /// is a domain primitive with culture-invariant validation rules, parsing is
    /// <b>always</b> performed using the same rules regardless of the provider passed.
    /// </para>
    /// <para>
    /// When only a validity check is needed and the resulting instance is not used, callers
    /// can discard it via <c>TryParse(value, null, out _)</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (PersonName.TryParse("John Doe", null, out var name))
    ///     Console.WriteLine(name.Value);
    /// </code>
    /// </example>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PersonName result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Returns the name as a string, or <c>"&lt;empty&gt;"</c> for a default-constructed instance.
    /// </summary>
    /// <returns>
    /// The validated, trimmed name, or <c>"&lt;empty&gt;"</c> when this is a
    /// default-constructed instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <c>Object.ToString()</c> is contractually required to never return <see langword="null"/>
    /// and never return an empty string, and is called implicitly by the debugger, logging
    /// frameworks, string interpolation, and exception messages. For this reason, this method
    /// returns the placeholder <c>"&lt;empty&gt;"</c> for a default-constructed instance.
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
    /// PersonName.Create("John Doe").OrThrow().ToString(); // "John Doe"
    /// default(PersonName).ToString();                      // "&lt;empty&gt;"
    /// </code>
    /// </example>
    public override string ToString() =>
        Value.Length == 0 ? "<empty>" : Value;

    /// <summary>
    /// Explicitly converts a <see cref="PersonName"/> to its string value,
    /// or <see cref="string.Empty"/> for a default-constructed instance.
    /// </summary>
    /// <param name="name">The person name instance to convert.</param>
    /// <returns>
    /// The string representation of the name, or <see cref="string.Empty"/> when
    /// <paramref name="name"/> is a default-constructed instance.
    /// </returns>
    /// <remarks>
    /// Mirrors <see cref="Value"/> (not <see cref="ToString"/>): the conversion returns an
    /// empty string (not <c>"&lt;empty&gt;"</c>), so that <c>(string)name</c> is safe to use
    /// in string concatenation and comparison without surprising the caller.
    /// </remarks>
    public static explicit operator string(PersonName name) => name.Value;
}
