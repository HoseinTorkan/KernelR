namespace KernelR.Domain.Events;

/// <summary>
/// Defines a contract for objects that maintain domain events.
/// </summary>
/// <remarks>
/// Domain events can be recorded and cleared through this contract.
/// It does not define how or when domain events are dispatched.
/// </remarks>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the domain events recorded by the current instance.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Records a domain event on the current instance.
    /// </summary>
    /// <param name="domainEvent">
    /// The domain event to record.
    /// </param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Clears all domain events recorded by the current instance.
    /// </summary>
    void ClearDomainEvents();
}
