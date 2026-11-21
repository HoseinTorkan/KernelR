namespace KernelR.Application.Services.SmsService.Models;

/// <summary>
/// Represents the outcome of a bulk SMS send operation, including how many
/// messages were delivered and which recipients failed.
/// </summary>
/// <param name="Sent">The number of messages that were sent successfully.</param>
/// <param name="Failed">The number of messages that could not be sent.</param>
/// <param name="FailedPhoneNumbers">
/// The phone numbers that could not be reached, if any. This is the list to
/// retry once the underlying issue (e.g. insufficient credit) is resolved.
/// </param>
public sealed record BulkSmsResult(
    int Sent,
    int Failed,
    IReadOnlyCollection<string> FailedPhoneNumbers);
