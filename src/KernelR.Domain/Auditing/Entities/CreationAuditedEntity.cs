using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Entities;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// A base class for entities that track their creation time and creator.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// This base class combines <see cref="BaseEntity{TId}"/> with
/// <see cref="ICreationAudited{TUserId}"/>. Use it for entities that are immutable
/// after creation (e.g., audit logs, immutable snapshots).
/// </para>
/// <para>
/// For entities that can be modified after creation, use
/// <see cref="ModificationAuditedEntity{TId, TUserId}"/> or
/// <see cref="FullAuditedEntity{TId, TUserId}"/> instead.
/// </para>
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
/// <seealso cref="ICreationAudited{TUserId}"/>
/// <seealso cref="BaseEntity{TId}"/>
public abstract class CreationAuditedEntity<TId, TUserId> : BaseEntity<TId>, ICreationAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc />
    /// <remarks>
    /// The setter is <see langword="protected"/> so that the persistence layer
    /// (e.g., an EF Core interceptor) can populate it automatically.
    /// </remarks>
    public DateTimeOffset CreatedAt { get; protected set; }

    /// <inheritdoc />
    /// <remarks>
    /// The setter is <see langword="protected"/> so that the persistence layer
    /// (e.g., an EF Core interceptor) can populate it automatically.
    /// </remarks>
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
