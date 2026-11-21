namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="System.ComponentModel.TypeConverter"/> that binds a string to a
/// <see cref="PhoneNumber"/>.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="PhoneNumber.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class PhoneNumberTypeConverter : BaseValueObjectTypeConverter<PhoneNumber>
{
    /// <inheritdoc />
    protected override Result<PhoneNumber> Create(string value) => PhoneNumber.Create(value);
}
