namespace KernelR.Domain.Auditing;

/// <summary>
/// Defines a contract for entities that track their last modification time.
/// </summary>
public interface IHasModificationTime
{
    /// <summary>
    /// Gets the moment when this entity was last updated.
    /// Returns <see langword="null"/> if the entity has never been modified.
    /// </summary>
    /// <remarks>
    /// Implementations should persist this value in UTC.
    /// </remarks>
    DateTimeOffset? UpdatedAt { get; }
}
