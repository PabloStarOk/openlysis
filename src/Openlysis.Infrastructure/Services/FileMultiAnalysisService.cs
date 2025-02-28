using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Interfaces.Ports;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Infrastructure.Services;

/// <summary>
/// Service to analyze a file using multi services.
/// </summary>
public class FileMultiAnalysisService : IFileMultiAnalysisService
{
    private readonly IOptions<FormOptions> _formOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="formOptions">The form options for configuring the service.</param>
    public FileMultiAnalysisService(
        IOptions<FormOptions> formOptions)
    {
        _formOptions = formOptions;
    }

    /// <inheritdoc/>
    public async Task StartJobAsync(
        FileAnalysisJobRequest request,
        FileMultiAnalysis multiAnalysis,
        CancellationToken cancellationToken)
    {
        Stream fileDataStream = await CopyStreamAsync(request.FileStreamData, cancellationToken);
        FileAnalysisJobRequest copiedRequest = request with
        {
            FileStreamData = fileDataStream,
        };

        // TODO: Send request to daemon.
    }

    /// <summary>
    /// Copies the provided stream to a new stream.
    /// </summary>
    /// <param name="stream">The stream to be copied.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the new stream.</returns>
    private async Task<Stream> CopyStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        int length = (int)stream.Length;
        Stream newStream;
        if (stream.Length <= _formOptions.Value.MemoryBufferThreshold)
        {
            newStream = new MemoryStream(length);
        }
        else
        {
            string tempFileFullPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            while (File.Exists(tempFileFullPath))
            {
                tempFileFullPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            var options = new FileStreamOptions
            {
                Access = FileAccess.ReadWrite,
                Mode = FileMode.CreateNew,
                Options = FileOptions.Asynchronous | FileOptions.DeleteOnClose,
                Share = FileShare.None,
                PreallocationSize = length,
            };

            newStream = new FileStream(tempFileFullPath, options);
        }

        stream.Position = 0;
        await stream.CopyToAsync(newStream, cancellationToken);
        newStream.Position = 0;
        return newStream;
    }
}