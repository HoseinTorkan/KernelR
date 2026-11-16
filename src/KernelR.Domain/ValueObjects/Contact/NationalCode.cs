using KernelR.Domain.ValueObjects.Common;
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
/// <b>Input normalization:</b> Spaces and hyphens are stripped, and Persian
/// (<c>۰–۹</c>) or Arabic-Indic (<c>٠–٩</c>) digits are converted to ASCII before
/// validation. This normalization is delegated to <see cref="Number"/>, which is the
/// canonical place for digit normalization in this library. After normalization,
/// the input must consist <b>entirely</b> of ASCII digits; any other character
/// (letters, symbols, emoji, other digit scripts) causes <c>INVALID_FORMAT</c>.
/// </para>
/// <para>
/// <b>Two factories:</b> <see cref="Create(string)"/> accepts raw input (with optional
/// separators and Persian/Arabic-Indic digits) and runs the full pipeline.
/// <see cref="Create(Number)"/> accepts an already-normalized <see cref="Number"/> and
/// runs only the national-code-specific validation (length, all-identical, checksum).
/// Use the latter when you already have a <see cref="Number"/> and want to avoid
/// re-normalizing.
/// </para>
/// <para>
/// <b>Struct design (per Microsoft guidelines):</b> A default-constructed
/// <see cref="NationalCode"/> represents an <b>empty national code</b> and is a valid
/// state. All property getters return <see cref="string.Empty"/> on such instances and
/// <b>never throw</b>, in accordance with CA1065 and the
/// <see cref="object.ToString"/> contract.
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
/// <seealso cref="Number"/>
public readonly partial record struct NationalCode :
    IValueObject,
    IParsable<NationalCode>
{
    /// <summary>
    /// The backing field for <see cref="Value"/>. <see langword="null"/> for a
    /// default-constructed <see cref="NationalCode"/>.
    /// </summary>
    private readonly string? _value;

    /// <summary>
    /// The exact number of digits in a valid Iranian national code.
    /// </summary>
    /// <value>10 digits.</value>
    public const int RequiredLength = 10;

    /// <summary>
    /// The generated regex pattern used to identify and remove separators (spaces and hyphens).
    /// </summary>
    /// <remarks>
    /// Matches any Unicode whitespace (<c>\s</c>) and the hyphen-minus (<c>-</c>).
    /// Other dash-like characters (en-dash <c>–</c>, em-dash <c>—</c>) are intentionally
    /// <b>not</b> matched, because they are often used as range separators rather than
    /// digit separators.
    /// </remarks>
    [GeneratedRegex(@"[\s\-]")]
    private static partial Regex Separators();

    /// <summary>
    /// Gets the normalized national code (10 ASCII digits), or <see cref="string.Empty"/>
    /// for a default-constructed instance.
    /// </summary>
    /// <value>
    /// A non-null string containing the 10-digit national code without separators, or
    /// <see cref="string.Empty"/> when this is a default-constructed instance.
    /// </value>
    /// <remarks>
    /// This getter <b>never throws</b>, in accordance with CA1065.
    /// </remarks>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalCode"/> struct.
    /// </summary>
    /// <param name="value">The normalized national code.</param>
    private NationalCode(string value) => _value = value;

    /// <summary>
    /// Creates a new <see cref="NationalCode"/> from the given raw string.
    /// </summary>
    /// <param name="value">
    /// The raw national code (for example <c>"0012345678"</c>, <c>"001-2345678"</c>,
    /// <c>"۱۲۳۴۵۶۷۸۹۰"</c>).
    /// </param>
    /// <returns>
    /// A successful result carrying the normalized code, or a failure with one of the
    /// following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.EMPTY</c> when the input is null, whitespace, or contains only separators.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.INVALID_FORMAT</c> when — after normalization — the input is not exactly 10 ASCII digits.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM</c> when the digits are all identical or the checksum fails.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This overload performs the full pipeline: empty check → separator stripping →
    /// digit normalization via <see cref="Number.Create(string)"/> → length check →
    /// all-identical check → checksum. After stripping separators and normalizing digits,
    /// it delegates to <see cref="Create(Number)"/> for the national-code-specific
    /// validation, so the two overloads never diverge.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// NationalCode.Create("0012345678");   // → Success
    /// NationalCode.Create("001-2345678");  // → Success (separators removed)
    /// NationalCode.Create("۱۲۳۴۵۶۷۸۹۰");   // → Failure (invalid checksum, format OK)
    /// NationalCode.Create("1111111111");   // → Failure (all-identical digits)
    /// NationalCode.Create("123");          // → Failure (not 10 digits)
    /// NationalCode.Create("fsfsf123");     // → Failure (INVALID_FORMAT, not digits)
    /// </code>
    /// </example>
    public static Result<NationalCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.NationalCode.EMPTY",
                "National code cannot be empty.",
                ErrorType.ValidationError);

        string stripped = Separators().Replace(value, string.Empty);

        if (stripped.Length == 0)
            return new Error(
                "KernelR.ValueObjects.NationalCode.EMPTY",
                "National code cannot be empty.",
                ErrorType.ValidationError);

        // Delegate digit validation and normalization to Number.
        var numberResult = Number.Create(stripped);

        return numberResult.IsFailure
            ? new Error(
                "KernelR.ValueObjects.NationalCode.INVALID_FORMAT",
                "National code must be exactly 10 digits.",
                ErrorType.ValidationError)
            : Create(numberResult.Value);
    }

    /// <summary>
    /// Creates a new <see cref="NationalCode"/> from an already-normalized <see cref="Number"/>.
    /// </summary>
    /// <param name="number">
    /// The normalized digit sequence. Must contain exactly <see cref="RequiredLength"/>
    /// digits and pass the national-code checksum.
    /// </param>
    /// <returns>
    /// A successful result carrying the new <see cref="NationalCode"/>, or a failure with
    /// one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.EMPTY</c> when <paramref name="number"/> is a default-constructed (empty) instance.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.INVALID_FORMAT</c> when <paramref name="number"/> does not contain exactly <see cref="RequiredLength"/> digits.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM</c> when the digits are all identical or the checksum fails.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This overload assumes the input has <b>already been normalized</b> (i.e., contains
    /// only ASCII digits, no separators). It performs only the national-code-specific
    /// validation: length, all-identical rejection, and checksum. Use this overload when
    /// you already have a <see cref="Number"/> from an earlier normalization step and want
    /// to avoid running the normalization again.
    /// </para>
    /// <para>
    /// A default-constructed <see cref="Number"/> (i.e., <c>default(Number)</c>) is treated
    /// as <c>EMPTY</c>, consistent with <see cref="Create(string)"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var number = Number.Create("0012345678").OrThrow();
    /// var code = NationalCode.Create(number); // → Success
    /// </code>
    /// </example>
    public static Result<NationalCode> Create(Number number)
    {
        if (number.IsEmpty)
            return new Error(
                "KernelR.ValueObjects.NationalCode.EMPTY",
                "National code cannot be empty.",
                ErrorType.ValidationError);

        if (number.Length != RequiredLength)
            return new Error(
                "KernelR.ValueObjects.NationalCode.INVALID_FORMAT",
                "National code must be exactly 10 digits.",
                ErrorType.ValidationError);

        string digits = number.Value;

        if (digits.All(c => c == digits[0]))
            return new Error(
                "KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM",
                "National code is invalid.",
                ErrorType.ValidationError);

        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (digits[i] - '0') * (10 - i);

        int remainder = sum % 11;
        int checkDigit = digits[9] - '0';

        bool isValid = remainder < 2
            ? checkDigit == remainder
            : checkDigit == 11 - remainder;

        return !isValid
            ? new Error(
                "KernelR.ValueObjects.NationalCode.INVALID_CHECKSUM",
                "National code is invalid.",
                ErrorType.ValidationError)
            : new NationalCode(digits);
    }

    /// <summary>
    /// Parses the specified string into a <see cref="NationalCode"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <returns>A <see cref="NationalCode"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="s"/> is null, empty, whitespace, not exactly 10 ASCII
    /// digits (after normalization), has all-identical digits, or has an invalid checksum.
    /// The exception message contains the JSON-serialized domain <see cref="Error"/>.
    /// </exception>
    /// <remarks>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract.
    /// Parsing is <b>always</b> culture-invariant; the <paramref name="provider"/>
    /// parameter has no effect.
    /// </remarks>
    public static NationalCode Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="NationalCode"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information.
    /// <b>This parameter is intentionally ignored.</b>
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="NationalCode"/> instance,
    /// if parsing succeeded, or the default value if it failed.
    /// </param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This overload exists to satisfy the <see cref="IParsable{TSelf}"/> contract.
    /// Parsing is <b>always</b> culture-invariant; the <paramref name="provider"/>
    /// parameter has no effect.
    /// </remarks>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out NationalCode result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Returns the normalized national code as a string, or <c>"&lt;empty&gt;"</c> for a
    /// default-constructed instance.
    /// </summary>
    /// <returns>
    /// The 10-digit national code, or <c>"&lt;empty&gt;"</c> when this is a
    /// default-constructed instance.
    /// </returns>
    /// <remarks>
    /// <c>Object.ToString()</c> is contractually required to never return
    /// <see langword="null"/> and never return an empty string. For this reason,
    /// this method returns the placeholder <c>"&lt;empty&gt;"</c> for a
    /// default-constructed instance.
    /// </remarks>
    public override string ToString() =>
        Value.Length == 0 ? "<empty>" : Value;

    /// <summary>
    /// Explicitly converts a <see cref="NationalCode"/> to its normalized string value,
    /// or <see cref="string.Empty"/> for a default-constructed instance.
    /// </summary>
    /// <param name="code">The national code to convert.</param>
    /// <returns>
    /// The normalized 10-digit string representation, or <see cref="string.Empty"/> when
    /// <paramref name="code"/> is a default-constructed instance.
    /// </returns>
    public static explicit operator string(NationalCode code) => code.Value;
}
