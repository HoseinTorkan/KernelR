namespace KernelR.Domain.Abstractions;

public interface IEntity<TId>
    where TId : struct
{
    TId Id { get; }
}
