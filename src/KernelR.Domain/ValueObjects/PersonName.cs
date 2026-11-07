using KernelR.Patterns.Results;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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
/// For non-person names (product, category, brand, etc.), do <b>not</b> use this type.
/// Those concepts have their own validation rules that differ by domain; model them
/// explicitly when the need arises.
/// </para>
/// </remarks>
public readonly partial record struct PersonName : IParsable<PersonName>
{
    [GeneratedRegex(@"^[\p{L}\u200C'\-]+([\s\u200C][\p{L}\u200C'\-]+)*$")]
    private static partial Regex AllowedPattern();

    /// <summary>
    /// Gets the trimmed name value.
    /// </summary>
    public string Value { get; }

    private PersonName(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="PersonName"/> from the given string.
    /// </summary>
    /// <param name="value">The raw name string.</param>
    /// <returns>
    /// A successful result carrying the trimmed name, or a failure with one of:
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
    /// PersonName.Create("A");             // → Failure (too short)
    /// PersonName.Create("User123");       // → Failure (digits not allowed)
    /// </code>
    /// </example>
    public static Result<PersonName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<PersonName>.Failure(
                new Error(
                    "KernelR.ValueObjects.PersonName.EMPTY",
                    "Person name cannot be empty.",
                    ErrorType.ValidationError));
        }

        string trimmed = value.Trim();

        if (trimmed.Length < 2)
        {
            return Result<PersonName>.Failure(
                new Error(
                    "KernelR.ValueObjects.PersonName.TOO_SHORT",
                    "Person name is too short.",
                    ErrorType.ValidationError));
        }

        if (trimmed.Length > 100)
        {
            return Result<PersonName>.Failure(
                new Error(
                    "KernelR.ValueObjects.PersonName.TOO_LONG",
                    "Person name is too long.",
                    ErrorType.ValidationError));
        }

        if (!AllowedPattern().IsMatch(trimmed))
        {
            return Result<PersonName>.Failure(
                new Error(
                    "KernelR.ValueObjects.PersonName.INVALID_CHARACTERS",
                    "Person name contains invalid characters.",
                    ErrorType.ValidationError));
        }

        return Result<PersonName>.Success(new PersonName(trimmed));
    }

    /// <inheritdoc />
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/>, empty, whitespace, too short,
    /// too long, or contains invalid characters. The exception message contains the
    /// JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static PersonName Parse(string s, IFormatProvider? provider)
    {
        Result<PersonName> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, ErrorSerialization.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
    public static bool TryParse(
        string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out PersonName result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<PersonName> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    /// <summary>
    /// Returns the name as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a <see cref="PersonName"/> to its string value.
    /// </summary>
    public static implicit operator string(PersonName name) => name.Value;
}
