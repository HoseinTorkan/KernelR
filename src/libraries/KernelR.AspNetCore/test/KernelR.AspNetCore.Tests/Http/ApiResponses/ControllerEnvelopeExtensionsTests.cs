namespace KernelAbstractions.AspNetCore.Tests.Http.ApiResponses;

public class ControllerEnvelopeExtensionsTests
{
    [Fact]
    public void EnvelopeResult_Generic_Success_ReturnsOkWithApiResponse()
    {
        var controller = CreateController();
        var result = Result<int>.Success(42);

        var actionResult = controller.EnvelopeResult(result);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        var response = Assert.IsType<ApiResponse<int>>(objectResult.Value);
        Assert.True(response.IsSuccess);
        Assert.Equal(42, response.Value);
    }

    [Fact]
    public void EnvelopeResult_Generic_Failure_ReturnsProblemDetails()
    {
        var controller = CreateController();
        var error = new Error("Test.Code", "Test error.", ErrorType.ValidationError);
        var result = Result<int>.Failure(error);

        var actionResult = controller.EnvelopeResult(result);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Test.Code", problem.Title);
        Assert.Equal("Test error.", problem.Detail);
    }

    [Fact]
    public void EnvelopeResult_NonGeneric_Success_ReturnsOkWithApiResponse()
    {
        var controller = CreateController();
        var result = Result.Success();

        var actionResult = controller.EnvelopeResult(result);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        var response = Assert.IsType<ApiResponse>(objectResult.Value);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public void EnvelopeResult_NonGeneric_Failure_ReturnsProblemDetails()
    {
        var controller = CreateController();
        var error = new Error("Test.Code", "Test error.", ErrorType.NotFound);
        var result = Result.Failure(error);

        var actionResult = controller.EnvelopeResult(result);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    private static TestController CreateController()
    {
        var httpContext = new DefaultHttpContext();
        return new TestController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private sealed class TestController : ControllerBase { }
}
