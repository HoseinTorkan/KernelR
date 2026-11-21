namespace KernelR.Application.Services.OtpService.Models;

/// <summary>
/// Represents a generated One-Time Password.
/// </summary>
/// <remarks>
/// The <see cref="Code"/> is intended for delivery via SMS or email by the
/// calling service. It must never be exposed in an API response.
/// </remarks>
/// <param name="Code">The generated OTP code.</param>
/// <param name="ExpiresAt">The moment when the OTP expires.</param>
/// <param name="MaxAttempts">
/// The maximum allowed verification attempts before invalidation.
/// </param>
public sealed record Otp(
    string Code,
    DateTimeOffset ExpiresAt,
    int MaxAttempts);
