namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines an entity that supports optimistic concurrency control
/// through a concurrency stamp.
/// </summary>
public interface IConcurrencyStamp
{
    /// <summary>
    /// Gets the concurrency stamp used to detect conflicting modifications.
    /// </summary>
    string ConcurrencyStamp { get; }
}
