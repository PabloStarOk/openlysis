using ErrorOr;

using FastEndpoints;

using MediatR;

using Openlysis.API.Endpoints.Analyses.File.Responses;
using Openlysis.Application.FileAnalyses.Queries;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.API.Endpoints.Analyses.File.GetById;

/// <summary>
/// Endpoint for retrieving file analysis by hash.
/// </summary>
public class Endpoint : EndpointWithoutRequest<FileAnalysisResponse>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Gets the name of the endpoint.
    /// </summary>
    public static string Name { get; } = "GetFileAnalysisById";

    /// <summary>
    /// Initializes a new instance of the <see cref="Endpoint"/> class.
    /// </summary>
    /// <param name="mediator">Mediator to send commands and receive responses to application layer.</param>
    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        Get("/{id}");
        Group<FileAnalysesGroup>();
        Description(b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Produces<FileAnalysisResponse>();
                b.ProducesProblemDetails();
                b.ProducesProblemDetails(StatusCodes.Status404NotFound);
            });
        Summary(
            s =>
            {
                s.Summary = "Get a file analysis by ID.";
                s.Description = "Get a file analysis by its ID.";
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

        var fileAnalysisId = FileAnalysisId.Create(guid);
        var query = new FileAnalysisQueryById(fileAnalysisId);

        ErrorOr<FileAnalysis> fileAnalysis = await _mediator.Send(query, ct);

        if (fileAnalysis.IsError)
        {
            // Not found
            if (fileAnalysis.Errors.Any(e => e.Type is ErrorType.NotFound))
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
                    "errors", fileAnalysis.Errors
                },
            };
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "One or more errors occurred.",
                extensions: extensions));
            return;
        }

        // Map to DTO
        Response = new FileAnalysisResponse(
            fileAnalysis.Value.Id.Value.ToString(),
            fileAnalysis.Value.LastScanDate,
            fileAnalysis.Value.ReportsAmount,
            fileAnalysis.Value.Verdict,
            fileAnalysis.Value.File,
            fileAnalysis.Value.Reports.ToArray());
        await SendOkAsync(Response, ct);
    }
}