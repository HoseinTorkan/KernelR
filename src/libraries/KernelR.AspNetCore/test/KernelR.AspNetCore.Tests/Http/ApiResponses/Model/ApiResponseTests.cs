namespace KernelAbstractions.AspNetCore.Tests.Http.ApiResponses.Model;

public class ApiResponseTests
{
    [Fact]
    public void Success_SetsIsSuccessTrueAndErrorNull()
    {
        var response = ApiResponse.Success();

        Assert.True(response.IsSuccess);
        Assert.False(response.IsFailure);
        Assert.Null(response.Error);
    }

    [Fact]
    public void Failure_SetsIsSuccessFalseAndErrorNotNull()
    {
        var error = new Error("Code", "Text", ErrorType.ValidationError);

        var response = ApiResponse.Failure(error);

        Assert.False(response.IsSuccess);
        Assert.True(response.IsFailure);
        Assert.Equal(error, response.Error);
    }

    [Fact]
    public void Failure_WithNullError_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ApiResponse.Failure(null!));
    }

    [Fact]
    public void Timestamp_IsSetOnCreation()
    {
        var before = DateTimeOffset.UtcNow;
        var response = ApiResponse.Success();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(response.Timestamp, before, after);
    }
}

public class ApiResponseGenericTests
{
    [Fact]
    public void Success_SetsValueAndIsSuccess()
    {
        var response = ApiResponse<int>.Success(42);

        Assert.True(response.IsSuccess);
        Assert.Equal(42, response.Value);
        Assert.Null(response.Error);
    }

    [Fact]
    public void Success_WithNullReferenceType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ApiResponse<string>.Success(null!));
    }

    [Fact]
    public void Success_WithNullNullableValueType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ApiResponse<int?>.Success(null));
    }

    [Fact]
    public void Success_WithValueType_DoesNotThrow()
    {
        var response = ApiResponse<int>.Success(0);

        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.Value);
    }

    [Fact]
    public void Failure_SetsErrorAndIsFailure()
    {
        var error = new Error("Code", "Text", ErrorType.ValidationError);

        var response = ApiResponse<int>.Failure(error);

        Assert.False(response.IsSuccess);
        Assert.True(response.IsFailure);
        Assert.Equal(error, response.Error);
        Assert.Equal(0, response.Value);
    }
}
