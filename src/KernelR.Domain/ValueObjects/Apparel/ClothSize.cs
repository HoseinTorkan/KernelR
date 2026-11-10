using KernelR.Domain.Enums;
using KernelR.Patterns;
using KernelR.Patterns.Results;
using KernelR.Patterns.Results.Extensions;
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
public readonly record struct ClothSize :
    IValueObject,
    IParsable<ClothSize>,
    IComparable<ClothSize>,
    IComparable
{
    #region ═══ Properties                                                 ═══

    /// <summary>
    /// Gets the underlying enum value representing the clothing size.
    /// </summary>
    /// <value>The <see cref="ClothSizeType"/> representing the size (e.g., Medium, Large).</value>
    public ClothSizeType Value { get; }

    #endregion ═══ Properties                                                 ═══

    #region ═══ Constructors & Factories                                   ═══

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
        if (!Enum.IsDefined<ClothSizeType>(value))
        {
            return Result<ClothSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.ClothSize.INVALID",
                    $"Cloth size type '{(int)value}' is not defined.",
                    ErrorType.ValidationError));
        }

        return Result<ClothSize>.Success(new ClothSize(value));
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
        {
            return Result<ClothSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.ClothSize.EMPTY",
                    "Cloth size cannot be empty.",
                    ErrorType.ValidationError));
        }

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

        if (type is null)
        {
            return Result<ClothSize>.Failure(
                new Error(
                    "KernelR.ValueObjects.ClothSize.INVALID",
                    "Cloth size is not recognized.",
                    ErrorType.ValidationError));
        }

        return Result<ClothSize>.Success(new ClothSize(type.Value));
    }

    #endregion ═══ Constructors & Factories                                   ═══

    #region ═══ IParsable Implementation                                   ═══

    /// <summary>
    /// Parses the specified string into a <see cref="ClothSize"/> instance.
    /// </summary>
    public static ClothSize Parse(string s, IFormatProvider? provider)
    {
        return Create(s).OrThrowFormatException();
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="ClothSize"/> instance.
    /// </summary>
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ClothSize result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        Result<ClothSize> createResult = Create(s);
        if (createResult.IsFailure)
        {
            return false;
        }

        result = createResult.Value;
        return true;
    }

    #endregion ═══ IParsable Implementation                                   ═══

    #region ═══ Comparison Operators                                       ═══

    /// <summary>Determines whether the left size is smaller than the right size (e.g., S &lt; M).</summary>
    public static bool operator <(ClothSize left, ClothSize right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>Determines whether the left size is larger than the right size (e.g., L &gt; M).</summary>
    public static bool operator >(ClothSize left, ClothSize right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>Determines whether the left size is smaller than or equal to the right size.</summary>
    public static bool operator <=(ClothSize left, ClothSize right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>Determines whether the left size is larger than or equal to the right size.</summary>
    public static bool operator >=(ClothSize left, ClothSize right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion ═══ Comparison Operators                                       ═══

    #region ═══ IComparable Implementation                                 ═══

    /// <inheritdoc />
    public int CompareTo(ClothSize other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1; // Any instance is greater than null.
        }

        if (obj is not ClothSize other)
        {
            throw new ArgumentException($"Object must be of type {nameof(ClothSize)}.", nameof(obj));
        }

        return CompareTo(other);
    }

    #endregion ═══ IComparable Implementation                                 ═══

    #region ═══ Helpers & Overrides                                        ═══

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

    #endregion ═══ Helpers & Overrides                                        ═══
}
