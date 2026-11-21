namespace KernelAbstractions.AspNetCore.Tests.Http.ApiResponses;

public class MinimalApiEnvelopeExtensionsTests
{
    [Fact]
    public void ToHttpResult_Generic_Success_ReturnsOk()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var okResult = Assert.IsType<Ok<ApiResponse<int>>>(httpResult);
        Assert.NotNull(okResult.Value);
        Assert.True(okResult.Value.IsSuccess);
        Assert.Equal(42, okResult.Value.Value);
    }

    [Fact]
    public void ToHttpResult_Generic_Failure_ReturnsProblem()
    {
        // Arrange
        var error = new Error("Test.Code", "Test error.", ErrorType.ValidationError);
        var result = Result<int>.Failure(error);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(httpResult);
        Assert.Equal(400, problemResult.StatusCode);
    }

    [Fact]
    public void ToHttpResult_NonGeneric_Success_ReturnsOk()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var okResult = Assert.IsType<Ok<ApiResponse>>(httpResult);
        Assert.NotNull(okResult.Value);
        Assert.True(okResult.Value.IsSuccess);
    }

    [Fact]
    public void ToHttpResult_NonGeneric_Failure_ReturnsProblem()
    {
        // Arrange
        var error = new Error("Test.Code", "Test error.", ErrorType.NotFound);
        var result = Result.Failure(error);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(httpResult);
        Assert.Equal(404, problemResult.StatusCode);
    }

    [Fact]
    public void ToHttpResult_UsesProvidedTraceId()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var traceId = "custom-trace-id";

        // Act
        var httpResult = result.ToHttpResult(traceId);

        // Assert
        var okResult = Assert.IsType<Ok<ApiResponse<int>>>(httpResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(traceId, okResult.Value.TraceId);
    }
}
