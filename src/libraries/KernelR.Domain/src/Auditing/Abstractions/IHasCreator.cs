namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track the user who created them.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the user identifier. Typically a <see cref="Guid"/> or a strongly-typed ID.
/// </typeparam>
/// <remarks>
/// This interface is typically implemented alongside <see cref="IHasCreationTime"/>
/// (via <see cref="ICreationAudited{TUserId}"/>) to form a complete creation audit trail.
/// </remarks>
public interface IHasCreator<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who created this entity.
    /// </summary>
    /// <value>
    /// The unique identifier of the creating user.
    /// </value>
    /// <remarks>
    /// This value is usually set by the persistence layer (e.g., an EF Core interceptor)
    /// using the current user's ID from an <c>ICurrentUser</c> service. Once set,
    /// it should never be modified.
    /// </remarks>
    TUserId CreatedBy { get; }
}
