using ErrorOr;

using FastEndpoints;

using Openlysis.API.Endpoints.Common.Requests;
using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.Application.Files.Services;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;

namespace Openlysis.API.Endpoints.Files.GetAnalysisById;

/// <summary>
/// Endpoint for retrieving file analysis by hash.
/// </summary>
public class GetAnalysisByIdEndpoint : Endpoint<GetAnalysisByIdRequest, FileMultiAnalysisDto>
{
    /// <summary>
    /// The name identifier for the GetAnalysisById endpoint.
    /// </summary>
    public const string Name = "GetFileAnalysisById";

    private readonly IFileMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysisByIdEndpoint"/> class.
    /// </summary>
    /// <param name="multiAnalysisService">
    /// The service used to retrieve file analysis by ID.
    /// </param>
    public GetAnalysisByIdEndpoint(IFileMultiAnalysisService multiAnalysisService)
    {
        _multiAnalysisService = multiAnalysisService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        Get("analyses/{id}");
        Group<FileAnalysesGroup>();
        Version(1);
        Description(b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Produces<FileMultiAnalysisDto>();
                b.ProducesProblem(StatusCodes.Status400BadRequest);
                b.ProducesProblem(StatusCodes.Status404NotFound);
            });
        Summary(
            s =>
            {
                s.Summary = "Get a file analysis by ID.";
                s.Description = "Get a file analysis by its ID.";
                s.RequestParam(r => r.Id, "ID of the analysis to get.");
            });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetAnalysisByIdRequest req, CancellationToken ct)
    {
        GlobalId userId = GlobalId.Parse(req.UserId);

        if (!GlobalId.TryParse(req.Id, out GlobalId? globalId))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Provided ID has an invalid format."));
            return;
        }

        ErrorOr<FileMultiAnalysis> result = await _multiAnalysisService
            .GetAnalysisByIdAsync(userId, globalId, ct);

        if (result.IsError)
        {
            if (result.Errors.Any(e => e.Type is ErrorType.NotFound))
            {
                await SendResultAsync(Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "File analysis with the specified ID does not exist."));
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

        Response = FileMultiAnalysisDto.Parse(result.Value);
    }
}