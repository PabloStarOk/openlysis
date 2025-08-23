using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using ErrorOr;

using FastEndpoints;

using NJsonSchema;
using NJsonSchema.Annotations;

using Openlysis.API.Endpoints.Common.Responses;
using Openlysis.API.Services.Abstractions;
using Openlysis.Application.Files.Services;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Endpoint to analyze a file.
/// </summary>
public class AnalyzeFileEndpoint : EndpointWithoutRequest<AnalysisIdentifiers>
{
    private const string Name = "AnalyzeFile";

    private readonly ILogger<AnalyzeFileEndpoint> _logger;
    private readonly IFileMultiAnalysisService _multiAnalysisService;
    private readonly MultipartRequestParser<AnalyzeFileRequest> _requestParser;
    private readonly Validator<AnalyzeFileRequest> _requestValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileEndpoint"/> class.
    /// </summary>
    /// <param name="logger">Logger for tracing and debugging.</param>
    /// <param name="multiAnalysisService">Service to perform multi-file analysis.</param>
    /// <param name="requestParser">Parses multipart file analysis requests.</param>
    /// <param name="requestValidator">Validates file analysis requests.</param>
    public AnalyzeFileEndpoint(
        ILogger<AnalyzeFileEndpoint> logger,
        IFileMultiAnalysisService multiAnalysisService,
        MultipartRequestParser<AnalyzeFileRequest> requestParser,
        Validator<AnalyzeFileRequest> requestValidator)
    {
        _logger = logger;
        _multiAnalysisService = multiAnalysisService;
        _requestParser = requestParser;
        _requestValidator = requestValidator;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        Post(string.Empty);
        Group<FileAnalysesGroup>();
        AllowFileUploads(dontAutoBindFormData: false);
        Version(1);
        Description(
            b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Accepts<AnalyzeFileRequestDto>(contentType: "multipart/form-data");
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
                s.Params[nameof(AnalyzeFileRequestDto.File)] = "File to be analyzed. If there are multiple files, only the first one will be accepted. Default content type is `application/octet-stream.`";
                s.Params[nameof(AnalyzeFileRequestDto.Password)] = "Password of the file if it is protected `(Not recommended to upload confidential files)` `(Optional)`.";
                s.Params[nameof(AnalyzeFileRequestDto.IsPrivate)] = "If the file analysis is private. `True` is the default. `(Optional)`";
                s.Params[nameof(AnalyzeFileRequestDto.Reanalyze)] = "If the file must analyzed again, instead of returning the last analysis. `False` is the default. `(Optional)`.";
            });
    }

    /// <summary>
    /// Handles the file analysis request.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        string userIdClaim = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var userId = GlobalId.Parse(userIdClaim);

        var request = await _requestParser.ParseAsync(FormMultipartSectionsAsync(ct), ct);
        var validationResult = await _requestValidator.ValidateAsync(request, ct);
        ValidationFailures.AddRange(validationResult.Errors);
        ThrowIfAnyErrors();

#if DEBUG
        LogFileMetadata(request);
#endif

        var result = await _multiAnalysisService.AnalyzeAsync(
            userId,
            request.File,
            request.Password,
            request.IsPrivate,
            request.Reanalyze,
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

        Response = AnalysisIdentifiers.Parse(result.Value);

        var routeValues = new Dictionary<string, string>
        {
            { "id", Response.Id },
        };

        await SendResultAsync(Results.AcceptedAtRoute(
            GetAnalysisById.GetAnalysisByIdEndpoint.Name,
            routeValues,
            Response));
    }

    /// <summary>
    /// DTO for documentation purposes.
    /// </summary>
    /// <param name="File">File to be analyzed.</param>
    /// <param name="Password">Password for the file if protected.</param>
    /// <param name="IsPrivate">Indicates if the analysis is private. Default is true.</param>
    /// <param name="Reanalyze">Indicates if the file should be reanalyzed. Default is false.</param>
    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local", Justification = "Documentation purposes.")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Documentation purposes.")]
    private sealed record AnalyzeFileRequestDto(
        [property: JsonSchema(JsonObjectType.File)] IFormFile File,
        string? Password,
        bool IsPrivate = true,
        bool Reanalyze = false);

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