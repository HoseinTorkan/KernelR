namespace KernelAbstractions.AspNetCore.Tests.Http.ErrorHandling;

public class KernelJsonExceptionHandlerTests
{
    private readonly KernelJsonExceptionHandler _handler = new();

    [Fact]
    public async Task TryHandleAsync_JsonExceptionWithDomainError_ReturnsValidationError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var jsonException = new JsonException(
            "Kernel.ValueObjects.PhoneNumber.INVALID_FORMAT: Phone number format is invalid.");

        // Act
        bool handled = await _handler.TryHandleAsync(context, jsonException, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        // Read the body to verify ProblemDetails
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        string body = await reader.ReadToEndAsync();

        Assert.Contains("Kernel.ValueObjects.PhoneNumber.INVALID_FORMAT", body);
        Assert.Contains("Phone number format is invalid.", body);
    }

    [Fact]
    public async Task TryHandleAsync_JsonExceptionWithoutDomainError_ReturnsGenericValidationError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var jsonException = new JsonException("Unexpected end of JSON input.");

        // Act
        bool handled = await _handler.TryHandleAsync(context, jsonException, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        string body = await reader.ReadToEndAsync();

        Assert.Contains("Kernel.Validation.InvalidJson", body);
    }

    [Fact]
    public async Task TryHandleAsync_OtherException_ReturnsInternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException("Something went wrong.");

        // Act
        bool handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        string body = await reader.ReadToEndAsync();

        Assert.Contains("Kernel.Infrastructure.UnhandledException", body);
        Assert.DoesNotContain("Something went wrong.", body); // Should not leak internal details
    }
}
