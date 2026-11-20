// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Common;

public readonly record struct Number :
    IParsable<Number>,
    ISpanParsable<Number>,
    IUtf8SpanParsable<Number>,
    IFormattable,
    ISpanFormattable,
    IUtf8SpanFormattable,
    IComparable<Number>,
    IComparable
{
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
    public int Length => _value?.Length ?? 0;

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
    /// <paramref name="value"/> is empty, consists only of white-space characters,
    /// or contains a character other than an ASCII, Persian, or Arabic-Indic decimal digit.
    /// </exception>
    public Number(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _value = NumberParse.NormalizeOrThrow(value);
    }

    /// <summary>
    /// Initializes a new instance using an already normalized value.
    /// </summary>
    /// <param name="normalizedValue">A non-empty string containing only ASCII digits.</param>
    /// <param name="_">Indicates that the value has already been normalized.</param>
    private Number(string normalizedValue, bool _) => _value = normalizedValue;

    // ------------------------------------------------------------------
    //  Instance helpers
    // ------------------------------------------------------------------

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

    // ------------------------------------------------------------------
    //  Static validation helpers (public)
    // ------------------------------------------------------------------

    /// <summary>
    /// Determines whether the specified string contains only supported decimal digits.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the string is non-empty and contains only supported digits;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid([NotNullWhen(true)] string? value) => NumberParse.IsValid(value);

    /// <summary>
    /// Determines whether the specified character span contains only supported decimal digits.
    /// </summary>
    /// <param name="value">The character span to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the span is non-empty and contains only supported digits;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid(ReadOnlySpan<char> value) => NumberParse.IsValid(value);

    /// <summary>
    /// Determines whether the specified UTF-8 span contains only supported decimal digits.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 data to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the data represents a non-empty sequence of supported digits;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid(ReadOnlySpan<byte> utf8Text) => NumberParse.IsValid(utf8Text);

    // ------------------------------------------------------------------
    //  Parse / TryParse (public surface)
    // ------------------------------------------------------------------

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
    public static Number Parse(string s) => NumberParse.Parse(s, CultureInfo.InvariantCulture);

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider)" />
    public static Number Parse(string s, IFormatProvider? provider) => NumberParse.Parse(s, provider);

    /// <summary>
    /// Converts the character representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">A character span containing supported decimal digits.</param>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is empty or contains unsupported characters.
    /// </exception>
    public static Number Parse(ReadOnlySpan<char> s) => NumberParse.Parse(s, CultureInfo.InvariantCulture);

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider)" />
    public static Number Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => NumberParse.Parse(s, provider);

    /// <summary>
    /// Converts the UTF-8 representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="utf8Text">UTF-8 data containing supported decimal digits.</param>
    /// <returns>A <see cref="Number"/> containing the normalized digit sequence.</returns>
    /// <exception cref="FormatException">
    /// The input is invalid UTF-8, empty, or contains unsupported characters.
    /// </exception>
    public static Number Parse(ReadOnlySpan<byte> utf8Text) => NumberParse.Parse(utf8Text, CultureInfo.InvariantCulture);

    /// <inheritdoc cref="IUtf8SpanParsable{TSelf}.Parse(ReadOnlySpan{byte}, IFormatProvider)" />
    public static Number Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider) => NumberParse.Parse(utf8Text, provider);

    /// <summary>
    /// Attempts to convert the string representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Number"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse([NotNullWhen(true)] string? s, out Number result) => NumberParse.TryParse(s, out result);

    /// <inheritdoc cref="IParsable{TSelf}.TryParse(string, IFormatProvider, out TSelf)" />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Number result) => NumberParse.TryParse(s, out result);

    /// <summary>
    /// Attempts to convert the character representation of a number to its equivalent <see cref="Number"/>.
    /// </summary>
    /// <param name="s">The character span to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Number"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> s, out Number result) => NumberParse.TryParse(s, out result);

    /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Number result) => NumberParse.TryParse(s, out result);

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
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, out Number result) => NumberParse.TryParse(utf8Text, out result);

    /// <inheritdoc cref="IUtf8SpanParsable{TSelf}.TryParse(ReadOnlySpan{byte}, IFormatProvider, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out Number result) => NumberParse.TryParse(utf8Text, out result);

    // ------------------------------------------------------------------
    //  Formatting
    // ------------------------------------------------------------------

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

    /// <inheritdoc cref="IFormattable.ToString(string, IFormatProvider)" />
    public string ToString(string? format, IFormatProvider? formatProvider)
        => NumberFormat.Format(this, format, formatProvider);

    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider)" />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => NumberFormat.TryFormat(this, destination, out charsWritten, format, provider);

    /// <inheritdoc cref="IUtf8SpanFormattable.TryFormat(Span{byte}, out int, ReadOnlySpan{char}, IFormatProvider)" />
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => NumberFormat.TryFormat(this, utf8Destination, out bytesWritten, format, provider);

    // ------------------------------------------------------------------
    //  Comparison
    // ------------------------------------------------------------------

    /// <summary>
    /// Compares the current instance with another <see cref="Number"/> and returns an integer
    /// that indicates whether the current instance precedes, follows, or occurs in the same
    /// position in the sort order as the other instance.
    /// </summary>
    /// <param name="other">The <see cref="Number"/> to compare with this instance.</param>
    /// <returns>
    /// A value less than zero if this instance precedes <paramref name="other"/> in the sort order;
    /// zero if this instance occurs in the same position as <paramref name="other"/>;
    /// greater than zero if this instance follows <paramref name="other"/>.
    /// </returns>
    /// <remarks>
    /// Comparison is ordinal and culture-independent. It uses
    /// <see cref="string.CompareOrdinal(string, string)"/> on the normalized ASCII digit sequences.
    /// Leading zeros are significant: <c>"00123"</c> precedes <c>"123"</c>.
    /// </remarks>
    public int CompareTo(Number other) => string.CompareOrdinal(Value, other.Value);

    /// <inheritdoc cref="IComparable.CompareTo(object)" />
    /// <exception cref="ArgumentException">
    /// <paramref name="obj"/> is not a <see cref="Number"/>.
    /// </exception>
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is Number other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(Number)}.", nameof(obj));
    }

    /// <summary>
    /// Returns a value indicating whether one <see cref="Number"/> is less than another.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator <(Number left, Number right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Returns a value indicating whether one <see cref="Number"/> is less than or equal to another.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator <=(Number left, Number right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Returns a value indicating whether one <see cref="Number"/> is greater than another.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator >(Number left, Number right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Returns a value indicating whether one <see cref="Number"/> is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator >=(Number left, Number right) => left.CompareTo(right) >= 0;

    // ------------------------------------------------------------------
    //  Internal factory (used by NumberParse)
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="Number"/> from an already-normalized value without re-validating.
    /// </summary>
    /// <param name="normalizedValue">A non-empty string containing only ASCII digits.</param>
    /// <returns>A new <see cref="Number"/> instance.</returns>
    /// <remarks>
    /// This member is an implementation detail and is hidden from IntelliSense.
    /// Callers are responsible for ensuring <paramref name="normalizedValue"/> is non-empty
    /// and contains only ASCII digits.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static Number CreateUnchecked(string normalizedValue) => new(normalizedValue, true);
}
