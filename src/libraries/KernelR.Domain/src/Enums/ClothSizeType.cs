using KernelR.Domain.ValueObjects.Apparel;

namespace KernelR.Domain.Enums;

/// <summary>
/// Specifies the standard types for a <see cref="ClothSize"/>.
/// </summary>
/// <remarks>
/// <para>
/// This enum represents standard clothing sizes. The numeric values are explicitly
/// assigned (0 to 6) and must preserve chronological order from smallest (XS) to
/// largest (XXXL).
/// </para>
/// <para>
/// <b>Important:</b> The comparison logic in <see cref="ClothSize.CompareTo(ClothSize)"/> 
/// relies on the underlying numeric order of these values. Do not change the explicit 
/// values without reviewing the comparison logic.
/// </para>
/// <para>
/// This enum is intended specifically for clothing sizes. Physical dimensions 
/// (which are continuous) should use <c>PhysicalSize</c>, and digital sizes should 
/// use <c>DataSize</c>.
/// </para>
/// </remarks>
/// <seealso cref="ClothSize"/>
public enum ClothSizeType
{
    /// <summary>
    /// Extra Small (XS).
    /// </summary>
    ExtraSmall = 0,

    /// <summary>
    /// Small (S).
    /// </summary>
    Small = 1,

    /// <summary>
    /// Medium (M).
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Large (L).
    /// </summary>
    Large = 3,

    /// <summary>
    /// Extra Large (XL).
    /// </summary>
    ExtraLarge = 4,

    /// <summary>
    /// Double Extra Large (XXL).
    /// </summary>
    DoubleExtraLarge = 5,

    /// <summary>
    /// Triple Extra Large (XXXL).
    /// </summary>
    TripleExtraLarge = 6,
}
