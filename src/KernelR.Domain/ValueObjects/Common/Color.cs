using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KernelR.Domain.ValueObjects.Common;

/// <summary>
/// Represents an immutable color with RGBA components.
/// </summary>
/// <remarks>
/// <para>
/// This value object is framework-independent (does not rely on System.Drawing or WPF) 
/// and provides rich mathematical and functional operations like Lighten, Darken, and Invert.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;Color&gt; result = Color.FromHex("#FF5733");
/// if (result.IsSuccess)
/// {
///     string rgb = result.Value.ToRgbString(); // Outputs: rgb(255, 87, 51)
///     HslColor hsl = result.Value.ToHsl();
/// }
/// </code>
/// </example>
public readonly partial record struct Color : IValueObject, IParsable<Color>
{
    /// <summary>
    /// The regex pattern used to validate standard 6-digit (RGB) and 8-digit (ARGB) hex colors.
    /// </summary>
    [GeneratedRegex(@"^#(?:[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$")]
    private static partial Regex HexPattern();

    /// <summary>Gets the alpha component (0 = fully transparent, 255 = fully opaque).</summary>
    public byte A { get; }

    /// <summary>Gets the red component (0 to 255).</summary>
    public byte R { get; }

    /// <summary>Gets the green component (0 to 255).</summary>
    public byte G { get; }

    /// <summary>Gets the blue component (0 to 255).</summary>
    public byte B { get; }

    /// <summary>
    /// Gets the color as a normalized hex string.
    /// </summary>
    public string Hex => A == 255
        ? $"#{R:X2}{G:X2}{B:X2}"
        : $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    /// <summary>Gets a value indicating whether the color is fully transparent.</summary>
    public bool IsTransparent => A == 0;

    /// <summary>Gets the perceived brightness of the color (0.0 to 1.0).</summary>
    public double Brightness => (0.299 * R + 0.587 * G + 0.114 * B) / 255.0;

    /// <summary>Gets a value indicating whether the color is considered dark.</summary>
    public bool IsDark => Brightness < 0.5;

    /// <summary>Gets a fully transparent color.</summary>
    public static Color Transparent => new(0, 0, 0, 0);

    /// <summary>Gets the pure black color.</summary>
    public static Color Black => new(255, 0, 0, 0);

    /// <summary>Gets the pure white color.</summary>
    public static Color White => new(255, 255, 255, 255);

    /// <summary>Gets the pure red color.</summary>
    public static Color Red => new(255, 255, 0, 0);

    /// <summary>Gets the pure green color.</summary>
    public static Color Green => new(255, 0, 128, 0);

    /// <summary>Gets the pure blue color.</summary>
    public static Color Blue => new(255, 0, 0, 255);

    /// <summary>Initializes a new instance of the <see cref="Color"/> struct.</summary>
    private Color(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    /// <summary>Creates a color from ARGB components.</summary>
    /// <param name="a">The alpha component.</param>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <returns>A new <see cref="Color"/> instance.</returns>
    public static Color FromArgb(byte a, byte r, byte g, byte b) => new(a, r, g, b);

    /// <summary>Creates an opaque color from RGB components (Alpha is set to 255).</summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <returns>A new <see cref="Color"/> instance.</returns>
    public static Color FromRgb(byte r, byte g, byte b) => new(255, r, g, b);

    /// <summary>Creates a new <see cref="Color"/> from an HSL color representation.</summary>
    /// <param name="hsl">The HSL color to convert.</param>
    /// <returns>A new <see cref="Color"/> instance.</returns>
    public static Color FromHsl(HslColor hsl) => hsl.ToColor();

    /// <summary>Creates a new <see cref="Color"/> from a hex string.</summary>
    /// <param name="hex">The hex string (e.g., "#FF5733" or "#80FF5733").</param>
    /// <returns>A successful result carrying the color, or a failure if the hex string is invalid.</returns>
    public static Result<Color> FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return new Error(
                "KernelR.ValueObjects.Color.EMPTY",
                "Color cannot be empty.",
                ErrorType.ValidationError);

        string normalized = hex.Trim().ToUpperInvariant();

        if (!HexPattern().IsMatch(normalized))
            return new Error(
                "KernelR.ValueObjects.Color.INVALID_FORMAT",
                "Color must be a valid hex string (e.g., #FF5733 or #80FF5733).",
                ErrorType.ValidationError);

        byte a = 255;
        int offset = 1;

        if (normalized.Length == 9) // #AARRGGBB
        {
            a = byte.Parse(normalized.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            offset = 3;
        }

        byte r = byte.Parse(normalized.AsSpan(offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte g = byte.Parse(normalized.AsSpan(offset + 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte b = byte.Parse(normalized.AsSpan(offset + 4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return new Color(a, r, g, b);
    }

    /// <summary>Parses the specified string into a <see cref="Color"/> instance.</summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="Color"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid hex color.</exception>
    public static Color Parse(string s, IFormatProvider? provider) => FromHex(s).OrThrowFormatException();

    /// <summary>Attempts to parse the specified string into a <see cref="Color"/> instance.</summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="Color"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Color result)
        => FromHex(s ?? string.Empty).TryGetValue(out result);

    /// <summary>Returns a new color with the specified alpha component.</summary>
    /// <param name="alpha">The alpha component (0 to 255).</param>
    /// <returns>A new <see cref="Color"/> with the specified alpha.</returns>
    public Color WithAlpha(byte alpha) => new(alpha, R, G, B);

    /// <summary>Returns a lighter version of the current color.</summary>
    /// <param name="amount">The amount to lighten (0.0 to 1.0).</param>
    /// <returns>A new, lighter <see cref="Color"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is outside 0.0 to 1.0.</exception>
    public Color Lighten(double amount)
    {
        if (amount is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be between 0.0 and 1.0.");

        return new Color(
            A,
            (byte)Math.Clamp(R + (255 - R) * amount, 0, 255),
            (byte)Math.Clamp(G + (255 - G) * amount, 0, 255),
            (byte)Math.Clamp(B + (255 - B) * amount, 0, 255));
    }

    /// <summary>Returns a darker version of the current color.</summary>
    /// <param name="amount">The amount to darken (0.0 to 1.0).</param>
    /// <returns>A new, darker <see cref="Color"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is outside 0.0 to 1.0.</exception>
    public Color Darken(double amount)
    {
        if (amount is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be between 0.0 and 1.0.");

        return new Color(
            A,
            (byte)Math.Clamp(R * (1 - amount), 0, 255),
            (byte)Math.Clamp(G * (1 - amount), 0, 255),
            (byte)Math.Clamp(B * (1 - amount), 0, 255));
    }

    /// <summary>Returns the inverted version of the current color.</summary>
    /// <returns>A new <see cref="Color"/> with inverted RGB values.</returns>
    public Color Invert() => new(A, (byte)(255 - R), (byte)(255 - G), (byte)(255 - B));

    /// <summary>
    /// Converts the color to a standard CSS-like RGB string format (e.g., <c>rgb(255, 87, 51)</c>).
    /// </summary>
    /// <returns>The RGB string representation.</returns>
    public string ToRgbString() => $"rgb({R}, {G}, {B})";

    /// <summary>
    /// Converts the color to a standard CSS-like RGBA string format (e.g., <c>rgba(255, 87, 51, 0.5)</c>).
    /// </summary>
    /// <returns>The RGBA string representation.</returns>
    public string ToRgbaString()
    {
        double alpha = Math.Round(A / 255.0, 2);
        return $"rgba({R}, {G}, {B}, {alpha.ToString(CultureInfo.InvariantCulture)})";
    }

    /// <summary>
    /// Converts the color to its HSL (Hue, Saturation, Lightness) representation.
    /// </summary>
    /// <returns>A new <see cref="HslColor"/> instance representing the same color in HSL space.</returns>
    public HslColor ToHsl()
    {
        double r = R / 255.0;
        double g = G / 255.0;
        double b = B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double h = 0;
        double l = (max + min) / 2.0;

        double s = delta == 0
            ? 0
            : l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

        if (delta != 0)
        {
            h = max == r
                ? (g - b) / delta + (g < b ? 6 : 0)
                : max == g
                    ? (b - r) / delta + 2
                    : (r - g) / delta + 4;

            h /= 6;
        }

        return HslColor.Create(h * 360, s * 100, l * 100).OrThrowInvalidOperationException();
    }

    /// <summary>Returns the color as a normalized hex string.</summary>
    /// <returns>The hex string representation.</returns>
    public override string ToString() => Hex;

    /// <summary>Explicitly converts a <see cref="Color"/> to its normalized hex string.</summary>
    /// <param name="color">The color instance to convert.</param>
    /// <returns>The normalized hex string.</returns>
    public static explicit operator string(Color color) => color.Hex;
}
