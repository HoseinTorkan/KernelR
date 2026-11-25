
using KernelR.Domain.Aggregates;
using KernelR.Domain.Auditing.Abstractions;
using KernelR.Domain.Auditing.Internal;

namespace KernelR.Domain.Auditing.Aggregates;

/// <summary>
/// Provides a base implementation for aggregate roots that track creation
/// audit information.
/// </summary>
/// <typeparam name="TId">
/// The type of the aggregate root identifier.
/// </typeparam>
/// <typeparam name="TUserId">
/// The type of the creator's identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="BaseAggregateRoot{TId}"/> with creation
/// auditing capabilities.
/// </para>
/// <para>
/// Creation audit information is assigned during construction and cannot
/// be changed through the public API after the aggregate root has been created.
/// </para>
/// <para>
/// By default, the creation timestamp is obtained from
/// <see cref="TimeProvider.System"/>. An alternative time provider or an
/// explicit timestamp can be supplied when needed.
/// </para>
/// </remarks>
public abstract class BaseCreationAuditedAggregateRoot<TId, TUserId> :
    BaseAggregateRoot<TId>,
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
    /// <see cref="BaseCreationAuditedAggregateRoot{TId, TUserId}"/> class
    /// using the system time provider.
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
    protected BaseCreationAuditedAggregateRoot(TId id, TUserId createdBy)
        : this(id, TimeProvider.System, createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BaseCreationAuditedAggregateRoot{TId, TUserId}"/> class
    /// using the specified time provider.
    /// </summary>
    /// <param name="id">
    /// The identifier of the aggregate root.
    /// </param>
    /// <param name="createdBy">
    /// The identifier of the user who created the aggregate root.
    /// </param>
    /// <param name="timeProvider">
    /// The provider used to obtain the creation timestamp.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="timeProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the creation audit information contains invalid values.
    /// </exception>
    protected BaseCreationAuditedAggregateRoot(TId id, TimeProvider createdAt, TUserId createdBy)
        : this(id, (createdAt ?? throw new ArgumentNullException(nameof(createdAt))).GetUtcNow(), createdBy) { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BaseCreationAuditedAggregateRoot{TId, TUserId}"/> class
    /// using an explicitly specified creation timestamp.
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
    protected BaseCreationAuditedAggregateRoot(TId id, DateTimeOffset createdAt, TUserId createdBy)
        : base(id)
    {
        CreationAuditedValidator.Validate(createdAt, createdBy);

        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }
}
