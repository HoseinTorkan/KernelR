using KernelR.Domain.Auditing;

namespace KernelR.Domain.Entities;

/// <summary>
/// A base class for entities that track creation, modification, and soft-deletion.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// This is the most comprehensive audit base class. Use it for entities that need full
/// lifecycle tracking: who created them, who modified them, and who soft-deleted them.
/// </para>
/// <para>
/// <b>Soft delete:</b> Deleting a <see cref="FullAuditedEntity{TId, TUserId}"/> does not remove
/// it from the database. Instead, it sets <see cref="DeletedAt"/> and <see cref="DeletedBy"/>,
/// and <see cref="IsDeleted"/> becomes <c>true</c>. Implementations are expected to configure
/// a global query filter (e.g., in EF Core via <c>HasQueryFilter</c>) to exclude soft-deleted
/// entities from normal queries.
/// </para>
/// <para>
/// <b>Initialization:</b> The audit properties are declared with a <c>protected set</c>
/// so that they can be initialized by the infrastructure layer (e.g., an EF Core
/// <c>SaveChanges</c> interceptor) or by derived classes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Order : FullAuditedEntity&lt;OrderId, Guid&gt;
/// {
///     public decimal Total { get; private set; }
///     
///     public Order(OrderId id, decimal total) : base(id) => Total = total;
/// }
/// </code>
/// </example>
public abstract class FullAuditedEntity<TId, TUserId> : ModificationAuditedEntity<TId, TUserId>, IDeletionAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; protected set; }

    /// <inheritdoc />
    public TUserId? DeletedBy { get; protected set; }

    /// <inheritdoc />
    public bool IsDeleted => DeletedAt is not null;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullAuditedEntity{TId, TUserId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    protected FullAuditedEntity(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullAuditedEntity{TId, TUserId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected FullAuditedEntity() { }
}
