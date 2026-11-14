using KernelR.Domain.Enums;
using KernelR.Primitives.Results;
using KernelR.Primitives.Results.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace KernelR.Domain.ValueObjects.Apparel;

/// <summary>
/// Represents a standard clothing size (e.g., S, M, L, XL) as an immutable value object.
/// </summary>
/// <remarks>
/// <para>
/// This value object is intended specifically for <b>clothing sizes</b>. Unlike physical 
/// dimensions (which are continuous) or digital sizes, clothing sizes are categorical 
/// and standardized.
/// </para>
/// <para>
/// The values are ordered sequentially (XS &lt; S &lt; M &lt; L &lt; XL &lt; XXL &lt; XXXL), 
/// making this type safe for sorting and comparison using <see cref="IComparable{T}"/>.
/// <b>Important:</b> This relies on the underlying <see cref="ClothSizeType"/> enum being 
/// defined in the correct chronological order.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;ClothSize&gt; result = ClothSize.Create("XL");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value); // Outputs: XL
/// }
/// </code>
/// </example>
/// <seealso cref="ClothSizeType"/>
/// <seealso cref="IValueObject"/>
public readonly record struct ClothSize : IValueObject, IParsable<ClothSize>, IComparable<ClothSize>, IComparable
{
    /// <summary>
    /// Gets the underlying enum value representing the clothing size.
    /// </summary>
    /// <value>The <see cref="ClothSizeType"/> representing the size (e.g., Medium, Large).</value>
    public ClothSizeType Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClothSize"/> struct.
    /// </summary>
    /// <param name="value">The validated clothing size type.</param>
    private ClothSize(ClothSizeType value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="ClothSize"/> from a defined <see cref="ClothSizeType"/>.
    /// </summary>
    /// <param name="value">The clothing size type. Must be a defined enum value.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="ClothSize"/>, or a failure with 
    /// <c>KernelR.ValueObjects.ClothSize.INVALID</c> if the enum value is undefined.
    /// </returns>
    public static Result<ClothSize> Create(ClothSizeType value)
    {
        return !Enum.IsDefined(value)
            ? new Error(
                "KernelR.ValueObjects.ClothSize.INVALID",
                $"Cloth size type '{(int)value}' is not defined.",
                ErrorType.ValidationError)
            : new ClothSize(value);
    }

    /// <summary>
    /// Creates a new <see cref="ClothSize"/> from a string abbreviation (e.g., "S", "M", "XL", "XXL").
    /// </summary>
    /// <param name="value">The raw clothing size string. Case-insensitive.</param>
    /// <returns>
    /// A successful result carrying the new <see cref="ClothSize"/>, or a failure with one of the following error codes:
    /// <list type="bullet">
    ///   <item><description><c>KernelR.ValueObjects.ClothSize.EMPTY</c> when the input is null or whitespace.</description></item>
    ///   <item><description><c>KernelR.ValueObjects.ClothSize.INVALID</c> when the input is not a recognized clothing size.</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// ClothSize.Create("M");   // → Success (Medium)
    /// ClothSize.Create("xl");  // → Success (ExtraLarge)
    /// ClothSize.Create("2XL"); // → Success (DoubleExtraLarge)
    /// </code>
    /// </example>
    public static Result<ClothSize> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(
                "KernelR.ValueObjects.ClothSize.EMPTY",
                "Cloth size cannot be empty.",
                ErrorType.ValidationError);

        string normalized = value.Trim().ToUpperInvariant();

        ClothSizeType? type = normalized switch
        {
            "XS" or "EXTRASMALL" => ClothSizeType.ExtraSmall,
            "S" or "SMALL" => ClothSizeType.Small,
            "M" or "MEDIUM" => ClothSizeType.Medium,
            "L" or "LARGE" => ClothSizeType.Large,
            "XL" or "EXTRALARGE" => ClothSizeType.ExtraLarge,
            "XXL" or "2XL" or "DOUBLEEXTRALARGE" => ClothSizeType.DoubleExtraLarge,
            "XXXL" or "3XL" or "TRIPLEEXTRALARGE" => ClothSizeType.TripleExtraLarge,
            _ => null
        };

        return type is null
            ? new Error(
                "KernelR.ValueObjects.ClothSize.INVALID",
                "Cloth size is not recognized.",
                ErrorType.ValidationError)
            : new ClothSize(type.Value);
    }

    /// <inheritdoc />
    public static ClothSize Parse(string s, IFormatProvider? provider) => Create(s).OrThrowFormatException();

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ClothSize result)
        => Create(s ?? string.Empty).TryGetValue(out result);

    /// <summary>Determines whether the left size is smaller than the right size (e.g., S &lt; M).</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is smaller; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(ClothSize left, ClothSize right) => left.CompareTo(right) < 0;

    /// <summary>Determines whether the left size is larger than the right size (e.g., L &gt; M).</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is larger; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(ClothSize left, ClothSize right) => left.CompareTo(right) > 0;

    /// <summary>Determines whether the left size is smaller than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is smaller or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(ClothSize left, ClothSize right) => left.CompareTo(right) <= 0;

    /// <summary>Determines whether the left size is larger than or equal to the right size.</summary>
    /// <param name="left">The left size.</param>
    /// <param name="right">The right size.</param>
    /// <returns><see langword="true"/> if left is larger or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(ClothSize left, ClothSize right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    public int CompareTo(ClothSize other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            ClothSize other => CompareTo(other),
            _ => throw new ArgumentException($"Object must be of type {nameof(ClothSize)}.", nameof(obj))
        };
    }

    /// <summary>
    /// Returns the standard abbreviation for the clothing size.
    /// </summary>
    /// <returns>The standard abbreviation (e.g., "S", "M", "XL").</returns>
    public override string ToString() => Value switch
    {
        ClothSizeType.ExtraSmall => "XS",
        ClothSizeType.Small => "S",
        ClothSizeType.Medium => "M",
        ClothSizeType.Large => "L",
        ClothSizeType.ExtraLarge => "XL",
        ClothSizeType.DoubleExtraLarge => "XXL",
        ClothSizeType.TripleExtraLarge => "XXXL",
        _ => throw new InvalidOperationException($"Unsupported cloth size type: {Value}")
    };

    /// <summary>
    /// Explicitly converts a <see cref="ClothSize"/> to its string abbreviation.
    /// </summary>
    /// <param name="size">The clothing size instance to convert.</param>
    /// <returns>The standard abbreviation of the clothing size.</returns>
    public static explicit operator string(ClothSize size) => size.ToString();
}
