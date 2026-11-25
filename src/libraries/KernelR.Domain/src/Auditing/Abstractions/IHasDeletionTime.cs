namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for types that track their deletion time.
/// </summary>
/// <remarks>
/// The deletion timestamp is nullable because an entity may not have been
/// deleted yet.
/// </remarks>
public interface IHasDeletionTime
{
    /// <summary>
    /// Gets the timestamp at which the entity was deleted.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> when the entity has not been deleted.
    /// </remarks>
    DateTimeOffset? DeletedAt { get; }
}
