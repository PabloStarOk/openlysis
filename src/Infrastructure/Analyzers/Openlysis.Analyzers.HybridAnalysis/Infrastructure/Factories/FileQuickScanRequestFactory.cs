using System.Net.Http.Headers;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Factories;

/// <summary>
/// Factory for creating file quick scan requests for Hybrid Analysis API.
/// Implements the <see cref="IRequestFactory"/> interface to standardize
/// the creation of scan requests.
/// </summary>
internal class FileQuickScanRequestFactory : IRequestFactory
{
    private const string ScanTypeBodyParamName = "scan_type";
    private const string FileBodyParamName = "file";

    private readonly AnalyzeFileRequest _request;
    private readonly string _scanType;
    private readonly string _mimeType;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileQuickScanRequestFactory"/> class.
    /// </summary>
    /// <param name="request">The analysis file request containing file data and name.</param>
    /// <param name="scanType">The type of scan to be performed.</param>
    /// <param name="mimeType">The MIME type of the file being scanned.</param>
    public FileQuickScanRequestFactory(
        AnalyzeFileRequest request,
        string scanType,
        string mimeType)
    {
        _request = request;
        _scanType = scanType;
        _mimeType = mimeType;
    }

    /// <inheritdoc/>
    public HybridAnalysisAnalyzeRequest Create()
    {
        var content = new MultipartFormDataContent();

        AddScanTypeToContent(content);
        AddFileToContent(content);

        return new HybridAnalysisAnalyzeRequest(
            Addresses.FileQuickScanEndpoint,
            content);
    }

    /// <summary>
    /// Adds the scan type parameter to the multipart form data content.
    /// </summary>
    /// <param name="content">The multipart form data content to add the scan type to.</param>
    private void AddScanTypeToContent(MultipartFormDataContent content)
    {
        var scanTypeContent = new StringContent(_scanType);
        content.Add(scanTypeContent, ScanTypeBodyParamName);
    }

    /// <summary>
    /// Adds the file parameter to the multipart form data content.
    /// </summary>
    /// <param name="content">The multipart form data content to add the file to.</param>
    private void AddFileToContent(MultipartFormDataContent content)
    {
        var fileContent = new StreamContent(_request.FileData);
        var disposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = FileBodyParamName,
            FileName = _request.FileName,
        };

        fileContent.Headers.ContentDisposition = disposition;
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(_mimeType);

        content.Add(fileContent);
    }
}