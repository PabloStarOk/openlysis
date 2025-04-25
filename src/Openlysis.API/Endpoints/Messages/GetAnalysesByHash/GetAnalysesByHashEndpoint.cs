using System.Security.Claims;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.API.Endpoints.Common.Requests;
using Openlysis.API.Endpoints.EmailAddresses.GetReputation;
using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.API.Endpoints.Messages.Common.Responses;
using Openlysis.API.Endpoints.Phones.GetReputation;
using Openlysis.API.Endpoints.URLs.Common;
using Openlysis.Application.Common.Enums;
using Openlysis.Application.Messages.Services;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.Messages.GetAnalysesByHash;

/// <summary>
/// Endpoint for retrieving message analyses by a specific hash.
/// </summary>
/// <remarks>
/// This endpoint handles requests to fetch analyses associated with a given hash.
/// It returns a collection of <see cref="MessageAnalysisDto"/> objects.
/// </remarks>
public class GetAnalysesByHashEndpoint
    : Endpoint<GetAnalysesByHashRequest, IEnumerable<MessageAnalysisDto>>
{
    private const string Name = "GetMessageAnalysesByHash";

    private readonly IMessageAnalysisService _messageAnalysisService;
    private readonly IMessageAnalysisResultsProvider _resultsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalysesByHashEndpoint"/> class.
    /// </summary>
    /// <param name="messageAnalysisService">
    /// The service responsible for retrieving message analyses.
    /// </param>
    /// <param name="resultsProvider">
    /// The provider responsible for fetching analysis results such as URLs, emails, and phone reputations.
    /// </param>
    public GetAnalysesByHashEndpoint(
        IMessageAnalysisService messageAnalysisService,
        IMessageAnalysisResultsProvider resultsProvider)
    {
        _messageAnalysisService = messageAnalysisService;
        _resultsProvider = resultsProvider;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("{hash}/analyses");
        Group<MessageAnalysesGroup>();
        Version(1);
        Description(
            builder =>
            {
                builder.WithName(Name);
                builder.WithDisplayName(Name);
                builder.Accepts<GetAnalysesByHashRequest>();
                builder.Produces<IEnumerable<MessageAnalysisDto>>();
                builder.ProducesValidationProblem();
                builder.ProducesProblem(StatusCodes.Status404NotFound);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Get several analyses for a message by Hash";
                s.Description = "Gets a collection of analyses by providing a MD5, SHA-1, SHA-256 or SHA-512 hash of an email message.";
                s.RequestParam(r => r.Hash, "A SHA-256, MD5, SHA-1 or SHA-512 hash of the message.");
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

        IReadOnlyList<MessageAnalysis> messageAnalyses = await _messageAnalysisService
            .GetAnalysesByHashAsync(
                userId,
                req.Hash,
                req.Amount,
                req.StartedDateOrder,
                ct);

        List<MessageAnalysisDto> messageAnalysisDtos = [];
        await Parallel.ForEachAsync(messageAnalyses, ct, async (analysis, token) =>
        {
            // TODO: Refactor duplicated logic with GetAnalysisByIdEndpoint.
            var fileMultiAnalyses = await _resultsProvider.GetFileMultiAnalysesAsync(
                analysis,
                ct);
            var urlMultiAnalyses = await _resultsProvider.GetUrlMultiAnalysesAsync(
                analysis,
                token);
            var emailMultiReputations = await _resultsProvider.GetEmailAddressesReputationsAsync(
                analysis,
                token);
            var phoneMultiReputations = await _resultsProvider.GetPhoneNumbersReputationsAsync(
                analysis,
                token);

            var results = new MessageAnalysisResults(
                fileMultiAnalyses.Select(FileMultiAnalysisDto.Parse),
                urlMultiAnalyses.Select(UrlMultiAnalysisDto.Parse),
                emailMultiReputations.Select(EmailAddressMultiReputationDto.Parse),
                phoneMultiReputations.Select(PhoneMultiReputationDto.Parse));

            var dto = MessageAnalysisDto.Parse(
                analysis,
                results);

            messageAnalysisDtos.Add(dto);
        });

        if (messageAnalyses.Count is 0)
        {
            IResult notFoundResult = Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: "There are no analyses for the given hash.");
            await SendResultAsync(notFoundResult);
            return;
        }

        // TODO: Refactor duplicated logic with app layer services.
        Response = req.StartedDateOrder switch
        {
            OrderType.Dsc => messageAnalysisDtos.OrderByDescending(u => u.StartedDate),
            OrderType.Asc => messageAnalysisDtos.OrderBy(u => u.StartedDate),
            _ => throw new InvalidOperationException("StartedDateOrder has an invalid enum value.")
        };
    }
}