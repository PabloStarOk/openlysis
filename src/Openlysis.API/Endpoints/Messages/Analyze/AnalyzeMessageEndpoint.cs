using System.Security.Claims;

using ErrorOr;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.API.Endpoints.Messages.GetAnalysisById;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Application.Messages.Services;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Enums;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.Messages.Analyze;

/// <summary>
/// Endpoint for analyzing messages.
/// </summary>
/// <remarks>
/// This endpoint handles requests to analyze messages by processing the input
/// and returning the ID and hash of a <see cref="MessageAnalysis"/>.
/// </remarks>
public class AnalyzeMessageEndpoint : Endpoint<AnalyzeMessageRequest, AnalyzeMessageResponse>
{
    private readonly IMessageAnalysisService _messageAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeMessageEndpoint"/> class.
    /// </summary>
    /// <param name="messageAnalysisService">The service responsible for providing the core functionality.</param>
    public AnalyzeMessageEndpoint(IMessageAnalysisService messageAnalysisService)
    {
        _messageAnalysisService = messageAnalysisService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post(string.Empty);
        Group<MessageAnalysesGroup>();
        Version(1);
        Description(
            builder =>
            {
                builder.WithName("AnalyzeMessage");
                builder.WithDisplayName("AnalyzeMessage");
                builder.Accepts<AnalyzeMessageRequest>("multipart/form-data");
                builder.Produces<AnalyzeMessageResponse>(statusCode: 202);
                builder.ProducesValidationProblem();
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Analyze a message.";
                s.Description = "Sends a message to be analyzed.";
                s.RequestParam(r => r.MessageType, "Type of the message. Accepted values are 'SMS' or 'email'."); // TODO: Documentation is not displayed.
                s.RequestParam(r => r.Sender, "Sender of the message.");
                s.RequestParam(r => r.Content, "Content of the message.");
                s.RequestParam(r => r.Subject, "Subject of the message, can be null or empty.");
                s.RequestParam(r => r.AttachedFiles, "Files attached to the message.");
                s.RequestParam(r => r.AttachedFilesPasswords, "A dictionary where the key is the name of an attached file and the value is its corresponding password, if required. Should be sent as a JSON for proper binding.");
                s.RequestParam(r => r.IsPrivate, "If the analysis is only available to the user who sends the message. Default is true");
                s.RequestParam(r => r.ReanalyzeData, "If the data that is detected in the message, should be analyzed again even if there are existing analysis results. Default is false.");
                s.RequestParam(r => r.CountryCode, "A code of the country where detected phone numbers can be associated to, it must be in ISO 3166-1 alpha-2 format (e.g. 'US').");
            });
        DontThrowIfValidationFails();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AnalyzeMessageRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await SendResultAsync(ValidationFailures.AsValidationProblem());
            return;
        }

        if (!_messageAnalysisService.AnalyzeIsAvailable)
        {
            IResult result = Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                detail: "Service to analyze a message is unavailable, try again later.");
            await SendResultAsync(result);
        }

        Claim userIdClaim = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier);
        UserId userId = UserId.Create(Guid.Parse(userIdClaim.Value));
        MessageType messageType = (MessageType)req.MessageType!;
        var message = new Message(messageType, req.Sender, req.Subject, req.Content);
        FileData[] files = CreateFileDataArray(req);
        ErrorOr<MessageAnalysis> analyzeResult = await _messageAnalysisService
            .AnalyzeAsync(
            userId: userId,
            isPrivate: req.IsPrivate,
            message: message,
            files: files,
            reanalyzeData: req.ReanalyzeData,
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
        Response = AnalyzeMessageResponse.Parse(messageAnalysis);

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

    /// <summary>
    /// Creates an array of <see cref="FileData"/> objects from the attached files in the request.
    /// </summary>
    /// <param name="request">The request containing the attached files and their corresponding passwords.</param>
    /// <returns>An array of <see cref="FileData"/> objects representing the attached files.</returns>
    private static FileData[] CreateFileDataArray(AnalyzeMessageRequest request)
    {
        if (request.AttachedFiles is null)
        {
            return [];
        }

        return request.AttachedFiles
            .Select(f => CreateFileData(f, request.AttachedFilesPasswords))
            .ToArray();
    }

    /// <summary>
    /// Creates a <see cref="FileData"/> object from an attached file and its corresponding password.
    /// </summary>
    /// <param name="attachedFile">The file attached to the message.</param>
    /// <param name="attachedFilePasswords">
    /// A dictionary where the key is the file name and the value is the corresponding password, if required.
    /// </param>
    /// <returns>A <see cref="FileData"/> object representing the attached file.</returns>
    private static FileData CreateFileData(
        IFormFile attachedFile,
        Dictionary<string, string>? attachedFilePasswords)
    {
        string fileName = attachedFile.FileName;
        string filePassword = string.Empty;

        if (attachedFilePasswords is not null
            && attachedFilePasswords.TryGetValue(fileName, out string? providedPassword))
        {
            filePassword = providedPassword;
        }

        return new FileData(
            fileName,
            attachedFile.ContentType,
            filePassword,
            attachedFile.OpenReadStream());
    }
}