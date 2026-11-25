namespace KernelR.Domain.Abstractions;

/// <summary>
/// Marks an entity that can represent the current user.
/// </summary>
/// <typeparam name="TId">
/// The type of the entity identifier.
/// </typeparam>
public interface ICanBeCurrentUser<TId> : IEntity<TId>
    where TId : struct;
