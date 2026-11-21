namespace KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;

/// <summary>
/// Specifies the JSON wire format for value objects that support multiple representations.
/// </summary>
public enum ValueObjectJsonFormat
{
    /// <summary>
    /// Structured JSON form, e.g. <c>{ "value": 5, "unit": "Gigabyte" }</c>.
    /// </summary>
    Structured = 0,

    /// <summary>
    /// Flat string form, e.g. <c>"5 Gigabyte"</c>.
    /// </summary>
    Flat = 1
}
