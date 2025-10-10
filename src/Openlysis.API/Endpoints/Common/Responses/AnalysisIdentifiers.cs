using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Messages;

namespace Openlysis.API.Endpoints.Common.Responses;

/// <summary>
/// Represents a set of identifiers and hash values for an analysis.
/// </summary>
/// <param name="Id">The unique identifier of the analysis.</param>
/// <param name="Sha256">The SHA-256 hash value.</param>
/// <param name="Md5">The MD5 hash value.</param>
/// <param name="Sha1">The SHA-1 hash value.</param>
/// <param name="Sha512">The SHA-512 hash value.</param>
public sealed record AnalysisIdentifiers(
    string Id,
    string Sha256,
    string Md5,
    string Sha1,
    string Sha512)
{
    /// <summary>
    /// Parses a <see cref="MultiAnalysis{TAnalysis}"/> instance and returns an <see cref="AnalysisIdentifiers"/> record.
    /// </summary>
    /// <typeparam name="TAnalysis">The type of analysis.</typeparam>
    /// <param name="source">The source <see cref="MultiAnalysis{TAnalysis}"/> object.</param>
    /// <returns>An <see cref="AnalysisIdentifiers"/> record containing the identifiers.</returns>
    public static AnalysisIdentifiers Parse<TAnalysis>(
        MultiAnalysis<TAnalysis> source)
        where TAnalysis : Analysis
    {
        return new AnalysisIdentifiers(
            source.Id.ToString(),
            source.DataHashValues.Sha256,
            source.DataHashValues.Md5,
            source.DataHashValues.Sha1,
            source.DataHashValues.Sha512);
    }

    /// <summary>
    /// Parses a <see cref="MessageAnalysis"/> instance and returns an <see cref="AnalysisIdentifiers"/> record.
    /// </summary>
    /// <param name="source">The source <see cref="MessageAnalysis"/> object.</param>
    /// <returns>An <see cref="AnalysisIdentifiers"/> record containing the identifiers.</returns>
    public static AnalysisIdentifiers Parse(MessageAnalysis source)
    {
        return new AnalysisIdentifiers(
            source.Id.ToString(),
            source.Message.HashValues.Sha256,
            source.Message.HashValues.Sha1,
            source.Message.HashValues.Md5,
            source.Message.HashValues.Sha512);
    }
}