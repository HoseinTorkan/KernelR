using System.Diagnostics.CodeAnalysis;

namespace KernelR.Domain.Results.Extensions;

/// <summary>
/// Provides extension methods to extract values from <see cref="Result{T}"/> instances.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Attempts to extract the success value without throwing when the result represents a failure.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="value">
    /// When this method returns, contains the success value if the result succeeds;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the result is successful; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    public static bool TryGetValue<T>(this Result<T> result, [MaybeNullWhen(false)] out T value)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            value = default;
            return false;
        }

        value = result.Value;
        return true;
    }
}
