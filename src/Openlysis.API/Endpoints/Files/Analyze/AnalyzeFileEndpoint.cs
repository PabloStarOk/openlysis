using ErrorOr;

using FastEndpoints;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

using Openlysis.API.Endpoints.Common.Responses;
using Openlysis.API.Middlewares.Files;
using Openlysis.Application.Common.Models;
using Openlysis.Application.Files.Services;
using Openlysis.Domain.Files;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Endpoint to analyze a file.
/// </summary>
public class AnalyzeFileEndpoint : Endpoint<AnalyzeFileRequest, AnalysisIdentifiers>
{
    private const string Name = "AnalyzeFile";

    private readonly ILogger<AnalyzeFileEndpoint> _logger;
    private readonly IFileMultiAnalysisService _multiAnalysisService;
    private readonly IOptions<FormOptions> _formOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileEndpoint"/> class.
    /// </summary>
    /// <param name="logger">Logger for tracing and debugging.</param>
    /// <param name="multiAnalysisService">Service to perform multi-file analysis.</param>
    /// <param name="formOptions">Options for form data, including file upload limits.</param>
    public AnalyzeFileEndpoint(
        ILogger<AnalyzeFileEndpoint> logger,
        IFileMultiAnalysisService multiAnalysisService,
        IOptions<FormOptions> formOptions)
    {
        _logger = logger;
        _multiAnalysisService = multiAnalysisService;
        _formOptions = formOptions;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        Post(string.Empty);
        PostProcessor<FileStorageCleanupPostProcessor<AnalyzeFileRequest, AnalysisIdentifiers>>();
        Group<FileAnalysesGroup>();
        AllowFileUploads(dontAutoBindFormData: true);
        Version(1);
        Description(
            b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Accepts<AnalyzeFileRequest>(contentType: "multipart/form-data");
                b.Produces<AnalysisIdentifiers>();
                b.Produces<AnalysisIdentifiers>(StatusCodes.Status202Accepted);
                b.ProducesProblem(StatusCodes.Status400BadRequest);
                b.ProducesProblem(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Uploads a file.";
                s.Description = "Uploads a file to be analyzed.";
                s.Responses[StatusCodes.Status200OK] = "Analysis result successfully retrieved.";
                s.Responses[StatusCodes.Status202Accepted] = "Analysis request accepted and queued for processing.";
                s.RequestParam(x => x.File, $"File to be analyzed. If there are multiple files, only the first one will be accepted. Default content type is `application/octet-stream.` Max file size: `{_formOptions.Value.MultipartBodyLengthLimit}` bytes.");
                s.RequestParam(x => x.Password, "Password of the file if it is protected `(Not recommended to upload confidential files)` `(Optional)`.");
                s.RequestParam(x => x.IsPrivate, "If the file analysis is private. `True` is the default. `(Optional)`");
                s.RequestParam(x => x.Reanalyze, "If the file must analyzed again, instead of returning the last analysis. `False` is the default. `(Optional)`.");
            });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AnalyzeFileRequest req, CancellationToken ct)
    {
#if DEBUG
        LogFileMetadata(req);
#endif

        var result = await _multiAnalysisService.AnalyzeAsync(
            req.UserId,
            req.File,
            req.Password,
            req.IsPrivate,
            req.Reanalyze,
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

        AnalysisRequestResult<FileMultiAnalysis> requestResult = result.Value;
        var analysisIdentifiers = AnalysisIdentifiers.Parse(requestResult.Analysis);

        // Retrieved final analysis result 200.
        if (requestResult.RequestStatus is AnalysisRequestStatus.Retrieved)
        {
            await SendOkAsync(analysisIdentifiers, CancellationToken.None);
            return;
        }

        // Retrieved queued analysis 202.
        var routeValues = new Dictionary<string, string>
        {
            { "id", analysisIdentifiers.Id },
        };

        await SendResultAsync(Results.AcceptedAtRoute(
            GetAnalysisById.GetAnalysisByIdEndpoint.Name,
            routeValues,
            analysisIdentifiers));
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
            request.File.Metadata.Name,
            request.File.Metadata.ContentType,
            request.Password);
    }
#endif
}