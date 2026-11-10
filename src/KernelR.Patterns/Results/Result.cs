using KernelR.Patterns.Results.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace KernelR.Patterns.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
/// <remarks>
/// A <see cref="Result"/> is either successful or failed:
/// on success, <see cref="Error"/> is <see langword="null"/>;
/// on failure, <see cref="Error"/> carries the actual error.
/// <para>
/// This type is a functional alternative to throwing exceptions for expected failures
/// and is intended to flow across all layers of the application.
/// Instances must be created via <see cref="Success"/> or <see cref="Failure"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var success = Result.Success();
/// var failure = Result.Failure(
///     new Error("PRODUCT_NOT_FOUND", "Product was not found.", ErrorType.NotFound));
/// </code>
/// </example>
/// <seealso cref="Result{T}"/>
/// <seealso cref="Error"/>
public sealed record Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    /// <remarks>
    /// When this property is <see langword="false"/>, <see cref="Error"/> is
    /// guaranteed to be non-null. This lets the compiler reason about
    /// <see cref="Error"/> in both branches of an <c>if (result.IsSuccess)</c>.
    /// </remarks>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <remarks>
    /// When this property is <see langword="true"/>, <see cref="Error"/> is guaranteed
    /// to be non-null. This contract is enforced by <see cref="Failure"/> and is visible
    /// to the compiler through <see cref="MemberNotNullWhenAttribute"/>.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error describing the failure, or <see langword="null"/> on success.
    /// </summary>
    public Error? Error { get; }

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with <see cref="Error"/> set to <see langword="null"/>.
    /// </summary>
    /// <returns>A new successful <see cref="Result"/>.</returns>
    public static Result Success()
    {
        return new(true, null);
    }

    /// <summary>
    /// Creates a failed result carrying the specified <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The error describing the failure. Must not be <see langword="null"/>.</param>
    /// <returns>A new failed <see cref="Result"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="error"/> is <see langword="null"/>.
    /// A failed result must carry a real error; a <see langword="null"/> error
    /// would collapse the success/failure distinction.
    /// </exception>
    public static Result Failure(Error error)
    {
        if (error is null)
        {
            new Error(
                    "KernelR.Patterns.Result.NULL_ERROR",
                    "A failed result must carry a real error; a null error would collapse the success/failure distinction.",
                    ErrorType.ValidationError)
                .OrThrowArgumentNullException(nameof(error));
        }

        return new(false, error);
    }
}

/// <summary>
/// Represents the outcome of an operation that returns a value on success.
/// </summary>
/// <typeparam name="T">The type of the value produced on success.</typeparam>
/// <remarks>
/// A <see cref="Result{T}"/> is either successful (with a non-null <see cref="Value"/>)
/// or failed (with a non-null <see cref="Error"/>).
/// <para>
/// The invariant that a successful result never carries a <see langword="null"/>
/// <see cref="Value"/> is enforced at construction time by <see cref="Success(T)"/>,
/// which rejects <see langword="null"/> for every <typeparamref name="T"/> whose
/// default value is <see langword="null"/> (reference types and nullable value types).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;Product&gt; result = await _repository.GetProductAsync(id);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.Name); // Value is known non-null here.
/// }
/// </code>
/// </example>
/// <seealso cref="Result"/>
/// <seealso cref="Error"/>
public sealed record Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    /// <remarks>
    /// When this property is <see langword="true"/>, <see cref="Value"/> is guaranteed
    /// to be non-null.
    /// When this property is <see langword="false"/>, <see cref="Error"/> is
    /// guaranteed to be non-null.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <remarks>
    /// When this property is <see langword="true"/>, <see cref="Error"/> is guaranteed
    /// to be non-null. This contract is enforced by <see cref="Failure"/> and is visible
    /// to the compiler through <see cref="MemberNotNullWhenAttribute"/>.
    /// When this property is <see langword="false"/>, <see cref="Value"/> is guaranteed
    /// to be non-null.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error describing the failure, or <see langword="null"/> on success.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Gets the value produced by a successful operation, or <see langword="default"/> on failure.
    /// </summary>
    public T? Value { get; }

    private Result(T? value, bool isSuccess, Error? error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result carrying the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value produced by the successful operation.</param>
    /// <returns>A new successful <see cref="Result{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/> and <typeparamref name="T"/>
    /// is a reference type or a nullable value type (e.g. <c>int?</c>).
    /// Use <see cref="Result.Success"/> for void operations when no value is produced.
    /// </exception>
    public static Result<T> Success(T value)
    {
        // A successful result must never carry a null Value, regardless of T.
        // For reference types and nullable value types (e.g. int?), default(T) is
        // null, so a null input is rejected. For non-nullable value types (e.g. int),
        // value can never be null and default(T) is not null, so the check is a no-op.
        if (value is null && default(T) is null)
        {
            new Error(
                    "KernelR.Patterns.Result.NULL_VALUE",
                    $"Result<{typeof(T).Name}>.Success requires a non-null value. Use Result.Success() for void operations.",
                    ErrorType.ValidationError)
                .OrThrowArgumentNullException(nameof(value));
        }

        return new(value, true, null);
    }

    /// <summary>
    /// Creates a failed result carrying the specified <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The error describing the failure. Must not be <see langword="null"/>.</param>
    /// <returns>A new failed <see cref="Result{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="error"/> is <see langword="null"/>.
    /// A failed result must carry a real error; a <see langword="null"/> error
    /// would collapse the success/failure distinction.
    /// </exception>
    public static Result<T> Failure(Error error)
    {
        if (error is null)
        {
            new Error(
                    "KernelR.Patterns.Result.NULL_ERROR",
                    "A failed result must carry a real error; a null error would collapse the success/failure distinction.",
                    ErrorType.ValidationError)
                .OrThrowArgumentNullException(nameof(error));
        }

        return new(default, false, error);
    }
}
