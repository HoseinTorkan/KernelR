namespace KernelR.Domain.Abstractions;

/// <summary>
/// Defines a contract for the root entity of an aggregate.
/// </summary>
/// <typeparam name="TId">
/// The type of the aggregate root identifier.
/// </typeparam>
/// <remarks>
/// <para>
/// An aggregate root is the primary entry point for interacting with an aggregate.
/// External objects should access and modify the aggregate through its root
/// to preserve the aggregate's consistency and business invariants.
/// </para>
/// <para>
/// This interface is a marker contract and does not define or enforce
/// aggregate behavior, lifecycle management, or domain event handling.
/// </para>
/// </remarks>
public interface IAggregateRoot<TId> :
    IEntity<TId>
    where TId : struct;
