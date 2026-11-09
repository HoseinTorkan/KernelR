using KernelR.Domain.ValueObjects.Measurement;

namespace KernelR.Domain.Enums;

/// <summary>
/// Specifies the unit of measurement for a <see cref="PhysicalSize"/>.
/// </summary>
public enum PhysicalSizeUnit
{
    /// <summary>
    /// Millimeter (mm).
    /// </summary>
    Millimeter = 0,

    /// <summary>
    /// Centimeter (cm).
    /// </summary>
    Centimeter = 1,

    /// <summary>
    /// Meter (m).
    /// </summary>
    Meter = 2,

    /// <summary>
    /// Inch (in).
    /// </summary>
    Inch = 3,

    /// <summary>
    /// Foot (ft).
    /// </summary>
    Foot = 4
}
