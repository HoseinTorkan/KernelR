namespace KernelR.Application.Services.AccountingService.Models;

/// <summary>
/// Input model for creating a new product in an external accounting system.
/// The product code is assigned by the accounting system and is not part of this model.
/// </summary>
/// <param name="Name">The product's display name.</param>
/// <param name="UnitPrice">The initial price per unit.</param>
/// <param name="Unit">The unit of measure (e.g. <c>"kg"</c>, <c>"piece"</c>).</param>
/// <param name="InitialStock">The initial stock on hand; may be fractional. Defaults to zero.</param>
public sealed record NewProductModel(
    string Name,
    decimal UnitPrice,
    string Unit,
    decimal InitialStock = 0
    );

/// <summary>
/// Summary of a product, used in list endpoints.
/// </summary>
/// <param name="Code">The unique code of the product in the accounting system.</param>
/// <param name="Name">The human-readable name of the product.</param>
/// <param name="UnitPrice">The current price per unit.</param>
public sealed record ProductSummary(
    string Code,
    string Name,
    decimal UnitPrice
    );

/// <summary>
/// Output describing a product as stored in an external accounting system.
/// </summary>
/// <param name="Code">The unique code of the product in the accounting system.</param>
/// <param name="Name">The human-readable name of the product.</param>
/// <param name="UnitPrice">The current price per unit.</param>
/// <param name="Stock">The current stock on hand; may be fractional.</param>
/// <param name="Unit">The unit of measure (e.g. <c>"kg"</c>, <c>"piece"</c>).</param>
public sealed record ProductDetail(
    string Code,
    string Name,
    decimal UnitPrice,
    decimal Stock,
    string Unit
    );

