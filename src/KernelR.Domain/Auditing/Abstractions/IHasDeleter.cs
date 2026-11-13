namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track the user who soft-deleted them.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
public interface IHasDeleter<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who soft-deleted this entity.
    /// Returns <see langword="null"/> if the entity has not been deleted.
    /// </summary>
    TUserId? DeletedBy { get; }
}
