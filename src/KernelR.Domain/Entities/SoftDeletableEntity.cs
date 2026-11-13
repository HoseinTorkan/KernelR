using KernelR.Domain.Auditing;

namespace KernelR.Domain.Entities;

/// <summary>
/// A base class for entities that support soft-deletion, but do not track creation or modification.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// Use this for entities where you only need to know <i>whether</i> they were deleted, and
/// by whom, but you do not need full audit trail (who created or last modified them).
/// If you need full auditing, use <see cref="FullAuditedEntity{TId, TUserId}"/> instead.
/// </remarks>
/// <example>
/// <code>
/// public sealed class Comment : SoftDeletableEntity&lt;Guid, Guid&gt;
/// {
///     public string Text { get; private set; }
///     
///     public Comment(Guid id, string text) : base(id) => Text = text;
/// }
/// </code>
/// </example>
public abstract class SoftDeletableEntity<TId, TUserId> : BaseEntity<TId>, IDeletionAudited<TUserId>
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
    /// Initializes a new instance of the <see cref="SoftDeletableEntity{TId, TUserId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    protected SoftDeletableEntity(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoftDeletableEntity{TId, TUserId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected SoftDeletableEntity() { }
}
