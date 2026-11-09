namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines a contract for retrieving the current date and time.
/// </summary>
/// <remarks>
/// <para>
/// Using this interface instead of directly calling <c>DateTimeOffset.UtcNow</c> 
/// in the domain model makes the code testable. In unit tests, you can mock this 
/// interface to return a fixed date and time.
/// </para>
/// <para>
/// The implementation is expected to be provided by the infrastructure layer 
/// (e.g., returning <c>DateTimeOffset.UtcNow</c> in production).
/// </para>
/// </remarks>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
