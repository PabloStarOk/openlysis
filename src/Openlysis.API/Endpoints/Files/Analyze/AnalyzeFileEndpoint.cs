using System.Security.Claims;

using ErrorOr;

using FastEndpoints;

using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Files.Services;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Endpoint to analyze a file.
/// </summary>
public class AnalyzeFileEndpoint : Endpoint<AnalyzeFileRequest, AnalyzeFileResponse>
{
    private readonly ILogger<AnalyzeFileEndpoint> _logger;
    private readonly IFileMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Gets the name of the endpoint.
    /// </summary>
    public static string Name { get; } = "AnalyzeFile";

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for debugging endpoint operations.</param>
    /// <param name="multiAnalysisService">The service used for analyzing files.</param>
    public AnalyzeFileEndpoint(
        ILogger<AnalyzeFileEndpoint> logger,
        IFileMultiAnalysisService multiAnalysisService)
    {
        _logger = logger;
        _multiAnalysisService = multiAnalysisService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        Post(string.Empty);
        Group<FileAnalysesGroup>();
        AllowFileUploads();
        Version(1);
        Description(
            b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Accepts<AnalyzeFileRequest>(contentType: "multipart/form-data");
                b.Produces<AnalyzeFileResponse>(StatusCodes.Status202Accepted);
                b.ProducesProblem(StatusCodes.Status400BadRequest);
                b.ProducesProblem(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Uploads a file.";
                s.Description = "Uploads a file to be analyzed.";
                s.RequestParam(r => r.File, "File to be analyzed.");
                s.RequestParam(r => r.Password, "Password of the file if it is protected (Not recommended to upload confidential files) (Optional).");
                s.RequestParam(r => r.IsPrivate, "If the file analysis is private. True is the default. (Optional)");
                s.RequestParam(r => r.Reanalyze, "If the file must analyzed again, instead of returning the last analysis. False is the default. (Optional).");
            });
    }

    /// <summary>
    /// Handles the file analysis request.
    /// </summary>
    /// <param name="request">The request containing the file to be analyzed.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task HandleAsync(AnalyzeFileRequest request, CancellationToken ct)
    {
        Claim userIdClaim = HttpContext.User.Claims.Single(c => c.Type is ClaimTypes.NameIdentifier);
        var userId = GlobalId.Parse(userIdClaim.Value);

#if DEBUG
        LogFileMetadata(request);
#endif

        await using var stream = request.File!.OpenReadStream();
        var fileData = new FileData(
            request.File.FileName,
            request.File.ContentType,
            request.Password,
            stream);
        var result = await _multiAnalysisService.AnalyzeAsync(
            userId,
            request.IsPrivate,
            request.Reanalyze,
            fileData,
            ct);

        if (result.IsError)
        {
            if (result.Errors.Any(e => e.Type is ErrorType.Unexpected))
            {
                await SendResultAsync(Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: "An internal error occured, try again later."));
                return;
            }

            var extensions = new Dictionary<string, object?>
            {
                {
                    "errors", result.Errors
                },
            };
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "One or more errors occurred.",
                extensions: extensions));
            return;
        }

        Response = AnalyzeFileResponse.Parse(result.Value);

        var routeValues = new Dictionary<string, string>
        {
            { "id", Response.Id },
        };

        await SendResultAsync(Results.AcceptedAtRoute(
            GetAnalysisById.GetAnalysisByIdEndpoint.Name,
            routeValues,
            Response));
    }

#if DEBUG
    /// <summary>
    /// Logs metadata about the file for debugging purposes.
    /// </summary>
    /// <param name="request">The file analysis request containing file.</param>
    private void LogFileMetadata(AnalyzeFileRequest request)
    {
        _logger.LogTrace(
            "File analysis requested:"
            + "\n\tFilename: {Filename}"
            + "\n\tContent type: {ContentType}"
            + "\n\tFile password: {Password}",
            request.File?.FileName,
            request.File?.ContentType,
            request.Password);
    }
#endif
}