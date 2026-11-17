using System.Globalization;
using System.Numerics;
using System.Text;

namespace KernelR.Domain.ValueObjects.Common;

/// <summary>
/// Represents a non-empty sequence of decimal digits.
/// </summary>
/// <remarks>
/// <para>
/// Number is a string-based value object, not a mathematical numeric type.
/// It preserves leading zeros and supports ASCII, Persian, and Arabic-Indic digits.
/// </para>
/// <para>
/// The stored representation always uses ASCII digits (0-9).
/// </para>
/// <para>
/// The default value is empty. An explicitly constructed or parsed <see cref="Number"/>
/// must contain at least one valid digit.
/// </para>
/// <para>
/// This type does not provide arithmetic operations or numeric ordering. Equality is based
/// on the normalized digit sequence, so leading zeros are significant.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Number identifier = new("۰۰۱۲۳");
///
/// Console.WriteLine(identifier.Value); // 00123
/// Console.WriteLine(identifier.Length); // 5
/// Console.WriteLine(identifier == new Number("123")); // False
/// </code>
/// </example>
public readonly record struct Number :
    IValueObject,
    IParsable<Number>,
    ISpanParsable<Number>,
    IUtf8SpanParsable<Number>,
    IFormattable,
    ISpanFormattable
{
    private static readonly UTF8Encoding s_strictUtf8Encoding = new(false, true);

    private static readonly string[] s_asciiDigits =
    [
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
    ];

    private static readonly string[] s_persianDigits =
    [
        "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹"
    ];

    private static readonly string[] s_arabicIndicDigits =
    [
        "٠", "١", "٢", "٣", "٤", "٥", "٦", "٧", "٨", "٩"
    ];

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
    /// <remarks>
    /// A default Number instance is empty. Explicitly constructed and parsed instances cannot be empty.
    /// </remarks>
    public bool IsEmpty => _value is null;

    /// <summary>
    /// Gets a value indicating whether the instance contains only zero digits.
    /// </summary>
    /// <value>
    /// <see langword="true"/> when the instance is non-empty and every digit is <c>0</c>;
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// An empty Number is not considered zero.
    /// </remarks>
    public bool IsZero
    {
        get
        {
            if (IsEmpty) return false;

            foreach (char digit in Value)
                if (digit != '0') return false;

            return true;
        }
    }

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
    /// <example>
    /// <code>
    /// Number first = new("00123");
    /// Number second = new("۱۲۳");
    ///
    /// Console.WriteLine(first.Value);  // 00123
    /// Console.WriteLine(second.Value); // 123
    /// </code>
    /// </example>
    public Number(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

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
    /// <returns>A read-only span over the stored value.</returns>
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
    public static bool IsValid(string? value) => value is not null && IsValid(value.AsSpan());

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
        ArgumentNullException.ThrowIfNull(value);
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
    /// An empty string can be normalized successfully, but it is not a valid Number.
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
    /// An empty span can be normalized successfully, but it is not a valid Number.
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
        or >= '\u06F0' and <= '\u06F9'
        or >= '\u0660' and <= '\u0669';

    private static bool TryConvertDigit(char value, out char digit)
    {
        if (value is >= '0' and <= '9')
        {
            digit = value;
            return true;
        }

        if (value is >= '\u06F0' and <= '\u06F9')
        {
            digit = (char)('0' + value - '\u06F0');
            return true;
        }

        if (value is >= '\u0660' and <= '\u0669')
        {
            digit = (char)('0' + value - '\u0660');
            return true;
        }

        digit = default;
        return false;
    }

    /// <summary>
    /// Converts the string representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="value">A string containing supported decimal digits.</param>
    /// <returns>The parsed Number.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> is empty or contains unsupported characters.
    /// </exception>
    /// <remarks>
    /// Parsing is culture-independent. The input must contain digits only; whitespace,
    /// signs, decimal separators, and group separators are not accepted.
    /// </remarks>
    /// <example>
    /// <code>
    /// Number accountCode = Number.Parse("۰۰۱۲۳۴");
    /// Console.WriteLine(accountCode.Value); // 001234
    /// </code>
    /// </example>
    public static Number Parse(string value) => Parse(value, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts the string representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">A string containing supported decimal digits.</param>
    /// <param name="provider">
    /// An object that supplies formatting information. This parameter is ignored because
    /// Number parsing is independent of culture.
    /// </param>
    /// <returns>The parsed Number.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="s"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (!TryParse(s.AsSpan(), out Number result))
            throw new FormatException("The input is not a valid Number.");

        return result;
    }

    /// <summary>
    /// Converts the character representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="value">A character span containing supported decimal digits.</param>
    /// <returns>The parsed Number.</returns>
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
    /// Number parsing is independent of culture.
    /// </param>
    /// <returns>The parsed Number.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, out Number result))
            throw new FormatException("The input is not a valid Number.");

        return result;
    }

    /// <summary>
    /// Converts the UTF-8 representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="utf8Text">UTF-8 data containing supported decimal digits.</param>
    /// <returns>The parsed Number.</returns>
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
    /// Number parsing is independent of culture.
    /// </param>
    /// <returns>The parsed Number.</returns>
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
    /// When this method returns, contains the parsed Number if successful; otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// If parsing fails, <paramref name="result"/> is set to <see langword="default"/>.
    /// Unlike <see cref="Parse(string)"/>, this method does not throw for invalid input.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (Number.TryParse("۱۲۳۴۵", out Number parsed))
    ///     Console.WriteLine(parsed.Value); // 12345
    /// </code>
    /// </example>
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
    /// Number parsing is independent of culture.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed Number if successful; otherwise, the default value.
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
    /// When this method returns, contains the parsed Number if successful; otherwise, the default value.
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
    /// Number parsing is independent of culture.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed Number if successful; otherwise, the default value.
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
    /// <param name="provider">
    /// An object that supplies formatting information. This parameter is ignored because
    /// Number parsing is independent of culture.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed Number if successful; otherwise, the default value.
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
    /// Returns the normalized value without applying numeric formatting.
    /// </summary>
    /// <returns>The stored ASCII digit sequence.</returns>
    /// <remarks>
    /// This overload preserves leading zeros and returns an empty string for the default value.
    /// </remarks>
    public override string ToString() => Value;

    /// <summary>
    /// Formats the value using the specified format.
    /// </summary>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <returns>The formatted value.</returns>
    public string ToString(string? format) => ToString(format, CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats the value using the specified format provider.
    /// </summary>
    /// <param name="formatProvider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <returns>The formatted value.</returns>
    public string ToString(IFormatProvider? formatProvider) => ToString("G", formatProvider);

    /// <summary>
    /// Formats the value using the specified format and format provider.
    /// </summary>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="formatProvider">
    /// An object that supplies culture-specific formatting information.
    /// If <see langword="null"/>, <see cref="CultureInfo.InvariantCulture"/> is used.
    /// </param>
    /// <returns>The formatted value.</returns>
    /// <remarks>
    /// <para>
    /// Formatting first interprets the stored digit sequence as a non-negative integer.
    /// It is therefore intended for display, not for preserving identifier-like text.
    /// </para>
    /// <para>
    /// Numeric formatting interprets the digit sequence as a non-negative integer.
    /// Consequently, numeric formats such as G, N, D, and X may remove leading zeros.
    /// </para>
    /// <para>
    /// Use the parameterless <see cref="ToString()"/> overload to preserve the exact stored sequence.
    /// </para>
    /// <para>
    /// Culture-specific separators and symbols are applied through the format provider.
    /// Persian and Arabic cultures also use their corresponding digit characters when appropriate.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Number value = new("001234");
    ///
    /// Console.WriteLine(value.ToString());       // 001234
    /// Console.WriteLine(value.ToString("N0"));   // 1,234 (culture-dependent separators)
    /// </code>
    /// </example>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (IsEmpty) return string.Empty;

        IFormatProvider provider = formatProvider ?? CultureInfo.InvariantCulture;
        BigInteger numericValue = ToBigInteger();
        string formattedValue = numericValue.ToString(format, provider);

        return ConvertFormattedDigits(formattedValue, GetNativeDigits(provider));
    }

    /// <summary>
    /// Attempts to format the value into the specified character span.
    /// </summary>
    /// <param name="destination">The destination character span.</param>
    /// <param name="charsWritten">
    /// When this method returns, contains the number of characters written.
    /// </param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// If <see langword="null"/>, <see cref="CultureInfo.InvariantCulture"/> is used.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the value was formatted successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;

        if (IsEmpty) return true;

        IFormatProvider effectiveProvider = provider ?? CultureInfo.InvariantCulture;
        string[] nativeDigits = GetNativeDigits(effectiveProvider);

        if (!AreSingleCharacterDigits(nativeDigits))
        {
            string formattedValue = ToString(format.IsEmpty ? null : format.ToString(), effectiveProvider);

            if (formattedValue.Length > destination.Length) return false;

            formattedValue.AsSpan().CopyTo(destination);
            charsWritten = formattedValue.Length;
            return true;
        }

        if (!ToBigInteger().TryFormat(destination, out charsWritten, format, effectiveProvider))
        {
            charsWritten = 0;
            return false;
        }

        ApplyNativeDigits(destination[..charsWritten], nativeDigits);
        return true;
    }

    private BigInteger ToBigInteger() =>
        BigInteger.Parse(Value, NumberStyles.None, CultureInfo.InvariantCulture);

    private static string[] GetNativeDigits(IFormatProvider provider)
    {
        string[] nativeDigits = NumberFormatInfo.GetInstance(provider).NativeDigits;

        if (!AreAsciiDigits(nativeDigits)) return nativeDigits;

        if (provider is CultureInfo culture)
        {
            if (culture.TwoLetterISOLanguageName.Equals("fa", StringComparison.OrdinalIgnoreCase))
                return s_persianDigits;

            if (culture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase))
                return s_arabicIndicDigits;
        }

        return s_asciiDigits;
    }

    private static bool AreAsciiDigits(string[] digits)
    {
        if (digits.Length != 10) return false;

        for (int i = 0; i < digits.Length; i++)
            if (digits[i] != s_asciiDigits[i]) return false;

        return true;
    }

    private static bool AreSingleCharacterDigits(string[] digits)
    {
        if (digits.Length != 10) return false;

        foreach (string digit in digits)
            if (digit.Length != 1) return false;

        return true;
    }

    private static string ConvertFormattedDigits(string value, string[] nativeDigits)
    {
        if (AreAsciiDigits(nativeDigits)) return value;

        StringBuilder builder = new(value.Length);

        foreach (char character in value)
        {
            if (character is >= '0' and <= '9')
                builder.Append(nativeDigits[character - '0']);
            else
                builder.Append(character);
        }

        return builder.ToString();
    }

    private static void ApplyNativeDigits(Span<char> value, string[] nativeDigits)
    {
        if (AreAsciiDigits(nativeDigits)) return;

        for (int i = 0; i < value.Length; i++)
            if (value[i] is >= '0' and <= '9')
                value[i] = nativeDigits[value[i] - '0'][0];
    }

    /// <summary>
    /// Converts an unsigned 8-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The unsigned integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    public static implicit operator Number(byte value) =>
        new(value.ToString(CultureInfo.InvariantCulture), true);

    /// <summary>
    /// Converts an unsigned 16-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The unsigned integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    public static implicit operator Number(ushort value) =>
        new(value.ToString(CultureInfo.InvariantCulture), true);

    /// <summary>
    /// Converts an unsigned 32-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The unsigned integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    public static implicit operator Number(uint value) =>
        new(value.ToString(CultureInfo.InvariantCulture), true);

    /// <summary>
    /// Converts an unsigned 64-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The unsigned integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    public static implicit operator Number(ulong value) =>
        new(value.ToString(CultureInfo.InvariantCulture), true);

    /// <summary>
    /// Converts an unsigned 128-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The unsigned integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    public static implicit operator Number(UInt128 value) =>
        new(value.ToString(CultureInfo.InvariantCulture), true);

    /// <summary>
    /// Converts a signed 8-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The signed integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public static explicit operator Number(sbyte value) => FromSignedInteger(value);

    /// <summary>
    /// Converts a signed 16-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The signed integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public static explicit operator Number(short value) => FromSignedInteger(value);

    /// <summary>
    /// Converts a signed 32-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The signed integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public static explicit operator Number(int value) => FromSignedInteger(value);

    /// <summary>
    /// Converts a signed 64-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The signed integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public static explicit operator Number(long value) => FromSignedInteger(value);

    /// <summary>
    /// Converts a signed 128-bit integer to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The signed integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public static explicit operator Number(Int128 value) => FromSignedInteger(value);

    /// <summary>
    /// Converts a <see cref="BigInteger"/> to a <see cref="Number"/>.
    /// </summary>
    /// <param name="value">The integer to convert.</param>
    /// <returns>A <see cref="Number"/> containing the invariant-culture decimal representation.</returns>
    /// <exception cref="OverflowException">
    /// The value is negative.
    /// </exception>
    public static explicit operator Number(BigInteger value)
        => value.Sign < 0
            ? throw new OverflowException("A negative integer cannot be represented by Number.")
            : new Number(value.ToString(CultureInfo.InvariantCulture), true);

    private static Number FromSignedInteger(long value)
        => value < 0
            ? throw new OverflowException("A negative integer cannot be represented by Number.")
            : new Number(value.ToString(CultureInfo.InvariantCulture), true);

    private static Number FromSignedInteger(Int128 value)
        => value < 0
            ? throw new OverflowException("A negative integer cannot be represented by Number.")
            : new Number(value.ToString(CultureInfo.InvariantCulture), true);

    /// <summary>
    /// Converts a Number to an unsigned 8-bit integer.
    /// </summary>
    public static explicit operator byte(Number value) =>
        byte.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a <see cref="Number"/> to a signed 8-bit integer.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The integer represented by the digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The value is the default (empty) instance.
    /// </exception>
    /// <exception cref="OverflowException">
    /// The represented integer is outside the range of sbyte.
    /// </exception>
    public static explicit operator sbyte(Number value) =>
        sbyte.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a Number to an unsigned 16-bit integer.
    /// </summary>
    public static explicit operator ushort(Number value) =>
        ushort.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a <see cref="Number"/> to a signed 16-bit integer.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The integer represented by the digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The value is the default (empty) instance.
    /// </exception>
    /// <exception cref="OverflowException">
    /// The represented integer is outside the range of short.
    /// </exception>
    public static explicit operator short(Number value) =>
        short.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a Number to an unsigned 32-bit integer.
    /// </summary>
    public static explicit operator uint(Number value) =>
        uint.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a <see cref="Number"/> to a signed 32-bit integer.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The integer represented by the digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The value is the default (empty) instance.
    /// </exception>
    /// <exception cref="OverflowException">
    /// The represented integer is outside the range of int.
    /// </exception>
    public static explicit operator int(Number value) =>
        int.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a Number to an unsigned 64-bit integer.
    /// </summary>
    public static explicit operator ulong(Number value) =>
        ulong.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a <see cref="Number"/> to a signed 64-bit integer.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The integer represented by the digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The value is the default (empty) instance.
    /// </exception>
    /// <exception cref="OverflowException">
    /// The represented integer is outside the range of long.
    /// </exception>
    public static explicit operator long(Number value) =>
        long.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a Number to an unsigned 128-bit integer.
    /// </summary>
    public static explicit operator UInt128(Number value) =>
        UInt128.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a <see cref="Number"/> to a signed 128-bit integer.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The integer represented by the digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The value is the default (empty) instance.
    /// </exception>
    /// <exception cref="OverflowException">
    /// The represented integer is outside the range of Int128.
    /// </exception>
    public static explicit operator Int128(Number value) =>
        Int128.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a <see cref="Number"/> to a <see cref="BigInteger"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The non-negative integer represented by the digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The value is the default (empty) instance.
    /// </exception>
    public static explicit operator BigInteger(Number value) =>
        BigInteger.Parse(value.Value, NumberStyles.None, CultureInfo.InvariantCulture);
}
