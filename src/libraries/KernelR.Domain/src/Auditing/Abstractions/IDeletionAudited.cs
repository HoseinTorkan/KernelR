namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for types that track deletion time, deleter identity,
/// and deletion status.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the deleter's identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface combines <see cref="IHasDeletionTime"/> and
/// <see cref="IHasDeleter{TUserId}"/> to provide a single contract
/// for deletion auditing.
/// </para>
/// <para>
/// Implementations expose the deletion timestamp, the identifier of the user
/// who deleted the entity, and whether the entity has been deleted.
/// </para>
/// <para>
/// The deletion status is represented by <see cref="IsDeleted"/> and may be
/// used to distinguish deleted entities from active ones.
/// </para>
/// <para>
/// This interface also defines operations for marking an entity as deleted
/// and restoring it.
/// </para>
/// </remarks>
public interface IDeletionAudited<TUserId> :
    IHasDeletionTime,
    IHasDeleter<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets a value indicating whether the entity has been deleted.
    /// </summary>
    /// <remarks>
    /// This property indicates the deletion state of the entity.
    /// It does not define how deleted entities are filtered.
    /// </remarks>
    bool IsDeleted { get; }

    /// <summary>
    /// Marks the entity as deleted and updates its deletion audit information.
    /// </summary>
    /// <param name="deletedAt">
    /// The timestamp at which the entity was deleted.
    /// </param>
    /// <param name="deletedBy">
    /// The identifier of the user who deleted the entity.
    /// </param>
    void MarkAsDeleted(DateTimeOffset deletedAt, TUserId deletedBy);

    /// <summary>
    /// Restores the entity by clearing its deletion audit information.
    /// </summary>
    void Restore();
}
