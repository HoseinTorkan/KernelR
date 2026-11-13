using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Entities;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// A base class for entities that track their creation time and creator.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// This base class combines <see cref="BaseEntity{TId}"/> with <see cref="ICreationAudited{TUserId}"/>.
/// Use this for entities that are immutable after creation (e.g., audit logs, snapshots).
/// For entities that can be modified, use <see cref="ModificationAuditedEntity{TId, TUserId}"/>
/// or <see cref="FullAuditedEntity{TId, TUserId}"/> instead.
/// </remarks>
/// <example>
/// <code>
/// public sealed class AuditLog : CreationAuditedEntity&lt;Guid, Guid&gt;
/// {
///     public string Action { get; private set; }
///     
///     public AuditLog(Guid id, string action) : base(id) => Action = action;
/// }
/// </code>
/// </example>
public abstract class CreationAuditedEntity<TId, TUserId> :
    BaseEntity<TId>,
    ICreationAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; protected set; }

    /// <inheritdoc />
    public TUserId CreatedBy { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationAuditedEntity{TId, TUserId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    protected CreationAuditedEntity(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationAuditedEntity{TId, TUserId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected CreationAuditedEntity() { }
}
