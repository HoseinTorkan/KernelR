namespace KernelR.Domain.Auditing;

/// <summary>
/// Defines a contract for entities that track the user who last modified them.
/// </summary>
/// <typeparam name="TUserId">The type of the user identifier.</typeparam>
public interface IHasModifier<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who last updated this entity.
    /// Returns <see langword="null"/> if the entity has never been modified.
    /// </summary>
    TUserId? UpdatedBy { get; }
}
