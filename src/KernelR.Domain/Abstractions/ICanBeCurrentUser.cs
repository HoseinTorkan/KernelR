namespace KernelR.Domain.Abstractions;

/// <summary>
/// Marker interface for entities that can be loaded as the "current user"
/// by an <c>ICurrentUserService</c> (typically defined in the Application layer).
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <remarks>
/// <para>
/// This interface has no members of its own: the identifier comes from
/// <see cref="IEntity{TId}"/>, and any additional data (e.g., display name, roles)
/// is read from the concrete entity by the service that consumes it.
/// </para>
/// <para>
/// <b>Why a marker interface?</b> The Application layer needs to know whether a
/// given entity type can serve as the "current user" without taking a dependency
/// on every concrete user entity. By implementing this interface, an entity
/// opts in to that capability explicitly. The service can then use
/// <c>entity is ICanBeCurrentUser&lt;TId&gt;</c> to perform the check.
/// </para>
/// <para>
/// <b>Relationship with <see cref="IEntity{TId}"/>:</b> This interface extends
/// <see cref="IEntity{TId}"/> so that the identifier is available without an
/// additional cast. Implementations are expected to be entities that already
/// implement <see cref="IEntity{TId}"/> (for example, a <c>User</c> entity).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class User : FullAuditedAggregateRoot&lt;UserId, Guid&gt;, ICanBeCurrentUser&lt;UserId&gt;
/// {
///     public string DisplayName { get; private set; }
///     // ...
/// }
/// </code>
/// </example>
/// <seealso cref="IEntity{TId}"/>
public interface ICanBeCurrentUser<TId> : IEntity<TId>
    where TId : struct;
