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
                b.WithName("GetFileAnalysisByHash"); // TODO: Should name be stored in a public static var?
                b.WithDisplayName("GetFileAnalysisByHash");
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
            AddError("Provided hash is not valid.");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, ct); // TODO: Return ProblemsDetails.
            return;
        }

        var query = new FileAnalysisQueryByHash(hash);
        ErrorOr<FileAnalysis> fileAnalysis = await _mediator.Send(query, ct);

        if (fileAnalysis.IsError)
        {
            foreach (var error in fileAnalysis.Errors)
            {
                AddError(error.Description);
            }

            await SendErrorsAsync(StatusCodes.Status400BadRequest, ct); // TODO: Return ProblemsDetails.
            return;
        }

        // Map to DTO
        var response = new FileAnalysisResponse(
            fileAnalysis.Value.Id.Value.ToString(),
            fileAnalysis.Value.LastScanDate,
            fileAnalysis.Value.ReportsAmount,
            fileAnalysis.Value.Verdict,
            fileAnalysis.Value.File,
            fileAnalysis.Value.Reports.ToArray());
        await SendOkAsync(response, ct);
    }
}