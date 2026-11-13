using KernelR.Domain.Auditing;

namespace KernelR.Domain.Entities;

/// <summary>
/// A base class for aggregate roots that track creation time and creator.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// This class combines <see cref="BaseAggregateRoot{TId}"/> with <see cref="ICreationAudited{TUserId}"/>.
/// It is intended for immutable aggregate roots (e.g., audit logs of the domain).
/// For mutable aggregate roots, use <see cref="ModificationAuditedAggregateRoot{TId, TUserId}"/>
/// or <see cref="FullAuditedAggregateRoot{TId, TUserId}"/>.
/// </remarks>
/// <example>
/// <code>
/// public sealed class AuditLog : CreationAuditedAggregateRoot&lt;Guid, Guid&gt;
/// {
///     public string Action { get; private set; }
///     
///     public AuditLog(Guid id, string action) : base(id) => Action = action;
/// }
/// </code>
/// </example>
public abstract class CreationAuditedAggregateRoot<TId, TUserId> : BaseAggregateRoot<TId>, ICreationAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; protected set; }

    /// <inheritdoc />
    public TUserId CreatedBy { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationAuditedAggregateRoot{TId, TUserId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    protected CreationAuditedAggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationAuditedAggregateRoot{TId, TUserId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected CreationAuditedAggregateRoot() { }
}
