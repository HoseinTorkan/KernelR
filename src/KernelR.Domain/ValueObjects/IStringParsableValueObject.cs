using KernelR.Primitives.Results;

namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Defines a contract for Value Objects that can be created from a string representation.
/// </summary>
/// <typeparam name="TSelf">The concrete type of the Value Object (CRTP pattern).</typeparam>
public interface IStringParsableValueObject<TSelf> : IValueObject
    where TSelf : struct, IStringParsableValueObject<TSelf>
{
    /// <summary>
    /// Creates a new instance of the Value Object from a string, returning a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>A successful result carrying the parsed Value Object, or a failure if the string is invalid.</returns>
    static abstract Result<TSelf> Create(string value);
}
