namespace KernelR.Application.Services.AccountingService.Models;

/// <summary>
/// Input model for registering an invoice in an external accounting system.
/// </summary>
/// <param name="CustomerCode">The unique code of the customer in the accounting system.</param>
/// <param name="Lines">The invoice lines; each line is one product with quantity and unit price.</param>
/// <param name="IssuedAt">The moment the invoice was issued, expressed as a DateTimeOffset.</param>
public sealed record NewInvoiceModel(
    string CustomerCode,
    IReadOnlyList<InvoiceLine> Lines,
    DateTimeOffset IssuedAt
    );

/// <summary>
/// Summary of an invoice, used in list endpoints.
/// </summary>
/// <param name="ReferenceNumber">The unique reference number assigned by the accounting system.</param>
/// <param name="CustomerCode">The code of the customer the invoice was issued to.</param>
/// <param name="TotalAmount">The total amount computed by the accounting system.</param>
/// <param name="IssuedAt">The moment the invoice was issued.</param>
public sealed record InvoiceSummary(
    string ReferenceNumber,
    string CustomerCode,
    decimal TotalAmount,
    DateTimeOffset IssuedAt
    );

/// <summary>
/// Output describing an invoice as stored in an external accounting system.
/// Unlike <see cref="NewInvoiceModel"/>, it carries a system-assigned reference number
/// and a computed total.
/// </summary>
/// <param name="ReferenceNumber">The unique reference number assigned by the accounting system.</param>
/// <param name="CustomerCode">The code of the customer the invoice was issued to.</param>
/// <param name="Lines">The invoice lines as stored by the accounting system.</param>
/// <param name="TotalAmount">The total amount computed by the accounting system.</param>
/// <param name="IssuedAt">The moment the invoice was issued.</param>
public sealed record InvoiceDetail(
    string ReferenceNumber,
    string CustomerCode,
    IReadOnlyList<InvoiceLine> Lines,
    decimal TotalAmount,
    DateTimeOffset IssuedAt
    );

/// <summary>
/// A single line in an invoice.
/// </summary>
/// <param name="ProductCode">The unique code of the product in the accounting system.</param>
/// <param name="Quantity">The quantity being invoiced; may be fractional.</param>
/// <param name="UnitPrice">The price per unit.</param>
public sealed record InvoiceLine(
    string ProductCode,
    decimal Quantity,
    decimal UnitPrice
    );
