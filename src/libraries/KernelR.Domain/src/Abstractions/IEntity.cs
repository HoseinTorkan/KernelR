namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines a contract for an entity that has an identifier.
/// </summary>
/// <typeparam name="TId">
/// The type of the entity identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// An entity is an object that is primarily identified by its identity
/// rather than by the values of its attributes.
/// </para>
/// <para>
/// The identifier uniquely distinguishes the entity within its identity scope
/// and remains associated with the entity throughout its lifetime.
/// </para>
/// </remarks>
public interface IEntity<TId>
    where TId : struct
{
    /// <summary>
    /// Gets the identifier of the entity.
    /// </summary>
    TId Id { get; }
}
