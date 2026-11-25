namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for types that track their last modification time.
/// </summary>
/// <remarks>
/// The modification timestamp is nullable because an entity may not have been
/// modified since it was created.
/// </remarks>
public interface IHasModificationTime
{
    /// <summary>
    /// Gets the timestamp of the entity's last modification.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> when the entity has not been
    /// modified since its creation.
    /// </remarks>
    DateTimeOffset? UpdatedAt { get; }
}
