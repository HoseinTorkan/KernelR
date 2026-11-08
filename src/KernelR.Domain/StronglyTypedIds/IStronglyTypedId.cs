using KernelR.Domain.ValueObjects;
using KernelR.Patterns.Results;

namespace KernelR.Domain.StronglyTypedIds;

public interface IStronglyTypedId<TSelf, TValue> : IValueObject, IParsable<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf, TValue>
    where TValue : struct, IEquatable<TValue>, IComparable<TValue>
{
    TValue Value { get; }
    static abstract Result<TSelf> Create(TValue value);
    static abstract TSelf New();
    static abstract TSelf From(TValue value);
    static abstract TSelf Empty { get; }
}
