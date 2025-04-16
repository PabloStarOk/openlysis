using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Common.Aggregates;

/// <summary>
/// Represents the analysis of a message, including its type, content, sender, and detected data.
/// Inherits from <see cref="AggregateRoot{T}"/> with a <see cref="GlobalId"/> as the identifier.
/// </summary>
public class MessageAnalysis : AggregateRoot<GlobalId>
{
    /// <summary>
    /// Gets the date and time when the analysis started.
    /// </summary>
    public DateTime StartedDate { get; }

    /// <summary>
    /// Gets the data of the message, including its type, sender, content, and hash set.
    /// </summary>
    public virtual MessageInformation Message { get; }

    /// <summary>
    /// Gets the current state of the analysis, including its status, verdict and threat zone.
    /// </summary>
    public AnalysisState State { get; private set; }

    /// <summary>
    /// Gets the detected URL results from the analysis.
    /// </summary>
    public IReadOnlyList<DataAssessmentResult<Uri>> DetectedUrlsResults { get; }

    /// <summary>
    /// Gets the detected email address results from the analysis.
    /// </summary>
    public IReadOnlyList<DataAssessmentResult<string>> DetectedEmailAddressesResults { get; }

    /// <summary>
    /// Gets the detected phone number results from the analysis.
    /// </summary>
    public IReadOnlyList<DataAssessmentResult<string>> DetectedPhoneNumbersResults { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the message analysis.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="message">The data of the message, including its type, sender, content, and hash set.</param>
    /// <param name="state">The current state of the analysis, including its status, verdict, and threat zone.</param>
    /// <param name="detectedUrlsResults">An array of detected URL results from the analysis.</param>
    /// <param name="detectedEmailAddressesResults">An array of detected email address results from the analysis.</param>
    /// <param name="detectedPhoneNumbersResults">An array of detected phone number results from the analysis.</param>
    protected MessageAnalysis(
        GlobalId id,
        DateTime startedDate,
        MessageInformation message,
        AnalysisState state,
        DataAssessmentResult<Uri>[] detectedUrlsResults,
        DataAssessmentResult<string>[] detectedEmailAddressesResults,
        DataAssessmentResult<string>[] detectedPhoneNumbersResults)
        : base(id)
    {
        StartedDate = startedDate;
        Message = message;
        State = state;
        DetectedUrlsResults = detectedUrlsResults;
        DetectedEmailAddressesResults = detectedEmailAddressesResults;
        DetectedPhoneNumbersResults = detectedPhoneNumbersResults;
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
    /// <param name="message">The data of the message, including its type, sender, content, and hash set.</param>
    /// <param name="detectedUrlsResults">An array of detected URL results from the analysis.</param>
    /// <param name="detectedEmailAddressesResults">An array of detected email address results from the analysis.</param>
    /// <param name="detectedPhoneNumbersResults">An array of detected phone number results from the analysis.</param>
    /// <returns>A new instance of the <see cref="MessageAnalysis"/> class.</returns>
    public static MessageAnalysis Create(
        DateTime startedDate,
        MessageInformation message,
        DataAssessmentResult<Uri>[] detectedUrlsResults,
        DataAssessmentResult<string>[] detectedEmailAddressesResults,
        DataAssessmentResult<string>[] detectedPhoneNumbersResults)
    {
        GlobalId id = GlobalId.CreateUnique();
        return new MessageAnalysis(
            id,
            startedDate,
            message,
            AnalysisState.Initial(),
            detectedUrlsResults,
            detectedEmailAddressesResults,
            detectedPhoneNumbersResults);
    }

    /// <summary>
    /// Updates the analysis with a new verdict and status for a specific detected result.
    /// </summary>
    /// <param name="resultId">The unique identifier of the result to update.</param>
    /// <param name="verdict">The new verdict to assign to the analysis.</param>
    /// <param name="status">The new status to assign to the analysis.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="resultId"/>, <paramref name="verdict"/>, or <paramref name="status"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="resultId"/> does not exist in the analysis.
    /// </exception>
    public void Update(
        GlobalId resultId,
        Verdict verdict,
        AnalysisStatus status)
    {
        ArgumentNullException.ThrowIfNull(resultId);
        ArgumentNullException.ThrowIfNull(verdict);
        ArgumentNullException.ThrowIfNull(status);

        GlobalId[] resultIds = GetAllGlobalIds();

        if (!resultIds.Contains(resultId))
        {
            throw new ArgumentException("MessageAnalysis does not contain an analysis with the given id.", nameof(resultId));
        }

        UpdateVerdict(verdict);
        State = State.WithStatus(status);
    }

    /// <summary>
    /// Retrieves all unique identifiers (GlobalIds) from the detected results,
    /// including URLs, email addresses, and phone numbers.
    /// </summary>
    /// <returns>An array of <see cref="GlobalId"/> representing all detected results.</returns>
    protected virtual GlobalId[] GetAllGlobalIds()
    {
        return
        [
            ..DetectedUrlsResults.Select(d => d.ResultId),
            ..DetectedEmailAddressesResults.Select(d => d.ResultId),
            ..DetectedPhoneNumbersResults.Select(d => d.ResultId)
        ];
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