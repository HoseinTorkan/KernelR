namespace KernelR.Application.Services.AuthTokenService.Models;

/// <summary>
/// Represents a pair of access and refresh tokens issued to a user.
/// </summary>
/// <param name="AccessToken">The short-lived access token.</param>
/// <param name="AccessTokenExpiresAt">The moment the access token expires.</param>
/// <param name="RefreshToken">The long-lived refresh token persisted server-side.</param>
/// <param name="RefreshTokenExpiresAt">The moment the refresh token expires.</param>
public sealed record TokenPair(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
