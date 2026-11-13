namespace KernelR.Primitives.Results;

/// <summary>
/// Defines the category of an error, indicating its nature.
/// </summary>
/// <remarks>
/// Values are grouped by category and spaced numerically to allow future
/// additions without renumbering existing members. 
/// </remarks>
/// <seealso cref="Error"/>
public enum ErrorType
{
    /// <summary>
    /// Unspecified or unknown error type (default value).
    /// </summary>
    /// <remarks>
    /// This value is essential for scenarios where an <see cref="Error"/> is created 
    /// without explicitly specifying a type (e.g., during deserialization). 
    /// It prevents the default value from falling outside the defined enum range.
    /// </remarks>
    None = 0,

    // ──────────────────────────────────────────────────────────────
    // Validation errors (20–29)
    // ──────────────────────────────────────────────────────────────

    /// <summary>The input is not valid.</summary>
    ValidationError = 20,

    /// <summary>The provided input is invalid or malformed.</summary>
    InvalidInput = 21,

    /// <summary>A required field is missing.</summary>
    RequiredField = 22,

    /// <summary>The format of the provided value is invalid.</summary>
    InvalidFormat = 23,

    // ──────────────────────────────────────────────────────────────
    // Resource errors (30–39)
    // ──────────────────────────────────────────────────────────────

    /// <summary>The requested resource was not found.</summary>
    NotFound = 30,

    /// <summary>The resource already exists (duplicate).</summary>
    Duplicate = 31,

    /// <summary>The operation conflicts with the current state of the resource.</summary>
    Conflict = 32,

    // ──────────────────────────────────────────────────────────────
    // Authentication and authorization errors (40–49)
    // ──────────────────────────────────────────────────────────────

    /// <summary>The caller is not authenticated.</summary>
    Unauthorized = 40,

    /// <summary>The caller does not have permission to perform the operation.</summary>
    Forbidden = 41,

    /// <summary>The authentication token is invalid or expired.</summary>
    InvalidToken = 42,

    /// <summary>Too many failed attempts; the caller is temporarily blocked.</summary>
    TooManyAttempts = 43,

    // ──────────────────────────────────────────────────────────────
    // Business rule violations (50–59)
    // ──────────────────────────────────────────────────────────────

    /// <summary>A business rule was violated.</summary>
    BusinessRuleViolation = 50,

    /// <summary>Insufficient stock for the requested operation.</summary>
    InsufficientStock = 51,

    /// <summary>The current status does not allow the requested operation.</summary>
    InvalidStatus = 52,

    /// <summary>The amount is below the minimum allowed.</summary>
    MinimumAmount = 53,

    /// <summary>The amount exceeds the maximum allowed.</summary>
    MaximumAmount = 54,

    // ──────────────────────────────────────────────────────────────
    // Infrastructure errors (60–69)
    // ──────────────────────────────────────────────────────────────

    /// <summary>An infrastructure error occurred.</summary>
    Infrastructure = 60,

    /// <summary>A database error occurred.</summary>
    DatabaseError = 61,

    /// <summary>A network error occurred while communicating with a downstream service.</summary>
    NetworkError = 62,

    /// <summary>An external service returned an error.</summary>
    ExternalServiceError = 63,

    /// <summary>A file-related error occurred.</summary>
    FileError = 64,

    // ──────────────────────────────────────────────────────────────
    // Transient errors (70–79)
    // ──────────────────────────────────────────────────────────────

    /// <summary>The operation timed out (caller-side).</summary>
    Timeout = 70,

    /// <summary>A concurrency conflict occurred (optimistic concurrency violation).</summary>
    Concurrency = 71,

    // ──────────────────────────────────────────────────────────────
    // General errors (80–99)
    // ──────────────────────────────────────────────────────────────

    /// <summary>An unexpected or uncategorized error occurred.</summary>
    UnknownError = 80
}
