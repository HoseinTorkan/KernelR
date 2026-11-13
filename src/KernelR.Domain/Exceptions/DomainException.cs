namespace KernelR.Domain.Exceptions;

/// <summary>
/// Represents the base class for all domain-specific exceptions.
/// </summary>
/// <remarks>
/// <para>
/// While the <c>Result</c> pattern is used for expected and recoverable failures,
/// exceptions derived from this class are used for unrecoverable violations of 
/// domain invariants (e.g., trying to cancel an already paid order).
/// </para>
/// </remarks>
public class DomainException : Exception
{
    /// <summary>
    /// Gets the optional error code for programmatic identification.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    public DomainException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DomainException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error code and message.
    /// </summary>
    /// <param name="code">The stable, machine-readable error code (e.g., "ORDER_ALREADY_PAID").</param>
    /// <param name="message">The message that describes the error.</param>
    public DomainException(string code, string message) : base(message) => Code = code;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified 
    /// error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
