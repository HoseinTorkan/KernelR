using KernelR.Application.Services.SmsService.Models;

namespace KernelR.Application.Services.SmsService;

/// <summary>
/// Contract for sending SMS messages.
/// </summary>
/// <remarks>
/// Implementations are provider-agnostic (Kavenegar, SMS.ir, Twilio, etc.).
/// Long messages are expected to be split into multiple SMS by the
/// implementation; the caller always sees a single result.
/// </remarks>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message to the specified phone number.
    /// </summary>
    /// <param name="phoneNumber">Destination phone number.</param>
    /// <param name="message">Message content.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// Success, or a failure describing why the message could not be sent.
    /// </returns>
    Task<Result> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellation = default);

    /// <summary>
    /// Sends an SMS message to multiple recipients.
    /// </summary>
    /// <param name="phoneNumbers">Destination phone numbers.</param>
    /// <param name="message">Message content.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The number of messages sent, how many failed, and which recipients
    /// could not be reached. Returns a failure only if the underlying provider
    /// could not be reached at all.
    /// </returns>
    Task<Result<BulkSmsResult>> SendBulkAsync(
        IReadOnlyCollection<string> phoneNumbers,
        string message,
        CancellationToken cancellation = default);
}
