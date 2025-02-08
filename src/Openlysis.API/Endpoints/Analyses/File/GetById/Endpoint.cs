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
                b.WithName("GetFileAnalysisById"); // TODO: Should name be stored in a public static var?
                b.WithDisplayName("GetFileAnalysisById");
                b.Produces<FileAnalysisResponse>();
                b.ProducesProblemDetails();
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

        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out Guid guid))
        {
            AddError("Provided id is not valid.");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, ct); // TODO: Return ProblemsDetails.
            return;
        }

        var fileAnalysisId = FileAnalysisId.Create(guid);
        var query = new FileAnalysisQueryById(fileAnalysisId);

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