using KernelR.AspNetCore.Conversions.JsonConverters.ValueObjects;
using KernelR.AspNetCore.ErrorHandling;
using KernelR.AspNetCore.Swagger;

namespace KernelR.AspNetCore.Extensions;

/// <summary>
/// Provides extension methods for registering KernelR ASP.NET Core services.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers all KernelR value-object converters, binding filters, exception handlers,
    /// and Swagger schema filters with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="format">
    /// The wire format for value objects that support multiple representations
    /// (e.g., <c>DataSize</c>, <c>Size</c>, <c>Money</c>).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddKernelR(
        this IServiceCollection services,
        ValueObjectJsonFormat format = ValueObjectJsonFormat.Structured)
    {
        // 👇 ۱. ثبت JSON Converterها (برای body)
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new ErrorJsonConverter());
            options.SerializerOptions.Converters.Add(new PhoneNumberJsonConverter());
            options.SerializerOptions.Converters.Add(new EmailJsonConverter());
            options.SerializerOptions.Converters.Add(new PersonNameJsonConverter());
            options.SerializerOptions.Converters.Add(new NationalCodeJsonConverter());
            options.SerializerOptions.Converters.Add(new UrlJsonConverter());
            options.SerializerOptions.Converters.Add(new CurrencyCodeJsonConverter());

            // 👇 Converterهای چندفرمتی
            options.SerializerOptions.Converters.Add(new DataSizeJsonConverter(format));
            options.SerializerOptions.Converters.Add(new SizeJsonConverter(format));
            options.SerializerOptions.Converters.Add(new MoneyJsonConverter(format));
        });

        // 👇 ۲. ثبت TypeConverterها (برای MVC query/form/route)
        TypeDescriptor.AddAttributes(typeof(PhoneNumber), new TypeConverterAttribute(typeof(PhoneNumberTypeConverter)));
        TypeDescriptor.AddAttributes(typeof(Email), new TypeConverterAttribute(typeof(EmailTypeConverter)));
        TypeDescriptor.AddAttributes(typeof(PersonName), new TypeConverterAttribute(typeof(PersonNameTypeConverter)));
        TypeDescriptor.AddAttributes(typeof(NationalCode), new TypeConverterAttribute(typeof(NationalCodeTypeConverter)));
        TypeDescriptor.AddAttributes(typeof(Url), new TypeConverterAttribute(typeof(UrlTypeConverter)));
        TypeDescriptor.AddAttributes(typeof(CurrencyCode), new TypeConverterAttribute(typeof(CurrencyCodeTypeConverter)));
        TypeDescriptor.AddAttributes(typeof(DataSize), new TypeDescriptorAttribute(typeof(DataSizeTypeConverter)));
        TypeDescriptor.AddAttributes(typeof(Size), new TypeConverterAttribute(typeof(SizeTypeConverter)));
        TypeDescriptor.AddAttributes(typeof(Money), new TypeConverterAttribute(typeof(MoneyTypeConverter)));

        // 👇 ۳. ثبت Exception Handler
        services.AddExceptionHandler<KernelRExceptionHandler>();

        // 👇 ۴. ثبت ModelStateFilter برای MVC
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<KernelModelStateFilter>();
        });

        // 👇 ۵. ثبت Endpoint Filter برای Minimal API
        services.AddSingleton<ValueObjectEndpointFilter>();

        return services;
    }

    /// <summary>
    /// Registers the KernelR Swagger schema filter.
    /// </summary>
    /// <param name="options">The <see cref="SwaggerGenOptions"/> to add the filter to.</param>
    /// <returns>The same <see cref="SwaggerGenOptions"/> instance for chaining.</returns>
    public static SwaggerGenOptions AddKernelRSwagger(this SwaggerGenOptions options)
    {
        options.SchemaFilter<KernelEnumSchemaFilter>();
        return options;
    }
}
