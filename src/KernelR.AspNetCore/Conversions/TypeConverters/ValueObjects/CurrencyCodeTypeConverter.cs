namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="TypeConverter"/> that binds a string to a
/// <see cref="CurrencyCode"/>.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="CurrencyCode.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class CurrencyCodeTypeConverter : BaseValueObjectTypeConverter<CurrencyCode>
{
    /// <inheritdoc />
    protected override Result<CurrencyCode> Create(string value) => CurrencyCode.Create(value);
}
