using System.Security.Claims;

using ErrorOr;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Files.Services;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Endpoint to analyze a file.
/// </summary>
public class AnalyzeFileEndpoint : Endpoint<AnalyzeFileRequest, AnalyzeFileResponse>
{
    private readonly IFileMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Gets the name of the endpoint.
    /// </summary>
    public static string Name { get; } = "AnalyzeFile";

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileEndpoint"/> class.
    /// </summary>
    /// <param name="multiAnalysisService">The service used for analyzing files.</param>
    public AnalyzeFileEndpoint(IFileMultiAnalysisService multiAnalysisService)
    {
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
                b.ProducesProblemDetails();
                b.ProducesProblemDetails(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Uploads a file.";
                s.Description = "Uploads a file to be analyzed.";
                s.RequestParam(r => r.File, "File to be analyzed.");
                s.RequestParam(r => r.FileDescription, "Description of the file (Optional).");
                s.RequestParam(r => r.FilePassword, "Password of the file if it is protected (Not recommended to upload confidential files) (Optional).");
                s.RequestParam(r => r.IsPrivate, "If the file analysis is private. True is the default. (Optional)");
                s.RequestParam(r => r.Reanalyze, "If the file must analyzed again, instead of returning the last analysis. False is the default. (Optional).");
            });
        DontThrowIfValidationFails();
    }

    /// <summary>
    /// Handles the file analysis request.
    /// </summary>
    /// <param name="request">The request containing the file to be analyzed.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task HandleAsync(AnalyzeFileRequest request, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await SendResultAsync(ValidationFailures.AsValidationProblem());
            return;
        }

        Claim claim = HttpContext.User.Claims.Single(c => c.Type is ClaimTypes.NameIdentifier);
        var userId = UserId.Create(Guid.Parse(claim.Value));

        await using var stream = request.File!.OpenReadStream();
        var fileData = new FileData(
            request.File.FileName,
            request.File.ContentType,
            request.FileDescription,
            request.FilePassword,
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

        Response = new AnalyzeFileResponse(
            result.Value.Id.Value.ToString(),
            result.Value.DataHashSet.Md5,
            result.Value.DataHashSet.Sha1,
            result.Value.DataHashSet.Sha256,
            result.Value.DataHashSet.Sha512);

        var routeValues = new Dictionary<string, string>
        {
            { "id", Response.FileAnalysisId },
        };

        await SendResultAsync(Results.AcceptedAtRoute(
            GetAnalysisById.GetAnalysisByIdEndpoint.Name,
            routeValues,
            Response));
    }
}