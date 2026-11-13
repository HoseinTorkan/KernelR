namespace KernelR.Domain.Auditing;

/// <summary>
/// Combines deletion time, deleter tracking, and the IsDeleted flag into a single contract.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
/// <remarks>
/// Implementations must ensure that soft-deleted entities are excluded from
/// normal queries (for example, via a global query filter in EF Core).
/// </remarks>
public interface IDeletionAudited<TUserId> : IHasDeletionTime, IHasDeleter<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets a value indicating whether this entity has been soft-deleted.
    /// </summary>
    /// <remarks>
    /// Derived from <see cref="IHasDeletionTime.DeletedAt"/>; an entity is considered deleted
    /// if and only if <c>DeletedAt</c> is non-null.
    /// </remarks>
    bool IsDeleted { get; }
}
