namespace KernelR.Domain.Auditing;

/// <summary>
/// Defines a contract for entities that track their creation time.
/// </summary>
public interface IHasCreationTime
{
    /// <summary>
    /// Gets the moment when this entity was created.
    /// </summary>
    /// <remarks>
    /// Implementations should persist this value in UTC.
    /// </remarks>
    DateTimeOffset CreatedAt { get; }
}
