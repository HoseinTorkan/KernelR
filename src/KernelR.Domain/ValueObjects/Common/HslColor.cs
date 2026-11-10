using KernelR.Patterns;
using KernelR.Patterns.Results;
using KernelR.Patterns.Results.Extensions;
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
public readonly record struct HslColor :
    IValueObject,
    IParsable<HslColor>
{
    #region ═══ Properties                                                 ═══

    /// <summary>Gets the hue component in degrees (0 to 360).</summary>
    public double H { get; }

    /// <summary>Gets the saturation component as a percentage (0 to 100).</summary>
    public double S { get; }

    /// <summary>Gets the lightness component as a percentage (0 to 100).</summary>
    public double L { get; }

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

    /// <summary>Initializes a new instance of the <see cref="HslColor"/> struct.</summary>
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
        {
            return Result<HslColor>.Failure(
                new Error(
                    "KernelR.ValueObjects.HslColor.INVALID_COMPONENTS",
                    "Hue must be between 0 and 360.",
                    ErrorType.ValidationError));
        }

        if (s is < 0 or > 100)
        {
            return Result<HslColor>.Failure(
                new Error(
                    "KernelR.ValueObjects.HslColor.INVALID_COMPONENTS",
                    "Saturation must be between 0 and 100.",
                    ErrorType.ValidationError));
        }

        if (l is < 0 or > 100)
        {
            return Result<HslColor>.Failure(
                new Error(
                    "KernelR.ValueObjects.HslColor.INVALID_COMPONENTS",
                    "Lightness must be between 0 and 100.",
                    ErrorType.ValidationError));
        }

        return Result<HslColor>.Success(new(h, s, l));
    }

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <summary>
    /// Parses a string in the format <c>"H,S,L"</c> or <c>"H, S, L"</c> into an <see cref="HslColor"/>.
    /// </summary>
    public static HslColor Parse(string s, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            throw new FormatException("HSL string cannot be empty.");
        }

        string[] parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length != 3)
        {
            throw new FormatException("HSL string must be formatted as 'H, S, L'.");
        }

        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double h) ||
            !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double sat) ||
            !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double l))
        {
            throw new FormatException("HSL components must be valid numbers.");
        }

        Result<HslColor> result = Create(h, sat, l);
        return result.OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse a string into an <see cref="HslColor"/> instance.
    /// </summary>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out HslColor result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        string[] parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double h) ||
            !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double sat) ||
            !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double l))
        {
            return false;
        }

        Result<HslColor> createResult = Create(h, sat, l);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Functional Operations                                      ═══

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
        {
            r = g = b = l; // Achromatic (gray)
        }
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

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0)
        {
            t += 1;
        }

        if (t > 1)
        {
            t -= 1;
        }

        if (t < 1.0 / 6.0)
        {
            return p + (q - p) * 6 * t;
        }

        if (t < 1.0 / 2.0)
        {
            return q;
        }

        return t < 2.0 / 3.0 ? p + (q - p) * (2.0 / 3.0 - t) * 6 : p;
    }

    #endregion ═══ Functional Operations                                      ═══

    #region ═══ Helpers & Overrides                                        ═══

    /// <summary>
    /// Returns the HSL color as a string (e.g., <c>hsl(120, 50%, 50%)</c>).
    /// </summary>
    public override string ToString()
    {
        return $"hsl({H.ToString(CultureInfo.InvariantCulture)}, {S.ToString(CultureInfo.InvariantCulture)}%, {L.ToString(CultureInfo.InvariantCulture)}%)";
    }

    #endregion ═══ Helpers & Overrides                                        ═══
}
