using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Emails.ValueObjects;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.Domain.Emails;

/// <summary>
/// Represents the analysis of an email message, inheriting from <see cref="MessageAnalysis"/>.
/// </summary>
public sealed class EmailAnalysis : MessageAnalysis
{
    /// <summary>
    /// Gets represents the email data associated with the analysis.
    /// </summary>
    public override EmailInformation Message { get; }

    /// <summary>
    /// Gets the results of the analyses for the attached files in the email.
    /// </summary>
    public IReadOnlyList<DataAssessmentResult<FileMetadata>> AttachedFilesResults { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAnalysis"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the email analysis.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="message">The email data associated with the analysis.</param>
    /// <param name="state">The state of the analysis.</param>
    /// <param name="detectedUrlsResults">The results of the analysis for detected URLs in the email.</param>
    /// <param name="detectedEmailAddressesResults">The results of the analysis for detected email addresses in the email.</param>
    /// <param name="detectedPhoneNumbersResults">The results of the analysis for detected phone numbers in the email.</param>
    /// <param name="attachedFilesResults">The results of the analysis for attached files in the email.</param>
    private EmailAnalysis(
        GlobalId id,
        DateTime startedDate,
        EmailInformation message,
        AnalysisState state,
        DataAssessmentResult<Uri>[] detectedUrlsResults,
        DataAssessmentResult<string>[] detectedEmailAddressesResults,
        DataAssessmentResult<string>[] detectedPhoneNumbersResults,
        DataAssessmentResult<FileMetadata>[] attachedFilesResults)
        : base(
            id,
            startedDate,
            message,
            state,
            detectedUrlsResults,
            detectedEmailAddressesResults,
            detectedPhoneNumbersResults)
    {
        Message = message;
        AttachedFilesResults = attachedFilesResults;
    }

    // For EF Core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAnalysis"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    private EmailAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="EmailAnalysis"/> class with the specified parameters.
    /// </summary>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="message">The email data associated with the analysis.</param>
    /// <param name="detectedUrlsResults">The results of the analysis for detected URLs in the email.</param>
    /// <param name="detectedEmailAddressesResults">The results of the analysis for detected email addresses in the email.</param>
    /// <param name="detectedPhoneNumbersResults">The results of the analysis for detected phone numbers in the email.</param>
    /// <param name="attachedFilesResults">The results of the analysis for attached files in the email.</param>
    /// <returns>A new instance of the <see cref="EmailAnalysis"/> class.</returns>
    /// <remarks>
    /// This factory method generates a new instance of the <see cref="EmailAnalysis"/> class,
    /// initializing it with the provided parameters. It assigns a unique identifier to the
    /// analysis and sets the default status, verdict, and threat zone values.
    /// </remarks>
    public static EmailAnalysis Create(
        DateTime startedDate,
        EmailInformation message,
        DataAssessmentResult<Uri>[] detectedUrlsResults,
        DataAssessmentResult<string>[] detectedEmailAddressesResults,
        DataAssessmentResult<string>[] detectedPhoneNumbersResults,
        DataAssessmentResult<FileMetadata>[] attachedFilesResults)
    {
        GlobalId id = GlobalId.CreateUnique();
        return new EmailAnalysis(
            id,
            startedDate,
            message,
            AnalysisState.Initial(),
            detectedUrlsResults,
            detectedEmailAddressesResults,
            detectedPhoneNumbersResults,
            attachedFilesResults);
    }

    /// <inheritdoc/>
    protected override GlobalId[] GetAllGlobalIds()
    {
        return
        [
            ..base.GetAllGlobalIds(),
            ..AttachedFilesResults.Select(a => a.ResultId)
        ];
    }
}