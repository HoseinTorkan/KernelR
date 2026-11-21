using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Entities;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// A base class for aggregate roots that support soft-deletion, but do not track creation or modification.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// Use this when you only need to know <i>whether</i> the aggregate was soft-deleted and by whom,
/// but you do not need a full audit trail.
/// </para>
/// <para>
/// For full auditing, use <see cref="FullAuditedAggregateRoot{TId, TUserId}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Comment : SoftDeletableAggregateRoot&lt;Guid, Guid&gt;
/// {
///     public string Text { get; private set; }
///
///     public Comment(Guid id, string text) : base(id) => Text = text;
/// }
/// </code>
/// </example>
/// <seealso cref="IDeletionAudited{TUserId}"/>
/// <seealso cref="BaseAggregateRoot{TId}"/>
public abstract class SoftDeletableAggregateRoot<TId, TUserId> :
    BaseAggregateRoot<TId>,
    IDeletionAudited<TUserId>
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
