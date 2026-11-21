namespace KernelAbstractions.AspNetCore.Tests.Http.ErrorHandling;

public class KernelModelStateFilterTests
{
    private static readonly JsonSerializerOptions ErrorJsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = false,
    };

    [Fact]
    public void OnResultExecuting_ValidModelState_DoesNothing()
    {
        var modelState = new ModelStateDictionary();
        var context = CreateContext(modelState);
        var originalResult = context.Result;
        var filter = new KernelModelStateFilter();

        filter.OnResultExecuting(context);

        Assert.Same(originalResult, context.Result);
    }

    [Fact]
    public void OnResultExecuting_InvalidModelState_WithDomainErrorJson_ProducesProblemDetails()
    {
        var modelState = new ModelStateDictionary();
        var domainError = new Error(
            "Kernel.ValueObjects.PhoneNumber.INVALID_FORMAT",
            "Phone number format is invalid.",
            ErrorType.ValidationError);

        string errorJson = JsonSerializer.Serialize(domainError, ErrorJsonOptions);
        modelState.AddModelError("PhoneNumber", errorJson);

        var context = CreateContext(modelState);
        var filter = new KernelModelStateFilter();

        filter.OnResultExecuting(context);

        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(400, problem.Status);
        Assert.Equal("Kernel.ValueObjects.PhoneNumber.INVALID_FORMAT", problem.Title);
        Assert.Equal("Phone number format is invalid.", problem.Detail);
    }

    [Fact]
    public void OnResultExecuting_InvalidModelState_WithoutDomainError_ProducesFallback()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", "The Name field is required."); // Regular validation error

        var context = CreateContext(modelState);
        var filter = new KernelModelStateFilter();

        filter.OnResultExecuting(context);

        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(400, problem.Status);
        Assert.Equal("Kernel.Validation.InvalidInput", problem.Title);
        Assert.Equal("The Name field is required.", problem.Detail);
    }

    [Fact]
    public void OnResultExecuting_MultipleErrors_PrefersDomainErrorOverRegularError()
    {
        var modelState = new ModelStateDictionary();

        // First, add a regular error
        modelState.AddModelError("Name", "The Name field is required.");

        // Then add a domain error
        var domainError = new Error(
            "Kernel.ValueObjects.PhoneNumber.INVALID_FORMAT",
            "Phone number format is invalid.",
            ErrorType.ValidationError);
        string errorJson = JsonSerializer.Serialize(domainError, ErrorJsonOptions);
        modelState.AddModelError("PhoneNumber", errorJson);

        var context = CreateContext(modelState);
        var filter = new KernelModelStateFilter();

        filter.OnResultExecuting(context);

        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);

        // It should pick the domain error, not the fallback
        Assert.Equal("Kernel.ValueObjects.PhoneNumber.INVALID_FORMAT", problem.Title);
    }

    private static ResultExecutingContext CreateContext(ModelStateDictionary modelState)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        return new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new ObjectResult(null),
            controller: null!);
    }
}
