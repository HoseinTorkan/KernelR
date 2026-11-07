using KernelR.AspNetCore.ApiResponses.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace KernelR.AspNetCore.E2ETests;

public class EndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndToEndTests(WebApplicationFactory<Program> factory)
    {
        // 👇 کلید حل مشکل ContentRoot: قبل از CreateClient، متغیر محیطی را تنظیم می‌کنی
        // تا WebApplicationFactory به جای جستجوی فایل .sln/.slnx،
        // مستقیماً از دایرکتوری خروجی تست به عنوان ContentRoot استفاده کند.
        Environment.SetEnvironmentVariable(
            "ASPNETCORE_TEST_CONTENTROOT_KERNELABSTRACTIONS_ASPNETCORE_E2ETESTS",
            AppContext.BaseDirectory);

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ValidPhoneNumber_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/test/phone?phone=09121234567");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_InvalidPhoneNumber_ReturnsProblemDetailsWith400()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/test/phone?phone=123");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);

        // 👇 Minimal API binding failure با BadHttpRequestException پرتاب می‌شود
        // که پیام آن الگوی "<code>: <text>" را ندارد، پس fallback فعال می‌شود.
        Assert.Equal("Kernel.Validation.InvalidInput", problem.Title);
    }

    [Fact]
    public async Task Post_ValidMoney_ReturnsOk()
    {
        StringContent content = new(
            """{"amount": 100, "currency": "IRR"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await _client.PostAsync("/api/test/money", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_InvalidMoney_ReturnsProblemDetailsWith400()
    {
        StringContent content = new(
            """{"amount": 100, "currency": "XYZ"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await _client.PostAsync("/api/test/money", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);

        // 👇 MoneyJsonConverter یک JsonException با پیام
        // "Kernel.ValueObjects.CurrencyCode.INVALID: ..." پرتاب می‌کند
        // که از InnerException استخراج و به عنوان title استفاده می‌شود.
        Assert.Equal("Kernel.ValueObjects.CurrencyCode.INVALID", problem.Title);
    }

    [Fact]
    public async Task Get_SuccessEndpoint_ReturnsApiResponse()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/test/success");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<int>? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.IsSuccess);
        Assert.Equal(42, apiResponse.Value);
    }

    [Fact]
    public async Task Get_FailureEndpoint_ReturnsProblemDetailsWith404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/test/failure");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_UnknownRoute_ReturnsProblemDetailsWith404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
