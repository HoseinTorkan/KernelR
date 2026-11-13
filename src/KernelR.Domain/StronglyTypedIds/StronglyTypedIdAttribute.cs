namespace KernelR.Domain.StronglyTypedIds;

/// <summary>
/// Marks a <see langword="partial"/> <see langword="record struct"/> as a strongly-typed identifier.
/// The KernelR Source Generator will automatically implement the rest of the boilerplate.
/// </summary>
/// <typeparam name="TValue">The underlying primitive type (e.g., <see cref="Guid"/>, <see cref="int"/>).</typeparam>
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class StronglyTypedIdAttribute<TValue> : Attribute
    where TValue : struct;
