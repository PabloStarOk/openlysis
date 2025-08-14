using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.ValueObjects;

namespace Openlysis.API.Endpoints.Messages.Common.Responses;

/// <summary>
/// Data transfer object (DTO) for <see cref="MessageAnalysis"/>.
/// </summary>
/// <param name="Id">The unique identifier of the message analysis.</param>
/// <param name="IsPrivate">Indicates whether the message analysis is private.</param>
/// <param name="StartedDate">The date and time when the analysis started.</param>
/// <param name="MessageInformation">Information about the analyzed message.</param>
/// <param name="Status">The current status of the analysis.</param>
/// <param name="Verdict">The verdict of the analysis.</param>
/// <param name="ThreatZone">The threat zone classification of the analysis.</param>
/// <param name="MessageDetectedData">The data detected during the analysis, such as URLs, email addresses, and phone numbers.</param>
/// <param name="Results">The results of the message analysis, including file, URL, email, and phone analyses.</param>
public record MessageAnalysisDto(
    string Id,
    bool IsPrivate,
    DateTime StartedDate,
    MessageInformation MessageInformation,
    AnalysisStatus Status,
    Verdict Verdict,
    ThreatZone ThreatZone,
    MessageDetectedData MessageDetectedData,
    MessageAnalysisResults Results)
{
    /// <summary>
    /// Parses a <see cref="MessageAnalysis"/> object and its associated results into a <see cref="MessageAnalysisDto"/> object.
    /// </summary>
    /// <param name="source">The <see cref="MessageAnalysis"/> object to parse.</param>
    /// <param name="results">The <see cref="MessageAnalysisResults"/> object containing analysis results.</param>
    /// <returns>A <see cref="MessageAnalysisDto"/> object representing the parsed data.</returns>
    public static MessageAnalysisDto Parse(
        MessageAnalysis source,
        MessageAnalysisResults results)
    {
        var detectedData = MessageDetectedData.CreateFromMessageAnalysis(source);

        return new MessageAnalysisDto(
            source.Id.ToString(),
            source.IsPrivate,
            source.StartedDate,
            source.Message,
            source.State.Status,
            source.State.Verdict,
            source.State.ThreatZone,
            detectedData,
            results);
    }
}