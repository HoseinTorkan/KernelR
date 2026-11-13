namespace KernelR.Domain.Abstractions;

/// <summary>
/// A base class for all entities, providing identity and equality based on the identifier.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class BaseEntity<TId> : IEntity<TId>, IEquatable<BaseEntity<TId>>
    where TId : struct
{
    /// <inheritdoc />
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

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BaseEntity<TId> entity && Equals(entity);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

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
}
