using System.Net.Http.Headers;

using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Constants;
using Openlysis.Analyzers.Filescan.Core.Models.Objects;
using Openlysis.Analyzers.Filescan.Core.Models.Requests;

namespace Openlysis.Analyzers.Filescan.Infrastructure.Factories;

/// <summary>
/// Factory class for creating file scan requests.
/// </summary>
public class FileRequestFactory : IRequestFactory
{
    private readonly ScanRequest _request;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileRequestFactory"/> class.
    /// </summary>
    /// <param name="request">The scan request containing the file and its metadata.</param>
    public FileRequestFactory(ScanRequest request)
    {
        _request = request;
    }

    /// <inheritdoc/>
    public FilescanAnalysisRequest Create()
    {
        var formDataContent = new MultipartFormDataContent();

        // Add file
        var fileStreamContent = new StreamContent(_request.FileData);
        fileStreamContent.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse(_request.FileMimeType);
        formDataContent.Add(fileStreamContent, ScanFieldNames.File, _request.FileName);

        // Add not required fields
        AddStringContent(formDataContent, _request.Description, ScanFieldNames.Description);
        var tags = string.Join('|', _request.Tags ?? []);
        AddStringContent(formDataContent, tags, ScanFieldNames.Tags);
        AddStringContent(formDataContent, _request.PropagateTags, ScanFieldNames.PropagateTags);
        AddStringContent(formDataContent, _request.Password, ScanFieldNames.Password);
        AddStringContent(formDataContent, _request.IsPrivateFile, ScanFieldNames.IsPrivateFile);
        AddStringContent(formDataContent, _request.IsPrivateReport, ScanFieldNames.IsPrivateReport);
        AddStringContent(formDataContent, _request.SkipWhiteListed, ScanFieldNames.SkipWhiteListed);
        AddStringContent(formDataContent, _request.ScanProfile, ScanFieldNames.ScanProfile);
        AddStringContent(formDataContent, _request.Options?.RapidMode, ScanFieldNames.RapidMode);
        AddStringContent(formDataContent, _request.Options?.EarlyTermination, ScanFieldNames.EarlyTermination);
        AddStringContent(formDataContent, _request.Options?.Osint, ScanFieldNames.Osint);
        AddStringContent(formDataContent, _request.Options?.ExtendedOsint, ScanFieldNames.ExtendedOsint);
        AddStringContent(formDataContent, _request.Options?.ExtractedFilesOsint, ScanFieldNames.ExtractedFilesOsint);
        AddStringContent(formDataContent, _request.Options?.Visualization, ScanFieldNames.Visualization);
        AddStringContent(formDataContent, _request.Options?.FilesDownload, ScanFieldNames.FilesDownload);
        AddStringContent(formDataContent, _request.Options?.ResolveDomains, ScanFieldNames.ResolveDomains);
        AddStringContent(formDataContent, _request.Options?.InputFileYara, ScanFieldNames.InputFileYara);
        AddStringContent(formDataContent, _request.Options?.ExtractedFilesYara, ScanFieldNames.ExtractedFilesYara);
        AddStringContent(formDataContent, _request.Options?.WhoIs, ScanFieldNames.Whois);
        AddStringContent(formDataContent, _request.Options?.IpsMeta, ScanFieldNames.IpsMeta);
        AddStringContent(formDataContent, _request.Options?.ImagesOcr, ScanFieldNames.ImagesOcr);
        AddStringContent(formDataContent, _request.Options?.Certificates, ScanFieldNames.Certificates);
        AddStringContent(formDataContent, _request.Options?.UrlAnalysis, ScanFieldNames.UrlAnalysis);
        AddStringContent(formDataContent, _request.Options?.ExtractStrings, ScanFieldNames.ExtractStrings);
        AddStringContent(formDataContent, _request.Options?.OcrQr, ScanFieldNames.OcrQr);
        AddStringContent(formDataContent, _request.Options?.PhishingDetection, ScanFieldNames.PhishingDetection);

        return new FilescanAnalysisRequest(
            Addresses.ScanFile,
            formDataContent);
    }

    /// <summary>
    /// Adds a content part to a multipart form data content.
    /// </summary>
    /// <param name="multipartContent">The multipart form data content to which the part will be added.</param>
    /// <param name="objToAdd">The object to be added as a content part. If null, an empty string will be used.</param>
    /// <param name="fieldName">The name of the field for the content part.</param>
    private static void AddStringContent(MultipartFormDataContent multipartContent, object? objToAdd, string fieldName)
    {
        string value = objToAdd?.ToString() ?? string.Empty;
        var skipWhitelistedContent = new StringContent(value);
        if (!string.IsNullOrWhiteSpace(value))
        {
            multipartContent.Add(skipWhitelistedContent, fieldName);
        }
    }
}