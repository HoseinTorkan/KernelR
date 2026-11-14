using KernelR.Domain.ValueObjects.Measurement;

namespace KernelR.Domain.Enums;

/// <summary>
/// Specifies the unit of measurement for a <see cref="PhysicalWeight"/>.
/// </summary>
/// <remarks>
/// <para>
/// This enum supports both metric (milligram, gram, kilogram, ton) and imperial
/// (pound, ounce) units, along with the canonical unit (microgram).
/// </para>
/// <para>
/// <b>Canonical representation:</b> The enum is only used for <i>input</i> to the
/// <see cref="PhysicalWeight"/> value object (via <c>FromXxx</c> or <c>Create(decimal, PhysicalWeightUnit)</c>).
/// Internally, every <see cref="PhysicalWeight"/> is stored as a <see cref="long"/> number 
/// of micrograms, so this enum is never used for storage or comparison.
/// </para>
/// <para>
/// <b>Precision Note:</b> The pound is exact in micrograms (<c>1 lb = 453,592,370 μg</c>).
/// The ounce has a fractional part (<c>1 oz = 28,349,523.125 μg</c>) and is rounded to 
/// <c>28,349,523 μg</c>. The resulting error (0.125 μg per ounce) is smaller than 1/800 
/// of a grain of salt and is negligible for all practical applications.
/// </para>
/// </remarks>
/// <seealso cref="PhysicalWeight"/>
public enum PhysicalWeightUnit
{
    /// <summary>
    /// Milligram (mg). 1 mg = 1,000 micrograms.
    /// </summary>
    Milligram = 0,

    /// <summary>
    /// Gram (g). 1 g = 1,000,000 micrograms.
    /// </summary>
    Gram = 1,

    /// <summary>
    /// Kilogram (kg). 1 kg = 1,000,000,000 micrograms.
    /// </summary>
    Kilogram = 2,

    /// <summary>
    /// Metric ton (t). 1 t = 1,000,000,000,000 micrograms.
    /// </summary>
    Ton = 3,

    /// <summary>
    /// Pound (lb). 1 lb = 453,592,370 micrograms (exact by the international avoirdupois definition).
    /// </summary>
    Pound = 4,

    /// <summary>
    /// Ounce (oz). 1 oz = 28,349,523 micrograms (rounded from 28,349,523.125).
    /// </summary>
    /// <remarks>
    /// See the precision note on the enum itself for the negligible impact of this rounding.
    /// </remarks>
    Ounce = 5,

    /// <summary>
    /// Microgram (μg). The canonical unit used internally by <see cref="PhysicalWeight"/>.
    /// </summary>
    Microgram = 6,
}
