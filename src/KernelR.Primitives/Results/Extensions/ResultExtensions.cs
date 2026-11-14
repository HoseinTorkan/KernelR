using System.Diagnostics.CodeAnalysis;

namespace KernelR.Primitives.Results.Extensions;

/// <summary>
/// Provides extension methods to extract values from <see cref="Result{T}"/> instances.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Returns the success value, or throws a default <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public static T OrThrowInvalidOperationException<T>(this Result<T> result)
    {
        if (result.IsSuccess) return result.Value;
        throw new InvalidOperationException(result.Error.ToJson());
    }

    /// <summary>
    /// Returns the success value, or throws a custom exception created by the <paramref name="exceptionFactory"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TException">The type of the custom exception.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="exceptionFactory">A factory that receives the JSON-serialized error and returns the custom exception.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="Exception">Thrown when the result is a failure.</exception>
    public static T OrThrow<T, TException>(this Result<T> result, Func<string, TException> exceptionFactory)
        where TException : Exception
    {
        if (result.IsSuccess) return result.Value;
        throw exceptionFactory(result.Error.ToJson());
    }

    /// <summary>
    /// Returns the success value, or throws a <see cref="FormatException"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="FormatException">Thrown when the result is a failure.</exception>
    public static T OrThrowFormatException<T>(this Result<T> result)
    {
        if (result.IsSuccess) return result.Value;
        throw new FormatException(result.Error.ToJson());
    }

    /// <summary>
    /// Attempts to extract the success value without throwing.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="value">When this method returns, contains the success value, or <see langword="default"/> on failure.</param>
    /// <returns><see langword="true"/> if the result is successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetValue<T>(this Result<T> result, [MaybeNullWhen(false)] out T value)
    {
        value = result.IsSuccess ? result.Value : default;
        return result.IsSuccess;
    }

    /// <summary>
    /// Throws a default <see cref="InvalidOperationException"/> for a non-generic failed result.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public static void OrThrowInvalidOperationException(this Result result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.ToJson());
    }
}
