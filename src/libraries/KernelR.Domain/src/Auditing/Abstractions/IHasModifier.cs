namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track the user who last modified them.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the user identifier. Typically a <see cref="Guid"/> or a strongly-typed ID.
/// </typeparam>
/// <remarks>
/// This interface is typically implemented alongside <see cref="IHasModificationTime"/>
/// (via <see cref="IModificationAudited{TUserId}"/>) to form a complete modification audit trail.
/// </remarks>
public interface IHasModifier<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who last updated this entity.
    /// </summary>
    /// <value>
    /// The unique identifier of the modifying user, or <see langword="null"/>
    /// if the entity has never been modified since its creation.
    /// </value>
    /// <remarks>
    /// This value is usually set by the persistence layer (e.g., an EF Core interceptor)
    /// using the current user's ID from an <c>ICurrentUser</c> service.
    /// </remarks>
    TUserId? UpdatedBy { get; }
}
