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
/// By implementing <see cref="IParsable{TSelf}"/> and <see cref="ISpanParsable{TSelf}"/>, 
/// these IDs can be seamlessly parsed from strings or character spans 
/// (e.g., route parameters in ASP.NET Core) without heap allocations.
/// </para>
/// <para>
/// <b>Implementation Note:</b> This interface is normally implemented automatically
/// by the KernelR Source Generator when a struct is decorated with the
/// <see cref="StronglyTypedIdAttribute{TValue}"/>. There is usually no need to
/// implement this interface manually.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [StronglyTypedId&lt;Guid&gt;]
/// public readonly partial record struct UserId(Guid Value);
/// </code>
/// </example>
/// <seealso cref="StronglyTypedIdAttribute{TValue}"/>
/// <seealso cref="IValueObject"/>
public interface IStronglyTypedId<TSelf, TValue> :
    IValueObject, IParsable<TSelf>, ISpanParsable<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf, TValue>
    where TValue : struct, IEquatable<TValue>, IComparable<TValue>, IParsable<TValue>, ISpanParsable<TValue>
{
    /// <summary>
    /// Gets the underlying primitive value of the identifier.
    /// </summary>
    /// <value>The raw value (e.g., a <see cref="Guid"/>).</value>
    TValue Value { get; }

    /// <summary>
    /// Creates a new instance of the strongly-typed ID, performing validation and returning a <see cref="Result{TSelf}"/>.
    /// </summary>
    /// <param name="value">The underlying primitive value to validate and wrap.</param>
    /// <returns>A successful result carrying the new ID, or a failure if the value is invalid.</returns>
    /// <remarks>
    /// This is the safest way to create an ID from an external source (like an API request),
    /// as it validates the input (e.g., rejecting <c>Guid.Empty</c>) and returns a <c>Result</c>
    /// instead of throwing an exception.
    /// </remarks>
    static abstract Result<TSelf> Create(TValue value);

    /// <summary>
    /// Generates a new, unique identifier (e.g., using <c>Guid.NewGuid()</c>).
    /// </summary>
    /// <returns>A new instance of the strongly-typed ID.</returns>
    /// <remarks>
    /// For <see cref="Guid"/>-based IDs, this usually generates a sequential GUID (v7) 
    /// to optimize database index performance, unless configured otherwise.
    /// </remarks>
    static abstract TSelf New();

    /// <summary>
    /// Wraps an existing primitive value directly, bypassing validation. 
    /// Usually used by ORMs (like EF Core) when materializing entities from the database.
    /// </summary>
    /// <param name="value">The underlying primitive value.</param>
    /// <returns>A new instance of the strongly-typed ID.</returns>
    /// <remarks>
    /// <b>Warning:</b> Do not use this method with untrusted user input, as it bypasses
    /// all validation logic. Use <see cref="Create(TValue)"/> instead.
    /// </remarks>
    static abstract TSelf From(TValue value);

    /// <summary>
    /// Gets the empty/default instance of the strongly-typed ID (e.g., <c>Guid.Empty</c>).
    /// </summary>
    /// <value>An instance representing the absence of a value.</value>
    static abstract TSelf Empty { get; }
}
