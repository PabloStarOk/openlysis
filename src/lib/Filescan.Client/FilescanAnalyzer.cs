using ErrorOr;

using Filescan.Client.Abstractions;
using Filescan.Client.Constants.Common;
using Filescan.Client.Models.Requests;
using Filescan.Client.Models.Scans;

using Openlysis.Application.FileAnalyses.Ports;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Filescan.Client;

/// <summary>
/// Represents a file scanner analyzer that implements the <see cref="IFileAnalyzer"/> interface.
/// </summary>
public class FilescanAnalyzer : IFileAnalyzer
{
    /// <inheritdoc/>
    public string ServiceName => ServiceConstants.ServiceName;

    private readonly IFileScannerService _fileScannerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilescanAnalyzer"/> class.
    /// </summary>
    /// <param name="fileScannerService">The file scanner service to be used for file analysis.</param>
    public FilescanAnalyzer(IFileScannerService fileScannerService)
    {
        _fileScannerService = fileScannerService;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ServiceFileAnalysisId>> AnalyzeAsync(AnalyzeFileRequest request, CancellationToken cancellationToken)
    {
        var options = ScanOptions.True;
        var scanRequest = new ScanRequest(
            request.FileName,
            request.FileContentType,
            request.FileStreamData,
            request.Description,
            Password: request.Password,
            IsPrivateFile: request.IsPrivateFile,
            Options: options);

        return await _fileScannerService.UploadAsync(scanRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ServiceFileAnalysis>> GetAnalysisAsync(ServiceFileAnalysisId analysisId, CancellationToken cancellationToken)
    {
        var getScanRequest = new GetScanRequest(analysisId.Value);

        return await _fileScannerService.GetScanAsync(getScanRequest, cancellationToken);
    }
}