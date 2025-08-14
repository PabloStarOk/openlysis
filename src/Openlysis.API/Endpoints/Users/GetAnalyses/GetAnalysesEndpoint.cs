using FastEndpoints;

using Openlysis.API.Endpoints.EmailAddresses.GetReputation;
using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.API.Endpoints.Messages.Common.Responses;
using Openlysis.API.Endpoints.Phones.GetReputation;
using Openlysis.API.Endpoints.URLs.Common;
using Openlysis.Application.Files.Services;
using Openlysis.Application.Messages.Services;
using Openlysis.Application.URLs.Services;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.API.Endpoints.Users.GetAnalyses;

/// <summary>
/// Endpoint for retrieving a paginated collection of analyses (message, URL, or file) of a user.
/// </summary>
public class GetAnalyses : Endpoint<GetAnalysesRequest, PaginatedAnalysisCollection>
{
    private const string Name = "GetAnalysesByUser";

    private readonly IUrlMultiAnalysisService _urlMultiAnalysisService;
    private readonly IFileMultiAnalysisService _fileMultiAnalysisService;
    private readonly IMessageAnalysisService _messageAnalysisService;
    private readonly IMessageAnalysisResultsProvider _resultsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnalyses"/> class.
    /// </summary>
    /// <param name="urlMultiAnalysisService">Service for retrieving URL analyses.</param>
    /// <param name="fileMultiAnalysisService">Service for retrieving file analyses.</param>
    /// <param name="messageAnalysisService">Service for retrieving message analyses.</param>
    /// <param name="resultsProvider">Service for providing message analysis results.</param>
    public GetAnalyses(
        IUrlMultiAnalysisService urlMultiAnalysisService,
        IFileMultiAnalysisService fileMultiAnalysisService,
        IMessageAnalysisService messageAnalysisService,
        IMessageAnalysisResultsProvider resultsProvider)
    {
        _urlMultiAnalysisService = urlMultiAnalysisService;
        _fileMultiAnalysisService = fileMultiAnalysisService;
        _messageAnalysisService = messageAnalysisService;
        _resultsProvider = resultsProvider;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("analyses");
        Group<UsersGroup>();
        Description(
            builder =>
            {
                builder.WithName(Name);
                builder.WithDisplayName(Name);
                builder.Accepts<GetAnalysesRequest>();
                builder.Produces<PaginatedAnalysisCollection>();
                builder.ProducesValidationProblem();
                builder.ProducesProblem(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Get analyses of the user";
                s.Description = "Get a paginated collection of message, url or file analyses of the user.";
                s.RequestParam(r => r.Type, "The type of analysis to retrieve.");
                s.RequestParam(r => r.Page, $"The page number for pagination (minimum is {GetAnalysesRequestValidator.MinPage}).");
                s.RequestParam(r => r.PageSize, $"The number of items per page (minimum is {GetAnalysesRequestValidator.MinPageSize}, maximum is {GetAnalysesRequestValidator.MaxPageSize}).");
                s.ResponseParam<PaginatedAnalysisCollection>(
                    r => r.Analyses,
                    $"A paginated collection containing analyses of type {nameof(UrlMultiAnalysisDto)}, {nameof(FileMultiAnalysisDto)}, or {nameof(MessageAnalysisDto)}. Refer to the schemas for detailed structure.");
            });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetAnalysesRequest req, CancellationToken ct)
    {
        var userId = GlobalId.Parse(req.UserId);

        Func<GlobalId, GetAnalysesRequest, CancellationToken, Task<IReadOnlyList<object>>>
            getAnalyses = req.Type switch
        {
            AnalysisType.Url => GetUrlMultiAnalysesAsync,
            AnalysisType.File => GetFileMultiAnalysesAsync,
            AnalysisType.Email or AnalysisType.Sms => GetMessageAnalysesAsync,
            _ => throw new ArgumentOutOfRangeException(nameof(req)),
        };

        IReadOnlyList<object> analyses = await getAnalyses(userId, req, ct);
        Response = new PaginatedAnalysisCollection(
            req.Page,
            req.PageSize,
            Total: analyses.Count,
            analyses);
    }

    /// <summary>
    /// Retrieves a paginated list of URL analyses for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose URL analyses are being retrieved.</param>
    /// <param name="request">The request containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of parsed URL analysis DTOs.</returns>
    private async Task<IReadOnlyList<object>> GetUrlMultiAnalysesAsync(
        GlobalId userId,
        GetAnalysesRequest request,
        CancellationToken cancellationToken)
    {
        var analyses = await _urlMultiAnalysisService.GetAnalysesByUserAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return analyses
            .Select(UrlMultiAnalysisDto.Parse)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Retrieves a paginated list of file analyses for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose file analyses are being retrieved.</param>
    /// <param name="request">The request containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of parsed file analysis DTOs.</returns>
    private async Task<IReadOnlyList<object>> GetFileMultiAnalysesAsync(
        GlobalId userId,
        GetAnalysesRequest request,
        CancellationToken cancellationToken)
    {
        var analyses = await _fileMultiAnalysisService.GetAnalysesByUserAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return analyses
            .Select(FileMultiAnalysisDto.Parse)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Retrieves a paginated list of message analyses for a specific user,
    /// including related file, URL, email, and phone analysis results.
    /// </summary>
    /// <param name="userId">The ID of the user whose analyses are being retrieved.</param>
    /// <param name="request">The request containing pagination and filter parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of parsed message analysis DTOs.</returns>
    private async Task<IReadOnlyList<object>> GetMessageAnalysesAsync(
        GlobalId userId,
        GetAnalysesRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Duplicated logic with GetMessageAnalysisById and GetMessageAnalysesByHash.
        var analyses = await _messageAnalysisService.GetAnalysesByUserAsync(
            userId,
            request.Type is AnalysisType.Email ? MessageType.Email : MessageType.Sms,
            request.Page,
            request.PageSize,
            cancellationToken);

        List<MessageAnalysisDto> parsedAnalyses = [];
        foreach (var analysis in analyses)
        {
            var fileMultiAnalyses =
                await _resultsProvider.GetFileMultiAnalysesAsync(
                    analysis,
                    cancellationToken);
            var urlMultiAnalyses =
                await _resultsProvider.GetUrlMultiAnalysesAsync(
                    analysis,
                    cancellationToken);
            var emailMultiReputations =
                await _resultsProvider.GetEmailAddressesReputationsAsync(
                    analysis,
                    cancellationToken);
            var phoneMultiReputations =
                await _resultsProvider.GetPhoneNumbersReputationsAsync(
                    analysis,
                    cancellationToken);

            var results = new MessageAnalysisResults(
                fileMultiAnalyses.Select(FileMultiAnalysisDto.Parse),
                urlMultiAnalyses.Select(UrlMultiAnalysisDto.Parse),
                emailMultiReputations.Select(EmailAddressMultiReputationDto.Parse),
                phoneMultiReputations.Select(PhoneMultiReputationDto.Parse));

            var parsedAnalysis = MessageAnalysisDto.Parse(
                analysis,
                results);
            parsedAnalyses.Add(parsedAnalysis);
        }

        return parsedAnalyses;
    }
}