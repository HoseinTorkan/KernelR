namespace KernelR.AspNetCore.Conversions.TypeConverters;

/// <summary>
/// A generic <see cref="TypeConverter"/> base class for KernelAbstractions value objects
/// that are created from a single string via a <c>Create(string)</c> factory returning
/// <see cref="Result{T}"/>.
/// </summary>
/// <typeparam name="T">The value object type. Must be a value type (struct).</typeparam>
/// <remarks>
/// <para>
/// Concrete converters only need to derive from this class and supply the
/// <c>Create</c> factory:
/// </para>
/// <code>
/// public sealed class EmailTypeConverter : BaseValueObjectTypeConverter&lt;Email&gt;
/// {
///     protected override Result&lt;Email&gt; Create(string value) => Email.Create(value);
/// }
/// </code>
/// <para>
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON with
/// <b>camelCase</b> property names and thrown inside a <see cref="FormatException"/>
/// message, exactly as the Web layer's <c>KernelModelStateFilter</c> expects.
/// </para>
/// <para>
/// <b>Serialization contract:</b> The options used here must stay identical to
/// <c>KernelModelStateFilter</c>'s. Both sides use <see cref="ErrorSerialization.Options"/>,
/// a shared instance in the ASP.NET Core package.
/// </para>
/// </remarks>
public abstract class BaseValueObjectTypeConverter<T> : TypeConverter
    where T : struct
{
    /// <summary>
    /// Creates the value object from a string.
    /// </summary>
    /// <param name="value">The raw string input.</param>
    /// <returns>
    /// A successful result carrying the value object, or a failure carrying the
    /// domain error to report.
    /// </returns>
    protected abstract Result<T> Create(string value);

    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            Result<T> result = Create(stringValue);

            if (result.IsFailure)
            {
                string errorJson = JsonSerializer.Serialize(result.Error, ErrorSerialization.Options);
                throw new FormatException(errorJson);
            }

            return result.Value;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
