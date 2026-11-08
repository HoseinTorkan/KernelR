WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<KernelJsonExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<MvcOptions>(options =>
{
    options.Filters.Add<KernelModelStateFilter>();
});

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseKernelStatusCodePages();

// 👇 بدون [FromQuery] — IParsable<PhoneNumber> کار binding را انجام می‌دهد
app.MapGet("/api/test/phone", (PhoneNumber phone) =>
{
    return Results.Ok(new { phone = phone.Value });
});

app.MapPost("/api/test/money", (Money money) =>
{
    return Results.Ok(new { amount = money.Amount, currency = money.Currency.Value });
});

app.MapGet("/api/test/success", () =>
{
    Result<int> result = Result<int>.Success(42);
    return result.ToHttpResult();
});

app.MapGet("/api/test/failure", () =>
{
    Result<int> result = Result<int>.Failure(
        new Error("Test.NotFound", "The resource was not found.", ErrorType.NotFound));
    return result.ToHttpResult();
});

app.Run();

public partial class Program { }
