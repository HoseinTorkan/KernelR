namespace KernelR.Domain.Abstractions;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
