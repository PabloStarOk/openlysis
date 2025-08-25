using ErrorOr;

using FastEndpoints;

using Openlysis.API.Endpoints.Common.Responses;
using Openlysis.API.Endpoints.Messages.GetAnalysisById;
using Openlysis.API.Middlewares.Files;
using Openlysis.Application.Common.Models;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Application.Messages.Services;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.API.Endpoints.Messages.Analyze;

/// <summary>
/// Endpoint for analyzing messages.
/// </summary>
/// <remarks>
/// This endpoint handles requests to analyze messages by processing the input
/// and returning the ID and hash of a <see cref="MessageAnalysis"/>.
/// </remarks>
public class AnalyzeMessageEndpoint : Endpoint<AnalyzeMessageRequest, AnalysisIdentifiers>
{
    private const string Name = "AnalyzeMessage";

    private readonly ILogger<AnalyzeMessageEndpoint> _logger;
    private readonly IMessageAnalysisService _messageAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeMessageEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for debugging endpoint operations.</param>
    /// <param name="messageAnalysisService">The service responsible for providing the core functionality.</param>
    public AnalyzeMessageEndpoint(
        ILogger<AnalyzeMessageEndpoint> logger,
        IMessageAnalysisService messageAnalysisService)
    {
        _logger = logger;
        _messageAnalysisService = messageAnalysisService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post(string.Empty);
        PostProcessor<FileStorageCleanupPostProcessor<AnalyzeMessageRequest, AnalysisIdentifiers>>();
        Group<MessageAnalysesGroup>();
        AllowFileUploads(dontAutoBindFormData: true);
        Version(1);
        Description(
            builder =>
            {
                builder.WithName(Name);
                builder.WithDisplayName(Name);
                builder.Accepts<AnalyzeMessageRequest>("multipart/form-data");
                builder.Produces<AnalysisIdentifiers>(StatusCodes.Status202Accepted);
                builder.ProducesValidationProblem();
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Analyze a message.";
                s.Description = "Sends a message to be analyzed.";
                s.RequestParam(r => r.MessageType, "Type of the message. Accepted values are 'SMS' or 'email'.");
                s.RequestParam(r => r.Sender, "Sender of the message.");
                s.RequestParam(r => r.Content, "Content of the message.");
                s.RequestParam(r => r.Subject, "Subject of the message, can be null or empty.");
                s.RequestParam(r => r.AttachedFiles, "Files attached to the message.");
                s.RequestParam(r => r.AttachedFilesPasswords, "A dictionary where the key is the name of an attached file and the value is its corresponding password, if required. Should be sent as a JSON for proper binding.");
                s.RequestParam(r => r.IsPrivate, "If the analysis is only available to the user who sends the message. Default is true");
                s.RequestParam(r => r.Reanalyze, "If the extracted data and the message should be reanalyzed even if there are existing analyses for theme. Default is false.");
                s.RequestParam(r => r.CountryCode, "A code of the country where detected phone numbers can be associated to, it must be in ISO 3166-1 alpha-2 format (e.g. 'US').");
            });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AnalyzeMessageRequest req, CancellationToken ct)
    {
        if (!_messageAnalysisService.AnalyzeIsAvailable)
        {
            IResult result = Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                detail: "Service to analyze a message is unavailable, try again later.");
            await SendResultAsync(result);
        }

        MessageType messageType = (MessageType)req.MessageType!;
        var message = new Message(messageType, req.Sender, req.Subject, req.Content);
        Dictionary<ProcessedFile, string> filePasswords = MapFilePasswords(req);

#if DEBUG
        LogAttachedFiles(req.AttachedFiles, filePasswords);
#endif

        ErrorOr<MessageAnalysis> analyzeResult = await _messageAnalysisService
            .AnalyzeAsync(
            userId: req.UserId,
            isPrivate: req.IsPrivate,
            message: message,
            files: req.AttachedFiles,
            filePasswords: filePasswords,
            reanalyze: req.Reanalyze,
            requestCountryCode: req.NormalizedCountryCode,
            cancellationToken: ct);

        if (analyzeResult.IsError)
        {
            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Internal error while analyzing message, try again later.");

            Logger.LogError(
                "An error occurred while analyzing message.\nErrors:\n\t{Errors}",
                analyzeResult.Errors);
            await SendResultAsync(internalError);
            return;
        }

        MessageAnalysis messageAnalysis = analyzeResult.Value;
        Response = AnalysisIdentifiers.Parse(messageAnalysis);

        var routeValues = new RouteValueDictionary
            {
                { "id", Response.Id },
            };
        IResult acceptedResult = Results.AcceptedAtRoute (
            GetAnalysisByIdEndpoint.Name,
            routeValues,
            Response);
        await SendResultAsync(acceptedResult);
    }

    private static Dictionary<ProcessedFile, string> MapFilePasswords(AnalyzeMessageRequest request)
    {
        if (request.AttachedFiles.Length is 0 || request.AttachedFilesPasswords.Count is 0)
        {
            return [];
        }

        Dictionary<string, string> passwords = request.AttachedFilesPasswords;
        return request.AttachedFiles
            .Where(f => passwords.ContainsKey(f.Metadata.Name))
            .ToDictionary(
                f => f,
                f => passwords[f.Metadata.Name]);
    }

#if DEBUG
    /// <summary>
    /// Logs detailed information about the attached files for debugging purposes.
    /// </summary>
    /// <param name="files">A collection of <see cref="ProcessedFile"/> objects representing the attached files.</param>
    /// <param name="passwords">A dictionary mapping each <see cref="ProcessedFile"/> to its password, if provided.</param>
    private void LogAttachedFiles(ProcessedFile[] files, Dictionary<ProcessedFile, string> passwords)
    {
        var attachedFilesLog = files.Select(file =>
        {
            _ = passwords.TryGetValue(file, out string? password);

            return $"\n\n\tFilename: {file.Metadata.Name}"
                + $"\n\tFile password: {password}"
                + $"\n\tContent type: {file.Metadata.ContentType}";
        });

        _logger.LogTrace(
            "Message analysis requested with attached files: {AttachedFiles}",
            attachedFilesLog);
    }
#endif
}