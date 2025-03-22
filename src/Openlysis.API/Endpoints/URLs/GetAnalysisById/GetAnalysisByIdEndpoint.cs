using System.Security.Claims;

using ErrorOr;

using FastEndpoints;

using MediatR;

using Openlysis.API.Endpoints.URLs.Common;
using Openlysis.Application.URLs.Queries;
using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.URLs.GetAnalysisById;

/// <summary>
/// Endpoint to handle the request for getting analysis by ID.
/// </summary>
/// <remarks>
/// This endpoint receives a `GetAnalysisByIdRequest` and returns a `UrlMultiAnalysisDto`.
/// </remarks>
public class GetAnalysisByIdEndpoint : Endpoint<GetAnalysisByIdRequest, UrlMultiAnalysisDto>
{
    private readonly ILogger<GetAnalysisByIdEndpoint> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysisByIdEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log information.</param>
    /// <param name="mediator">The mediator instance to send queries.</param>
    public GetAnalysisByIdEndpoint(
        ILogger<GetAnalysisByIdEndpoint> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("analyses/{id}");
        Group<UrlAnalysesGroup>();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetAnalysisByIdRequest req, CancellationToken ct)
    {
        Claim userIdClaim = HttpContext.User.Claims.Single(c => c.Type is ClaimTypes.NameIdentifier);
        UserId userId = UserId.Create(Guid.Parse(userIdClaim.Value));

        var multiAnalysisId = MultiAnalysisId.Create(req.Id);

        var query = new UrlMultiAnalysisQuery(
            multiAnalysisId,
            userId);

        ErrorOr<UrlMultiAnalysis> result = await _mediator.Send(query, ct);
        if (result.IsError)
        {
            if (result.Errors.Any(e => e.Type is ErrorType.NotFound))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            _logger.LogError(
                "An error occurred while sending a MediatR query to retrieve a UrlMultiAnalysis by ID.\nErrors: \n\t{Errors}",
                result.Errors);

            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "An internal error occurred, try again later.");

            await SendResultAsync(internalError);
            return;
        }

        Response = UrlMultiAnalysisDto.Parse(result.Value);
    }
}