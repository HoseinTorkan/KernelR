using KernelR.Domain.Auditing.Abstractions;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// A base class for aggregate roots that track creation and modification.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="CreationAuditedAggregateRoot{TId, TUserId}"/> with
/// modification tracking. Use it for mutable aggregate roots that do not need soft-deletion.
/// </para>
/// <para>
/// For soft-deletion support, use <see cref="FullAuditedAggregateRoot{TId, TUserId}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class BlogPost : ModificationAuditedAggregateRoot&lt;Guid, Guid&gt;
/// {
///     public string Title { get; private set; }
///
///     public BlogPost(Guid id, string title) : base(id) => Title = title;
/// }
/// </code>
/// </example>
/// <seealso cref="IModificationAudited{TUserId}"/>
/// <seealso cref="CreationAuditedAggregateRoot{TId, TUserId}"/>
public abstract class ModificationAuditedAggregateRoot<TId, TUserId> : CreationAuditedAggregateRoot<TId, TUserId>, IModificationAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc />
    /// <remarks>
    /// The setter is <see langword="protected"/> so that the persistence layer
    /// (e.g., an EF Core interceptor) can populate it automatically.
    /// </remarks>
    public DateTimeOffset? UpdatedAt { get; protected set; }

    /// <inheritdoc />
    /// <remarks>
    /// The setter is <see langword="protected"/> so that the persistence layer
    /// (e.g., an EF Core interceptor) can populate it automatically.
    /// </remarks>
    public TUserId? UpdatedBy { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModificationAuditedAggregateRoot{TId, TUserId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    protected ModificationAuditedAggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModificationAuditedAggregateRoot{TId, TUserId}"/> class.
    /// Required for ORM (e.g., EF Core) to materialize objects from the database.
    /// </summary>
    protected ModificationAuditedAggregateRoot() { }
}
