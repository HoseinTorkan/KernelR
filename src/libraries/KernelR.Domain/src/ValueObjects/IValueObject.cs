namespace KernelR.Domain.ValueObjects;

/// <summary>
/// Serves as a marker interface for all Value Objects in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// In Domain-Driven Design (DDD), a Value Object is an immutable type that is defined
/// by its attributes rather than an identity. This marker interface allows the 
/// framework, the infrastructure layer, and other libraries to easily identify 
/// and apply specific behaviors (e.g., custom serialization, EF Core value converters, 
/// or generic constraints) to all Value Objects without relying on a base class.
/// </para>
/// <para>
/// Types implementing this interface are expected to be immutable, self-validating, 
/// and free of any side effects.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public readonly record struct Email : IValueObject
/// {
///     // ...
/// }
/// </code>
/// </example>
public interface IValueObject;
