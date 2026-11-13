using KernelR.Domain.Auditing.Abstractions;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// A base class for aggregate roots that track creation, modification, and soft-deletion.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// This is the most comprehensive audit base class for aggregate roots. Use it when the
/// aggregate needs full lifecycle tracking: who created it, who modified it, and who
/// soft-deleted it.
/// </para>
/// <para>
/// <b>Soft delete:</b> Deleting a <see cref="FullAuditedAggregateRoot{TId, TUserId}"/> does not
/// remove it from the database. Instead, it sets <see cref="DeletedAt"/> and <see cref="DeletedBy"/>,
/// and <see cref="IsDeleted"/> becomes <c>true</c>. A global query filter (e.g., via EF Core's
/// <c>HasQueryFilter</c>) is expected to exclude soft-deleted aggregate roots from normal queries.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Order : FullAuditedAggregateRoot&lt;OrderId, Guid&gt;
/// {
///     public decimal Total { get; private set; }
///     
///     public Order(OrderId id, decimal total) : base(id) => Total = total;
/// }
/// </code>
/// </example>
public abstract class FullAuditedAggregateRoot<TId, TUserId> : ModificationAuditedAggregateRoot<TId, TUserId>, IDeletionAudited<TUserId>
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
    /// Initializes a new instance of the <see cref="FullAuditedAggregateRoot{TId, TUserId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    protected FullAuditedAggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullAuditedAggregateRoot{TId, TUserId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected FullAuditedAggregateRoot() { }
}
