namespace KernelR.Domain.Abstractions;

public interface ICanBeCurrentUser<TId> : IEntity<TId>
    where TId : struct;
