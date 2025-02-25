using ErrorOr;

using FastEndpoints;

using MediatR;

using Openlysis.API.Endpoints.Analyses.File.Responses;
using Openlysis.Application.FileAnalyses.Queries;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.API.Endpoints.Analyses.File.GetByHash;

/// <summary>
/// Endpoint for retrieving file analysis by hash.
/// </summary>
public class Endpoint : EndpointWithoutRequest<FileAnalysisResponse>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Gets the name of the endpoint.
    /// </summary>
    public static string Name { get; } = "GetFileAnalysisByHash";

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
        Get("/hash/{hash}");
        Group<FileAnalysesGroup>();
        Description(b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Produces<FileAnalysisResponse>();
                b.ProducesProblemDetails();
            });
        Summary(
            s =>
            {
                s.Summary = "Get a file analysis by hash.";
                s.Description = "Get a file analysis by providing a MD5, SHA1, SHA256 or SHA512 hash.";
            });
    }

    /// <summary>
    /// Handles the request to get a file analysis by its hash.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The result of the file analysis.</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        string hash = Route<string>("hash") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(hash))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Hash is required."));
            return;
        }

        var query = new FileAnalysisQueryByHash(hash);
        ErrorOr<FileMultiAnalysis> mediatorResult = await _mediator.Send(query, ct);

        if (mediatorResult.IsError)
        {
            // Not found
            if (mediatorResult.Errors.Any(e => e.Type is ErrorType.NotFound))
            {
                await SendResultAsync(Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "File analysis with the specified hash does not exist."));
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

        // Map to DTO
        var serviceAnalyses = mediatorResult.Value.ServiceFileAnalyses.Select(
            s =>
            {
                IEnumerable<ReportDto> reportDtos = s.Reports
                    .Select(
                        r => new ReportDto(
                            r.Id.Value,
                            r.Verdict.ToString(),
                            r.ThreatZone.ToString(),
                            r.ThreatLevel));

                return new ServiceFileAnalysisDto(
                    s.ServiceName,
                    s.Status.ToString(),
                    reportDtos);
            });

        Response = new FileAnalysisResponse(
            mediatorResult.Value.Id.Value.ToString(),
            mediatorResult.Value.StartedDate,
            mediatorResult.Value.AverageVerdict.ToString(),
            mediatorResult.Value.AverageThreatZone.ToString(),
            mediatorResult.Value.Status.ToString(),
            mediatorResult.Value.FileMetadata,
            mediatorResult.Value.ContentHashSet,
            serviceAnalyses,
            mediatorResult.Value.ReportsAmount);
        await SendOkAsync(Response, ct);
    }
}