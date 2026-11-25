using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Auditing.Internal;
using KernelR.Domain.Entities;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// Provides a base implementation for entities that track creation
/// audit information.
/// </summary>
/// <typeparam name="TId">
/// The type of the entity identifier.
/// </typeparam>
/// <typeparam name="TUserId">
/// The type of the creator's identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="BaseEntity{TId}"/> with creation
/// auditing capabilities.
/// </para>
/// <para>
/// Creation audit information is assigned during construction and cannot
/// be changed through the public API after the entity has been created.
/// </para>
/// </remarks>
public abstract class BaseCreationAuditedEntity<TId, TUserId> :
    BaseEntity<TId>,
    ICreationAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc/>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc/>
    public TUserId CreatedBy { get; private set; }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BaseCreationAuditedEntity{TId, TUserId}"/> class
    /// using the system time provider.
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
    protected BaseCreationAuditedEntity(TId id, TUserId createdBy)
        : this(id, TimeProvider.System, createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BaseCreationAuditedEntity{TId, TUserId}"/> class
    /// using the specified time provider.
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
    protected BaseCreationAuditedEntity(TId id, TimeProvider createdAt, TUserId createdBy)
        : this(
            id,
            (createdAt ?? throw new ArgumentNullException(nameof(createdAt))).GetUtcNow(),
            createdBy)
    { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BaseCreationAuditedEntity{TId, TUserId}"/> class
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
    protected BaseCreationAuditedEntity(TId id, DateTimeOffset createdAt, TUserId createdBy)
        : base(id)
    {
        CreationAuditedValidator.Validate(createdAt, createdBy);

        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }
}
