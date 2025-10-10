using ErrorOr;

using FastEndpoints;

using Openlysis.API.Endpoints.Common.Requests;
using Openlysis.API.Endpoints.URLs.Common;
using Openlysis.Application.URLs.Services;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;

namespace Openlysis.API.Endpoints.URLs.GetAnalysisById;

/// <summary>
/// Endpoint to handle the request for getting analysis by ID.
/// </summary>
/// <remarks>
/// This endpoint receives a `GetAnalysisByIdRequest` and returns a `UrlMultiAnalysisDto`.
/// </remarks>
public class GetAnalysisByIdEndpoint : Endpoint<GetAnalysisByIdRequest, UrlMultiAnalysisDto>
{
    /// <summary>
    /// The name of the endpoint for getting URL analysis by ID.
    /// </summary>
    public const string Name = "GetUrlAnalysisById";

    private readonly ILogger<GetAnalysisByIdEndpoint> _logger;
    private readonly IUrlMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysisByIdEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log information.</param>
    /// <param name="multiAnalysisService">The service responsible for analyzing URLs.</param>
    public GetAnalysisByIdEndpoint(
        ILogger<GetAnalysisByIdEndpoint> logger,
        IUrlMultiAnalysisService multiAnalysisService)
    {
        _logger = logger;
        _multiAnalysisService = multiAnalysisService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("analyses/{id}");
        Group<UrlAnalysesGroup>();
        Version(1);
        Description(
            builder =>
            {
                builder.WithName(Name);
                builder.WithDisplayName(Name);
                builder.Accepts<GetAnalysisByIdRequest>();
                builder.Produces<UrlMultiAnalysisDto>();
                builder.ProducesProblem(StatusCodes.Status404NotFound);
                builder.ProducesProblem(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Gets a URL analysis by ID";
                s.Description = "Gets a URL analysis by using an ID";
                s.RequestParam(r => r.Id, "ID of the analysis to get.");
            });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetAnalysisByIdRequest req, CancellationToken ct)
    {
        var userId = GlobalId.Parse(req.UserId);

        if (!GlobalId.TryParse(req.Id, out GlobalId? id))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Provided ID has an invalid format."));
            return;
        }

        ErrorOr<UrlMultiAnalysis> result = await _multiAnalysisService
            .GetAnalysisByIdAsync(userId, id, ct);
        if (result.IsError)
        {
            if (result.Errors.Any(e => e.Type is ErrorType.NotFound))
            {
                await SendResultAsync(Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "File analysis with the specified ID does not exist."));
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