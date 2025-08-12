using System.Security.Claims;

using ErrorOr;

using FastEndpoints;

using Openlysis.API.Endpoints.Common.Requests;
using Openlysis.API.Endpoints.EmailAddresses.GetReputation;
using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.API.Endpoints.Messages.Common.Responses;
using Openlysis.API.Endpoints.Phones.GetReputation;
using Openlysis.API.Endpoints.URLs.Common;
using Openlysis.Application.Messages.Services;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;

namespace Openlysis.API.Endpoints.Messages.GetAnalysisById;

/// <summary>
/// Endpoint for retrieving a <see cref="MessageAnalysis"/> by its unique identifier.
/// </summary>
/// <remarks>
/// This endpoint handles requests to fetch detailed analysis of a message,
/// including associated URLs, email addresses, and phone reputations.
/// </remarks>
public class GetAnalysisByIdEndpoint : Endpoint<GetAnalysisByIdRequest, MessageAnalysisDto>
{
    /// <summary>
    /// The name of the endpoint for retrieving message analysis by ID.
    /// </summary>
    public const string Name = "GetMessageAnalysisById";

    private readonly ILogger<GetAnalysisByIdEndpoint> _logger;
    private readonly IMessageAnalysisService _messageAnalysisService;
    private readonly IMessageAnalysisResultsProvider _resultsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysisByIdEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="messageAnalysisService">The service for retrieving message analysis data.</param>
    /// <param name="resultsProvider">The provider for fetching analysis results such as URLs, emails, and phone reputations.</param>
    public GetAnalysisByIdEndpoint(
        ILogger<GetAnalysisByIdEndpoint> logger,
        IMessageAnalysisService messageAnalysisService,
        IMessageAnalysisResultsProvider resultsProvider)
    {
        _logger = logger;
        _messageAnalysisService = messageAnalysisService;
        _resultsProvider = resultsProvider;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("analyses/{id}");
        Group<MessageAnalysesGroup>();
        Version(1);
        Description(
            builder =>
            {
                builder.WithName(Name);
                builder.WithDisplayName(Name);
                builder.Accepts<GetAnalysisByIdRequest>();
                builder.Produces<MessageAnalysisDto>();
                builder.ProducesProblem(StatusCodes.Status404NotFound);
                builder.ProducesProblem(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Gets a message analysis by Id.";
                s.Description = "Retrieves a message analysis by its ID.";
                s.RequestParam(r => r.Id, "ID of the message analysis to retrieve.");
            });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(
        GetAnalysisByIdRequest req,
        CancellationToken ct)
    {
        Claim userIdClaim = User.Claims.Single(c => c.Type is ClaimTypes.NameIdentifier);
        var userId = GlobalId.Parse(userIdClaim.Value);

        if (!GlobalId.TryParse(req.Id, out GlobalId? id))
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Provided ID has an invalid format."));
            return;
        }

        ErrorOr<MessageAnalysis> result =
            await _messageAnalysisService.GetAnalysisByIdAsync(userId, id, ct);

        if (result.IsError)
        {
            if (result.Errors.Any(e => e.Type is ErrorType.NotFound))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            _logger.LogError(
                "An error occurred while retrieving a {Type} by ID.\nErrors: \n\t{Errors}",
                typeof(MessageAnalysis),
                result.Errors);

            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "An internal error occurred, try again later.");

            await SendResultAsync(internalError);
            return;
        }

        var messageAnalysis = result.Value;
        var fileMultiAnalyses = await _resultsProvider.GetFileMultiAnalysesAsync(
            messageAnalysis,
            ct);
        var urlMultiAnalyses = await _resultsProvider.GetUrlMultiAnalysesAsync(
            messageAnalysis,
            ct);
        var emailMultiReputations = await _resultsProvider.GetEmailAddressesReputationsAsync(
            messageAnalysis,
            ct);
        var phoneMultiReputations = await _resultsProvider.GetPhoneNumbersReputationsAsync(
            messageAnalysis,
            ct);

        var results = new MessageAnalysisResults(
            fileMultiAnalyses.Select(FileMultiAnalysisDto.Parse),
            urlMultiAnalyses.Select(UrlMultiAnalysisDto.Parse),
            emailMultiReputations.Select(EmailAddressMultiReputationDto.Parse),
            phoneMultiReputations.Select(PhoneMultiReputationDto.Parse));

        Response = MessageAnalysisDto.Parse(
            messageAnalysis,
            results);
    }
}