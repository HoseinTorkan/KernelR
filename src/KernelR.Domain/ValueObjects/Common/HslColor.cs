using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KernelR.Domain.ValueObjects.Common;

/// <summary>
/// Represents a color in the HSL (Hue, Saturation, Lightness) color space.
/// </summary>
/// <remarks>
/// <para>
/// HSL is a more human-friendly way to represent colors compared to RGB. This value object 
/// ensures that the components are always within their valid ranges:
/// </para>
/// <list type="bullet">
///   <item><description><b>H (Hue):</b> 0 to 360 degrees.</description></item>
///   <item><description><b>S (Saturation):</b> 0 to 100 percent.</description></item>
///   <item><description><b>L (Lightness):</b> 0 to 100 percent.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;HslColor&gt; result = HslColor.Create("120, 50, 50");
/// if (result.IsSuccess)
/// {
///     Color rgb = result.Value.ToColor();
/// }
/// </code>
/// </example>
public readonly record struct HslColor : IValueObject, IParsable<HslColor>
{
    /// <summary>Gets the hue component in degrees (0 to 360).</summary>
    public double H { get; }

    /// <summary>Gets the saturation component as a percentage (0 to 100).</summary>
    public double S { get; }

    /// <summary>Gets the lightness component as a percentage (0 to 100).</summary>
    public double L { get; }

    /// <summary>Initializes a new instance of the <see cref="HslColor"/> struct.</summary>
    /// <param name="h">The hue component.</param>
    /// <param name="s">The saturation component.</param>
    /// <param name="l">The lightness component.</param>
    private HslColor(double h, double s, double l)
    {
        H = h;
        S = s;
        L = l;
    }

    /// <summary>
    /// Creates a new <see cref="HslColor"/> ensuring all components are within valid ranges.
    /// </summary>
    /// <param name="h">Hue in degrees (0 to 360).</param>
    /// <param name="s">Saturation percentage (0 to 100).</param>
    /// <param name="l">Lightness percentage (0 to 100).</param>
    /// <returns>
    /// A successful result carrying the new <see cref="HslColor"/>, or a failure with 
    /// <c>KernelR.ValueObjects.HslColor.INVALID_COMPONENTS</c> if any value is out of range.
    /// </returns>
    public static Result<HslColor> Create(double h, double s, double l)
    {
        if (h is < 0 or > 360)
            return new Error(
                "KernelR.ValueObjects.HslColor.INVALID_COMPONENTS",
                "Hue must be between 0 and 360.",
                ErrorType.ValidationError);

        if (s is < 0 or > 100)
            return new Error(
                "KernelR.ValueObjects.HslColor.INVALID_COMPONENTS",
                "Saturation must be between 0 and 100.",
                ErrorType.ValidationError);

        if (l is < 0 or > 100)
            return new Error(
                "KernelR.ValueObjects.HslColor.INVALID_COMPONENTS",
                "Lightness must be between 0 and 100.",
                ErrorType.ValidationError);

        return new HslColor(h, s, l);
    }

    /// <summary>
    /// Creates a new <see cref="HslColor"/> from a string in the format <c>"H,S,L"</c> or <c>"H, S, L"</c>.
    /// </summary>
    /// <param name="value">The raw HSL string (e.g., "120, 50, 50").</param>
    /// <returns>
    /// A successful result carrying the new <see cref="HslColor"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.HslColor.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.HslColor.INVALID_FORMAT</c> when the input is not formatted correctly or contains non-numeric values.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.HslColor.INVALID_COMPONENTS</c> when any value is out of range.</description></item>
    /// </list>
    /// </returns>
    public static Result<HslColor> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.HslColor.EMPTY",
                "HSL string cannot be empty.",
                ErrorType.ValidationError);

        string[] parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length != 3)
            return new Error(
                "KernelR.ValueObjects.HslColor.INVALID_FORMAT",
                "HSL string must be formatted as 'H, S, L'.",
                ErrorType.ValidationError);

        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double h) ||
            !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double s) ||
            !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double l))
            return new Error(
                "KernelR.ValueObjects.HslColor.INVALID_FORMAT",
                "HSL components must be valid numbers.",
                ErrorType.ValidationError);

        return Create(h, s, l);
    }

    /// <summary>
    /// Parses a string in the format <c>"H,S,L"</c> or <c>"H, S, L"</c> into an <see cref="HslColor"/>.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="HslColor"/> instance representing the parsed value.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid HSL representation.</exception>
    public static HslColor Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <summary>
    /// Attempts to parse a string into an <see cref="HslColor"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="HslColor"/> instance, if the parsing succeeded, or the default value if it failed.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out HslColor result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>
    /// Converts this HSL color to an RGB <see cref="Color"/>.
    /// </summary>
    /// <returns>A new <see cref="Color"/> instance representing the same color in RGB space.</returns>
    public Color ToColor()
    {
        double h = H / 360.0;
        double s = S / 100.0;
        double l = L / 100.0;

        double r, g, b;

        if (s == 0)
            r = g = b = l; // Achromatic (gray)
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;

            r = HueToRgb(p, q, h + 1.0 / 3.0);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3.0);
        }

        return Color.FromRgb(
            (byte)Math.Round(r * 255),
            (byte)Math.Round(g * 255),
            (byte)Math.Round(b * 255));
    }

    /// <summary>
    /// Converts a hue component to its corresponding RGB value.
    /// </summary>
    /// <param name="p">The minimum RGB value.</param>
    /// <param name="q">The maximum RGB value.</param>
    /// <param name="t">The normalized hue.</param>
    /// <returns>The RGB component value between 0.0 and 1.0.</returns>
    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;

        return t switch
        {
            < 1.0 / 6.0 => p + (q - p) * 6 * t,
            < 1.0 / 2.0 => q,
            _ => t < 2.0 / 3.0 ? p + (q - p) * (2.0 / 3.0 - t) * 6 : p,
        };
    }

    /// <summary>
    /// Returns the HSL color as a string (e.g., <c>hsl(120, 50%, 50%)</c>).
    /// </summary>
    /// <returns>The HSL string representation.</returns>
    public override string ToString()
    {
        return $"hsl({H.ToString(CultureInfo.InvariantCulture)}, {S.ToString(CultureInfo.InvariantCulture)}%, {L.ToString(CultureInfo.InvariantCulture)}%)";
    }
}
