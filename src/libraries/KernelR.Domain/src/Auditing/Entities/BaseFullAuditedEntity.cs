using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Auditing.Internal;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// Provides a base implementation for entities that support creation, modification,
/// and soft-deletion auditing.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <typeparam name="TUserId">The type of the user identifier used for audit information.</typeparam>
/// <remarks>
/// <para>
/// This class combines creation auditing, modification auditing, and deletion auditing.
/// </para>
/// <para>
/// Deleted entities are not physically removed. Their deletion state is determined
/// by the presence of a deletion timestamp.
/// </para>
/// <para>
/// This class is intended to be inherited by entities that require full audit tracking.
/// </para>
/// </remarks>
public abstract class FullAuditedEntity<TId, TUserId> :
    BaseAuditedEntity<TId, TUserId>,
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
    /// <see cref="FullAuditedEntity{TId, TUserId}"/> class
    /// using the system time provider for creation auditing.
    /// </summary>
    /// <param name="id">
    /// The identifier of the entity.
    /// </param>
    /// <param name="createdBy">
    /// The identifier of the user who created the entity.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the creation audit information contains invalid values.
    /// </exception>
    protected FullAuditedEntity(TId id, TUserId createdBy)
        : base(id, createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="FullAuditedEntity{TId, TUserId}"/> class
    /// using the specified time provider for creation auditing.
    /// </summary>
    /// <param name="id">
    /// The identifier of the entity.
    /// </param>
    /// <param name="createdAt">
    /// The provider used to obtain the creation timestamp.
    /// </param>
    /// <param name="createdBy">
    /// The identifier of the user who created the entity.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="createdAt"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the creation audit information contains invalid values.
    /// </exception>
    protected FullAuditedEntity(TId id, TimeProvider createdAt, TUserId createdBy)
        : base(id, createdAt, createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="FullAuditedEntity{TId, TUserId}"/> class
    /// using the specified creation timestamp.
    /// </summary>
    /// <param name="id">
    /// The identifier of the entity.
    /// </param>
    /// <param name="createdAt">
    /// The timestamp at which the entity was created.
    /// </param>
    /// <param name="createdBy">
    /// The identifier of the user who created the entity.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the creation audit information contains invalid values.
    /// </exception>
    protected FullAuditedEntity(TId id, DateTimeOffset createdAt, TUserId createdBy)
        : base(id, createdAt, createdBy) { }

    /// <summary>
    /// Marks the entity as deleted using the system time.
    /// </summary>
    /// <param name="deletedBy">
    /// The identifier of the user who deleted the entity.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the deletion audit information contains invalid values.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the entity has already been deleted with different audit information.
    /// </exception>
    public virtual void MarkAsDeleted(TUserId deletedBy)
        => MarkAsDeleted(TimeProvider.System, deletedBy);

    /// <summary>
    /// Marks the entity as deleted using the specified time provider.
    /// </summary>
    /// <param name="deletedAt">
    /// The provider used to obtain the deletion timestamp.
    /// </param>
    /// <param name="deletedBy">
    /// The identifier of the user who deleted the entity.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="deletedAt"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the deletion audit information contains invalid values.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the entity has already been deleted with different audit information.
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
                "The entity has already been deleted with different audit information.");
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
