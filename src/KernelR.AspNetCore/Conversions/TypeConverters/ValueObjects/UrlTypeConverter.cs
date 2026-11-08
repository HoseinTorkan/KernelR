namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="TypeConverter"/> that binds a string to a
/// <see cref="Url"/>.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="Url.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class UrlTypeConverter : BaseValueObjectTypeConverter<Url>
{
    /// <inheritdoc />
    protected override Result<Url> Create(string value) => Url.Create(value);
}
