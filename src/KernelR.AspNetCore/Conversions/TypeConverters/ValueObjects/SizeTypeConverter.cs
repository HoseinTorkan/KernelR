namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="TypeConverter"/> that binds a string of the form
/// <c>"&lt;value&gt; &lt;unit&gt;"</c> to a <see cref="Size"/> value object.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="Size.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class SizeTypeConverter : BaseValueObjectTypeConverter<Size>
{
    /// <inheritdoc />
    protected override Result<Size> Create(string value) => Size.Create(value);
}
