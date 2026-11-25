namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for types that track the identity of the user
/// who deleted the entity.
/// </summary>
/// <typeparam name="TUserId">
/// The type of the deleter's identifier.
/// </typeparam>
/// <remarks>
/// The deleter's identifier is nullable because an entity may not have been
/// deleted yet.
/// </remarks>
public interface IHasDeleter<TUserId>
    where TUserId : struct
{
    /// <summary>
    /// Gets the identifier of the user who deleted the entity.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> when the entity has not been deleted
    /// or when the deleter's identity is not available.
    /// </remarks>
    TUserId? DeletedBy { get; }
}
