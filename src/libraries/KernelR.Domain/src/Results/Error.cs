namespace KernelR.Domain.Results;

/// <summary>
/// Represents an error that describes the failure of an operation.
/// </summary>
/// <remarks>
/// An error contains a code for programmatic identification,
/// a human-readable message, and a type that indicates its nature.
/// Errors are used with <see cref="Result"/> and <see cref="Result{T}"/>
/// to represent expected failure scenarios without throwing exceptions.
/// </remarks>
/// <example>
/// <code>
/// var notFound = new Error(
///     "PRODUCT_NOT_FOUND",
///     "Product was not found.",
///     ErrorType.NotFound);
///
/// var validation = new Error(
///     "INVALID_EMAIL",
///     "Email format is invalid.",
///     ErrorType.ValidationError);
/// </code>
/// </example>
/// <seealso cref="ErrorType"/>
/// <seealso cref="Result"/>
/// <seealso cref="Result{T}"/>
public sealed record Error
{
    /// <summary>
    /// Gets the code identifying the error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable message describing the error.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the type indicating the nature of the error.
    /// </summary>
    public ErrorType ErrorType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The stable, machine-readable error code.</param>
    /// <param name="message">The human-readable error message.</param>
    /// <param name="errorType">The category of the error.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is empty or consists only of whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="errorType"/> is not a defined value.
    /// </exception>
    public Error(string code, string message, ErrorType errorType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (!Enum.IsDefined(errorType))
            throw new ArgumentOutOfRangeException(nameof(errorType));


        Code = code;
        Message = message;
        ErrorType = errorType;
    }
}
