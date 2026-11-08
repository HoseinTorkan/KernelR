namespace KernelR.Application.Services.CurrentUserService.Models;

/// <summary>
/// Represents the currently authenticated user's identity, built from the
/// request's JWT claims without any database access.
/// </summary>
/// <param name="Id">
/// The user's unique identifier as stored in the token's claims. It is exposed
/// as a string so the application layer stays independent of the concrete
/// identifier type used by the host project (Guid, long, ...).
/// </param>
/// <param name="Role">The user's role name.</param>
/// <param name="PhoneNumber">The user's phone number.</param>
/// <param name="Name">The user's display name.</param>
/// <param name="Claims">All claims associated with the user.</param>
public sealed record CurrentUser(
    string Id,
    string Role,
    string PhoneNumber,
    string Name,
    IReadOnlyCollection<Claim> Claims);
