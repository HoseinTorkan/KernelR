using KernelR.Domain.ValueObjects.Measurement;

namespace KernelR.Domain.Enums;

/// <summary>
/// Specifies the unit of measurement for a <see cref="PhysicalSize"/>.
/// </summary>
/// <remarks>
/// <para>
/// This enum supports both metric (millimeter, centimeter, meter) and imperial
/// (inch, foot) units, along with the canonical unit (micrometer).
/// </para>
/// <para>
/// <b>Canonical representation:</b> The enum is only used for <i>input</i> to the
/// <see cref="PhysicalSize"/> value object (via <c>FromXxx</c> or <c>Create(decimal, PhysicalSizeUnit)</c>).
/// Internally, every <see cref="PhysicalSize"/> is stored as a <see cref="long"/> number 
/// of micrometers, so this enum is never used for storage or comparison.
/// </para>
/// <para>
/// <b>Precision:</b> All supported conversions are exact integers in micrometers
/// (e.g., <c>1 in = 25,400 μm</c>, <c>1 ft = 304,800 μm</c>), meaning there is no
/// precision loss when creating or converting values.
/// </para>
/// </remarks>
/// <seealso cref="PhysicalSize"/>
public enum PhysicalSizeUnit
{
    /// <summary>
    /// Millimeter (mm). 1 mm = 1,000 micrometers.
    /// </summary>
    Millimeter = 0,

    /// <summary>
    /// Centimeter (cm). 1 cm = 10,000 micrometers.
    /// </summary>
    Centimeter = 1,

    /// <summary>
    /// Meter (m). 1 m = 1,000,000 micrometers.
    /// </summary>
    Meter = 2,

    /// <summary>
    /// Inch (in). 1 in = 25,400 micrometers (exact by the international definition).
    /// </summary>
    Inch = 3,

    /// <summary>
    /// Foot (ft). 1 ft = 304,800 micrometers (exact by the international definition).
    /// </summary>
    Foot = 4,

    /// <summary>
    /// Micrometer (μm). The canonical unit used internally by <see cref="PhysicalSize"/>.
    /// </summary>
    Micrometer = 5,
}
