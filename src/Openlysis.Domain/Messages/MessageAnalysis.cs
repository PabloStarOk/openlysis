using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages.Entities;
using Openlysis.Domain.Messages.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

using DataType = Openlysis.Domain.Messages.Enums.DataType;

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
    public DateTimeOffset StartedDate { get; }

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
    public IReadOnlyList<Indicator> AttachedFilesIndicators => _attachedFilesIndicators;

    /// <summary>
    /// Gets the detected URL results from the analysis.
    /// </summary>
    public IReadOnlyList<Indicator> DetectedUrlsIndicators => _detectedUrlsIndicators;

    /// <summary>
    /// Gets the detected email address results from the analysis.
    /// </summary>
    public IReadOnlyList<Indicator> DetectedEmailAddressesIndicators => _detectedEmailAddressesIndicators;

    /// <summary>
    /// Gets the detected phone number results from the analysis.
    /// </summary>
    public IReadOnlyList<Indicator> DetectedPhoneNumbersIndicators => _detectedPhoneNumbersIndicators;

    private readonly List<Indicator> _attachedFilesIndicators = [];
    private readonly List<Indicator> _detectedUrlsIndicators = [];
    private readonly List<Indicator> _detectedEmailAddressesIndicators = [];
    private readonly List<Indicator> _detectedPhoneNumbersIndicators = [];
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysis"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="userId">The unique identifier of the user associated with the analysis.</param>
    /// <param name="isPrivate">Indicates whether the message is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="message">The message information being analyzed.</param>
    /// <param name="state">The initial state of the analysis.</param>
    private MessageAnalysis(
        GlobalId id,
        GlobalId userId,
        bool isPrivate,
        DateTimeOffset startedDate,
        MessageInformation message,
        AnalysisState state)
        : base(id)
    {
        UserId = userId;
        IsPrivate = isPrivate;
        StartedDate = startedDate;
        Message = message;
        State = state;
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
    private MessageAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new <see cref="MessageAnalysis"/> instance with the specified parameters.
    /// </summary>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="userId">The unique identifier of the user associated with the analysis.</param>
    /// <param name="isPrivate">Indicates whether the message is private.</param>
    /// <param name="message">The message information being analyzed.</param>
    /// <returns>A new <see cref="MessageAnalysis"/> object.</returns>
    public static MessageAnalysis Create(
        DateTimeOffset startedDate,
        GlobalId userId,
        bool isPrivate,
        MessageInformation message)
    {
        GlobalId id = GlobalId.CreateUnique();
        var analysisState = AnalysisState.Initial();
        return new MessageAnalysis(
            id,
            userId,
            isPrivate,
            startedDate,
            message,
            analysisState);
    }

    /// <summary>
    /// Adds the results of file analyses to the attached files indicators.
    /// </summary>
    /// <param name="multiAnalyses">An array of <see cref="FileMultiAnalysis"/> objects representing the file analyses to add.</param>
    public void AddFileResults(params FileMultiAnalysis[] multiAnalyses)
    {
        ArgumentNullException.ThrowIfNull(multiAnalyses);

        if (multiAnalyses.Length is 0)
        {
            return;
        }

        var indicators = multiAnalyses
            .Select(f => Indicator.Create(
                DataType.File,
                f.FileMetadata.Name,
                f.Id,
                f.State.Verdict,
                f.State.Status))
            .ToArray();

        _attachedFilesIndicators.AddRange(indicators);
    }

    /// <summary>
    /// Adds the results of URL analyses to the detected URLs indicators.
    /// </summary>
    /// <param name="multiAnalyses">An array of <see cref="UrlMultiAnalysis"/> objects representing the URL analyses to add.</param>
    public void AddUrlResults(params UrlMultiAnalysis[] multiAnalyses)
    {
        ArgumentNullException.ThrowIfNull(multiAnalyses);

        if (multiAnalyses.Length is 0)
        {
            return;
        }

        var indicators = multiAnalyses
                .Select(u => Indicator.Create(
                    DataType.Url,
                    u.Url.ToString(),
                    u.Id,
                    u.State.Verdict,
                    u.State.Status))
                .ToArray();

        _detectedUrlsIndicators.AddRange(indicators);
    }

    /// <summary>
    /// Adds the results of email address reputation analyses to the detected email addresses indicators.
    /// </summary>
    /// <param name="multiReputations">An array of <see cref="EmailAddressMultiReputation"/> objects representing the email address reputation analyses to add.</param>
    public void AddEmailAddressResults(params EmailAddressMultiReputation[] multiReputations)
    {
        ArgumentNullException.ThrowIfNull(multiReputations);

        if (multiReputations.Length is 0)
        {
            return;
        }

        var indicators = multiReputations
            .Select(e => Indicator.Create(
                DataType.EmailAddress,
                e.EmailAddress.ToString(),
                e.Id,
                e.FinalVerdict,
                AnalysisStatus.Completed))
            .ToArray();

        _detectedEmailAddressesIndicators.AddRange(indicators);
    }

    /// <summary>
    /// Adds the results of phone number reputation analyses to the detected phone numbers indicators.
    /// </summary>
    /// <param name="multiReputations">An array of <c>PhoneMultiReputation</c> objects representing the phone number reputation analyses to add.</param>
    public void AddPhoneNumberResults(params PhoneMultiReputation[] multiReputations)
    {
        ArgumentNullException.ThrowIfNull(multiReputations);

        if (multiReputations.Length is 0)
        {
            return;
        }

        var indicators = multiReputations
            .Select(p => Indicator.Create(
                DataType.PhoneNumber,
                p.Reputations[0].PhoneInfo.LocalFormat,
                p.Id,
                p.FinalVerdict,
                AnalysisStatus.Completed))
            .ToArray();

        _detectedPhoneNumbersIndicators.AddRange(indicators);
    }

    /// <summary>
    /// Marks the analysis as initialized and updates its information.
    /// Ensures initialization logic is only executed once.
    /// </summary>
    public void CompleteInitialization()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        UpdateInformation();
    }

    /// <summary>
    /// Updates the result of a specific analysis type.
    /// Throws <see cref="ArgumentNullException"/> if <paramref name="multiAnalysis"/> is null.
    /// </summary>
    /// <typeparam name="TAnalysis">The type of analysis.</typeparam>
    /// <param name="multiAnalysis">The analysis result to update.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="multiAnalysis"/> is null.</exception>
    public void UpdateResult<TAnalysis>(MultiAnalysis<TAnalysis> multiAnalysis)
        where TAnalysis : Analysis
    {
        ArgumentNullException.ThrowIfNull(multiAnalysis);

        List<Indicator> indicators = multiAnalysis switch
        {
            FileMultiAnalysis => _attachedFilesIndicators,
            UrlMultiAnalysis => _detectedUrlsIndicators,
            _ => throw new InvalidOperationException($"Unsupported analysis type: {typeof(TAnalysis).Name}"),
        };

        GlobalId id = multiAnalysis.Id;
        if (indicators.All(i => i.ResultId != id))
        {
            throw new InvalidOperationException($"No indicator found for the given analysis ID: {id}.");
        }

        var indicator = indicators.Single(i => i.ResultId == id);
        indicator.UpdateVerdict(multiAnalysis.State.Verdict);
        indicator.UpdateStatus(multiAnalysis.State.Status);
        UpdateInformation();
    }

    private void UpdateInformation()
    {
        UpdateVerdict();
        UpdateStatus();
    }

    private void UpdateVerdict()
    {
        Verdict[] verdicts = _attachedFilesIndicators
            .Concat(_detectedUrlsIndicators)
            .Concat(_detectedEmailAddressesIndicators)
            .Concat(_detectedPhoneNumbersIndicators)
            .Select(i => i.State.Verdict)
            .ToArray();

        if (verdicts.Length is 0)
        {
            State = State.WithVerdict(Verdict.Unknown);
            return;
        }

        State = State.WithVerdict(verdicts.Max());
    }

    private void UpdateStatus()
    {
        AnalysisStatus[] analysisStatuses = _attachedFilesIndicators
            .Concat(_detectedUrlsIndicators)
            .Concat(_detectedEmailAddressesIndicators)
            .Concat(_detectedPhoneNumbersIndicators)
            .Select(i => i.State.Status)
            .ToArray();

        // If there are no long-running analyses (files/URLs), the process is considered complete.
        if (analysisStatuses.Length is 0)
        {
            State = State.WithStatus(AnalysisStatus.Completed);
            return;
        }

        // If all analyses have timed out, set the overall status to Timeout.
        if (analysisStatuses.All(s => s is AnalysisStatus.Timeout))
        {
            State = State.WithStatus(AnalysisStatus.Timeout);
            return;
        }

        // If all analyses have failed, set the overall status to Failed.
        if (analysisStatuses.All(s => s is AnalysisStatus.Failed))
        {
            State = State.WithStatus(AnalysisStatus.Failed);
            return;
        }

        // If all analyses are finished (i.e., not Queued or InProgress) and at least one has 'Completed',
        // set the overall status to 'Completed'.
        if (analysisStatuses.All(s => s is not AnalysisStatus.Queued and not AnalysisStatus.InProgress)
            && analysisStatuses.Any(s => s is AnalysisStatus.Completed))
        {
            State = State.WithStatus(AnalysisStatus.Completed);
            return;
        }

        // Otherwise, determine the most representative status from the ongoing analyses.
        var ongoingStatus = analysisStatuses
            .Where(s => s is AnalysisStatus.Queued or AnalysisStatus.InProgress)
            .Min(); // Queued (0) < InProgress (1)

        State = State.WithStatus(ongoingStatus);
    }
}