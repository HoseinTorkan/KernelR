using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a valid human name (first name, last name, or full name).
/// </summary>
/// <remarks>
/// <para>
/// This value object is intended <b>only for human names</b>. It accepts Unicode letters,
/// the zero-width non-joiner (<c>U+200C</c>, used in Persian names such as
/// <c>فاطمه‌زهرا</c>), apostrophes, and hyphens. Digits and other symbols are rejected.
/// </para>
/// <para>
/// <b>Normalization:</b> Consecutive whitespace characters (spaces, tabs, etc.) are
/// collapsed into a single space before validation. The zero-width non-joiner (ZWNJ)
/// is preserved because it carries meaning in Persian names (e.g., <c>فاطمه‌زهرا</c>).
/// </para>
/// <para>
/// For non-person names (product, category, brand, etc.), do <b>not</b> use this type.
/// Those concepts have their own validation rules that differ by domain; model them
/// explicitly when the need arises.
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
    /// The minimum allowed length for a person's name.
    /// </summary>
    private const int MinLength = 2;

    /// <summary>
    /// The maximum allowed length for a person's name.
    /// </summary>
    private const int MaxLength = 100;

    /// <summary>
    /// The generated regex pattern used to validate human names, allowing Unicode letters,
    /// ZWNJ (for Persian), apostrophes, and hyphens.
    /// </summary>
    [GeneratedRegex(@"^[\p{L}\u200C'\-]+([\s\u200C][\p{L}\u200C'\-]+)*$")]
    private static partial Regex AllowedPattern();

    /// <summary>
    /// The generated regex pattern used to collapse consecutive whitespace characters
    /// into a single space. ZWNJ is intentionally not matched here.
    /// </summary>
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceCollapser();

    /// <summary>
    /// Gets the trimmed name value.
    /// </summary>
    /// <value>A string containing the validated name.</value>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonName"/> struct.
    /// </summary>
    /// <param name="value">The validated name.</param>
    private PersonName(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="PersonName"/> from the given string.
    /// </summary>
    /// <param name="value">The raw name string.</param>
    /// <returns>
    /// A successful result carrying the trimmed name, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.PersonName.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PersonName.TOO_SHORT</c> when the trimmed name has fewer than 2 characters.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PersonName.TOO_LONG</c> when the trimmed name has more than 100 characters.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.PersonName.INVALID_CHARACTERS</c> when the trimmed name contains disallowed characters.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// PersonName.Create("علی");           // → Success
    /// PersonName.Create("فاطمه‌زهرا");    // → Success (ZWNJ allowed)
    /// PersonName.Create("John Doe");      // → Success
    /// PersonName.Create("John  Doe");     // → Success (collapsed to "John Doe")
    /// PersonName.Create("A");             // → Failure (too short)
    /// PersonName.Create("User123");       // → Failure (digits not allowed)
    /// </code>
    /// </example>
    public static Result<PersonName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.PersonName.EMPTY",
                "Person name cannot be empty.",
                ErrorType.ValidationError);

        string normalized = WhitespaceCollapser().Replace(value.Trim(), " ");

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

        if (!AllowedPattern().IsMatch(normalized))
            return new Error(
                "KernelR.ValueObjects.PersonName.INVALID_CHARACTERS",
                "Person name contains invalid characters.",
                ErrorType.ValidationError);

        return new PersonName(normalized);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="PersonName"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="PersonName"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, too short, too long, 
    /// or contains invalid characters. The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static PersonName Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="PersonName"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="PersonName"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PersonName result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Returns the name as a string.
    /// </summary>
    /// <returns>The validated, trimmed name.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Explicitly converts a <see cref="PersonName"/> to its string value.
    /// </summary>
    /// <param name="name">The person name instance to convert.</param>
    /// <returns>The string representation of the name.</returns>
    public static explicit operator string(PersonName name) => name.Value;
}
