namespace KernelAbstractions.AspNetCore.Tests.Http.ApiResponses;

public class ResultConversionExtensionsTests
{
    [Fact]
    public void ToApiResponse_NonGeneric_Success_ReturnsSuccessResponse()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var response = result.ToApiResponse("trace-123");

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Null(response.Error);
        Assert.Equal("trace-123", response.TraceId);
    }

    [Fact]
    public void ToApiResponse_NonGeneric_Failure_ReturnsFailureResponse()
    {
        // Arrange
        var error = new Error("Test.Code", "Test error.", ErrorType.ValidationError);
        var result = Result.Failure(error);

        // Act
        var response = result.ToApiResponse("trace-123");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(error, response.Error);
        Assert.Equal("trace-123", response.TraceId);
    }

    [Fact]
    public void ToApiResponse_Generic_Success_ReturnsSuccessResponseWithValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var response = result.ToApiResponse("trace-123");

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(42, response.Value);
        Assert.Equal("trace-123", response.TraceId);
    }

    [Fact]
    public void ToApiResponse_Generic_Failure_ReturnsFailureResponse()
    {
        // Arrange
        var error = new Error("Test.Code", "Test error.", ErrorType.ValidationError);
        var result = Result<int>.Failure(error);

        // Act
        var response = result.ToApiResponse("trace-123");

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(error, response.Error);
        Assert.Equal("trace-123", response.TraceId);
    }

    [Fact]
    public void ToApiResponse_WithNullTraceId_GeneratesTraceId()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var response = result.ToApiResponse(null);

        // Assert
        Assert.NotNull(response.TraceId);
        Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
    }

    [Fact]
    public void ToApiResponse_WithWhitespaceTraceId_GeneratesTraceId()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var response = result.ToApiResponse("   ");

        // Assert
        Assert.NotNull(response.TraceId);
        Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
    }

    [Fact]
    public void ToApiResponse_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        Result<int> result = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.ToApiResponse());
    }

    [Fact]
    public void ResolveTraceId_WhenActivityExists_ReturnsActivityId()
    {
        // Arrange
        using var activity = new Activity("TestActivity").Start();

        // Act
        string traceId = ResultConversionExtensions.ResolveTraceId();

        // Assert
        Assert.Equal(activity.Id, traceId);
    }

    [Fact]
    public void ResolveTraceId_WhenActivityIsNull_ReturnsGuid()
    {
        // Arrange
        Activity.Current = null;

        // Act
        string traceId = ResultConversionExtensions.ResolveTraceId();

        // Assert
        Assert.NotNull(traceId);
        Assert.True(Guid.TryParseExact(traceId, "N", out _));
    }
}
