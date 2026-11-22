using KernelR.Domain.Events;

namespace KernelR.Domain.Abstractions;

public interface IAggregateRoot<TId> : IEntity<TId>, IHasDomainEvents
    where TId : struct
{
    void AddDomainEvent(IDomainEvent domainEvent);
}
