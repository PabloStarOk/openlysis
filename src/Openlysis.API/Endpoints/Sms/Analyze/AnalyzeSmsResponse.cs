using Openlysis.Domain.Messages;

namespace Openlysis.API.Endpoints.Sms.Analyze;

/// <summary>
/// Response given by <see cref="AnalyzeSmsEndpoint"/>.
/// </summary>
/// <param name="Id">The unique identifier for the analysis.</param>
/// <param name="Sha256">The SHA-256 hash of the SMS.</param>
/// <param name="Sha1">The SHA-1 hash of the SMS.</param>
/// <param name="Md5">The MD5 hash of the SMS.</param>
/// <param name="Sha512">The SHA-512 hash of the SMS.</param>
public record AnalyzeSmsResponse(
    string Id,
    string Sha256,
    string Sha1,
    string Md5,
    string Sha512)
{
    /// <summary>
    /// Parses a <see cref="MessageAnalysis"/> object into an <see cref="AnalyzeSmsResponse"/>.
    /// </summary>
    /// <param name="source">The source <see cref="MessageAnalysis"/> object to parse.</param>
    /// <returns>An instance of <see cref="AnalyzeSmsResponse"/> containing the parsed data.</returns>
    public static AnalyzeSmsResponse Parse(MessageAnalysis source)
    {
        return new AnalyzeSmsResponse(
            source.Id.Value.ToString(),
            source.Message.MessageHashSet.Sha256,
            source.Message.MessageHashSet.Sha1,
            source.Message.MessageHashSet.Md5,
            source.Message.MessageHashSet.Sha512);
    }
}