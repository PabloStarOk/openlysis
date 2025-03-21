using System.Security.Claims;

using ErrorOr;

using FastEndpoints;

using MediatR;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.Application.URLs.Commands;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.URLs.Analyze;

/// <summary>
/// Endpoint for analyzing URLs.
/// </summary>
/// <remarks>
/// This endpoint handles the analysis of URLs by validating the request,
/// retrieving the user ID, creating and sending the analysis command,
/// and finally creating the response.
/// </remarks>
public class AnalyzeUrlEndpoint : Endpoint<AnalyzeUrlRequest, AnalyzeUrlResponse>
{
    private readonly ILogger<AnalyzeUrlEndpoint> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging.</param>
    /// <param name="mediator">The mediator instance used to send commands.</param>
    public AnalyzeUrlEndpoint(
        ILogger<AnalyzeUrlEndpoint> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post(string.Empty);
        Group<UrlAnalysesGroup>();
        Description(
            builder =>
            {
                builder.Accepts<AnalyzeUrlRequest>("application/x-www-form-urlencoded");
            });
        Version(1);
        DontThrowIfValidationFails();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AnalyzeUrlRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await SendResultAsync(ValidationFailures.AsValidationProblem());
            return;
        }

        Claim claim = HttpContext.User.Claims.Single(c => c.Type is ClaimTypes.NameIdentifier);
        var userId = UserId.Create(Guid.Parse(claim.Value));

        var command = new AnalyzeUrlCommand(
            req.Url,
            userId,
            req.IsPrivate);

        ErrorOr<UrlMultiAnalysis> result = await _mediator.Send(command, ct);
        if (result.IsError)
        {
            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Internal error while validating user, try again later.");

            _logger.LogError(
                "An error occurred while sending AnalyzeUrlCommand with mediator.\nErrors:\n\t{Errors}",
                result.Errors);
            await SendResultAsync(internalError);
            return;
        }

        UrlMultiAnalysis multiAnalysis = result.Value;
        Response = new AnalyzeUrlResponse(
            multiAnalysis.Id.Value.ToString(),
            multiAnalysis.UrlHashSet.Sha256,
            multiAnalysis.UrlHashSet.Md5,
            multiAnalysis.UrlHashSet.Sha1,
            multiAnalysis.UrlHashSet.Sha512);
    }
}