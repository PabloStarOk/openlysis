using System.Collections.Immutable;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeDetective;
using MimeDetective.Engine;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Files;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Files;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Files;

/// <summary>
/// Detects MIME types based on analyzing content streams of files.
/// </summary>
/// <remarks>
/// This service inspects binary content to determine the most likely MIME type,
/// returning appropriate MIME types based on file signatures and patterns.
/// </remarks>
internal class MimeTypeDetector : IMimeTypeDetector
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<FileTypeDetectorOptions> _options;
    private readonly IContentInspector _contentInspector;

    /// <summary>
    /// Initializes a new instance of the <see cref="MimeTypeDetector"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging detection information and debug data.</param>
    /// <param name="options">The options containing configuration settings for MIME type detection.</param>
    /// <param name="contentInspector">The content inspector service used to analyze file content.</param>
    public MimeTypeDetector(
        ILogger<MimeTypeDetector> logger,
        IOptionsMonitor<FileTypeDetectorOptions> options,
        IContentInspector contentInspector)
    {
        _logger = logger;
        _options = options;
        _contentInspector = contentInspector;
    }

    /// <inheritdoc/>
    public async Task<string> DetectAsync(
        Stream stream,
        string? contentTypeFromRequest,
        CancellationToken cancellationToken = default)
    {
        byte[] fileHeaderBuffer = await ReadHeaderBufferAsync(
            stream,
            cancellationToken);

        ImmutableArray<DefinitionMatch> matches =
            FindMostLikelyMatches(fileHeaderBuffer);

        string fallbackMimeType = contentTypeFromRequest
            ?? _options.CurrentValue.FallbackMimeType;
#if DEBUG
        Debug(matches, contentTypeFromRequest, fallbackMimeType);
#endif

        if (matches.Length is 0)
        {
            return fallbackMimeType;
        }

        DefinitionMatch? fallbackMatch = matches.FirstOrDefault(m =>
            m.Definition.File.MimeType is not null
            && m.Definition.File.MimeType.Equals(
                fallbackMimeType,
                StringComparison.OrdinalIgnoreCase));

        if (fallbackMatch is not null)
        {
            return fallbackMimeType;
        }

        string? matchMimeType = matches.First().Definition.File.MimeType;
        return matchMimeType ?? fallbackMimeType;
    }

    /// <summary>
    /// Reads a specified number of bytes from the beginning of a stream into a buffer.
    /// </summary>
    /// <param name="stream">The stream to read data from.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <returns>A byte array containing the header data from the stream.</returns>
    private async Task<byte[]> ReadHeaderBufferAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        byte[] fileHeaderBuffer =
            new byte[_options.CurrentValue.FileHeaderSizeInBytes];
        _ = await stream.ReadAsync(
            buffer: fileHeaderBuffer,
            cancellationToken)
            .ConfigureAwait(false);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return fileHeaderBuffer;
    }

    /// <summary>
    /// Finds the most likely MIME type matches based on a file's header buffer.
    /// </summary>
    /// <param name="fileBuffer">The byte array containing the file header data to analyze.</param>
    /// <returns>An immutable array of definition matches, sorted by match percentage in descending order.</returns>
    private ImmutableArray<DefinitionMatch> FindMostLikelyMatches(byte[] fileBuffer)
    {
        return _contentInspector
            .Inspect(fileBuffer)
            .Where(m => m.Type is DefinitionMatchType.Complete)
            .Where(m =>
                m.Percentage >= _options.CurrentValue.MinAcceptableMatchPercentage)
            .ToHashSet()
            .OrderByDescending(m => m.Percentage)
            .ToImmutableArray();
    }

#if DEBUG
    /// <summary>
    /// Logs detailed information about MIME type matches to the debug log.
    /// </summary>
    /// <param name="matches">The collection of MIME type definition matches to be logged.</param>
    /// <param name="contentTypeFromRequest">The content type specified in the original request.</param>
    /// <param name="fallbackMimeType">The fallback MIME type to use if no match is found.</param>
    private void Debug(
        ImmutableArray<DefinitionMatch> matches,
        string? contentTypeFromRequest,
        string fallbackMimeType)
    {
        _logger.LogDebug(
            "Found a total of {Count} matches\n\tRequest Content Type: {RequestContentType}\n\tFallback Content Type: {FallbackContentType}",
            matches.Length,
            contentTypeFromRequest ?? "null",
            fallbackMimeType);
        foreach (var match in matches)
        {
            DebugSingleMatch(match);
        }
    }

    /// <summary>
    /// Logs detailed information about a single MIME type match to the debug log.
    /// </summary>
    /// <param name="match">The definition match containing MIME type information to be logged.</param>
    private void DebugSingleMatch(DefinitionMatch match)
    {
        _logger.LogDebug(
            "Content Type Match:\n\tMatch Type: {Type}\n\tPercentage: {Percentage}\n\tMIME Type: {MimeType}",
            match.Type,
            match.Percentage,
            match.Definition.File.MimeType);
    }
#endif
}