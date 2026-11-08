namespace KernelR.AspNetCore.Conversions.TypeConverters.ValueObjects;

/// <summary>
/// A <see cref="TypeConverter"/> that binds a string of the form
/// <c>"&lt;amount&gt; &lt;currency&gt;"</c> to a <see cref="Money"/> value object.
/// </summary>
/// <remarks>
/// Validation goes through <see cref="Money.Create(string)"/>.
/// If the input is invalid, the domain <see cref="Error"/> is serialized to JSON
/// and thrown inside a <see cref="FormatException"/>.
/// </remarks>
public sealed class MoneyTypeConverter : BaseValueObjectTypeConverter<Money>
{
    /// <inheritdoc />
    protected override Result<Money> Create(string value) => Money.Create(value);
}
