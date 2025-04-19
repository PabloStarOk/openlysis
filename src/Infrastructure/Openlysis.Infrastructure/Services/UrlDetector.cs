using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Infrastructure.Persistence;

using DataType = Openlysis.Domain.Messages.Enums.DataType;

namespace Openlysis.Infrastructure.Services;

/// <summary>
/// A service that detects URLs in a given input string.
/// </summary>
internal class UrlDetector : DataDetector
{
    /// <inheritdoc/>
    public override DataType DetectableData => DataType.Url;

    private static readonly string DefaultScheme = Uri.UriSchemeHttps;
    private readonly UrlAttribute _urlAttribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlDetector"/> class with the specified URL attribute.
    /// </summary>
    /// <param name="logger">The logger instance used for logging within the detector.</param>
    public UrlDetector(ILogger<UrlDetector> logger)
        : base(logger)
    {
        _urlAttribute = new UrlAttribute();
    }

    /// <inheritdoc/>
    protected override Regex GetDetectionPattern() => RegularExpressions.Uri();

    /// <inheritdoc/>
    protected override IEnumerable<Regex> GetExclusionPatterns()
        => RegularExpressions.GetAllExcluding(GetDetectionPattern());

    /// <inheritdoc/>
    protected override string NormalizeDetection(string detection)
    {
        string cleanUrl = base
            .NormalizeDetection(detection)
            .Replace("[", string.Empty)
            .Replace("]", string.Empty)
            .Trim('"', '\'', '(', ')', '[', ']', '<', '>', '“', '”', '‘', '’', '.');

        if (Uri.TryCreate(cleanUrl, UriKind.RelativeOrAbsolute, out Uri? url)
            && url.IsAbsoluteUri)
        {
            return url.AbsoluteUri;
        }

        var urlWithScheme = $"{DefaultScheme}://{cleanUrl}";
        return Uri.TryCreate(urlWithScheme, UriKind.Absolute, out url)
            ? url.AbsoluteUri :
            cleanUrl;
    }

    /// <inheritdoc/>
    protected override bool ValidateDetection(string detection)
    {
        return _urlAttribute.IsValid(detection);
    }
}