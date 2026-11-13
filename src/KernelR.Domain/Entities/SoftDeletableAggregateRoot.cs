using KernelR.Domain.Auditing;

namespace KernelR.Domain.Entities;

/// <summary>
/// A base class for aggregate roots that support soft-deletion, but do not track creation or modification.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// Use this when you only need to know <i>whether</i> the aggregate was soft-deleted and by whom,
/// but you do not need full audit trail. For full auditing, use
/// <see cref="FullAuditedAggregateRoot{TId, TUserId}"/>.
/// </remarks>
public abstract class SoftDeletableAggregateRoot<TId, TUserId> : BaseAggregateRoot<TId>, IDeletionAudited<TUserId>
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
    /// Initializes a new instance of the <see cref="SoftDeletableAggregateRoot{TId, TUserId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    protected SoftDeletableAggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoftDeletableAggregateRoot{TId, TUserId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected SoftDeletableAggregateRoot() { }
}
