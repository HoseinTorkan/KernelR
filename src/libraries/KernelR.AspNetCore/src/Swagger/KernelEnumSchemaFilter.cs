namespace KernelR.AspNetCore.Swagger;

/// <summary>
/// A schema filter that renders KernelR enum units as strings with a fixed
/// set of allowed values, so Swagger UI displays them as dropdowns instead of integers.
/// </summary>
/// <remarks>
/// <para>
/// By default, Swashbuckle renders enums as integers, which forces API consumers to
/// remember the numeric mapping. This filter changes <c>SizeUnit</c> and
/// <c>DataSizeUnit</c> to <c>string</c> schemas with an explicit <c>enum</c> list,
/// matching the string representation used by the JSON converters.
/// </para>
/// <para>
/// <b>Registration:</b> Add this filter in <c>AddSwaggerGen</c>:
/// </para>
/// <code>
/// builder.Services.AddSwaggerGen(options =>
/// {
///     options.SchemaFilter&lt;KernelEnumSchemaFilter&gt;();
/// });
/// </code>
/// </remarks>
public sealed class KernelEnumSchemaFilter : ISchemaFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(SizeUnit))
        {
            ApplyStringEnum<SizeUnit>(schema);
        }
        else if (context.Type == typeof(DataSizeUnit))
        {
            ApplyStringEnum<DataSizeUnit>(schema);
        }
    }

    /// <summary>
    /// Rewrites the schema to a string enum using the names of <typeparamref name="TEnum"/>.
    /// </summary>
    private static void ApplyStringEnum<TEnum>(OpenApiSchema schema)
        where TEnum : struct, Enum
    {
        schema.Type = "string";
        schema.Format = null;

        schema.Enum = [.. Enum.GetNames(typeof(TEnum)).Select(name => (IOpenApiAny)new OpenApiString(name))];
    }
}
