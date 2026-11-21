using KernelR.Application.Services.OtpService.Models;

namespace KernelR.Application.Services.OtpService;

/// <summary>
/// Contract for generating, verifying, and rate-limiting One-Time Passwords (OTPs)
/// used in passwordless authentication and multi-factor verification flows.
/// </summary>
/// <remarks>
/// Implementations are storage-agnostic (in-memory, Redis, etc.) and must
/// enforce expiration, attempt limits, and rate limiting to prevent
/// brute-force attacks.
/// </remarks>
public interface IOtpService
{
    /// <summary>
    /// Generates and stores a new OTP for the given identifier, replacing any
    /// existing one.
    /// </summary>
    /// <param name="identifier">Unique target such as a phone number or email.</param>
    /// <param name="purpose">The purpose of the OTP (login, register admin, etc.).</param>
    /// <param name="expiration">
    /// Validity duration; uses the default if <see langword="null"/>.
    /// </param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>The generated OTP details.</returns>
    Task<Result<Otp>> GenerateAsync(
        string identifier,
        OtpPurpose purpose,
        TimeSpan? expiration = null,
        CancellationToken cancellation = default);

    /// <summary>
    /// Verifies a user-provided code against the stored OTP.
    /// On success, the OTP is removed; on failure, the attempt counter is
    /// incremented.
    /// </summary>
    /// <param name="identifier">Same identifier used during generation.</param>
    /// <param name="code">User-provided code to validate.</param>
    /// <param name="purpose">The purpose the OTP was originally generated for.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>Success, or a failure describing why verification failed.</returns>
    Task<Result> VerifyAsync(
        string identifier,
        string code,
        OtpPurpose purpose,
        CancellationToken cancellation = default);

    /// <summary>
    /// Determines whether the identifier has exceeded the allowed OTP request
    /// frequency within the configured time window.
    /// </summary>
    /// <param name="identifier">The identifier to check.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the identifier is rate-limited; otherwise,
    /// <see langword="false"/>. Returns a failure if the store cannot be reached.
    /// </returns>
    Task<Result<bool>> IsRateLimitedAsync(
        string identifier,
        CancellationToken cancellation = default);

    /// <summary>
    /// Removes any stored OTP for the given identifier without verifying it.
    /// Used for rollback scenarios (e.g., when downstream delivery such as SMS
    /// fails) to prevent orphaned OTPs from blocking future requests or consuming
    /// rate limits.
    /// </summary>
    /// <param name="identifier">Same identifier used during generation.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// Success even if no OTP existed (idempotent), or a failure if the store
    /// could not be reached.
    /// </returns>
    Task<Result> InvalidateAsync(
        string identifier,
        CancellationToken cancellation = default);
}
