using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
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
/// <example>
/// <code>
/// Result&lt;NationalCode&gt; result = NationalCode.Create("0012345678");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value); // Outputs: 0012345678
/// }
/// </code>
/// </example>
/// <seealso cref="IValueObject"/>
public readonly partial record struct NationalCode : IValueObject, IParsable<NationalCode>
{
    /// <summary>
    /// The generated regex pattern used to validate the exact 10-digit format.
    /// </summary>
    [GeneratedRegex(@"^\d{10}$")]
    private static partial Regex AllowedPattern();

    /// <summary>
    /// The generated regex pattern used to identify and remove separators (spaces and dashes).
    /// </summary>
    [GeneratedRegex(@"[\s\-]")]
    private static partial Regex Separators();

    /// <summary>
    /// Gets the normalized national code (10 digits).
    /// </summary>
    /// <value>A string containing the 10-digit national code without separators.</value>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalCode"/> struct.
    /// </summary>
    /// <param name="value">The normalized national code.</param>
    private NationalCode(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="NationalCode"/> from the given string.
    /// </summary>
    /// <param name="value">
    /// The raw national code (for example <c>"0012345678"</c>, <c>"001-2345678"</c>).
    /// </param>
    /// <returns>
    /// A successful result carrying the normalized code, or a failure with one of the following error codes:
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
            return new Error(
                "KernelR.ValueObjects.NationalCode.EMPTY",
                "National code cannot be empty.",
                ErrorType.ValidationError);

        string normalized = Separators().Replace(value, string.Empty);

        if (!AllowedPattern().IsMatch(normalized))
            return new Error(
                "KernelR.ValueObjects.NationalCode.INVALID_FORMAT",
                "National code must be exactly 10 digits.",
                ErrorType.ValidationError);

        if (normalized.All(c => c == normalized[0]))
            return new Error(
                "KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM",
                "National code is invalid.",
                ErrorType.ValidationError);

        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (normalized[i] - '0') * (10 - i);

        int remainder = sum % 11;
        int checkDigit = normalized[9] - '0';

        bool isValid = remainder < 2
            ? checkDigit == remainder
            : checkDigit == 11 - remainder;

        return !isValid
            ? new Error(
                "KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM",
                "National code is invalid.",
                ErrorType.ValidationError)
            : new NationalCode(normalized);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="NationalCode"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="NationalCode"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not exactly 10 digits, 
    /// has all-identical digits, or has an invalid checksum. The exception message contains 
    /// the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    public static NationalCode Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="NationalCode"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="NationalCode"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out NationalCode result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Returns the normalized national code as a string.
    /// </summary>
    /// <returns>The 10-digit national code.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Explicitly converts a <see cref="NationalCode"/> to its normalized string value.
    /// </summary>
    /// <param name="code">The national code to convert.</param>
    /// <returns>The normalized 10-digit string representation.</returns>
    public static explicit operator string(NationalCode code) => code.Value;
}
