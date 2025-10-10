using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Messages.Contracts.Abstractions;

using DataType = Openlysis.Domain.Messages.Enums.DataType;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// A service that detects URLs in a given input string.
/// </summary>
internal class UrlDetector : DataDetector
{
    /// <inheritdoc/>
    public override DataType DetectableData => DataType.Url;

    private const string MalformedHttpScheme = "http//";
    private const string MalformedHttpsScheme = "https//";

    private static readonly string DefaultScheme = Uri.UriSchemeHttps;
    private static readonly char[] TrimmableChars = ['"', '\'', '(', ')', '[', ']', '{', '}', '<', '>', '“', '”', '‘', '’', '.', ',', ';', ':', '!', '?'];
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
        string cleanUrl = base.NormalizeDetection(detection).Trim(TrimmableChars);

        cleanUrl = CorrectMalformedSchemes(cleanUrl);

        if (Uri.TryCreate(cleanUrl, UriKind.RelativeOrAbsolute, out Uri? url)
            && url.IsAbsoluteUri)
        {
            return url.AbsoluteUri;
        }

        var urlWithScheme = $"{DefaultScheme}{Uri.SchemeDelimiter}{cleanUrl}";
        return Uri.TryCreate(urlWithScheme, UriKind.Absolute, out url)
            ? url.AbsoluteUri :
            cleanUrl;
    }

    /// <inheritdoc/>
    protected override bool ValidateDetection(string detection)
    {
        if (!_urlAttribute.IsValid(detection)
            || !Uri.TryCreate(detection, UriKind.Absolute, out var url))
        {
            return false;
        }

        if (ValidateHostNameType(url))
        {
            return true;
        }

        return url.HostNameType is UriHostNameType.Dns &&
            ValidateScheme(url) && ValidateHost(url) && ValidateTld(url);
    }

    private static string CorrectMalformedSchemes(string url)
    {
        if (url.StartsWith(MalformedHttpScheme, StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat(
                Uri.UriSchemeHttp,
                Uri.SchemeDelimiter,
                url.AsSpan(MalformedHttpScheme.Length));
        }

        if (url.StartsWith(MalformedHttpsScheme, StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat(
                Uri.UriSchemeHttps,
                Uri.SchemeDelimiter,
                url.AsSpan(MalformedHttpsScheme.Length));
        }

        return url;
    }

    private static bool ValidateScheme(Uri url)
    {
        return url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps;
    }

    private static bool ValidateHostNameType(Uri url)
    {
        return url.HostNameType is UriHostNameType.IPv4 or UriHostNameType.IPv6;
    }

    private static bool ValidateHost(Uri url)
    {
        return url.Host.Contains('.') && !url.Host.EndsWith('.') && !url.Host.StartsWith('.');
    }

    private static bool ValidateTld(Uri url)
    {
        var hostParts = url.Host.Split('.');
        if (hostParts.Length < 2)
        {
            return false;
        }

        var tld = hostParts[^1];
        return tld.Length > 1 && !int.TryParse(tld, out _);
    }
}