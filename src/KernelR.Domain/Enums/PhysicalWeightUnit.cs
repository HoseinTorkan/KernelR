using KernelR.Domain.ValueObjects.Measurement;

namespace KernelR.Domain.Enums;

/// <summary>
/// Specifies the unit of measurement for a <see cref="PhysicalWeight"/>.
/// </summary>
/// <remarks>
/// Values are grouped and spaced to allow future additions without renumbering.
/// The canonical unit (used internally by <see cref="PhysicalWeight"/>) is
/// <see cref="Microgram"/>.
/// </remarks>
public enum PhysicalWeightUnit
{
    /// <summary>Microgram (μg). The canonical unit used internally.</summary>
    Microgram = 0,

    /// <summary>Milligram (mg). 1 mg = 1,000 μg.</summary>
    Milligram = 1,

    /// <summary>Gram (g). 1 g = 1,000,000 μg.</summary>
    Gram = 2,

    /// <summary>Kilogram (kg). 1 kg = 1,000,000,000 μg.</summary>
    Kilogram = 3,

    /// <summary>Ton (t). 1 t = 1,000,000,000,000 μg.</summary>
    Ton = 4,

    /// <summary>Pound (lb). 1 lb = 453,592,370 μg (exact).</summary>
    Pound = 5,

    /// <summary>Ounce (oz). 1 oz = 28,349,523 μg (rounded from 28,349,523.125).</summary>
    Ounce = 6,
}
