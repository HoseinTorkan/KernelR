namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="TypeConverter"/> that binds a string of the form
/// <c>"&lt;value&gt; &lt;unit&gt;"</c> to a <see cref="DataSize"/> value object.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="DataSize.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class DataSizeTypeConverter : BaseValueObjectTypeConverter<DataSize>
{
    /// <inheritdoc />
    protected override Result<DataSize> Create(string value) => DataSize.Create(value);
}
