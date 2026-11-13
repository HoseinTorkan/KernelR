namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for entities that track their soft-deletion time.
/// </summary>
public interface IHasDeletionTime
{
    /// <summary>
    /// Gets the moment when this entity was soft-deleted.
    /// Returns <see langword="null"/> if the entity has not been deleted.
    /// </summary>
    /// <remarks>
    /// Implementations should persist this value in UTC.
    /// </remarks>
    DateTimeOffset? DeletedAt { get; }
}
