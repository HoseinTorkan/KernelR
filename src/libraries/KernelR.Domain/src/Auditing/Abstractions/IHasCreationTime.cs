namespace KernelR.Domain.Auditing.Abstractions;

/// <summary>
/// Defines a contract for types that track their creation time.
/// </summary>
/// <remarks>
/// Implementations expose the timestamp at which the entity was created.
/// </remarks>
public interface IHasCreationTime
{
    /// <summary>
    /// Gets the creation timestamp of the entity.
    /// </summary>
    DateTimeOffset CreatedAt { get; }
}
