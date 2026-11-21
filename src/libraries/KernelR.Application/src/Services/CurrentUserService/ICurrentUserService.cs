using KernelR.Application.Services.CurrentUserService.Models;

namespace KernelR.Application.Services.CurrentUserService;

/// <summary>
/// Provides access to the currently authenticated user's identity.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the currently authenticated user from JWT claims (no database access).
    /// </summary>
    /// <returns>
    /// The current user built from the token's claims, or a failure if the
    /// request is not authenticated or required claims are missing.
    /// </returns>
    Result<CurrentUser> GetCurrentUserFromClaims();

    /// <summary>
    /// Loads the full user entity from the database for the current user.
    /// </summary>
    /// <typeparam name="TUser">
    /// The concrete user entity type. Must implement
    /// <see cref="ICanBeCurrentUser"/>; this prevents other entities from
    /// being passed by mistake.
    /// </typeparam>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The current user entity, or a failure if the user is not found.
    /// </returns>
    Task<Result<TUser>> GetCurrentUserFromDbAsync<TUser>(
        CancellationToken cancellation = default)
        where TUser : class, ICanBeCurrentUser;

    /// <summary>
    /// Loads the full user entity from the database for the current user,
    /// applying the specified predicate before reading.
    /// </summary>
    /// <remarks>
    /// The predicate is translated to the underlying query so that only a
    /// matching row is fetched — use this overload to push authorization
    /// conditions (e.g. a required role) into the database instead of
    /// fetching the entity and checking the condition in memory.
    /// If no row matches the predicate, the method returns a failure with
    /// <c>Error.Code = CURRENT_USER_NOT_FOUND</c>.
    /// </remarks>
    /// <typeparam name="TUser">
    /// The concrete user entity type. Must implement
    /// <see cref="ICanBeCurrentUser"/>.
    /// </typeparam>
    /// <param name="predicate">
    /// The condition the current user must satisfy (for example
    /// <c>u =&gt; u.Role == UserRole.SuperAdmin</c>).
    /// </param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The current user entity if a matching row exists; otherwise, a failure.
    /// </returns>
    Task<Result<TUser>> GetCurrentUserFromDbAsync<TUser>(
        Expression<Func<TUser, bool>> predicate,
        CancellationToken cancellation = default)
        where TUser : class, ICanBeCurrentUser;
}
