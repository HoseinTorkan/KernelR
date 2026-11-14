using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Entities;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// A base class for entities that support soft-deletion, but do not track creation or modification.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// Use this for entities where you only need to know <i>whether</i> they were deleted, and
/// by whom, but you do not need a full audit trail (who created or last modified them).
/// </para>
/// <para>
/// If you need full auditing, use <see cref="FullAuditedEntity{TId, TUserId}"/> instead.
/// </para>
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
/// <seealso cref="IDeletionAudited{TUserId}"/>
/// <seealso cref="BaseEntity{TId}"/>
public abstract class SoftDeletableEntity<TId, TUserId> : BaseEntity<TId>, IDeletionAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc />
    /// <remarks>
    /// The setter is <see langword="protected"/> so that the persistence layer
    /// (e.g., an EF Core interceptor) can populate it automatically.
    /// </remarks>
    public DateTimeOffset? DeletedAt { get; protected set; }

    /// <inheritdoc />
    /// <remarks>
    /// The setter is <see langword="protected"/> so that the persistence layer
    /// (e.g., an EF Core interceptor) can populate it automatically.
    /// </remarks>
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
