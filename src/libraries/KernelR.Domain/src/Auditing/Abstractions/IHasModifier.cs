namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for types that track the identity of the user
/// who last modified the entity.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the modifier's identifier.
/// </typeparam>
/// <remarks>
/// The modifier's identifier is nullable because an entity may not have been
/// modified since its creation or the modifier's identity may not be available.
/// </remarks>
public interface IHasModifier<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who last modified the entity.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> when the entity has not been
    /// modified since its creation or when the modifier's identity
    /// is not available.
    /// </remarks>
    TUserId? UpdatedBy { get; }
}
