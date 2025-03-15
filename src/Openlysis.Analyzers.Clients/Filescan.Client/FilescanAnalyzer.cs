using ErrorOr;

using Filescan.Client.Abstractions;
using Filescan.Client.Constants.Common;
using Filescan.Client.Models.Requests;
using Filescan.Client.Models.Scans;

using Openlysis.Analyzers.Contracts.Core.Files.Requests;
using Openlysis.Analyzers.Contracts.Interfaces;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Filescan.Client;

/// <summary>
/// Represents a file scanner analyzer that implements the <see cref="IServiceAnalyzer{TAnalysis,TAnalysisId}"/> interface.
/// </summary>
public class FilescanAnalyzer : IServiceAnalyzer<ServiceFileAnalysis, ServiceAnalysisId>
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
    public async Task<ErrorOr<ServiceAnalysisId>> AnalyzeAsync(FileAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var options = ScanOptions.True;
        var scanRequest = new ScanRequest(
            request.FileName,
            request.FileContentType,
            request.FileData,
            request.FileDescription,
            Password: request.FilePassword,
            IsPrivateFile: request.IsPrivateFile,
            Options: options);

        return await _fileScannerService.UploadAsync(scanRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ServiceFileAnalysis>> GetAnalysisAsync(ServiceAnalysisId analysisId, CancellationToken cancellationToken = default)
    {
        var getScanRequest = new GetScanRequest(analysisId.Value);

        return await _fileScannerService.GetScanAsync(getScanRequest, cancellationToken);
    }
}