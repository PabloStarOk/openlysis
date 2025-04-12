using System.Security.Claims;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.API.Endpoints.URLs.Common;
using Openlysis.Application.URLs.Services;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.URLs.GetAnalysesByHash;

/// <summary>
/// Endpoint to handle requests for getting analyses by hash.
/// </summary>
/// <remarks>
/// This endpoint processes requests to retrieve multiple analyses associated with a specific hash.
/// </remarks>
public class GetAnalysesByHashEndpoint
    : Endpoint<GetAnalysesByHashRequest, IEnumerable<UrlMultiAnalysisDto>>
{
    private const string Name = "GetUrlAnalysisByHash";

    private readonly IUrlMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysesByHashEndpoint"/> class.
    /// </summary>
    /// <param name="multiAnalysisService">The service responsible for analyzing URLs.</param>
    public GetAnalysesByHashEndpoint(
        IUrlMultiAnalysisService multiAnalysisService)
    {
        _multiAnalysisService = multiAnalysisService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("{hash}/analyses");
        Group<UrlAnalysesGroup>();
        Version(1);
        Description(
            builder =>
            {
                builder.WithName(Name);
                builder.WithDisplayName(Name);
                builder.Accepts<GetAnalysesByHashRequest>();
                builder.Produces<IReadOnlyList<UrlMultiAnalysisDto>>();
                builder.ProducesValidationProblem();
                builder.ProducesProblem(StatusCodes.Status404NotFound);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Get several multi analyses of an URL by Hash";
                s.Description = "Gets a collection of multi analyses by providing a MD5, SHA-1, SHA-256 or SHA-512 hash of a URL.";
                s.RequestParam(r => r.Hash, "A SHA-256, MD5, SHA-1 or SHA-512 hash of the URL.");
                s.RequestParam(r => r.Amount, "(Pagination) Amount of analyses to retrieve.");
                s.RequestParam(r => r.StartedDateOrder, "Order of the collection by started date.");
            });
        DontThrowIfValidationFails();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetAnalysesByHashRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await SendResultAsync(ValidationFailures.AsValidationProblem());
            return;
        }

        Claim userIdClaim = HttpContext.User.Claims.Single(c => c.Type is ClaimTypes.NameIdentifier);
        UserId userId = UserId.Create(Guid.Parse(userIdClaim.Value));

        IReadOnlyList<UrlMultiAnalysis> analyses = await _multiAnalysisService
            .GetAnalysesByHashAsync(
                userId,
                req.Hash,
                req.Amount,
                req.StartedDateOrder,
                ct);

        if (analyses.Count is 0)
        {
            IResult notFoundResult = Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: "There are no analyses for the given hash.");
            await SendResultAsync(notFoundResult);
            return;
        }

        Response = analyses.Select(UrlMultiAnalysisDto.Parse);
    }
}