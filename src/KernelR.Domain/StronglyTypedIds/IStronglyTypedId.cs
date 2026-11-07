using KernelR.Domain.Entities;
using KernelR.Patterns.Results;

namespace KernelR.Domain.StronglyTypedIds;

/// <summary>
/// Defines the full contract for strongly-typed identifiers, including
/// validation, factory methods, and parsing.
/// </summary>
/// <typeparam name="TSelf">
/// The self-referencing type of the identifier itself (for example,
/// <c>ProductId</c>, <c>OrderId</c>).
/// </typeparam>
/// <typeparam name="TValue">
/// The underlying primitive type (for example <see cref="Guid"/>, <see cref="long"/>).
/// </typeparam>
/// <remarks>
/// <para>
/// A strongly-typed identifier wraps a primitive value into a dedicated type.
/// This provides compile-time type safety.
/// </para>
/// <para>
/// <b>Validation is centralized in <see cref="Create(TValue)"/>.</b> Every
/// other factory (<see cref="New"/>, <see cref="From"/>, <see cref="Parse"/>,
/// <see cref="TryParse"/>) delegates to <see cref="Create(TValue)"/> and turns
/// a failed <see cref="Result{T}"/> into an exception. This keeps the
/// validation rules in one place and lets callers choose between
/// <see cref="Result{T}"/> (for expected failures) and exceptions (for
/// programming errors or API-boundary translation).
/// </para>
/// <para>
/// <b>Recommended implementation:</b> Use a <c>readonly record struct</c> that
/// implements this interface. A <c>record struct</c> automatically provides
/// value equality, a meaningful <see cref="object.ToString"/> override,
/// deconstruction, and <see cref="IComparable{T}"/>:
/// </para>
/// <code>
/// public readonly record struct ProductId(Guid Value) : IStronglyTypedId&lt;ProductId, Guid&gt;
/// {
///     public static ProductId New() =&gt; new(Guid.NewGuid());
///     public static ProductId From(Guid value) =&gt; new(value);
///     public static ProductId Empty =&gt; new(Guid.Empty);
///
///     public static Result&lt;ProductId&gt; Create(Guid value)
///     {
///         if (value == Guid.Empty)
///             return Result&lt;ProductId&gt;.Failure(
///                 new Error("KernelR.ValueObjects.ProductId.EMPTY", "ProductId cannot be empty.", ErrorType.ValidationError));
///         return Result&lt;ProductId&gt;.Success(new ProductId(value));
///     }
/// }
/// </code>
/// </remarks>
/// <seealso cref="IEntity{TId}"/>
public interface IStronglyTypedId<TSelf, TValue> : IParsable<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf, TValue>
    where TValue : struct, IEquatable<TValue>, IComparable<TValue>
{
    /// <summary>
    /// Gets the underlying primitive value of the strongly-typed identifier.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Validates <paramref name="value"/> and creates a new identifier, or
    /// returns a failure with the domain <see cref="Error"/>.
    /// </summary>
    /// <param name="value">The underlying value.</param>
    /// <returns>
    /// A successful result carrying the new identifier, or a failure carrying
    /// the domain error.
    /// </returns>
    /// <remarks>
    /// This is the <b>only</b> method that performs validation. All other
    /// factories delegate to it.
    /// </remarks>
    static abstract Result<TSelf> Create(TValue value);

    /// <summary>
    /// Creates a new identifier with a freshly generated unique value.
    /// </summary>
    /// <exception cref="FormatException">
    /// The generated value is invalid (which indicates a bug in the generator,
    /// not a caller error).
    /// </exception>
    static abstract TSelf New();

    /// <summary>
    /// Creates an identifier from an existing underlying value.
    /// </summary>
    /// <param name="value">The underlying value.</param>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> is invalid.
    /// </exception>
    static abstract TSelf From(TValue value);

    /// <summary>
    /// Gets the empty identifier (all zeros or equivalent).
    /// </summary>
    static abstract TSelf Empty { get; }
}
