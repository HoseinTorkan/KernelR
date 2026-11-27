
namespace KernelR.Domain.Results;

/// <summary>
/// Defines the category of an error, indicating its nature.
/// </summary>
/// <remarks>
/// Values are assigned to numeric ranges by category to allow future
/// additions without changing the values of existing members.
/// Categories provide a basis for mapping errors to HTTP status codes
/// and other error-handling policies.
/// </remarks>
/// <seealso cref="Error"/>
public enum ErrorType
{
    /// <summary>
    /// Indicates that the error type has not been specified.
    /// </summary>
    None = 0,

    // Validation

    /// <summary>
    /// The input failed validation.
    /// </summary>
    ValidationError = 20,

    /// <summary>
    /// The provided input is invalid.
    /// </summary>
    InvalidInput = 21,

    /// <summary>
    /// A required field is missing.
    /// </summary>
    RequiredField = 22,

    /// <summary>
    /// The provided value has an invalid format.
    /// </summary>
    InvalidFormat = 23,

    // Resource

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 30,

    /// <summary>
    /// A resource with the same identifying value already exists.
    /// </summary>
    Duplicate = 31,

    /// <summary>
    /// The operation conflicts with the current state of a resource.
    /// </summary>
    Conflict = 32,

    // Authentication and authorization

    /// <summary>
    /// The caller is not authenticated.
    /// </summary>
    Unauthorized = 40,

    /// <summary>
    /// The caller is not permitted to perform the requested operation.
    /// </summary>
    Forbidden = 41,

    /// <summary>
    /// The authentication token is invalid or expired.
    /// </summary>
    InvalidToken = 42,

    /// <summary>
    /// The caller exceeded the allowed number of attempts.
    /// </summary>
    TooManyAttempts = 43,

    // Business rules

    /// <summary>
    /// The operation violates a business rule.
    /// </summary>
    BusinessRuleViolation = 50,

    /// <summary>
    /// The available stock is insufficient for the requested operation.
    /// </summary>
    InsufficientStock = 51,

    /// <summary>
    /// The current status does not allow the requested operation.
    /// </summary>
    InvalidStatus = 52,

    /// <summary>
    /// The amount is below the allowed minimum.
    /// </summary>
    MinimumAmount = 53,

    /// <summary>
    /// The amount exceeds the allowed maximum.
    /// </summary>
    MaximumAmount = 54,

    // Infrastructure

    /// <summary>
    /// An infrastructure-related error occurred.
    /// </summary>
    Infrastructure = 60,

    /// <summary>
    /// A database operation failed.
    /// </summary>
    DatabaseError = 61,

    /// <summary>
    /// A network operation failed.
    /// </summary>
    NetworkError = 62,

    /// <summary>
    /// An external service operation failed.
    /// </summary>
    ExternalServiceError = 63,

    /// <summary>
    /// A file-related operation failed.
    /// </summary>
    FileError = 64,

    // Operational

    /// <summary>
    /// The operation exceeded the allowed time.
    /// </summary>
    Timeout = 70,

    /// <summary>
    /// An operation failed due to a concurrency conflict.
    /// </summary>
    Concurrency = 71,

    // General

    /// <summary>
    /// An unexpected or uncategorized error occurred.
    /// </summary>
    UnknownError = 80
}
