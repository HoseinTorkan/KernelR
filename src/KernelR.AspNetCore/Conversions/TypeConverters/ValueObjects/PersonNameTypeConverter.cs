namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="System.ComponentModel.TypeConverter"/> that binds a string to a
/// <see cref="PersonName"/>.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="PersonName.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class PersonNameTypeConverter : BaseValueObjectTypeConverter<PersonName>
{
    /// <inheritdoc />
    protected override Result<PersonName> Create(string value) => PersonName.Create(value);
}
