using FastEndpoints;

using MediatR;

using Openlysis.API.Endpoints.Analyses.File.Responses;
using Openlysis.Application.FileAnalyses.Queries;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.API.Endpoints.Analyses.File.GetByHash;

/// <summary>
/// Endpoint for retrieving file analysis by hash.
/// </summary>
public class Endpoint : Endpoint<Request, IEnumerable<FileMultiAnalysisDto>>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Gets the name of the endpoint.
    /// </summary>
    public static string Name { get; } = "GetMultiAnalysesByHash";

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
        Get("/{hash}/analyses");
        Group<FileAnalysesGroup>();
        Description(b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Produces<IEnumerable<FileMultiAnalysisDto>>();
                b.ProducesProblemDetails(StatusCodes.Status404NotFound);
            });
        Summary(
            s =>
            {
                s.Summary = "Gets several multi analyses of a file identified by a hash.";
                s.Description = "Gets a collection of multi analyses by providing a MD5, SHA1, SHA256 or SHA512 hash of a file.";
                s.RequestParam(r => r.Hash, "A SHA-256 (Preferred), MD5, SHA-1 or SHA-512 hash.");
                s.RequestParam(r => r.Amount, "Amount of analyses to retrieve.");
                s.RequestParam(r => r.StartedDateOrder, "Either 'asc' or 'dsc' specifying order to get the last or oldest started analyses.");
            });
    }

    /// <summary>
    /// Handles the request to get several multi analysis of a file identified by hash.
    /// </summary>
    /// <param name="request">A <see cref="Request"/>.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>An <see cref="IEnumerable{FileMultiAnalysis}"/>.</returns>
    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Hash))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Hash is required."));
            return;
        }

        if (!string.IsNullOrWhiteSpace(request.StartedDateOrder)
            && request.StartedDateOrder is not("asc" or "dsc"))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "startDateOrder query param must be 'asc' or 'dsc'."));
            return;
        }

        var query = new FileMultiAnalysesQueryByHash(
            request.Hash,
            request.Amount < 1 ? 10 : request.Amount,
            request.StartedDateOrder);
        IReadOnlyList<FileMultiAnalysis> multiAnalyses = await _mediator.Send(query, ct);

        if (multiAnalyses.Count < 1)
        {
            await SendResultAsync(
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "There are no analyses to retrieve."));
            return;
        }

        // Map to DTO
        Response = multiAnalyses.Select(f =>
            {
                var serviceAnalyses = f.ServiceFileAnalyses.Select(
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

                return new FileMultiAnalysisDto(
                    f.Id.Value.ToString(),
                    f.StartedDate,
                    f.AverageVerdict.ToString(),
                    f.AverageThreatZone.ToString(),
                    f.Status.ToString(),
                    f.FileMetadata,
                    f.ContentHashSet,
                    serviceAnalyses,
                    f.ReportsAmount);
            });

        await SendOkAsync(Response, ct);
    }
}