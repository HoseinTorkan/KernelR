using System.Diagnostics.CodeAnalysis;

namespace KernelR.Primitives.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
/// <remarks>
/// A <see cref="Result"/> is either successful (with <see cref="Error"/> set to <see langword="null"/>)
/// or failed (with <see cref="Error"/> carrying the actual error).
/// </remarks>
public sealed record Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    /// <value><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</value>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value><see langword="true"/> if the operation failed; otherwise, <see langword="false"/>.</value>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error describing the failure, or <see langword="null"/> on success.
    /// </summary>
    /// <value>The <see cref="Error"/> instance, or <see langword="null"/> if the result is successful.</value>
    public Error? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Whether the result is successful.</param>
    /// <param name="error">The error, if the result is a failure.</param>
    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with <see cref="Error"/> set to <see langword="null"/>.
    /// </summary>
    /// <returns>A new successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result carrying the specified <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The error describing the failure. Must not be <see langword="null"/>.</param>
    /// <returns>A new failed <see cref="Result"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, error);
    }

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="Result"/> carrying the specified error.</returns>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Executes one of the provided functions based on the success or failure of the result.
    /// </summary>
    /// <typeparam name="TOut">The return type of both branches.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The value returned by the executed function.</returns>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value on success.
/// </summary>
/// <typeparam name="T">The type of the value produced on success.</typeparam>
/// <remarks>
/// A <see cref="Result{T}"/> is either successful (with a non-null <see cref="Value"/>) or failed
/// (with a non-null <see cref="Error"/>). The invariant that a successful result never carries a
/// <see langword="null"/> <see cref="Value"/> is enforced at construction time.
/// </remarks>
public sealed record Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    /// <value><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</value>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value><see langword="true"/> if the operation failed; otherwise, <see langword="false"/>.</value>
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error describing the failure, or <see langword="null"/> on success.
    /// </summary>
    /// <value>The <see cref="Error"/> instance, or <see langword="null"/> if the result is successful.</value>
    public Error? Error { get; }

    /// <summary>
    /// Gets the value produced by a successful operation, or <see langword="default"/> on failure.
    /// </summary>
    /// <value>The success value, or <see langword="default"/> if the result is a failure.</value>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <param name="isSuccess">Whether the result is successful.</param>
    /// <param name="error">The error, if the result is a failure.</param>
    private Result(T? value, bool isSuccess, Error? error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result carrying the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value produced by the successful operation. Must not be <see langword="null"/>.</param>
    /// <returns>A new successful <see cref="Result{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(value, true, null);
    }

    /// <summary>
    /// Creates a failed result carrying the specified <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The error describing the failure. Must not be <see langword="null"/>.</param>
    /// <returns>A new failed <see cref="Result{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default, false, error);
    }

    /// <summary>
    /// Implicitly converts a value to a successful <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful <see cref="Result{T}"/> carrying the specified value.</returns>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the specified error.</returns>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    /// Executes one of the provided functions based on the success or failure of the result.
    /// </summary>
    /// <typeparam name="TOut">The return type of both branches.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The value returned by the executed function.</returns>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Error);
}
