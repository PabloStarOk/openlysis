using System.Security.Claims;

using FastEndpoints;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.API.Endpoints.Common.Responses.Messages;
using Openlysis.API.Endpoints.Emails.GetAnalysisById;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Application.Messages.Services;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Enums;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.API.Endpoints.Emails.Analyze;

/// <summary>
/// Endpoint for analyzing email messages.
/// </summary>
/// <remarks>
/// This endpoint handles requests to analyze email messages by processing the input
/// and returning the ID or hash of a <see cref="MessageAnalysis"/>.
/// </remarks>
public class AnalyzeEmailEndpoint : Endpoint<AnalyzeEmailRequest, AnalyzeMessageResponse>
{
    private readonly IMessageAnalysisService _messageAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeEmailEndpoint"/> class.
    /// </summary>
    /// <param name="messageAnalysisService">The service responsible for analyzing email messages.</param>
    public AnalyzeEmailEndpoint(IMessageAnalysisService messageAnalysisService)
    {
        _messageAnalysisService = messageAnalysisService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post(string.Empty);
        Group<EmailAnalysesGroup>();
        Version(1);
        Description(
            builder =>
            {
                builder.WithName("AnalyzeEmail");
                builder.WithDisplayName("AnalyzeEmail");
                builder.Accepts<AnalyzeEmailRequest>("multipart/form-data");
                builder.Produces<AnalyzeMessageResponse>(statusCode: 202);
                builder.ProducesProblem(statusCode: StatusCodes.Status400BadRequest);
                builder.ProducesValidationProblem();
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Analyze an email message.";
                s.Description = "Sends an email message to be analyzed.";
                s.RequestParam(r => r.Sender, "Sender of the email message.");
                s.RequestParam(r => r.Content, "Content of the email message.");
                s.RequestParam(r => r.Subject, "Subject of the email message, can be null or empty.");
                s.RequestParam(r => r.AttachedFiles, "Files attached to the email message.");
                s.RequestParam(r => r.AttachedFilesPasswords, "A dictionary where the key is the name of an attached file and the value is its corresponding password, if required. Should be sent as a JSON for proper binding.");
                s.RequestParam(r => r.IsPrivate, "If the analysis is only available to the user who sends the email message. Default is true");
                s.RequestParam(r => r.ReanalyzeData, "If the data that is detected in the email message, should be analyzed again even if there are existing analysis results. Default is false.");
                s.RequestParam(r => r.CountryCode, "A code of the country where detected phone numbers can be associated to, it must be in ISO 3166-1 alpha-2 format (e.g. 'US').");
            });
        DontThrowIfValidationFails();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AnalyzeEmailRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await SendResultAsync(ValidationFailures.AsValidationProblem());
            return;
        }

        Claim userIdClaim = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier);
        UserId userId = UserId.Create(Guid.Parse(userIdClaim.Value));
        var message = new Message(MessageType.Email, req.Sender, null, req.Content);
        FileData[] files = CreateFileDataArray(req);
        MessageAnalysis messageAnalysis = await _messageAnalysisService.AnalyzeAsync(
            userId: userId,
            isPrivate: req.IsPrivate,
            message: message,
            files: files,
            reanalyzeData: req.ReanalyzeData,
            requestCountryCode: req.NormalizedCountryCode,
            cancellationToken: ct);

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
    private static FileData[] CreateFileDataArray(AnalyzeEmailRequest request)
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
    /// <param name="attachedFile">The file attached to the email message.</param>
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
            string.Empty, // TODO: Should I remove file description from FileData? Maybe a bit unnecessary.
            filePassword,
            attachedFile.OpenReadStream());
    }
}