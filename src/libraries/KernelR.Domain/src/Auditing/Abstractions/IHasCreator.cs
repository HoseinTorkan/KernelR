namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for types that track the identity of their creator.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the creator's identifier.
/// </typeparam>
/// <remarks>
/// Implementations expose the identifier of the user who created the entity.
/// </remarks>
public interface IHasCreator<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who created the entity.
    /// </summary>
    TUserId CreatedBy { get; }
}
