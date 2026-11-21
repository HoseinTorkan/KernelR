namespace KernelR.Application.Services.AccountingService.Models;

/// <summary>
/// Input model for creating a new customer in an external accounting system.
/// The customer code is assigned by the accounting system and is not part of this model.
/// </summary>
/// <param name="Name">The customer's display name.</param>
/// <param name="PhoneNumber">The customer's phone number.</param>
/// <param name="NationalId">The customer's national ID, when available.</param>
/// <param name="Address">The customer's postal address, when available.</param>
public sealed record NewCustomerModel(
    string Name,
    string PhoneNumber,
    string? NationalId = null,
    string? Address = null
    );

/// <summary>
/// Summary of a customer, used in list endpoints.
/// </summary>
/// <param name="Code">The unique code of the customer in the accounting system.</param>
/// <param name="Name">The customer's display name.</param>
/// <param name="PhoneNumber">The customer's phone number.</param>
public sealed record CustomerSummary(
    string Code,
    string Name,
    string PhoneNumber);

/// <summary>
/// Output describing a customer as stored in an external accounting system.
/// </summary>
/// <param name="Code">The unique code of the customer in the accounting system.</param>
/// <param name="Name">The customer's display name.</param>
/// <param name="PhoneNumber">The customer's phone number.</param>
/// <param name="Balance">The customer's current account balance; may be negative (debt).</param>
public sealed record CustomerDetail(
    string Code,
    string Name,
    string PhoneNumber,
    decimal Balance
    );
