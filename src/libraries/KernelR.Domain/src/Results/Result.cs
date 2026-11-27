using System.Diagnostics.CodeAnalysis;

namespace KernelR.Domain.Results;

/// <summary>
/// Represents the outcome of an operation that either succeeds or fails.
/// </summary>
/// <remarks>
/// A successful result has no associated <see cref="Error"/>.
/// A failed result carries an error describing the failure.
/// </remarks>
public sealed record Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the failed operation,
    /// or <see langword="null"/> if the operation succeeded.
    /// </summary>
    public Error? Error { get; }

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success()
        => new(true, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    /// <returns>A failed result containing the specified error.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="error"/> is <see langword="null"/>.
    /// </exception>
    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, error);
    }

    /// <summary>
    /// Executes the function corresponding to the result state
    /// and returns its output.
    /// </summary>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="onSuccess">The function to execute when the result succeeds.</param>
    /// <param name="onFailure">The function to execute when the result fails.</param>
    /// <returns>The output of the executed function.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either function is <see langword="null"/>.
    /// </exception>
    public TOut Match<TOut>(
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess
            ? onSuccess()
            : onFailure(Error);
    }

    /// <summary>
    /// Converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    /// <returns>A failed result containing the specified error.</returns>
    public static implicit operator Result(Error error)
        => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that either succeeds with a value
/// or fails with an error.
/// </summary>
/// <typeparam name="T">The type of the value returned by a successful operation.</typeparam>
/// <remarks>
/// A successful result contains a value of type <typeparamref name="T"/>
/// and has no associated <see cref="Error"/>.
/// A failed result carries an error describing the failure.
/// Accessing <see cref="Value"/> on a failed result throws an exception.
/// </remarks>
public sealed record Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the failed operation,
    /// or <see langword="null"/> if the operation succeeded.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Gets the value returned by a successful operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result represents a failure.
    /// </exception>
    public T Value => IsFailure
        ? throw new InvalidOperationException(
            "The value is not available because the result represents a failure.")
        : field;

    private Result(T value, bool isSuccess, Error? error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The value returned by the successful operation.</param>
    /// <returns>A successful result containing the specified value.</returns>
    public static Result<T> Success(T value)
        => new(value, true, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    /// <returns>A failed result containing the specified error.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="error"/> is <see langword="null"/>.
    /// </exception>
    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default!, false, error);
    }

    /// <summary>
    /// Executes the function corresponding to the result state
    /// and returns its output.
    /// </summary>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="onSuccess">The function to execute with the successful value.</param>
    /// <param name="onFailure">The function to execute with the failure error.</param>
    /// <returns>The output of the executed function.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either function is <see langword="null"/>.
    /// </exception>
    public TOut Match<TOut>(
        Func<T, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess
            ? onSuccess(Value)
            : onFailure(Error);
    }

    /// <summary>
    /// Converts a value to a successful result containing that value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful result containing the specified value.</returns>
    public static implicit operator Result<T>(T value)
        => Success(value);

    /// <summary>
    /// Converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    /// <returns>A failed result containing the specified error.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="error"/> is <see langword="null"/>.
    /// </exception>
    public static implicit operator Result<T>(Error error)
        => Failure(error);
}
