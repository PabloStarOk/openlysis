using Openlysis.Domain.Files;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Represents the response containing file analysis hash values.
/// </summary>
/// <param name="Id">The unique identifier of the multi-analysis.</param>
/// <param name="Sha256">The SHA-256 hash of the file.</param>
/// <param name="Md5">The MD5 hash of the file.</param>
/// <param name="Sha1">The SHA-1 hash of the file.</param>
/// <param name="Sha512">The SHA-512 hash of the file.</param>
public record AnalyzeFileResponse(
    string Id,
    string Sha256,
    string Md5,
    string Sha1,
    string Sha512)
{
    /// <summary>
    /// Parses a <see cref="FileMultiAnalysis"/> object into an <see cref="AnalyzeFileResponse"/>.
    /// </summary>
    /// <param name="source">The source <see cref="FileMultiAnalysis"/> object.</param>
    /// <returns>An <see cref="AnalyzeFileResponse"/> containing hash values from the source.</returns>
    public static AnalyzeFileResponse Parse(FileMultiAnalysis source)
    {
        return new AnalyzeFileResponse(
            source.Id.Value.ToString(),
            source.DataHashValues.Sha256,
            source.DataHashValues.Md5,
            source.DataHashValues.Sha1,
            source.DataHashValues.Sha512);
    }
}