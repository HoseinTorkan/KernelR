using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Auditing.Internal;

namespace KernelR.Domain.Auditing.Aggregates;

/// <summary>
/// Provides a base implementation for aggregate roots that track creation,
/// modification, and deletion audit information.
/// </summary>
/// <typeparam name="TId">
/// The type of the aggregate root identifier.
/// </typeparam>
/// <typeparam name="TUserId">
/// The type of the user identifiers used for auditing.
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="BaseAuditedAggregateRoot{TId, TUserId}"/>
/// with deletion auditing capabilities.
/// </para>
/// <para>
/// Deletion is represented by deletion audit information rather than physical
/// removal of the aggregate root. The aggregate root can be restored by clearing
/// its deletion audit information.
/// </para>
/// <para>
/// The deletion state is determined by whether <see cref="DeletedAt"/>
/// contains a value.
/// </para>
/// </remarks>
public abstract class FullAuditedAggregateRoot<TId, TUserId> :
    BaseAuditedAggregateRoot<TId, TUserId>,
    IDeletionAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc/>
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <inheritdoc/>
    public TUserId? DeletedBy { get; private set; }

    /// <inheritdoc/>
    public bool IsDeleted => DeletedAt is not null;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="FullAuditedAggregateRoot{TId, TUserId}"/> class
    /// using the system time provider for creation auditing.
    /// </summary>
    /// <param name="id">
    /// The identifier of the aggregate root.
    /// </param>
    /// <param name="createdBy">
    /// The identifier of the user who created the aggregate root.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the creation audit information contains invalid values.
    /// </exception>
    protected FullAuditedAggregateRoot(TId id, TUserId createdBy)
        : base(id, createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="FullAuditedAggregateRoot{TId, TUserId}"/> class
    /// using the specified time provider for creation auditing.
    /// </summary>
    /// <param name="id">
    /// The identifier of the aggregate root.
    /// </param>
    /// <param name="createdAt">
    /// The provider used to obtain the creation timestamp.
    /// </param>
    /// <param name="createdBy">
    /// The identifier of the user who created the aggregate root.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="createdAt"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the creation audit information contains invalid values.
    /// </exception>
    protected FullAuditedAggregateRoot(TId id, TimeProvider createdAt, TUserId createdBy)
        : base(id, createdAt, createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="FullAuditedAggregateRoot{TId, TUserId}"/> class
    /// using the specified creation timestamp.
    /// </summary>
    /// <param name="id">
    /// The identifier of the aggregate root.
    /// </param>
    /// <param name="createdAt">
    /// The timestamp at which the aggregate root was created.
    /// </param>
    /// <param name="createdBy">
    /// The identifier of the user who created the aggregate root.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the creation audit information contains invalid values.
    /// </exception>
    protected FullAuditedAggregateRoot(TId id, DateTimeOffset createdAt, TUserId createdBy)
        : base(id, createdAt, createdBy) { }

    /// <summary>
    /// Marks the aggregate root as deleted using the system time.
    /// </summary>
    /// <param name="deletedBy">
    /// The identifier of the user who deleted the aggregate root.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the deletion audit information contains invalid values.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the aggregate root has already been deleted with different audit information.
    /// </exception>
    public virtual void MarkAsDeleted(TUserId deletedBy)
        => MarkAsDeleted(TimeProvider.System, deletedBy);

    /// <summary>
    /// Marks the aggregate root as deleted using the specified time provider.
    /// </summary>
    /// <param name="deletedAt">
    /// The provider used to obtain the deletion timestamp.
    /// </param>
    /// <param name="deletedBy">
    /// The identifier of the user who deleted the aggregate root.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="deletedAt"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the deletion audit information contains invalid values.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the aggregate root has already been deleted with different audit information.
    /// </exception>
    public virtual void MarkAsDeleted(TimeProvider deletedAt, TUserId deletedBy)
        => MarkAsDeleted(
            (deletedAt ?? throw new ArgumentNullException(nameof(deletedAt))
            ).GetUtcNow(), deletedBy);

    /// <inheritdoc/>
    public virtual void MarkAsDeleted(DateTimeOffset deletedAt, TUserId deletedBy)
    {
        FullAuditedValidator.Validate(deletedAt, deletedBy);

        if (IsDeleted)
        {
            if (DeletedAt == deletedAt &&
                EqualityComparer<TUserId?>.Default.Equals(DeletedBy, deletedBy))
                return;

            throw new InvalidOperationException(
                "The aggregate has already been deleted with different audit information.");
        }

        DeletedAt = deletedAt;
        DeletedBy = deletedBy;
    }

    /// <inheritdoc/>
    public virtual void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
    }
}
