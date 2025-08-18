using System.Net.Mail;

using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages.Entities;
using Openlysis.Domain.Messages.ValueObjects;

namespace Openlysis.Domain.Messages;

/// <summary>
/// Represents the analysis of a message, including its type, content, sender, and detected data.
/// Inherits from <see cref="AggregateRoot{T}"/> with a <see cref="GlobalId"/> as the identifier.
/// </summary>
public class MessageAnalysis : AggregateRoot<GlobalId>
{
    /// <summary>
    /// Gets the unique identifier of the user associated with the message analysis.
    /// </summary>
    public GlobalId UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the message is private.
    /// </summary>
    public bool IsPrivate { get; }

    /// <summary>
    /// Gets the date and time when the analysis started.
    /// </summary>
    public DateTime StartedDate { get; }

    /// <summary>
    /// Gets the data of the message, including its type, sender, content, and hash set.
    /// </summary>
    public MessageInformation Message { get; }

    /// <summary>
    /// Gets the current state of the analysis, including its status, verdict and threat zone.
    /// </summary>
    public AnalysisState State { get; private set; }

    /// <summary>
    /// Gets the results of the analyses for the attached files in the message.
    /// </summary>
    public IReadOnlyList<DataAssessmentResult<string>> AttachedFilesResults { get; }

    /// <summary>
    /// Gets the detected URL results from the analysis.
    /// </summary>
    public IReadOnlyList<DataAssessmentResult<Uri>> DetectedUrlsResults { get; }

    /// <summary>
    /// Gets the detected email address results from the analysis.
    /// </summary>
    public IReadOnlyList<DataAssessmentResult<MailAddress>> DetectedEmailAddressesResults { get; }

    /// <summary>
    /// Gets the detected phone number results from the analysis.
    /// </summary>
    public IReadOnlyList<DataAssessmentResult<string>> DetectedPhoneNumbersResults { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the message analysis.</param>
    /// <param name="userId">The unique identifier of the user associated with the message analysis.</param>
    /// <param name="isPrivate">A value indicating whether the message is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="message">The data of the message, including its type, sender, content, and hash set.</param>
    /// <param name="state">The current state of the analysis, including its status, verdict, and threat zone.</param>
    /// <param name="attachedFilesResults">An array of multi-analysis results for the attached files in the message.</param>
    /// <param name="detectedUrlsResults">An array of multi-analysis results for the detected URLs in the message.</param>
    /// <param name="detectedEmailAddressesResults">An array of reputation results for the detected email addresses in the message.</param>
    /// <param name="detectedPhoneNumbersResults">An array of reputation results for the detected phone numbers in the message.</param>
    protected MessageAnalysis(
        GlobalId id,
        GlobalId userId,
        bool isPrivate,
        DateTime startedDate,
        MessageInformation message,
        AnalysisState state,
        DataAssessmentResult<string>[] attachedFilesResults,
        DataAssessmentResult<Uri>[] detectedUrlsResults,
        DataAssessmentResult<MailAddress>[] detectedEmailAddressesResults,
        DataAssessmentResult<string>[] detectedPhoneNumbersResults)
        : base(id)
    {
        UserId = userId;
        IsPrivate = isPrivate;
        StartedDate = startedDate;
        Message = message;
        State = state;
        DetectedUrlsResults = detectedUrlsResults;
        DetectedEmailAddressesResults = detectedEmailAddressesResults;
        DetectedPhoneNumbersResults = detectedPhoneNumbersResults;
        AttachedFilesResults = attachedFilesResults;
    }

    // For EF Core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysis"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    protected MessageAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="MessageAnalysis"/> class.
    /// </summary>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="userId">The unique identifier of the user associated with the message analysis.</param>
    /// <param name="isPrivate">A value indicating whether the message is private.</param>
    /// <param name="message">The data of the message, including its type, sender, content, and hash set.</param>
    /// <param name="verdict">The verdict to assign to the analysis, indicating the severity or outcome.</param>
    /// <param name="attachedFilesResults">An array of multi-analysis results for the attached files in the message.</param>
    /// <param name="detectedUrlsResults">An array of multi-analysis results for the detected URLs in the message.</param>
    /// <param name="detectedEmailAddressesResults">An array of reputation results for the detected email addresses in the message.</param>
    /// <param name="detectedPhoneNumbersResults">An array of reputation results for the detected phone numbers in the message.</param>
    /// <returns>A new instance of the <see cref="MessageAnalysis"/> class.</returns>
    /// <remarks>
    /// This factory method generates a new instance of the <see cref="MessageAnalysis"/> class,
    /// initializing it with the provided parameters. It assigns a unique identifier to the
    /// analysis, an initial verdict, and sets the default status and threat zone values.
    /// </remarks>
    public static MessageAnalysis Create(
        DateTime startedDate,
        GlobalId userId,
        bool isPrivate,
        MessageInformation message,
        Verdict verdict,
        DataAssessmentResult<string>[] attachedFilesResults,
        DataAssessmentResult<Uri>[] detectedUrlsResults,
        DataAssessmentResult<MailAddress>[] detectedEmailAddressesResults,
        DataAssessmentResult<string>[] detectedPhoneNumbersResults)
    {
        GlobalId id = GlobalId.CreateUnique();
        var analysisState = AnalysisState.Initial();
        return new MessageAnalysis(
            id,
            userId,
            isPrivate,
            startedDate,
            message,
            analysisState.WithVerdict(verdict),
            attachedFilesResults,
            detectedUrlsResults,
            detectedEmailAddressesResults,
            detectedPhoneNumbersResults);
    }

    /// <summary>
    /// Updates the analysis with a new verdict and overall status.
    /// </summary>
    /// <param name="verdict">The new verdict to assign to the analysis.</param>
    /// <param name="status">The new status to assign to the analysis.</param>
    public void Update(
        Verdict verdict,
        AnalysisStatus status)
    {
        ArgumentNullException.ThrowIfNull(verdict);
        ArgumentNullException.ThrowIfNull(status);

        UpdateVerdict(verdict);
        State = State.WithStatus(status);
    }

    /// <summary>
    /// If the incoming verdict has a higher severity, updates the current one.
    /// </summary>
    /// <param name="incomingVerdict">The new verdict to evaluate and potentially assign.</param>
    private void UpdateVerdict(Verdict incomingVerdict)
    {
        if (incomingVerdict > State.Verdict)
        {
            State = State.WithVerdict(incomingVerdict);
        }
    }
}