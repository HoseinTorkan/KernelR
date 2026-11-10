using KernelR.Patterns.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace KernelR.Patterns.Results.Extensions;

/// <summary>
/// Provides extension methods to extract values from <see cref="Result{T}"/> or to throw 
/// standard .NET exceptions containing JSON-serialized <see cref="Error"/> details.
/// </summary>
public static class ResultExtensions
{
    #region ═══ Result Extensions                                        ═══

    /// <summary>Returns the success value, or throws a default <see cref="InvalidOperationException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public static T OrThrow<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new InvalidOperationException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a custom exception created by the <paramref name="exceptionFactory"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TException">The type of the custom exception.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="exceptionFactory">A factory that receives the JSON-serialized error and returns the custom exception.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="Exception">Thrown when the result is a failure.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "The generic constraint allows any exception. It is up to the consumer to provide a specific exception.")]
    public static T OrThrow<T, TException>(this Result<T> result, Func<string, TException> exceptionFactory)
        where TException : Exception
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw exceptionFactory(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="ArgumentException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="ArgumentException">Thrown when the result is a failure.</exception>
    public static T OrThrowArgumentException<T>(this Result<T> result, string? paramName = null)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new ArgumentException(result.Error.ToJson(), paramName);
    }

    /// <summary>Returns the success value, or throws an <see cref="ArgumentNullException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the result is a failure.</exception>
    public static T OrThrowArgumentNullException<T>(this Result<T> result, string? paramName = null)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new ArgumentNullException(paramName, result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="ArgumentOutOfRangeException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the result is a failure.</exception>
    public static T OrThrowArgumentOutOfRangeException<T>(this Result<T> result, string? paramName = null)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new ArgumentOutOfRangeException(paramName, result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a <see cref="FormatException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="FormatException">Thrown when the result is a failure.</exception>
    public static T OrThrowFormatException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new FormatException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="InvalidOperationException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public static T OrThrowInvalidOperationException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new InvalidOperationException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a <see cref="NotSupportedException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="NotSupportedException">Thrown when the result is a failure.</exception>
    public static T OrThrowNotSupportedException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new NotSupportedException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a <see cref="NotImplementedException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="NotImplementedException">Thrown when the result is a failure.</exception>
    public static T OrThrowNotImplementedException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new NotImplementedException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a <see cref="TimeoutException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="TimeoutException">Thrown when the result is a failure.</exception>
    public static T OrThrowTimeoutException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new TimeoutException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a <see cref="KeyNotFoundException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the result is a failure.</exception>
    public static T OrThrowKeyNotFoundException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new KeyNotFoundException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="UnauthorizedAccessException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the result is a failure.</exception>
    public static T OrThrowUnauthorizedAccessException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new UnauthorizedAccessException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="OperationCanceledException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the result is a failure.</exception>
    public static T OrThrowOperationCanceledException<T>(this Result<T> result, CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new OperationCanceledException(result.Error.ToJson(), cancellationToken);
    }

    /// <summary>Returns the success value, or throws an <see cref="ObjectDisposedException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="objectName">The name of the disposed object.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the result is a failure.</exception>
    public static T OrThrowObjectDisposedException<T>(this Result<T> result, string? objectName = null)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new ObjectDisposedException(objectName, result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="IOException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="IOException">Thrown when the result is a failure.</exception>
    public static T OrThrowIOException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new IOException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a <see cref="PlatformNotSupportedException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when the result is a failure.</exception>
    public static T OrThrowPlatformNotSupportedException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new PlatformNotSupportedException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="OverflowException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="OverflowException">Thrown when the result is a failure.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "Explicitly requested by the consumer to map domain errors to this specific exception.")]
    public static T OrThrowOverflowException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new OverflowException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="InvalidCastException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidCastException">Thrown when the result is a failure.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "Explicitly requested by the consumer to map domain errors to this specific exception.")]
    public static T OrThrowInvalidCastException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new InvalidCastException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws a <see cref="DivideByZeroException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="DivideByZeroException">Thrown when the result is a failure.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "Explicitly requested by the consumer to map domain errors to this specific exception.")]
    public static T OrThrowDivideByZeroException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new DivideByZeroException(result.Error.ToJson());
    }

    /// <summary>Returns the success value, or throws an <see cref="IndexOutOfRangeException"/>.</summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when the result is a failure.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "Explicitly requested by the consumer to map domain errors to this specific exception.")]
    public static T OrThrowIndexOutOfRangeException<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new IndexOutOfRangeException(result.Error.ToJson());
    }

    #endregion ═══ Result Extensions                                        ═══

    #region ═══ Error Extensions                                         ═══

    /// <summary>Throws a default <see cref="InvalidOperationException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [DoesNotReturn]
    public static void Throw(this Error error)
    {
        throw new InvalidOperationException(error.ToJson());
    }

    /// <summary>Throws a custom exception created by the <paramref name="exceptionFactory"/>.</summary>
    /// <typeparam name="TException">The type of the custom exception.</typeparam>
    /// <param name="error">The error to serialize.</param>
    /// <param name="exceptionFactory">A factory that receives the JSON-serialized error and returns the custom exception.</param>
    /// <exception cref="Exception">Always thrown.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "The generic constraint allows any exception. It is up to the consumer to provide a specific exception.")]
    [DoesNotReturn]
    public static void Throw<TException>(this Error error, Func<string, TException> exceptionFactory)
        where TException : Exception
    {
        throw exceptionFactory(error.ToJson());
    }

    /// <summary>Throws an <see cref="ArgumentException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <exception cref="ArgumentException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowArgumentException(this Error error, string? paramName = null)
    {
        throw new ArgumentException(error.ToJson(), paramName);
    }

    /// <summary>Throws an <see cref="ArgumentNullException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <exception cref="ArgumentNullException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowArgumentNullException(this Error error, string? paramName = null)
    {
        throw new ArgumentNullException(paramName, error.ToJson());
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <exception cref="ArgumentOutOfRangeException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException(this Error error, string? paramName = null)
    {
        throw new ArgumentOutOfRangeException(paramName, error.ToJson());
    }

    /// <summary>Throws a <see cref="FormatException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="FormatException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowFormatException(this Error error)
    {
        throw new FormatException(error.ToJson());
    }

    /// <summary>Throws an <see cref="InvalidOperationException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowInvalidOperationException(this Error error)
    {
        throw new InvalidOperationException(error.ToJson());
    }

    /// <summary>Throws a <see cref="NotSupportedException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowNotSupportedException(this Error error)
    {
        throw new NotSupportedException(error.ToJson());
    }

    /// <summary>Throws a <see cref="NotImplementedException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowNotImplementedException(this Error error)
    {
        throw new NotImplementedException(error.ToJson());
    }

    /// <summary>Throws a <see cref="TimeoutException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="TimeoutException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowTimeoutException(this Error error)
    {
        throw new TimeoutException(error.ToJson());
    }

    /// <summary>Throws a <see cref="KeyNotFoundException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="KeyNotFoundException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowKeyNotFoundException(this Error error)
    {
        throw new KeyNotFoundException(error.ToJson());
    }

    /// <summary>Throws an <see cref="UnauthorizedAccessException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="UnauthorizedAccessException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowUnauthorizedAccessException(this Error error)
    {
        throw new UnauthorizedAccessException(error.ToJson());
    }

    /// <summary>Throws an <see cref="OperationCanceledException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="OperationCanceledException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowOperationCanceledException(this Error error, CancellationToken cancellationToken = default)
    {
        throw new OperationCanceledException(error.ToJson(), cancellationToken);
    }

    /// <summary>Throws an <see cref="ObjectDisposedException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <param name="objectName">The name of the disposed object.</param>
    /// <exception cref="ObjectDisposedException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowObjectDisposedException(this Error error, string? objectName = null)
    {
        throw new ObjectDisposedException(objectName, error.ToJson());
    }

    /// <summary>Throws an <see cref="IOException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="IOException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowIOException(this Error error)
    {
        throw new IOException(error.ToJson());
    }

    /// <summary>Throws a <see cref="PlatformNotSupportedException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="PlatformNotSupportedException">Always thrown.</exception>
    [DoesNotReturn]
    public static void ThrowPlatformNotSupportedException(this Error error)
    {
        throw new PlatformNotSupportedException(error.ToJson());
    }

    /// <summary>Throws an <see cref="OverflowException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="OverflowException">Always thrown.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "Explicitly requested by the consumer to map domain errors to this specific exception.")]
    [DoesNotReturn]
    public static void ThrowOverflowException(this Error error)
    {
        throw new OverflowException(error.ToJson());
    }

    /// <summary>Throws an <see cref="InvalidCastException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="InvalidCastException">Always thrown.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "Explicitly requested by the consumer to map domain errors to this specific exception.")]
    [DoesNotReturn]
    public static void ThrowInvalidCastException(this Error error)
    {
        throw new InvalidCastException(error.ToJson());
    }

    /// <summary>Throws a <see cref="DivideByZeroException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="DivideByZeroException">Always thrown.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "Explicitly requested by the consumer to map domain errors to this specific exception.")]
    [DoesNotReturn]
    public static void ThrowDivideByZeroException(this Error error)
    {
        throw new DivideByZeroException(error.ToJson());
    }

    /// <summary>Throws an <see cref="IndexOutOfRangeException"/>.</summary>
    /// <param name="error">The error to serialize.</param>
    /// <exception cref="IndexOutOfRangeException">Always thrown.</exception>
    [SuppressMessage("Design", "CA2201:Do not raise reserved exception types", Justification = "Explicitly requested by the consumer to map domain errors to this specific exception.")]
    [DoesNotReturn]
    public static void ThrowIndexOutOfRangeException(this Error error)
    {
        throw new IndexOutOfRangeException(error.ToJson());
    }

    #endregion ═══ Error Extensions                                         ═══

    #region ═══ Custom Exception Support                                  ═══

    /// <summary>
    /// Serializes the error to a JSON string for use with custom exceptions.
    /// </summary>
    /// <param name="error">The error to serialize.</param>
    /// <returns>The JSON-serialized representation of the error.</returns>
    /// <example>
    /// <code>
    /// throw new OrderNotFoundException(error.ToJson());
    /// </code>
    /// </example>
    public static string ToJson(this Error error)
    {
        return JsonSerializer.Serialize(error, KernelRJsonOptions.Options);
    }

    #endregion ═══ Custom Exception Support                                  ═══
}
