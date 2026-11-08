namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="TypeConverter"/> that binds a string to a
/// <see cref="NationalCode"/>.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="NationalCode.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class NationalCodeTypeConverter : BaseValueObjectTypeConverter<NationalCode>
{
    /// <inheritdoc />
    protected override Result<NationalCode> Create(string value) => NationalCode.Create(value);
}
