using System.Globalization;
using System.Text;

namespace KernelR.Domain.ValueObjects.Common;

/// <summary>
/// Represents a non-empty sequence of decimal digits, preserving leading zeros.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Number"/> is a string-based value object, not a mathematical numeric type.
/// It preserves leading zeros and accepts ASCII, Persian, and Arabic-Indic digits.
/// The stored representation always uses ASCII digits (<c>0</c> through <c>9</c>).
/// </para>
/// <para>
/// The default value is empty. An explicitly constructed or parsed <see cref="Number"/>
/// must contain at least one valid digit.
/// </para>
/// <para>
/// This type does not provide arithmetic operations, numeric ordering, or numeric
/// formatting. Equality is based on the normalized digit sequence, so leading zeros
/// are significant.
/// </para>
/// <para>
/// The display digit set is controlled exclusively by the <see cref="IFormatProvider"/>.
/// When the provider is <see langword="null"/>, <see cref="CultureInfo.CurrentCulture"/> is used.
/// Persian cultures (<c>"fa"</c>) yield Persian digits, Arabic cultures (<c>"ar"</c>)
/// yield Arabic-Indic digits, and all other cultures yield ASCII digits. To always obtain
/// ASCII digits, pass <see cref="CultureInfo.InvariantCulture"/>.
/// </para>
/// <para>
/// Only the general format specifier <c>"G"</c> is supported. Any other format string
/// results in a <see cref="FormatException"/>.
/// </para>
/// </remarks>
/// <example>
/// <code language="csharp">
/// Number identifier = new("۰۰۱۲۳");
///
/// Console.WriteLine(identifier.Value);                                           // 00123
/// Console.WriteLine(identifier.Length);                                          // 5
/// Console.WriteLine(identifier.ToString(CultureInfo.InvariantCulture));          // 00123
/// Console.WriteLine(identifier.ToString(CultureInfo.GetCultureInfo("fa-IR")));   // ۰۰۱۲۳
/// Console.WriteLine(identifier.ToString(CultureInfo.GetCultureInfo("ar-SA")));   // ٠٠١٢٣
/// Console.WriteLine(identifier == new Number("123"));                            // False
/// </code>
/// </example>
/// <seealso cref="IFormattable"/>
/// <seealso cref="ISpanFormattable"/>
/// <seealso cref="IUtf8SpanFormattable"/>
public readonly record struct Number :
    IParsable<Number>,
    ISpanParsable<Number>,
    IUtf8SpanParsable<Number>,
    IFormattable,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    private const char PersianZero = '\u06F0';
    private const char ArabicIndicZero = '\u0660';

    private static readonly UTF8Encoding s_strictUtf8Encoding = new(false, true);

    private readonly string? _value;

    /// <summary>
    /// Gets the normalized value using ASCII digits.
    /// </summary>
    /// <value>
    /// A string containing only ASCII digits, or an empty string when the instance is default.
    /// </value>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets the number of characters in the normalized digit sequence.
    /// </summary>
    /// <value>
    /// The number of ASCII digits in <see cref="Value"/>. Returns zero for the default instance.
    /// </value>
    public int Length => Value.Length;

    /// <summary>
    /// Gets a value indicating whether the instance is empty.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the instance is the default value; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsEmpty => _value is null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Number"/> structure.
    /// </summary>
    /// <param name="value">A non-empty string containing supported decimal digits.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is empty or contains a character other than an ASCII,
    /// Persian, or Arabic-Indic decimal digit.
    /// </exception>
    public Number(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!TryNormalizeDigits(value.AsSpan(), out string normalizedValue) || normalizedValue.Length == 0)
            throw new ArgumentException("The value must contain one or more supported decimal digits.", nameof(value));

        _value = normalizedValue;
    }

    /// <summary>
    /// Initializes a new instance using an already normalized value.
    /// </summary>
    /// <param name="normalizedValue">A non-empty string containing only ASCII digits.</param>
    /// <param name="_">Indicates that the value has already been normalized.</param>
    private Number(string normalizedValue, bool _) => _value = normalizedValue;

    /// <summary>
    /// Returns the value as a read-only character span.
    /// </summary>
    /// <returns>A read-only span over the stored ASCII digit sequence.</returns>
    public ReadOnlySpan<char> AsSpan() => Value.AsSpan();

    /// <summary>
    /// Copies the value into the specified destination.
    /// </summary>
    /// <param name="destination">The destination character span.</param>
    /// <exception cref="ArgumentException">
    /// The destination is shorter than the value.
    /// </exception>
    public void CopyTo(Span<char> destination) => Value.AsSpan().CopyTo(destination);

    /// <summary>
    /// Attempts to copy the value into the specified destination.
    /// </summary>
    /// <param name="destination">The destination character span.</param>
    /// <returns>
    /// <see langword="true"/> if the value was copied; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCopyTo(Span<char> destination) => Value.AsSpan().TryCopyTo(destination);

    /// <summary>
    /// Determines whether the specified string contains only supported decimal digits.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the string is non-empty and contains only supported digits;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid(string value) => value is not null && IsValid(value.AsSpan());

    /// <summary>
    /// Determines whether the specified character span contains only supported decimal digits.
    /// </summary>
    /// <param name="value">The character span to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the span is non-empty and contains only supported digits;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return false;

        foreach (char digit in value)
            if (!IsSupportedDigit(digit)) return false;

        return true;
    }

    /// <summary>
    /// Determines whether the specified UTF-8 span contains only supported decimal digits.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 data to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the data represents a non-empty sequence of supported digits;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid(ReadOnlySpan<byte> utf8Text)
        => !utf8Text.IsEmpty
        && TryNormalizeDigits(utf8Text, out string normalizedValue)
        && normalizedValue.Length > 0;

    /// <summary>
    /// Normalizes supported Persian and Arabic-Indic digits to ASCII digits.
    /// </summary>
    /// <param name="value">The string to normalize.</param>
    /// <returns>A string containing the normalized ASCII digits.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// The string contains an unsupported character.
    /// </exception>
    public static string NormalizeDigits(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return NormalizeDigits(value.AsSpan());
    }

    /// <summary>
    /// Normalizes supported Persian and Arabic-Indic digits to ASCII digits.
    /// </summary>
    /// <param name="value">The character span to normalize.</param>
    /// <returns>A string containing the normalized ASCII digits.</returns>
    /// <exception cref="FormatException">
    /// The span contains an unsupported character.
    /// </exception>
    public static string NormalizeDigits(ReadOnlySpan<char> value)
        => !TryNormalizeDigits(value, out string normalizedValue)
            ? throw new FormatException("The input contains unsupported characters.")
            : normalizedValue;

    /// <summary>
    /// Normalizes supported Persian and Arabic-Indic digits in UTF-8 data to ASCII digits.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 data to normalize.</param>
    /// <returns>A string containing the normalized ASCII digits.</returns>
    /// <exception cref="FormatException">
    /// The data contains invalid UTF-8 or unsupported characters.
    /// </exception>
    public static string NormalizeDigits(ReadOnlySpan<byte> utf8Text)
        => !TryNormalizeDigits(utf8Text, out string normalizedValue)
            ? throw new FormatException("The input contains invalid UTF-8 or unsupported characters.")
            : normalizedValue;

    /// <summary>
    /// Attempts to normalize supported Persian and Arabic-Indic digits to ASCII digits.
    /// </summary>
    /// <param name="value">The string to normalize.</param>
    /// <param name="normalizedValue">
    /// When this method returns, contains the normalized value if successful;
    /// otherwise, an empty string.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input could be normalized; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// An empty string can be normalized successfully, but it is not a valid <see cref="Number"/>.
    /// </remarks>
    public static bool TryNormalizeDigits(string? value, out string normalizedValue)
    {
        if (value is null)
        {
            normalizedValue = string.Empty;
            return false;
        }

        return TryNormalizeDigits(value.AsSpan(), out normalizedValue);
    }

    /// <summary>
    /// Attempts to normalize supported Persian and Arabic-Indic digits to ASCII digits.
    /// </summary>
    /// <param name="value">The character span to normalize.</param>
    /// <param name="normalizedValue">
    /// When this method returns, contains the normalized value if successful;
    /// otherwise, an empty string.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input could be normalized; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// An empty span can be normalized successfully, but it is not a valid <see cref="Number"/>.
    /// Unsupported characters are never removed or ignored.
    /// </remarks>
    public static bool TryNormalizeDigits(ReadOnlySpan<char> value, out string normalizedValue)
    {
        char[] buffer = new char[value.Length];

        for (int i = 0; i < value.Length; i++)
        {
            if (!TryConvertDigit(value[i], out char digit))
            {
                normalizedValue = string.Empty;
                return false;
            }

            buffer[i] = digit;
        }

        normalizedValue = new string(buffer);
        return true;
    }

    /// <summary>
    /// Attempts to normalize supported Persian and Arabic-Indic digits in UTF-8 data.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 data to normalize.</param>
    /// <param name="normalizedValue">
    /// When this method returns, contains the normalized value if successful;
    /// otherwise, an empty string.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input is valid UTF-8 and could be normalized;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryNormalizeDigits(ReadOnlySpan<byte> utf8Text, out string normalizedValue)
    {
        try
        {
            string decodedValue = s_strictUtf8Encoding.GetString(utf8Text);
            return TryNormalizeDigits(decodedValue.AsSpan(), out normalizedValue);
        }
        catch (DecoderFallbackException)
        {
            normalizedValue = string.Empty;
            return false;
        }
    }

    private static bool IsSupportedDigit(char value)
        => value
        is >= '0' and <= '9'
        or >= PersianZero and <= '\u06F9'
        or >= ArabicIndicZero and <= '\u0669';

    private static bool TryConvertDigit(char value, out char digit)
    {
        if (value is >= '0' and <= '9')
        {
            digit = value;
            return true;
        }

        if (value is >= PersianZero and <= '\u06F9')
        {
            digit = (char)('0' + value - PersianZero);
            return true;
        }

        if (value is >= ArabicIndicZero and <= '\u0669')
        {
            digit = (char)('0' + value - ArabicIndicZero);
            return true;
        }

        digit = default;
        return false;
    }

    /// <summary>
    /// Converts the string representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">A string containing supported decimal digits.</param>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="s"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is empty or contains unsupported characters.
    /// </exception>
    /// <remarks>
    /// Parsing is culture-independent. The input must contain digits only; whitespace,
    /// signs, decimal separators, and group separators are not accepted.
    /// </remarks>
    public static Number Parse(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the string representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">A string containing supported decimal digits.</param>
    /// <param name="provider">
    /// An object that supplies formatting information. This parameter is ignored because
    /// <see cref="Number"/> parsing is independent of culture.
    /// </param>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="s"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);

        return !TryParse(s.AsSpan(), out Number result)
            ? throw new FormatException("The input is not a valid Number.")
            : result;
    }

    /// <summary>
    /// Converts the character representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="value">A character span containing supported decimal digits.</param>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(ReadOnlySpan<char> value) => Parse(value, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts the character representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">A character span containing supported decimal digits.</param>
    /// <param name="provider">
    /// An object that supplies formatting information. This parameter is ignored because
    /// <see cref="Number"/> parsing is independent of culture.
    /// </param>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => !TryParse(s, out Number result)
            ? throw new FormatException("The input is not a valid Number.")
            : result;

    /// <summary>
    /// Converts the UTF-8 representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="utf8Text">UTF-8 data containing supported decimal digits.</param>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The input is invalid UTF-8, empty, or contains unsupported characters.
    /// </exception>
    public static Number Parse(ReadOnlySpan<byte> utf8Text) => Parse(utf8Text, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts the UTF-8 representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="utf8Text">UTF-8 data containing supported decimal digits.</param>
    /// <param name="provider">
    /// An object that supplies formatting information. This parameter is ignored because
    /// <see cref="Number"/> parsing is independent of culture.
    /// </param>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The input is invalid UTF-8, empty, or contains unsupported characters.
    /// </exception>
    public static Number Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
        => !TryParse(utf8Text, provider, out Number result)
            ? throw new FormatException("The input is not a valid UTF-8 Number.")
            : result;

    /// <summary>
    /// Attempts to convert the string representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Number"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? value, out Number result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }

        return TryParse(value.AsSpan(), out result);
    }

    /// <summary>
    /// Attempts to convert the string representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that supplies formatting information. This parameter is ignored because
    /// <see cref="Number"/> parsing is independent of culture.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Number"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? s, IFormatProvider? provider, out Number result) =>
        TryParse(s, out result);

    /// <summary>
    /// Attempts to convert the character representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The character span to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Number"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> value, out Number result) =>
        TryParse(value, CultureInfo.InvariantCulture, out result);

    /// <summary>
    /// Attempts to convert the character representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">The character span to parse.</param>
    /// <param name="provider">
    /// An object that supplies formatting information. This parameter is ignored because
    /// <see cref="Number"/> parsing is independent of culture.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Number"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Number result)
    {
        if (s.IsEmpty || !TryNormalizeDigits(s, out string normalizedValue) || normalizedValue.Length == 0)
        {
            result = default;
            return false;
        }

        result = new Number(normalizedValue, true);
        return true;
    }

    /// <summary>
    /// Attempts to convert the UTF-8 representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 data to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Number"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, out Number result) =>
        TryParse(utf8Text, CultureInfo.InvariantCulture, out result);

    /// <summary>
    /// Attempts to convert the UTF-8 representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 data to parse.</param>
    /// <param name="provider">
    /// An object that supplies formatting information. This parameter is ignored because
    /// <see cref="Number"/> parsing is independent of culture.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Number"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out Number result)
    {
        if (utf8Text.IsEmpty || !TryNormalizeDigits(utf8Text, out string normalizedValue) || normalizedValue.Length == 0)
        {
            result = default;
            return false;
        }

        result = new Number(normalizedValue, true);
        return true;
    }

    /// <summary>
    /// Returns the value using the digit set of the current culture, preserving leading zeros.
    /// </summary>
    /// <returns>The stored digit sequence formatted with the current culture's digit set.</returns>
    /// <remarks>
    /// This overload is equivalent to <c>ToString("G", null)</c> and uses
    /// <see cref="CultureInfo.CurrentCulture"/>. To obtain ASCII digits regardless of the
    /// current culture, use <c>ToString(CultureInfo.InvariantCulture)</c>.
    /// </remarks>
    public override string ToString() => ToString("G", null);

    /// <summary>
    /// Formats the value using the specified format provider.
    /// </summary>
    /// <param name="formatProvider">
    /// An object that supplies culture-specific formatting information.
    /// If <see langword="null"/>, <see cref="CultureInfo.CurrentCulture"/> is used.
    /// </param>
    /// <returns>The formatted value.</returns>
    /// <remarks>
    /// This overload is equivalent to <c>ToString("G", formatProvider)</c>. The provider
    /// selects the digit set: Persian cultures yield Persian digits, Arabic cultures yield
    /// Arabic-Indic digits, and all other cultures yield ASCII digits.
    /// </remarks>
    public string ToString(IFormatProvider? formatProvider) => ToString("G", formatProvider);

    /// <summary>
    /// Formats the value using the specified format and format provider.
    /// </summary>
    /// <param name="format">
    /// The format string. Only the general format specifier <c>"G"</c> is supported.
    /// A value of <see langword="null"/> or an empty string is treated as <c>"G"</c>.
    /// </param>
    /// <param name="formatProvider">
    /// An object that supplies culture-specific formatting information.
    /// If <see langword="null"/>, <see cref="CultureInfo.CurrentCulture"/> is used.
    /// </param>
    /// <returns>The formatted value.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is not <see langword="null"/>, empty, or <c>"G"</c>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Unlike numeric formatting, this method preserves leading zeros because it controls
    /// only the display representation of the stored digit sequence, not its numeric value.
    /// </para>
    /// <para>
    /// The parameterless <see cref="ToString()"/> overload is equivalent to <c>ToString("G", null)</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// Number value = new("001234");
    ///
    /// Console.WriteLine(value.ToString("G", CultureInfo.InvariantCulture));         // 001234
    /// Console.WriteLine(value.ToString("G", CultureInfo.GetCultureInfo("fa-IR")));  // ۰۰۱۲۳۴
    /// Console.WriteLine(value.ToString("G", CultureInfo.GetCultureInfo("ar-SA")));  // ٠٠١٢٣٤
    /// </code>
    /// </example>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format is not (null or "" or "G"))
            throw new FormatException(
                $"Format '{format}' is not supported. Only the general format specifier 'G' is valid.");

        char digitStart = GetDigitStartFromCulture(formatProvider ?? CultureInfo.CurrentCulture);
        return ConvertDigits(Value, digitStart);
    }

    /// <summary>
    /// Attempts to format the value into the specified character span.
    /// </summary>
    /// <param name="destination">The destination character span.</param>
    /// <param name="charsWritten">
    /// When this method returns, contains the number of characters written.
    /// </param>
    /// <param name="format">
    /// The format string. Only the general format specifier <c>"G"</c> is supported.
    /// </param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// If <see langword="null"/>, <see cref="CultureInfo.CurrentCulture"/> is used.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the value was formatted successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        string formattedValue;
        try
        {
            formattedValue = ToString(format.IsEmpty ? null : format.ToString(), provider);
        }
        catch (FormatException)
        {
            charsWritten = 0;
            return false;
        }

        if (formattedValue.Length > destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        formattedValue.AsSpan().CopyTo(destination);
        charsWritten = formattedValue.Length;
        return true;
    }

    /// <summary>
    /// Attempts to format the value as UTF-8 into the specified byte span.
    /// </summary>
    /// <param name="utf8Destination">The destination UTF-8 byte span.</param>
    /// <param name="bytesWritten">
    /// When this method returns, contains the number of bytes written.
    /// </param>
    /// <param name="format">
    /// The format string. Only the general format specifier <c>"G"</c> is supported.
    /// </param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// If <see langword="null"/>, <see cref="CultureInfo.CurrentCulture"/> is used.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the value was formatted successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        string formattedValue;
        try
        {
            formattedValue = ToString(format.IsEmpty ? null : format.ToString(), provider);
        }
        catch (FormatException)
        {
            bytesWritten = 0;
            return false;
        }

        int byteCount = s_strictUtf8Encoding.GetByteCount(formattedValue);
        if (byteCount > utf8Destination.Length)
        {
            bytesWritten = 0;
            return false;
        }

        bytesWritten = s_strictUtf8Encoding.GetBytes(formattedValue, utf8Destination);
        return true;
    }

    private static char GetDigitStartFromCulture(IFormatProvider provider)
    {
        if (provider is CultureInfo culture)
        {
            return culture.TwoLetterISOLanguageName switch
            {
                "fa" => PersianZero,
                "ar" => ArabicIndicZero,
                _ => '0'
            };
        }

        return '0';
    }

    private static string ConvertDigits(string value, char digitStart)
    {
        if (digitStart == '0') return value;

        return string.Create(value.Length, (value, digitStart), static (span, state) =>
        {
            for (int i = 0; i < state.value.Length; i++)
            {
                char c = state.value[i];
                span[i] = c is >= '0' and <= '9'
                    ? (char)(state.digitStart + (c - '0'))
                    : c;
            }
        });
    }
}
