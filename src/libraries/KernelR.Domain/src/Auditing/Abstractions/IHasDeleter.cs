namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track the user who soft-deleted them.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the user identifier. Typically a <see cref="Guid"/> or a strongly-typed ID.
/// </typeparam>
/// <remarks>
/// This interface is typically implemented alongside <see cref="IHasDeletionTime"/>
/// and <see cref="ISoftDeletable"/> (via <see cref="IDeletionAudited{TUserId}"/>)
/// to form a complete soft-deletion audit trail.
/// </remarks>
public interface IHasDeleter<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who soft-deleted this entity.
    /// </summary>
    /// <value>
    /// The unique identifier of the deleting user, or <see langword="null"/>
    /// if the entity has not been deleted.
    /// </value>
    /// <remarks>
    /// This value is usually set by the persistence layer (e.g., an EF Core interceptor)
    /// using the current user's ID from an <c>ICurrentUser</c> service.
    /// </remarks>
    TUserId? DeletedBy { get; }
}
