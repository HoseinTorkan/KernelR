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

    public string Value => _value ?? string.Empty;

    public int Length => _value?.Length ?? 0;

    public bool IsEmpty => _value is null;

    public Number(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _value = NumberParse.NormalizeOrThrow(value);
    }

    private Number(string normalizedValue, bool _) => _value = normalizedValue;

    public ReadOnlySpan<char> AsSpan() => Value.AsSpan();

    public void CopyTo(Span<char> destination) => Value.AsSpan().CopyTo(destination);

    public bool TryCopyTo(Span<char> destination) => Value.AsSpan().TryCopyTo(destination);

    public static bool IsValid([NotNullWhen(true)] string? value) => NumberParse.IsValid(value);

    public static bool IsValid(ReadOnlySpan<char> value) => NumberParse.IsValid(value);

    public static bool IsValid(ReadOnlySpan<byte> utf8Text) => NumberParse.IsValid(utf8Text);

    public static Number Parse(string s) => NumberParse.Parse(s, CultureInfo.InvariantCulture);

    public static Number Parse(string s, IFormatProvider? provider) => NumberParse.Parse(s, provider);

    public static Number Parse(ReadOnlySpan<char> s) => NumberParse.Parse(s, CultureInfo.InvariantCulture);

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider)" />
    public static Number Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => NumberParse.Parse(s, provider);

    public static Number Parse(ReadOnlySpan<byte> utf8Text) => NumberParse.Parse(utf8Text, CultureInfo.InvariantCulture);

    public static Number Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider) => NumberParse.Parse(utf8Text, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, out Number result) => NumberParse.TryParse(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Number result) => NumberParse.TryParse(s, out result);

    public static bool TryParse(ReadOnlySpan<char> s, out Number result) => NumberParse.TryParse(s, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Number result) => NumberParse.TryParse(s, out result);

    public static bool TryParse(ReadOnlySpan<byte> utf8Text, out Number result) => NumberParse.TryParse(utf8Text, out result);

    /// <inheritdoc cref="IUtf8SpanParsable{TSelf}.TryParse(ReadOnlySpan{byte}, IFormatProvider, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out Number result) => NumberParse.TryParse(utf8Text, out result);

    public override string ToString() => ToString("G", null);

    public string ToString(IFormatProvider? formatProvider) => ToString("G", formatProvider);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => NumberFormat.Format(this, format, formatProvider);

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => NumberFormat.TryFormat(this, destination, out charsWritten, format, provider);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => NumberFormat.TryFormat(this, utf8Destination, out bytesWritten, format, provider);

    public int CompareTo(Number other) => string.CompareOrdinal(Value, other.Value);

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is Number other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(Number)}.", nameof(obj));
    }

    public static bool operator <(Number left, Number right) => left.CompareTo(right) < 0;

    public static bool operator <=(Number left, Number right) => left.CompareTo(right) <= 0;

    public static bool operator >(Number left, Number right) => left.CompareTo(right) > 0;

    public static bool operator >=(Number left, Number right) => left.CompareTo(right) >= 0;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static Number CreateUnchecked(string normalizedValue) => new(normalizedValue, true);
}
