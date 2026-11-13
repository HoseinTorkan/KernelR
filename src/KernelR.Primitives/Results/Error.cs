namespace KernelR.Primitives.Results;

/// <summary>
/// Represents an error that describes the failure of an operation.
/// </summary>
/// <remarks>
/// An error always contains a code for programmatic identification,
/// a human-readable message, and a type that indicates its nature.
/// <para>
/// Errors are used in conjunction with <see cref="Result"/> and <see cref="Result{T}"/>
/// to describe why an operation failed. They replace the need for throwing exceptions
/// in expected failure scenarios.
/// </para>
/// <para>
/// An <see cref="Error"/> instance is guaranteed to have non-empty <see cref="Code"/>
/// and <see cref="Message"/> values; null and whitespace inputs are rejected at construction.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var notFound = new Error("PRODUCT_NOT_FOUND", "Product was not found.", ErrorType.NotFound);
/// var validation = new Error("INVALID_EMAIL", "Email format is invalid.", ErrorType.ValidationError);
/// </code>
/// </example>
/// <seealso cref="ErrorType"/>
/// <seealso cref="Result"/>
/// <seealso cref="Result{T}"/>
public sealed record Error
{
    /// <summary>
    /// Gets the unique code identifying this error.
    /// </summary>
    /// <remarks>
    /// The code is intended for programmatic identification and should be
    /// stable across versions. Examples: "PRODUCT_NOT_FOUND", "INVALID_EMAIL".
    /// </remarks>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable message of this error.
    /// </summary>
    /// <remarks>
    /// The message is intended for display to users or for logging.
    /// It should be concise and informative.
    /// </remarks>
    public string Message { get; }

    /// <summary>
    /// Gets the type of this error, indicating its nature
    /// (e.g., validation error, not found, conflict).
    /// </summary>
    public ErrorType ErrorType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">
    /// A stable, machine-readable identifier for the error (e.g. <c>"PRODUCT_NOT_FOUND"</c>).
    /// Must not be <see langword="null"/>, empty, or whitespace.
    /// </param>
    /// <param name="message">
    /// A human-readable description of the error.
    /// Must not be <see langword="null"/>, empty, or whitespace.
    /// </param>
    /// <param name="errorType">The category of the error.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="code"/> or <paramref name="message"/> is <see langword="null"/>,
    /// empty, or whitespace.
    /// </exception>
    public Error(string code, string message, ErrorType errorType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Message = message;
        ErrorType = errorType;
    }
}
