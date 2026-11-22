namespace KernelR.Domain.Abstractions;

public interface IConcurrencyStamp
{
    string ConcurrencyStamp { get; }
}
