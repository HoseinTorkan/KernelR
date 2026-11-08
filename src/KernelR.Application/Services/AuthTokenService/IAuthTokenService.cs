using KernelR.Application.Services.AuthTokenService.Models;

namespace KernelR.Application.Services.AuthTokenService;

/// <summary>
/// Contract for generating and validating authentication tokens
/// (access tokens and refresh tokens) used in passwordless and multi-factor
/// authentication flows.
/// </summary>
/// <remarks>
/// Implementations are JWT-provider-agnostic. The access token is short-lived
/// and self-contained; the refresh token is a long-lived opaque value persisted
/// server-side for revocation support.
/// </remarks>
public interface IAuthTokenService
{
    // ──────────────────────────────────────────────────────────────
    // Generation
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a new access token and refresh token pair for the given claims.
    /// </summary>
    /// <param name="claims">The claims to embed in the access token.</param>
    /// <returns>
    /// A <see cref="TokenPair"/> containing the access token, the refresh token,
    /// and their respective expiration timestamps.
    /// </returns>
    TokenPair GenerateTokens(IEnumerable<Claim> claims);

    // ──────────────────────────────────────────────────────────────
    // Validation
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates the specified access token and returns its claims if valid.
    /// </summary>
    /// <param name="accessToken">The access token to validate.</param>
    /// <returns>
    /// A <see cref="ClaimsPrincipal"/> on success; otherwise, a failure whose
    /// <c>Error.Code</c> describes the reason (expired, invalid signature,
    /// malformed, ...).
    /// </returns>
    Result<ClaimsPrincipal> ValidateAccessToken(string accessToken);

    // ──────────────────────────────────────────────────────────────
    // Refresh & revocation
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Exchanges a valid refresh token for a new access token / refresh token pair.
    /// </summary>
    /// <param name="refreshToken">The refresh token to exchange.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// A new <see cref="TokenPair"/>, or a failure if the refresh token is
    /// invalid, expired, or revoked.
    /// </returns>
    Task<Result<TokenPair>> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellation = default);

    /// <summary>
    /// Revokes a refresh token, preventing it from being exchanged again.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the token was revoked; otherwise,
    /// <see langword="false"/> (already revoked or not found).
    /// </returns>
    Task<Result<bool>> RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellation = default);

    /// <summary>
    /// Revokes all refresh tokens issued to a user (e.g. on password change
    /// or "log out everywhere").
    /// </summary>
    /// <param name="userId">The user whose tokens should be revoked.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The number of tokens revoked.
    /// </returns>
    Task<Result<int>> RevokeAllUserTokensAsync(
        string userId,
        CancellationToken cancellation = default);
}
