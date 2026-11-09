namespace KernelR.Domain.Abstractions;

/// <summary>
/// A base class for all entities, providing the identity property.
/// </summary>
public abstract class BaseEntity<TId> : IEntity<TId>
    where TId : struct
{
    /// <inheritdoc />
    public TId Id { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity{TId}"/> class.
    /// </summary>
    protected BaseEntity(TId id) => Id = id;

    /// <summary>
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected BaseEntity() { }
}
