using KernelR.Domain.Abstractions;
using System.Runtime.CompilerServices;

namespace KernelR.Domain.Entities;

/// <summary>
/// A base class for all entities, providing identity and equality based on the identifier.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <remarks>
/// <para>
/// Equality is based on <see cref="Id"/> when the entity has been persisted (i.e., its
/// identifier is not the default value). Two transient entities (both with a default
/// identifier) are considered equal only if they are the same instance.
/// </para>
/// <para>
/// <b>Important:</b> This class assumes that <c>default(TId)</c> is never a valid identity.
/// This is true for <see cref="Guid"/>, but for <see cref="int"/> or <see cref="long"/>
/// identifiers, it means <c>0</c> cannot be used as a primary key. This is the standard
/// convention in DDD and is also enforced by most ORMs.
/// </para>
/// <para>
/// Derived classes do not need to override <see cref="Equals(object)"/> or
/// <see cref="GetHashCode"/>; the implementation here is based on identity and is
/// suitable for all entities.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Product : BaseEntity&lt;Guid&gt;
/// {
///     public string Name { get; private set; }
///
///     public Product(Guid id, string name) : base(id)
///     {
///         Name = name;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IEntity{TId}"/>
public abstract class BaseEntity<TId> : IEntity<TId>, IEquatable<BaseEntity<TId>>
    where TId : struct
{
    /// <inheritdoc />
    /// <remarks>
    /// The setter is <see langword="protected"/> to allow ORMs (such as EF Core) and
    /// derived classes to assign the identifier during materialization or construction.
    /// Application code should not mutate the identifier after the entity is created.
    /// </remarks>
    public TId Id { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity{TId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    protected BaseEntity(TId id) => Id = id;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity{TId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected BaseEntity() { }

    /// <inheritdoc />
    public bool Equals(BaseEntity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        // If either entity is transient (no identity yet), they are equal only by reference
        // (which is already handled by ReferenceEquals above).
        return
            !IsTransient(this) &&
            !IsTransient(other) &&
            EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BaseEntity<TId> entity && Equals(entity);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Transient entities must be hashed by reference to keep Equals/GetHashCode consistent.
        return IsTransient(this)
            ? RuntimeHelpers.GetHashCode(this)
            : HashCode.Combine(GetType(), Id);
    }

    /// <summary>
    /// Determines whether two entity instances are equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><see langword="true"/> if the entities are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right) => Equals(left, right);

    /// <summary>
    /// Determines whether two entity instances are not equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><see langword="true"/> if the entities are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right) => !Equals(left, right);

    /// <summary>
    /// Determines whether the entity has not yet been persisted (i.e., its identifier is the default value).
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns><see langword="true"/> if the entity is transient; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// A transient entity has not been assigned a real identifier, so it cannot be
    /// compared by identity. Two transient entities are considered equal only if they
    /// are the same instance.
    /// </remarks>
    private static bool IsTransient(BaseEntity<TId> entity) =>
        EqualityComparer<TId>.Default.Equals(entity.Id, default!);
}
