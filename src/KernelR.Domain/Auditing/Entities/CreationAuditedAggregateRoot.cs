using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Entities;

namespace KernelR.Domain.Auditing.Entities;


public abstract class CreationAuditedAggregateRoot<TId, TUserId> :
    BaseAggregateRoot<TId>,
    ICreationAudited<TUserId>
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
