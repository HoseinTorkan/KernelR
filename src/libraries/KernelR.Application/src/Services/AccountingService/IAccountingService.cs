using KernelR.Application.Services.AccountingService.Models;

namespace KernelR.Application.Services.AccountingService;

/// <summary>
/// Defines a contract for integrating with external accounting systems
/// (e.g., Holoo, Sepidar, Parsian).
/// </summary>
/// <remarks>
/// Implementations adapt the shared models to the specific format required by
/// each accounting system, so callers do not need to know which system is
/// configured. List operations return <c>Summary</c> models; single-item
/// operations return <c>Detail</c> models, matching the typical shape of an
/// external API.
/// </remarks>
public interface IAccountingService
{
    // ──────────────────────────────────────────────────────────────
    // Invoices
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers a new invoice in the accounting system.
    /// </summary>
    /// <param name="invoice">The invoice data to register.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The reference number assigned by the accounting system, or a failure.
    /// </returns>
    Task<Result<string>> RegisterInvoiceAsync(
        NewInvoiceModel invoice,
        CancellationToken cancellation = default);

    /// <summary>
    /// Lists invoices (summary view).
    /// </summary>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The invoice summaries, or a failure if the connection fails.
    /// </returns>
    Task<Result<IReadOnlyList<InvoiceSummary>>> ListInvoicesAsync(
        CancellationToken cancellation = default);

    /// <summary>
    /// Gets full details of a single invoice.
    /// </summary>
    /// <param name="referenceNumber">The reference number returned by <see cref="RegisterInvoiceAsync"/>.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The invoice details, or a failure if the invoice is not found.
    /// </returns>
    Task<Result<InvoiceDetail>> GetInvoiceAsync(
        string referenceNumber,
        CancellationToken cancellation = default);

    /// <summary>
    /// Cancels an invoice that was previously registered.
    /// </summary>
    /// <param name="referenceNumber">The reference number returned by <see cref="RegisterInvoiceAsync"/>.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the invoice was cancelled; otherwise,
    /// <see langword="false"/>. Returns a failure if the invoice is not found
    /// or the connection fails.
    /// </returns>
    Task<Result<bool>> CancelInvoiceAsync(
        string referenceNumber,
        CancellationToken cancellation = default);

    // ──────────────────────────────────────────────────────────────
    // Products
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new product in the accounting system.
    /// </summary>
    /// <param name="product">The product data to create.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The product code assigned by the accounting system, or a failure if
    /// creation fails.
    /// </returns>
    Task<Result<string>> CreateProductAsync(
        NewProductModel product,
        CancellationToken cancellation = default);

    /// <summary>
    /// Lists products (summary view).
    /// </summary>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The product summaries, or a failure if the connection fails.
    /// </returns>
    Task<Result<IReadOnlyList<ProductSummary>>> ListProductsAsync(
        CancellationToken cancellation = default);

    /// <summary>
    /// Gets full details of a single product.
    /// </summary>
    /// <param name="productCode">The unique code of the product in the accounting system.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The product details, or a failure if the product is not found.
    /// </returns>
    Task<Result<ProductDetail>> GetProductAsync(
        string productCode,
        CancellationToken cancellation = default);

    /// <summary>
    /// Gets the current stock quantity of a specific product.
    /// </summary>
    /// <param name="productCode">The unique code of the product in the accounting system.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The available stock quantity, or a failure if the product is not found
    /// or the connection fails.
    /// </returns>
    Task<Result<decimal>> GetProductStockAsync(
        string productCode,
        CancellationToken cancellation = default);

    // ──────────────────────────────────────────────────────────────
    // Customers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new customer in the accounting system.
    /// </summary>
    /// <param name="customer">The customer data to create.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The customer code assigned by the accounting system, or a failure if
    /// creation fails.
    /// </returns>
    Task<Result<string>> CreateCustomerAsync(
        NewCustomerModel customer,
        CancellationToken cancellation = default);

    /// <summary>
    /// Lists customers (summary view).
    /// </summary>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The customer summaries, or a failure if the connection fails.
    /// </returns>
    Task<Result<IReadOnlyList<CustomerSummary>>> ListCustomersAsync(
        CancellationToken cancellation = default);

    /// <summary>
    /// Gets full details of a single customer.
    /// </summary>
    /// <param name="customerCode">The unique code of the customer in the accounting system.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The customer details, or a failure if the customer is not found.
    /// </returns>
    Task<Result<CustomerDetail>> GetCustomerAsync(
        string customerCode,
        CancellationToken cancellation = default);

    /// <summary>
    /// Checks whether a customer with the given phone number already exists.
    /// </summary>
    /// <param name="phoneNumber">The customer's phone number.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the customer exists; otherwise,
    /// <see langword="false"/>. Returns a failure if the accounting system
    /// cannot be reached.
    /// </returns>
    Task<Result<bool>> CustomerExistsAsync(
        string phoneNumber,
        CancellationToken cancellation = default);

    // ──────────────────────────────────────────────────────────────
    // Health
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the accounting system is reachable and accepting requests.
    /// </summary>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the system is reachable; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    Task<Result<bool>> IsAvailableAsync(
        CancellationToken cancellation = default);
}
