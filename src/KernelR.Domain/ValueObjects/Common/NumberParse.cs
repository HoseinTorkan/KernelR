// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KernelR.Domain.ValueObjects.Common;

/// <summary>
/// Provides parsing and normalization logic for <see cref="Number"/>.
/// </summary>
/// <remarks>
/// <para>
/// This type is an implementation detail and is not intended to be used directly.
/// All public access goes through <see cref="Number"/>.
/// </para>
/// <para>
/// Supported digit sets:
/// <list type="bullet">
/// <item><description>ASCII digits: <c>0</c> through <c>9</c> (U+0030 to U+0039)</description></item>
/// <item><description>Persian digits: U+06F0 to U+06F9</description></item>
/// <item><description>Arabic-Indic digits: U+0660 to U+0669</description></item>
/// </list>
/// </para>
/// </remarks>
internal static class NumberParse
{
    private const char AsciiZero = '0';
    private const char AsciiNine = '9';
    private const char PersianZero = '\u06F0';
    private const char PersianNine = '\u06F9';
    private const char ArabicIndicZero = '\u0660';
    private const char ArabicIndicNine = '\u0669';

    private static readonly UTF8Encoding s_strictUtf8Encoding = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    // ------------------------------------------------------------------
    //  Validation
    // ------------------------------------------------------------------

    /// <summary>
    /// Determines whether the specified string contains only supported decimal digits.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the string is non-empty and contains only supported digits;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool IsValid([NotNullWhen(true)] string? value)
    {
        if (value is null) return false;
        return IsValid(value.AsSpan());
    }

    /// <summary>
    /// Determines whether the specified character span contains only supported decimal digits.
    /// </summary>
    /// <param name="value">The character span to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the span is non-empty and contains only supported digits;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool IsValid(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return false;
        foreach (char c in value)
            if (!IsSupportedDigit(c)) return false;
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
    internal static bool IsValid(ReadOnlySpan<byte> utf8Text)
    {
        if (utf8Text.IsEmpty) return false;
        try
        {
            string decoded = s_strictUtf8Encoding.GetString(utf8Text);
            return IsValid(decoded.AsSpan());
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    // ------------------------------------------------------------------
    //  Normalization
    // ------------------------------------------------------------------

    /// <summary>
    /// Normalizes the specified value to ASCII digits, throwing if it contains unsupported characters.
    /// </summary>
    /// <param name="value">The value to normalize. Must be non-null and non-whitespace.</param>
    /// <returns>A string containing only ASCII digits.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is empty or contains unsupported characters.
    /// </exception>
    internal static string NormalizeOrThrow(string value)
    {
        if (!TryNormalize(value.AsSpan(), out string? normalized) || normalized.Length == 0)
        {
            throw new ArgumentException(
                "The value must contain one or more supported decimal digits.",
                nameof(value));
        }
        return normalized;
    }

    /// <summary>
    /// Attempts to normalize supported Persian and Arabic-Indic digits to ASCII digits.
    /// </summary>
    /// <param name="value">The character span to normalize.</param>
    /// <param name="normalizedValue">
    /// When this method returns, contains the normalized value if successful;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input could be normalized; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// An empty span can be normalized successfully (returning an empty string), but it is not
    /// a valid <see cref="Number"/>. Unsupported characters are never removed or ignored.
    /// </remarks>
    internal static bool TryNormalize(ReadOnlySpan<char> value, [NotNullWhen(true)] out string? normalizedValue)
    {
        // Fast path: all ASCII digits.
        bool allAscii = true;
        foreach (char c in value)
        {
            if (c < AsciiZero || c > AsciiNine)
            {
                allAscii = false;
                break;
            }
        }

        if (allAscii)
        {
            normalizedValue = value.ToString();
            return true;
        }

        // Slow path: normalize character by character.
        char[] buffer = new char[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            if (!TryConvertDigit(value[i], out char digit))
            {
                normalizedValue = null;
                return false;
            }
            buffer[i] = digit;
        }
        normalizedValue = new string(buffer);
        return true;
    }

    // ------------------------------------------------------------------
    //  Parse / TryParse
    // ------------------------------------------------------------------

    internal static Number Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        return TryParse(s.AsSpan(), out Number result)
            ? result
            : throw new FormatException("The input is not a valid Number.");
    }

    internal static Number Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, out Number result))
            throw new FormatException("The input is not a valid Number.");
        return result;
    }

    internal static Number Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
    {
        if (!TryParse(utf8Text, out Number result))
            throw new FormatException("The input is not a valid UTF-8 Number.");
        return result;
    }

    internal static bool TryParse([NotNullWhen(true)] string? s, out Number result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }
        return TryParse(s.AsSpan(), out result);
    }

    internal static bool TryParse(ReadOnlySpan<char> s, out Number result)
    {
        if (s.IsEmpty || !TryNormalize(s, out string? normalized) || normalized.Length == 0)
        {
            result = default;
            return false;
        }
        result = Number.CreateUnchecked(normalized);
        return true;
    }

    internal static bool TryParse(ReadOnlySpan<byte> utf8Text, out Number result)
    {
        if (utf8Text.IsEmpty)
        {
            result = default;
            return false;
        }
        try
        {
            string decoded = s_strictUtf8Encoding.GetString(utf8Text);
            return TryParse(decoded.AsSpan(), out result);
        }
        catch (DecoderFallbackException)
        {
            result = default;
            return false;
        }
    }

    // ------------------------------------------------------------------
    //  Private helpers
    // ------------------------------------------------------------------

    private static bool IsSupportedDigit(char c)
        => c is >= AsciiZero and <= AsciiNine
           or >= PersianZero and <= PersianNine
           or >= ArabicIndicZero and <= ArabicIndicNine;

    private static bool TryConvertDigit(char c, out char digit)
    {
        if (c is >= AsciiZero and <= AsciiNine)
        {
            digit = c;
            return true;
        }
        if (c is >= PersianZero and <= PersianNine)
        {
            digit = (char)(AsciiZero + (c - PersianZero));
            return true;
        }
        if (c is >= ArabicIndicZero and <= ArabicIndicNine)
        {
            digit = (char)(AsciiZero + (c - ArabicIndicZero));
            return true;
        }
        digit = default;
        return false;
    }
}
