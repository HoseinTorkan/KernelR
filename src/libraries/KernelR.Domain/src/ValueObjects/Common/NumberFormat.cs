// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace KernelR.Domain.ValueObjects.Common;

/// <summary>
/// Provides formatting logic for <see cref="Number"/>.
/// </summary>
/// <remarks>
/// <para>
/// This type is an implementation detail and is not intended to be used directly.
/// All public access goes through <see cref="Number"/>.
/// </para>
/// <para>
/// The display digit set is selected based on the <see cref="IFormatProvider"/>:
/// <list type="bullet">
/// <item><description>Persian cultures (<c>"fa"</c>) yield Persian digits (U+06F0 to U+06F9).</description></item>
/// <item><description>Arabic cultures (<c>"ar"</c>) yield Arabic-Indic digits (U+0660 to U+0669).</description></item>
/// <item><description>All other cultures yield ASCII digits.</description></item>
/// </list>
/// </para>
/// </remarks>
internal static class NumberFormat
{
    private const char AsciiZero = '0';
    private const char AsciiNine = '9';
    private const char PersianZero = '\u06F0';
    private const char ArabicIndicZero = '\u0660';

    // ------------------------------------------------------------------
    //  Format (allocating)
    // ------------------------------------------------------------------

    internal static string Format(Number number, string? format, IFormatProvider? provider)
    {
        ValidateFormat(format);
        char digitStart = GetDigitStart(provider);
        return ConvertDigits(number.Value, digitStart);
    }

    // ------------------------------------------------------------------
    //  TryFormat (non-allocating)
    // ------------------------------------------------------------------

    internal static bool TryFormat(
        Number number,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (!ValidateFormatSpan(format))
        {
            charsWritten = 0;
            return false;
        }

        char digitStart = GetDigitStart(provider);
        string value = number.Value;

        if (value.Length > destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            destination[i] = c is >= AsciiZero and <= AsciiNine
                ? (char)(digitStart + (c - AsciiZero))
                : c;
        }
        charsWritten = value.Length;
        return true;
    }

    internal static bool TryFormat(
        Number number,
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (!ValidateFormatSpan(format))
        {
            bytesWritten = 0;
            return false;
        }

        char digitStart = GetDigitStart(provider);
        string value = number.Value;

        // First pass: calculate required bytes.
        int requiredBytes = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char output = MapDigit(value[i], digitStart);
            requiredBytes += output < 0x80 ? 1 : (output < 0x800 ? 2 : 3);
        }

        if (requiredBytes > utf8Destination.Length)
        {
            bytesWritten = 0;
            return false;
        }

        // Second pass: write UTF-8 bytes.
        int pos = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char output = MapDigit(value[i], digitStart);
            if (output < 0x80)
            {
                utf8Destination[pos++] = (byte)output;
            }
            else if (output < 0x800)
            {
                utf8Destination[pos++] = (byte)(0xC0 | (output >> 6));
                utf8Destination[pos++] = (byte)(0x80 | (output & 0x3F));
            }
            else
            {
                utf8Destination[pos++] = (byte)(0xE0 | (output >> 12));
                utf8Destination[pos++] = (byte)(0x80 | ((output >> 6) & 0x3F));
                utf8Destination[pos++] = (byte)(0x80 | (output & 0x3F));
            }
        }
        bytesWritten = pos;
        return true;
    }

    // ------------------------------------------------------------------
    //  Private helpers
    // ------------------------------------------------------------------

    private static void ValidateFormat(string? format)
    {
        if (format is not (null or "" or "G"))
        {
            throw new FormatException(
                $"Format '{format}' is not supported. Only the general format specifier 'G' is valid.");
        }
    }

    private static bool ValidateFormatSpan(ReadOnlySpan<char> format)
        => format.IsEmpty || format.SequenceEqual("G");

    private static char GetDigitStart(IFormatProvider? provider)
    {
        if (provider is CultureInfo culture)
        {
            return culture.TwoLetterISOLanguageName switch
            {
                "fa" => PersianZero,
                "ar" => ArabicIndicZero,
                _ => AsciiZero
            };
        }
        return AsciiZero;
    }

    private static char MapDigit(char c, char digitStart)
        => c is >= AsciiZero and <= AsciiNine
            ? (char)(digitStart + (c - AsciiZero))
            : c;

    private static string ConvertDigits(string value, char digitStart)
    {
        if (digitStart == AsciiZero) return value;

        return string.Create(value.Length, (value, digitStart), static (span, state) =>
        {
            for (int i = 0; i < state.value.Length; i++)
            {
                char c = state.value[i];
                span[i] = c is >= AsciiZero and <= AsciiNine
                    ? (char)(state.digitStart + (c - AsciiZero))
                    : c;
            }
        });
    }
}
