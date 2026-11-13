using KernelR.Domain.Auditing.Abstractions;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// A base class for entities that track creation, modification time, and the modifying user.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// This base class extends <see cref="CreationAuditedEntity{TId, TUserId}"/> with modification tracking.
/// Use this for entities that can be updated, but not soft-deleted.
/// For entities that also support soft-delete, use <see cref="FullAuditedEntity{TId, TUserId}"/>.
/// </remarks>
/// <example>
/// <code>
/// public sealed class BlogPost : ModificationAuditedEntity&lt;Guid, Guid&gt;
/// {
///     public string Title { get; private set; }
///     
///     public BlogPost(Guid id, string title) : base(id) => Title = title;
/// }
/// </code>
/// </example>
public abstract class ModificationAuditedEntity<TId, TUserId> : CreationAuditedEntity<TId, TUserId>, IModificationAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; protected set; }

    /// <inheritdoc />
    public TUserId? UpdatedBy { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModificationAuditedEntity{TId, TUserId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    protected ModificationAuditedEntity(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModificationAuditedEntity{TId, TUserId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected ModificationAuditedEntity() { }
}
