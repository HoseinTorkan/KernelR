using KernelR.Domain.ValueObjects.Measurement;

namespace KernelR.Domain.Enums;

/// <summary>
/// Specifies the unit of measurement for a <see cref="PhysicalWeight"/>.
/// </summary>
public enum PhysicalWeightUnit
{
    /// <summary>
    /// Milligram (mg).
    /// </summary>
    Milligram = 0,

    /// <summary>
    /// Gram (g).
    /// </summary>
    Gram = 1,

    /// <summary>
    /// Kilogram (kg).
    /// </summary>
    Kilogram = 2,

    /// <summary>
    /// Ton (t).
    /// </summary>
    Ton = 3,

    /// <summary>
    /// Pound (lb).
    /// </summary>
    Pound = 4,

    /// <summary>
    /// Ounce (oz).
    /// </summary>
    Ounce = 5
}
