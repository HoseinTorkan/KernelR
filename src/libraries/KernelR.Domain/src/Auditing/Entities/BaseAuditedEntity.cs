using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Auditing.Internal;

namespace KernelR.Domain.Auditing.Entities;

/// <summary>
/// Provides a base implementation for entities that track creation
/// and modification audit information.
/// </summary>
/// <typeparam name="TId">
/// The type of the entity identifier.
/// </typeparam>
/// <typeparam name="TUserId">
/// The type of the user identifiers used for auditing.
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="BaseCreationAuditedEntity{TId, TUserId}"/>
/// with modification auditing capabilities.
/// </para>
/// <para>
/// Modification audit information is initially unset and can be updated through
/// the provided methods.
/// </para>
/// </remarks>
public abstract class BaseAuditedEntity<TId, TUserId> :
    BaseCreationAuditedEntity<TId, TUserId>,
    IModificationAudited<TUserId>
    where TId : struct
    where TUserId : struct
{
    /// <inheritdoc/>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <inheritdoc/>
    public TUserId? UpdatedBy { get; private set; }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BaseAuditedEntity{TId, TUserId}"/> class
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
    protected BaseAuditedEntity(TId id, TUserId createdBy)
        : base(id, createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BaseAuditedEntity{TId, TUserId}"/> class
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
    protected BaseAuditedEntity(TId id, TimeProvider createdAt, TUserId createdBy)
        : base(id, createdAt, createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BaseAuditedEntity{TId, TUserId}"/> class
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
    protected BaseAuditedEntity(TId id, DateTimeOffset createdAt, TUserId createdBy)
        : base(id, createdAt, createdBy) { }

    /// <summary>
    /// Updates the modification audit information using the system time.
    /// </summary>
    /// <param name="updatedBy">
    /// The identifier of the user who modified the entity.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the modification audit information contains invalid values.
    /// </exception>
    public virtual void UpdateModificationAudit(TUserId updatedBy)
        => UpdateModificationAudit(TimeProvider.System, updatedBy);

    /// <summary>
    /// Updates the modification audit information using the specified time provider.
    /// </summary>
    /// <param name="updatedAt">
    /// The provider used to obtain the modification timestamp.
    /// </param>
    /// <param name="updatedBy">
    /// The identifier of the user who modified the entity.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="updatedAt"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the modification audit information contains invalid values.
    /// </exception>
    public virtual void UpdateModificationAudit(TimeProvider updatedAt, TUserId updatedBy)
        => UpdateModificationAudit(
            (updatedAt ?? throw new ArgumentNullException(nameof(updatedAt))
            ).GetUtcNow(), updatedBy);

    /// <inheritdoc/>
    public virtual void UpdateModificationAudit(DateTimeOffset updatedAt, TUserId updatedBy)
    {
        ModificationAuditedValidator.Validate(updatedAt, updatedBy);

        if (UpdatedAt == updatedAt &&
            EqualityComparer<TUserId?>.Default.Equals(UpdatedBy, updatedBy))
            return;

        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;
    }
}
