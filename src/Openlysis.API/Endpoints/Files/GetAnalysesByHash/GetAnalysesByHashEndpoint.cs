using System.Security.Claims;

using FastEndpoints;

using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.Application.Files.Services;
using Openlysis.Domain.Files;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.Files.GetAnalysesByHash;

/// <summary>
/// Endpoint for retrieving file analysis by hash.
/// </summary>
public class GetAnalysesByHashEndpoint : Endpoint<GetAnalysesByHash, IEnumerable<FileMultiAnalysisDto>>
{
    private readonly IFileMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Gets the name of the endpoint.
    /// </summary>
    public static string Name { get; } = "GetMultiAnalysesByHash";

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysesByHashEndpoint"/> class.
    /// </summary>
    /// <param name="multiAnalysisService">The service used to retrieve file multi-analyses.</param>
    public GetAnalysesByHashEndpoint(IFileMultiAnalysisService multiAnalysisService)
    {
        _multiAnalysisService = multiAnalysisService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        Get("{hash}/analyses");
        Group<FileAnalysesGroup>();
        Version(1);
        Description(b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Produces<IEnumerable<FileMultiAnalysisDto>>();
                b.ProducesProblemDetails();
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
    /// <param name="request">A <see cref="GetAnalysesByHash"/>.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>An <see cref="IEnumerable{FileMultiAnalysis}"/>.</returns>
    public override async Task HandleAsync(GetAnalysesByHash request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Hash))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Hash is required."));
            return;
        }

        Claim userIdClaim = HttpContext.User.Claims.Single(c => c.Type is ClaimTypes.NameIdentifier);
        UserId userId = UserId.Create(Guid.Parse(userIdClaim.Value));

        IReadOnlyList<FileMultiAnalysis> multiAnalyses = await _multiAnalysisService
            .GetAnalysesByHashAsync(
                userId,
                request.Hash,
                request.Amount < 1 ? 10 : request.Amount,
                request.StartedDateOrder,
                ct);

        if (multiAnalyses.Count < 1)
        {
            await SendResultAsync(
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "There are no analyses to retrieve."));
            return;
        }

        Response = multiAnalyses.Select(FileMultiAnalysisDto.Parse);
        await SendOkAsync(Response, ct);
    }
}