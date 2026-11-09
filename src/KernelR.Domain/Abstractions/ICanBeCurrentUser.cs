namespace KernelR.Domain.Abstractions;

/// <summary>
/// Marker interface for entities that can be loaded as the "current user"
/// by <c>ICurrentUserService</c>.
/// </summary>
/// <remarks>
/// This interface has no members: the identifier comes from
/// <see cref="IEntity{TId}"/>, and any additional data is read from the
/// concrete entity.
/// </remarks>
public interface ICanBeCurrentUser;
