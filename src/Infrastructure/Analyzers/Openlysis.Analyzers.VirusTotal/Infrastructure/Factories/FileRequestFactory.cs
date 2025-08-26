using System.Net.Http.Headers;

using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;

namespace Openlysis.Analyzers.VirusTotal.Infrastructure.Factories;

/// <summary>
/// Creates requests to analyze a file.
/// </summary>
internal class FileRequestFactory : IRequestFactory
{
    private const string FileBodyParamName = "file";
    private const string PasswordBodyParamName = "password";

    private readonly AnalyzeFileRequest _request;
    private readonly Stream _fileData;
    private readonly string? _largeFileUploadUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileRequestFactory"/> class.
    /// </summary>
    /// <param name="request">The file analysis request containing file data and metadata.</param>
    /// <param name="fileData">The stream containing the file data to be scanned.</param>
    /// <param name="largeFileUploadUrl">Optional URL for uploading large files (greater than 32MB).</param>
    internal FileRequestFactory(
        AnalyzeFileRequest request,
        Stream fileData,
        string? largeFileUploadUrl)
    {
        _request = request;
        _fileData = fileData;
        _largeFileUploadUrl = largeFileUploadUrl;
    }

    /// <inheritdoc/>
    public VirusTotalAnalysisRequest Create()
    {
        long fileSize = _request.FileSize;
        if (fileSize > Files.SmallFilesMaxSizeInBytes && _largeFileUploadUrl is null)
        {
            throw new InvalidOperationException("Cannot process files larger than 32MB without a valid large file upload URL.");
        }

        string endpointUrl = fileSize <= Files.SmallFilesMaxSizeInBytes
            || string.IsNullOrWhiteSpace(_largeFileUploadUrl)
                ? Addresses.SmallFilesEndpoint
                : _largeFileUploadUrl;

        var httpContent = new MultipartFormDataContent();
        AddFileToContent(httpContent);
        AddPasswordToContent(httpContent);

        return new VirusTotalAnalysisRequest(
            endpointUrl,
            httpContent);
    }

    /// <summary>
    /// Adds the file from the request to the multipart form data content.
    /// </summary>
    /// <param name="httpContent">The HTTP content to add the file to.</param>
    private void AddFileToContent(MultipartFormDataContent httpContent)
    {
        var fileContent = new StreamContent(_fileData);
        var disposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = $"\"{FileBodyParamName}\"",
            FileName = $"\"{_request.FileName}\"",
        };

        fileContent.Headers.ContentDisposition = disposition;
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(_request.FileContentType);
        httpContent.Add(fileContent);
    }

    /// <summary>
    /// Adds the password from the request to the multipart form data content if one is provided.
    /// </summary>
    /// <param name="httpContent">The HTTP content to add the password to.</param>
    private void AddPasswordToContent(MultipartFormDataContent httpContent)
    {
        if (string.IsNullOrEmpty(_request.FilePassword))
        {
            return;
        }

        var stringContent = new StringContent(_request.FilePassword);
        httpContent.Add(stringContent, PasswordBodyParamName);
    }
}