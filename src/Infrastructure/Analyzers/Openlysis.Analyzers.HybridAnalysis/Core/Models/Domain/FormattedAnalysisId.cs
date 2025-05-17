using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Domain;

/// <summary>
/// Represents a formatted analysis identifier consisting of a raw ID and an analysis type.
/// This record is used to uniquely identify analyses in the Hybrid Analysis system.
/// </summary>
internal record FormattedAnalysisId
{
    /// <summary>
    /// Gets the raw identifier of the analysis.
    /// </summary>
    public string RawId { get; }

    /// <summary>
    /// Gets the type of the analysis.
    /// </summary>
    public AnalysisType AnalysisType { get; }

    private const char CharSeparator = '!';

    /// <summary>
    /// Initializes a new instance of the <see cref="FormattedAnalysisId"/> class.
    /// </summary>
    /// <param name="rawId">The raw identifier of the analysis.</param>
    /// <param name="analysisType">The type of analysis.</param>
    private FormattedAnalysisId(string rawId, AnalysisType analysisType)
    {
        RawId = rawId;
        AnalysisType = analysisType;
    }

    /// <summary>
    /// Creates a new instance of <see cref="FormattedAnalysisId"/> with the specified parameters.
    /// </summary>
    /// <param name="rawId">The raw identifier of the analysis.</param>
    /// <param name="analysisType">The type of analysis.</param>
    /// <returns>A new <see cref="FormattedAnalysisId"/> instance.</returns>
    internal static FormattedAnalysisId Create(
        string rawId,
        AnalysisType analysisType)
    {
        return new FormattedAnalysisId(rawId, analysisType);
    }

    /// <summary>
    /// Parses a formatted analysis ID string into a <see cref="FormattedAnalysisId"/> instance.
    /// </summary>
    /// <param name="formattedId">The formatted ID string to parse, in the format "AnalysisType:RawId".</param>
    /// <returns>A new <see cref="FormattedAnalysisId"/> instance created from the parsed components.</returns>
    /// <exception cref="FormatException">Thrown when the formatted ID is invalid or contains an invalid analysis type.</exception>
    internal static FormattedAnalysisId Parse(string formattedId)
    {
        string[] idParts = formattedId.Split(CharSeparator);

        if (idParts.Length != 2)
        {
            throw new FormatException($"\"{formattedId}\"is an invalid Hybrid Analysis formatted ID.");
        }

        string analysisTypeString = idParts[0].Trim();
        if (!Enum.TryParse(
                analysisTypeString,
                ignoreCase: true,
                out AnalysisType analysisType))
        {
            throw new FormatException($"\"{analysisTypeString}\" is an invalid analysis type for Hybrid Analysis formatted ID.");
        }

        string rawId = idParts[1].Trim();
        return new FormattedAnalysisId(
            rawId,
            analysisType);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string formattedAnalysisType = AnalysisType.ToString().ToLower();
        return string
            .Join(CharSeparator, formattedAnalysisType, RawId)
            .Trim();
    }
}