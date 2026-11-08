namespace KernelR.Application.Services.PdfService;

/// <summary>
/// Provides functionality to generate PDF documents.
/// </summary>
/// <remarks>
/// Implementations are template-engine-agnostic and rendering-engine-agnostic
/// (QuestPDF, iText, PuppeteerSharp, etc.). Template lookup is the
/// implementation's responsibility (embedded resources, files, database, ...).
/// </remarks>
public interface IPdfService
{
    /// <summary>
    /// Generates a PDF document from raw HTML content.
    /// </summary>
    /// <param name="htmlContent">The complete HTML document to convert to PDF.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The generated PDF file as a byte array, or a failure if generation fails.
    /// </returns>
    Task<Result<byte[]>> GenerateFromHtmlAsync(
        string htmlContent,
        CancellationToken cancellation = default);

    /// <summary>
    /// Generates a PDF document from a strongly-typed model using a predefined template.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model containing data for the template.
    /// </typeparam>
    /// <param name="templateName">The name of the predefined template to use.</param>
    /// <param name="model">The data model used to populate the template.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The generated PDF file as a byte array, or a failure if the template is
    /// not found or generation fails.
    /// </returns>
    Task<Result<byte[]>> GenerateFromTemplateAsync<TModel>(
        string templateName,
        TModel model,
        CancellationToken cancellation = default) where TModel : class;
}
