using KernelR.Domain.ValueObjects.Measurement;

namespace KernelR.Domain.Enums;

/// <summary>
/// Specifies the unit of measurement for a <see cref="PhysicalSize"/>.
/// </summary>
/// <remarks>
/// Values are grouped and spaced to allow future additions without renumbering.
/// The canonical unit (used internally by <see cref="PhysicalSize"/>) is
/// <see cref="Micrometer"/>.
/// </remarks>
public enum PhysicalSizeUnit
{
    /// <summary>Micrometer (μm). The canonical unit used internally.</summary>
    Micrometer = 0,

    /// <summary>Millimeter (mm). 1 mm = 1,000 μm.</summary>
    Millimeter = 1,

    /// <summary>Centimeter (cm). 1 cm = 10,000 μm.</summary>
    Centimeter = 2,

    /// <summary>Meter (m). 1 m = 1,000,000 μm.</summary>
    Meter = 3,

    /// <summary>Inch (in). 1 in = 25,400 μm (exact).</summary>
    Inch = 4,

    /// <summary>Foot (ft). 1 ft = 304,800 μm (exact).</summary>
    Foot = 5,
}
