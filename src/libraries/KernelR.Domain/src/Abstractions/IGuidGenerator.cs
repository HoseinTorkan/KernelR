namespace KernelR.Domain.Abstractions;

public interface IGuidGenerator
{
    Guid Create(bool useSequential = true);
}
