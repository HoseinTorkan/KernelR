using KernelR.AspNetCore.ErrorHandling;

namespace KernelR.AspNetCore.Extensions;

/// <summary>
/// Provides extension methods for configuring the KernelR ASP.NET Core pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the KernelR exception handler and status code pages middleware.
    /// </summary>
    /// <remarks>
    /// Call this method once in <c>Program.cs</c>, after <c>builder.Build()</c>:
    /// <code>
    /// app.UseKernelR();
    /// </code>
    /// </remarks>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseKernelR(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseKernelStatusCodePages();
        return app;
    }
}
