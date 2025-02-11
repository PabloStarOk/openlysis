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
        ErrorOr<FileAnalysis> fileAnalysis = await _mediator.Send(query, ct);

        if (fileAnalysis.IsError)
        {
            // Not found
            if (fileAnalysis.Errors.Any(e => e.Type is ErrorType.NotFound))
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
        var response = new FileAnalysisResponse(
            fileAnalysis.Value.Id.Value.ToString(),
            fileAnalysis.Value.LastScanDate,
            fileAnalysis.Value.ReportsAmount,
            fileAnalysis.Value.Verdict,
            fileAnalysis.Value.Metadata,
            fileAnalysis.Value.Reports.ToArray());
        await SendOkAsync(response, ct);
    }
}