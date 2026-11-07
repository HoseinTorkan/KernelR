using KernelR.Patterns.Results;
using KernelR.Patterns.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Represents a valid Iranian national code (کد ملی).
/// </summary>
/// <remarks>
/// <para>
/// The national code is a 10-digit number with a built-in checksum. Validation
/// rejects codes whose digits are all identical (for example <c>1111111111</c>),
/// which are formally invalid even though their checksum matches.
/// </para>
/// <para>
/// Input is normalized by removing spaces and dashes before validation.
/// </para>
/// </remarks>
public readonly partial record struct NationalCode : IParsable<NationalCode>
{
    [GeneratedRegex(@"^\d{10}$")]
    private static partial Regex AllowedPattern();

    [GeneratedRegex(@"[\s\-]")]
    private static partial Regex Separators();

    /// <summary>
    /// Gets the normalized national code (10 digits).
    /// </summary>
    public string Value { get; }

    private NationalCode(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="NationalCode"/> from the given string.
    /// </summary>
    /// <param name="value">
    /// The raw national code (for example <c>"0012345678"</c>, <c>"001-2345678"</c>).
    /// </param>
    /// <returns>
    /// A successful result carrying the normalized code, or a failure with one of:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.INVALID_FORMAT</c> when the input is not exactly 10 digits.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM</c> when the digits are all identical or the checksum fails.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// NationalCode.Create("0012345678");   // → Success
    /// NationalCode.Create("001-2345678");  // → Success (separators removed)
    /// NationalCode.Create("1111111111");   // → Failure (all-identical digits)
    /// NationalCode.Create("123");          // → Failure (not 10 digits)
    /// </code>
    /// </example>
    public static Result<NationalCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<NationalCode>.Failure(
                new Error(
                    "KernelR.ValueObjects.NationalCode.EMPTY",
                    "National code cannot be empty.",
                    ErrorType.ValidationError));
        }

        string normalized = Separators().Replace(value, string.Empty);

        if (!AllowedPattern().IsMatch(normalized))
        {
            return Result<NationalCode>.Failure(
                new Error(
                    "KernelR.ValueObjects.NationalCode.INVALID_FORMAT",
                    "National code must be exactly 10 digits.",
                    ErrorType.ValidationError));
        }

        if (normalized.All(c => c == normalized[0]))
        {
            return Result<NationalCode>.Failure(
                new Error(
                    "KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM",
                    "National code is invalid.",
                    ErrorType.ValidationError));
        }

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (normalized[i] - '0') * (10 - i);
        }

        int remainder = sum % 11;
        int checkDigit = normalized[9] - '0';

        bool isValid = remainder < 2
            ? checkDigit == remainder
            : checkDigit == 11 - remainder;

        return !isValid
            ? Result<NationalCode>.Failure(
                new Error(
                    "KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM",
                    "National code is invalid.",
                    ErrorType.ValidationError))
            : Result<NationalCode>.Success(new NationalCode(normalized));
    }

    /// <inheritdoc />
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/>, empty, whitespace, not exactly
    /// 10 digits, all-identical digits, or has an invalid checksum. The exception message
    /// contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static NationalCode Parse(string s, IFormatProvider? provider)
    {
        Result<NationalCode> result = Create(s);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        string errorJson = JsonSerializer.Serialize(result.Error, KernelRJsonOptions.Options);
        throw new FormatException(errorJson);
    }

    /// <inheritdoc />
    public static bool TryParse(
        string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out NationalCode result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<NationalCode> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    /// <summary>
    /// Returns the normalized national code as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a <see cref="NationalCode"/> to its normalized string value.
    /// </summary>
    public static implicit operator string(NationalCode code) => code.Value;
}
