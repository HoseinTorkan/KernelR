namespace KernelAbstractions.AspNetCore.Tests.Http.Swagger;

public class KernelEnumSchemaFilterTests
{
    private readonly KernelEnumSchemaFilter _filter = new();

    [Fact]
    public void Apply_ForSizeUnit_RewritesSchemaAsStringEnum()
    {
        OpenApiSchema schema = new();
        SchemaFilterContext context = CreateContext(typeof(SizeUnit));

        _filter.Apply(schema, context);

        Assert.Equal("string", schema.Type);
        Assert.Null(schema.Format);
        Assert.NotNull(schema.Enum);
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Millimeter");
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Centimeter");
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Meter");
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Inch");
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Foot");
    }

    [Fact]
    public void Apply_ForDataSizeUnit_RewritesSchemaAsStringEnum()
    {
        OpenApiSchema schema = new();
        SchemaFilterContext context = CreateContext(typeof(DataSizeUnit));

        _filter.Apply(schema, context);

        Assert.Equal("string", schema.Type);
        Assert.Null(schema.Format);
        Assert.NotNull(schema.Enum);
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Byte");
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Kilobyte");
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Megabyte");
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Gigabyte");
        Assert.Contains(schema.Enum, e => e is OpenApiString s && s.Value == "Terabyte");
    }

    [Fact]
    public void Apply_ForOtherType_DoesNotModifySchema()
    {
        OpenApiSchema schema = new() { Type = "integer" };
        SchemaFilterContext context = CreateContext(typeof(int));

        _filter.Apply(schema, context);

        Assert.Equal("integer", schema.Type);
        Assert.Empty(schema.Enum);
    }

    private static SchemaFilterContext CreateContext(Type type)
    {
        SchemaFilterContext context = (SchemaFilterContext)RuntimeHelpers.GetUninitializedObject(
            typeof(SchemaFilterContext));

        FieldInfo? backingField = typeof(SchemaFilterContext)
            .GetField("<Type>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        if (backingField is not null)
        {
            backingField.SetValue(context, type);
        }
        else
        {
            PropertyInfo? property = typeof(SchemaFilterContext).GetProperty("Type");
            property?.SetValue(context, type);
        }

        return context;
    }
}
