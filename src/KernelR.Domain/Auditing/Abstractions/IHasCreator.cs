namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track the user who created them.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
public interface IHasCreator<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who created this entity.
    /// </summary>
    TUserId CreatedBy { get; }
}
