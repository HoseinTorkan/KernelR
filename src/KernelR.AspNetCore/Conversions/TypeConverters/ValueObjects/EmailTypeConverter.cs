namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="TypeConverter"/> that binds a string to an
/// <see cref="Email"/>.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="Email.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class EmailTypeConverter : BaseValueObjectTypeConverter<Email>
{
    /// <inheritdoc />
    protected override Result<Email> Create(string value) => Email.Create(value);
}
