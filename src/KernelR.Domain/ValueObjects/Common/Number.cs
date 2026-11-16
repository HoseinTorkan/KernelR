
using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace KernelR.Domain.ValueObjects.Common;

/// <summary>
/// Represents a sequence of decimal digits normalized to ASCII characters.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Number"/> represents a digit sequence rather than a mathematical
/// numeric value. Consequently, leading zeros are preserved, and no numeric
/// conversion, arithmetic operation, or precision loss is performed.
/// </para>
/// <para>
/// The supported input digit sets are:
/// <list type="bullet">
/// <item>
/// <description>ASCII digits: U+0030 through U+0039.</description>
/// </item>
/// <item>
/// <description>Persian digits: U+06F0 through U+06F9.</description>
/// </item>
/// <item>
/// <description>Arabic-Indic digits: U+0660 through U+0669.</description>
/// </item>
/// </list>
/// </para>
/// <para>
/// All accepted digits are normalized to their ASCII equivalents.
/// Other Unicode digit sets, whitespace, separators, and non-digit
/// characters are rejected.
/// </para>
/// <para>
/// This value object does not impose length constraints, prefixes,
/// checksums, or domain-specific validation rules. Such rules belong
/// to the corresponding specialized value objects.
/// </para>
/// <para>
/// A default-constructed instance represents an empty digit sequence.
/// Its <see cref="Value"/> property and string representations return
/// <see cref="string.Empty"/>.
/// </para>
/// <para>
/// Formatting supports the following format specifiers:
/// <list type="bullet">
/// <item>
/// <description>
/// <c>G</c>: Formats digits using the native digits of the supplied
/// <see cref="IFormatProvider"/>.
/// </description>
/// </item>
/// <item>
/// <description>
/// <c>A</c>: Formats digits using ASCII characters, regardless of culture.
/// </description>
/// </item>
/// </list>
/// When no format provider is supplied, <see cref="CultureInfo.InvariantCulture"/>
/// is used.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;Number&gt; result = Number.Create("۱۲۳۴۵");
///
/// if (result.IsSuccess)
/// {
///     Number number = result.Value;
///
///     Console.WriteLine(number.Value);             // 12345
///     Console.WriteLine(number.Length);            // 5
///     Console.WriteLine(number.ToString("G"));     // 12345
///     Console.WriteLine(number.ToString("A"));     // 12345
///
///     CultureInfo persianCulture = new("fa-IR");
///
///     Console.WriteLine(number.ToString("G", persianCulture));
/// }
/// </code>
/// </example>
/// <seealso cref="IValueObject"/>
/// <seealso cref="IFormattable"/>
/// <seealso cref="IParsable{TSelf}"/>
/// <seealso cref="ISpanParsable{TSelf}"/>
public readonly partial record struct Number :
    IValueObject,
    IFormattable,
    IParsable<Number>,
    ISpanParsable<Number>,
    IUtf8SpanParsable<Number>
{
    /// <summary>
    /// Stores the normalized ASCII digit sequence.
    /// </summary>
    /// <remarks>
    /// This field is <see langword="null"/> only for a default-constructed instance.
    /// </remarks>
    private readonly string? _value;

    /// <summary>
    /// Gets the normalized ASCII digit sequence.
    /// </summary>
    /// <value>
    /// A string containing only ASCII digits, or <see cref="string.Empty"/>
    /// when the instance is default-constructed.
    /// </value>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets the number of digits in the sequence.
    /// </summary>
    /// <value>
    /// The number of digits, or zero for an empty instance.
    /// </value>
    public int Length => Value.Length;

    /// <summary>
    /// Gets a value indicating whether the digit sequence is empty.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the instance contains no digits;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool IsEmpty => Value.Length == 0;

    /// <summary>
    /// Gets a value indicating whether all digits are represented using ASCII.
    /// </summary>
    /// <value>
    /// Always <see langword="true"/> for a non-empty valid instance,
    /// because all instances are normalized during creation.
    /// </value>
    public bool IsAscii => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Number"/> struct.
    /// </summary>
    /// <param name="value">
    /// A non-empty sequence of normalized ASCII digits.
    /// </param>
    private Number(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Number"/> from the specified digit sequence.
    /// </summary>
    /// <param name="value">
    /// The input sequence. ASCII, Persian, and Arabic-Indic digits are supported.
    /// </param>
    /// <returns>
    /// A successful result containing the normalized <see cref="Number"/>,
    /// or a validation error when the input is empty or contains unsupported characters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Normalization and validation are performed in a single pass before
    /// constructing the value object.
    /// </para>
    /// <para>
    /// The input is not trimmed. Leading or trailing whitespace is rejected.
    /// </para>
    /// <para>
    /// Leading zeros are preserved.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// This method does not throw for invalid input. Validation failures
    /// are returned through the <see cref="Result{T}"/> value.
    /// </exception>
    /// <example>
    /// <code>
    /// Number.Create("12345");       // Success: 12345
    /// Number.Create("۱۲۳۴۵");      // Success: 12345
    /// Number.Create("١٢٣٤٥");      // Success: 12345
    /// Number.Create("001234");      // Success: 001234
    /// Number.Create("12-345");      // Failure
    /// Number.Create("");            // Failure
    /// Number.Create(" 12345 ");     // Failure
    /// </code>
    /// </example>
    public static Result<Number> Create(string? value)
    {
        if (!TryNormalizeDigitsCore(value, out string normalized, out Error error))
        {
            return error;
        }

        return new Number(normalized);
    }

    /// <summary>
    /// Normalizes supported decimal digits to their ASCII equivalents.
    /// </summary>
    /// <param name="input">
    /// A non-empty sequence containing only ASCII, Persian,
    /// or Arabic-Indic digits.
    /// </param>
    /// <returns>
    /// The normalized ASCII digit sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// The input is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The input is empty, contains only whitespace, or contains
    /// unsupported characters.
    /// </exception>
    /// <remarks>
    /// This method validates and normalizes the input in a single pass.
    /// If the input already contains only ASCII digits, the original
    /// string reference is returned without allocating another string.
    /// </remarks>
    /// <example>
    /// <code>
    /// Number.NormalizeDigits("۱۲۳۴۵");       // 12345
    /// Number.NormalizeDigits("١٢٣٤٥");       // 12345
    /// Number.NormalizeDigits("001234");       // 001234
    /// Number.NormalizeDigits("12-345");       // Throws ArgumentException
    /// </code>
    /// </example>
    public static string NormalizeDigits(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!TryNormalizeDigitsCore(input, out string normalized, out Error error))
        {
            throw new ArgumentException(
                "Input must contain only ASCII, Persian, or Arabic-Indic digits.",
                nameof(input));
        }

        return normalized;
    }

    /// <summary>
    /// Attempts to normalize supported decimal digits to ASCII.
    /// </summary>
    /// <param name="input">
    /// The input sequence, or <see langword="null"/>.
    /// </param>
    /// <returns>
    /// A successful result containing the normalized digit sequence,
    /// or a validation error when the input is invalid.
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="NormalizeDigits"/>, this method does not throw
    /// for invalid input.
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;string&gt; result = Number.TryNormalizeDigits("۱۲۳۴۵");
    /// // Success: 12345
    ///
    /// Result&lt;string&gt; invalid = Number.TryNormalizeDigits("12-345");
    /// // Failure: INVALID_CHARACTERS
    /// </code>
    /// </example>
    public static Result<string> TryNormalizeDigits(string? input)
    {
        if (!TryNormalizeDigitsCore(input, out string normalized, out Error error))
        {
            return error;
        }

        return normalized;
    }

    /// <summary>
    /// Attempts to normalize and validate a digit sequence.
    /// </summary>
    /// <param name="input">
    /// The input sequence, or <see langword="null"/>.
    /// </param>
    /// <param name="normalized">
    /// When this method returns <see langword="true"/>,
    /// contains the normalized ASCII digit sequence.
    /// Otherwise, contains <see cref="string.Empty"/>.
    /// </param>
    /// <param name="error">
    /// When this method returns <see langword="false"/>,
    /// contains the corresponding validation error.
    /// Otherwise, contains <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the input is valid;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method is the shared normalization implementation used by
    /// <see cref="Create"/>, <see cref="NormalizeDigits"/>,
    /// and <see cref="TryNormalizeDigits"/>.
    /// </remarks>
    private static bool TryNormalizeDigitsCore(
        string? input,
        out string normalized,
        out Error error)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = new Error(
                "KernelR.ValueObjects.Number.EMPTY",
                "Number cannot be empty.",
                ErrorType.ValidationError);

            return false;
        }

        char[] buffer = new char[input.Length];
        bool needsNormalization = false;

        for (int index = 0; index < input.Length; index++)
        {
            char character = input[index];

            if (character is >= '0' and <= '9')
            {
                buffer[index] = character;
                continue;
            }

            if (character is >= '\u06F0' and <= '\u06F9')
            {
                buffer[index] = (char)('0' + (character - '\u06F0'));
                needsNormalization = true;
                continue;
            }

            if (character is >= '\u0660' and <= '\u0669')
            {
                buffer[index] = (char)('0' + (character - '\u0660'));
                needsNormalization = true;
                continue;
            }

            error = new Error(
                "KernelR.ValueObjects.Number.INVALID_CHARACTERS",
                "Number must contain only ASCII, Persian, or Arabic-Indic digits.",
                ErrorType.ValidationError);

            return false;
        }

        normalized = needsNormalization
            ? new string(buffer)
            : input;

        error = null!;

        return true;
    }

    /// <summary>
    /// Parses a string into a <see cref="Number"/>.
    /// </summary>
    /// <param name="s">The string containing the digit sequence.</param>
    /// <param name="provider">
    /// The format provider. It is not used because parsing is independent
    /// of culture.
    /// </param>
    /// <returns>The parsed and normalized <see cref="Number"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// The input is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// The input is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse a string into a <see cref="Number"/>.
    /// </summary>
    /// <param name="s">The input string, or <see langword="null"/>.</param>
    /// <param name="provider">
    /// The format provider. It is not used because parsing is independent
    /// of culture.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>,
    /// contains the parsed number; otherwise, contains the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing succeeds; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Number result)
    {
        return Create(s).TryGetValue(out result);
    }

    /// <summary>
    /// Parses a character span into a <see cref="Number"/>.
    /// </summary>
    /// <param name="s">The character span containing the digit sequence.</param>
    /// <param name="provider">The format provider, which is ignored during parsing.</param>
    /// <returns>The parsed and normalized <see cref="Number"/>.</returns>
    /// <exception cref="FormatException">
    /// The input is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Create(s.ToString()).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse a character span into a <see cref="Number"/>.
    /// </summary>
    /// <param name="s">The character span containing the digit sequence.</param>
    /// <param name="provider">The format provider, which is ignored during parsing.</param>
    /// <param name="result">
    /// When successful, contains the parsed number; otherwise,
    /// contains the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing succeeds; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Number result)
    {
        return Create(s.ToString()).TryGetValue(out result);
    }

    /// <summary>
    /// Parses a UTF-8 byte span into a <see cref="Number"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 encoded digit sequence.</param>
    /// <param name="provider">The format provider, which is ignored during parsing.</param>
    /// <returns>The parsed and normalized <see cref="Number"/>.</returns>
    /// <exception cref="FormatException">
    /// The decoded input is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
    {
        return Create(Encoding.UTF8.GetString(utf8Text)).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse a UTF-8 byte span into a <see cref="Number"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 encoded digit sequence.</param>
    /// <param name="provider">The format provider, which is ignored during parsing.</param>
    /// <param name="result">
    /// When successful, contains the parsed number; otherwise,
    /// contains the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing succeeds; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Number result)
    {
        return Create(Encoding.UTF8.GetString(utf8Text)).TryGetValue(out result);
    }

    /// <summary>
    /// Returns the normalized digit sequence using invariant formatting.
    /// </summary>
    /// <returns>
    /// The ASCII digit sequence, or <see cref="string.Empty"/>
    /// for a default-constructed instance.
    /// </returns>
    public override string ToString()
    {
        return ToString("G", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats the digit sequence using the specified format and culture.
    /// </summary>
    /// <param name="format">
    /// The format specifier:
    /// <list type="bullet">
    /// <item>
    /// <description><c>G</c>: Culture-specific native digits.</description>
    /// </item>
    /// <item>
    /// <description><c>A</c>: ASCII digits.</description>
    /// </item>
    /// </list>
    /// A null or empty format is treated as <c>G</c>.
    /// </param>
    /// <param name="formatProvider">
    /// The format provider supplying native digits.
    /// When <see langword="null"/>, invariant culture is used.
    /// </param>
    /// <returns>
    /// The formatted digit sequence, or <see cref="string.Empty"/>
    /// for a default-constructed instance.
    /// </returns>
    /// <exception cref="FormatException">
    /// The specified format is not supported.
    /// </exception>
    /// <example>
    /// <code>
    /// Number number = Number.Create("123456").Value;
    ///
    /// string ascii = number.ToString("A", CultureInfo.InvariantCulture);
    ///
    /// CultureInfo persianCulture = new("fa-IR");
    /// string localized = number.ToString("G", persianCulture);
    ///
    /// string invariant = number.ToString("G", null);
    /// </code>
    /// </example>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        string effectiveFormat = string.IsNullOrEmpty(format)
            ? "G"
            : format;

        if (effectiveFormat.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            return Value;
        }

        if (!effectiveFormat.Equals("G", StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException(
                $"The '{format}' format string is not supported by Number.");
        }

        if (IsEmpty)
        {
            return string.Empty;
        }

        IFormatProvider provider = formatProvider ?? CultureInfo.InvariantCulture;

        NumberFormatInfo numberFormatInfo = NumberFormatInfo.GetInstance(provider);

        string[] nativeDigits = numberFormatInfo.NativeDigits;

        bool usesAsciiDigits = true;

        for (int index = 0; index < nativeDigits.Length; index++)
        {
            if (nativeDigits[index] != index.ToString(CultureInfo.InvariantCulture))
            {
                usesAsciiDigits = false;
                break;
            }
        }

        if (usesAsciiDigits)
        {
            return Value;
        }

        StringBuilder builder = new(Value.Length);

        foreach (char digit in Value)
        {
            builder.Append(nativeDigits[digit - '0']);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Formats the digit sequence using the specified format and invariant culture.
    /// </summary>
    /// <param name="format">
    /// The format specifier. Supported values are <c>G</c> and <c>A</c>.
    /// </param>
    /// <returns>The formatted digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The specified format is not supported.
    /// </exception>
    public string ToString(string? format)
    {
        return ToString(format, CultureInfo.InvariantCulture);
    }
}
