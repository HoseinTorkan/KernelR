using KernelR.Domain.ValueObjects;
using KernelR.Primitives.Results;

namespace KernelR.Domain.StronglyTypedIds;

/// <summary>
/// Defines a contract for strongly-typed identifiers in the domain model.
/// </summary>
/// <typeparam name="TSelf">The type of the strongly-typed identifier itself (CRTP pattern).</typeparam>
/// <typeparam name="TValue">The underlying primitive type (e.g., <see cref="Guid"/>, <see cref="int"/>).</typeparam>
/// <remarks>
/// <para>
/// Strongly-typed identifiers prevent primitive obsession and eliminate the risk of 
/// accidentally passing an <c>OrderId</c> where a <c>UserId</c> is expected. 
/// </para>
/// <para>
/// By implementing <see cref="IParsable{TSelf}"/>, these IDs can be seamlessly parsed 
/// from strings (e.g., route parameters in ASP.NET Core) and converted back to strings.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public readonly record struct UserId(Guid Value) : IStronglyTypedId&lt;UserId, Guid&gt;
/// {
///     public static UserId New() => new(Guid.NewGuid());
///     public static UserId From(Guid value) => new(value);
///     public static UserId Empty => new(Guid.Empty);
///     public static Result&lt;UserId&gt; Create(Guid value) => Result&lt;UserId&gt;.Success(new(value));
///     public static UserId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s));
///     public static bool TryParse(string? s, IFormatProvider? provider, out UserId result) { ... }
/// }
/// </code>
/// </example>
public interface IStronglyTypedId<TSelf, TValue> :
    IValueObject, IParsable<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf, TValue>
    where TValue : struct, IEquatable<TValue>, IComparable<TValue>, IParsable<TValue>
{
    /// <summary>
    /// Gets the underlying primitive value of the identifier.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Creates a new instance of the strongly-typed ID, performing validation and returning a <see cref="Result{TSelf}"/>.
    /// </summary>
    /// <param name="value">The underlying primitive value to validate and wrap.</param>
    /// <returns>A successful result carrying the new ID, or a failure if the value is invalid.</returns>
    static abstract Result<TSelf> Create(TValue value);

    /// <summary>
    /// Generates a new, unique identifier (e.g., using <c>Guid.NewGuid()</c>).
    /// </summary>
    /// <returns>A new instance of the strongly-typed ID.</returns>
    static abstract TSelf New();

    /// <summary>
    /// Wraps an existing primitive value directly, bypassing validation. 
    /// Usually used by ORMs (like EF Core) when materializing entities from the database.
    /// </summary>
    /// <param name="value">The underlying primitive value.</param>
    /// <returns>A new instance of the strongly-typed ID.</returns>
    static abstract TSelf From(TValue value);

    /// <summary>
    /// Gets the empty/default instance of the strongly-typed ID (e.g., <c>Guid.Empty</c>).
    /// </summary>
    static abstract TSelf Empty { get; }
}
