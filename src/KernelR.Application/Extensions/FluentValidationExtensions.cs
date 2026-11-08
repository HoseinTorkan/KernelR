namespace KernelR.Application.Extensions;

/// <summary>
/// Provides extension methods that integrate <see cref="Error"/> with
/// FluentValidation rule builders.
/// </summary>
/// <remarks>
/// Use <see cref="WithError{T, TProperty}"/> to attach an <see cref="Error"/>'s
/// message, code, and type to a validation rule, so that validation failures
/// carry the same metadata as failures produced by the domain layer.
/// </remarks>
public static class FluentValidationExtensions
{
    /// <summary>
    /// Sets the error message, code, and type from the specified <see cref="Error"/>.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="rule">The validation rule.</param>
    /// <param name="error">The error to use for message, code, and type.</param>
    /// <returns>The same rule builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="rule"/> or <paramref name="error"/> is <see langword="null"/>.
    /// </exception>
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(error);

        return rule
            .WithMessage(error.Text)
            .WithErrorCode(error.Code)
            .WithState(_ => error.ErrorType);
    }
}
