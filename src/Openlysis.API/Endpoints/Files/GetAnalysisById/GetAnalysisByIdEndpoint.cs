using ErrorOr;

using FastEndpoints;

using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.Application.Files.Services;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;

namespace Openlysis.API.Endpoints.Files.GetAnalysisById;

/// <summary>
/// Endpoint for retrieving file analysis by hash.
/// </summary>
public class GetAnalysisByIdEndpoint : EndpointWithoutRequest<FileMultiAnalysisDto>
{
    private readonly IFileMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Gets the name of the endpoint.
    /// </summary>
    public static string Name { get; } = "GetFileAnalysisById";

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
                b.ProducesProblemDetails();
                b.ProducesProblemDetails(StatusCodes.Status404NotFound);
            });
        Summary(
            s =>
            {
                s.Summary = "Get a file analysis by ID.";
                s.Description = "Get a file analysis by its ID.";
                s.Params = new Dictionary<string, string>
                {
                    { "id", "ID of the analysis to retrieve." },
                };
            });
    }

    /// <summary>
    /// Handles the request to get a file analysis by its hash.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The result of the file analysis.</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        string id = Route<string>("id") ?? string.Empty;

        // ID is null or empty
        if (string.IsNullOrWhiteSpace(id))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "ID is required."));
            return;
        }

        // Invalid ID
        if (!Guid.TryParse(id, out Guid guid))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Provided ID has an invalid format."));
            return;
        }

        ErrorOr<FileMultiAnalysis> mediatorResult = await _multiAnalysisService
            .GetAnalysisByIdAsync(GlobalId.Parse(guid), ct);

        if (mediatorResult.IsError)
        {
            // Not found
            if (mediatorResult.Errors.Any(e => e.Type is ErrorType.NotFound))
            {
                await SendResultAsync(Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "File analysis with the specified ID does not exist."));
                return;
            }

            // Other errors
            var extensions = new Dictionary<string, object?>
            {
                {
                    "errors", mediatorResult.Errors
                },
            };
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "One or more errors occurred.",
                extensions: extensions));
            return;
        }

        Response = FileMultiAnalysisDto.Parse(mediatorResult.Value);
        await SendOkAsync(Response, ct);
    }
}