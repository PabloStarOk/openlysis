using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Openlysis.FileMultiAnalysisWorker;

/// <summary>
/// A background service to keep running the file multi analysis service.
/// </summary>
public class FileMultiAnalysisWorker : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
        }
    }
}