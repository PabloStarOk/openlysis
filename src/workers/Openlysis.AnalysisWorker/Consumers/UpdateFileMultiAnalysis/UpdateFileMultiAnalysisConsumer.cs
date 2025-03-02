using System;
using System.Linq;
using System.Threading.Tasks;

using MassTransit;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.AnalysisWorker.Consumers.UpdateFileMultiAnalysis;

/// <summary>
/// Consumer class for handling the UpdateFileMultiAnalysis message.
/// </summary>
/// <remarks>
/// This class consumes messages of type <see cref="UpdateFileMultiAnalysis"/> and processes them.
/// </remarks>
public class UpdateFileMultiAnalysisConsumer : IConsumer<UpdateFileMultiAnalysis>
{
    /// <summary>
    /// Name of the endpoint.
    /// </summary>
    public const string EndpointName = "update-file-multi-analysis";

    private readonly IRepository<FileMultiAnalysis, FileMultiAnalysisId> _multiAnalysisRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFileMultiAnalysisConsumer"/> class.
    /// </summary>
    /// <param name="multiAnalysisRepository">An <see cref="IRepository{TModel,TModelId}"/> to save <see cref="FileMultiAnalysis"/> entities.</param>
    public UpdateFileMultiAnalysisConsumer(
        IRepository<FileMultiAnalysis, FileMultiAnalysisId> multiAnalysisRepository)
    {
        _multiAnalysisRepository = multiAnalysisRepository;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<UpdateFileMultiAnalysis> context)
    {
        UpdateFileMultiAnalysis request = context.Message;
        FileMultiAnalysis fileMultiAnalysis = await _multiAnalysisRepository.GetAsync(request.Id);
        ArgumentNullException.ThrowIfNull(fileMultiAnalysis);

        foreach (var serviceAnalysis in request.ServiceFileAnalyses)
        {
            if (fileMultiAnalysis.ServiceFileAnalyses.Contains(serviceAnalysis))
            {
                fileMultiAnalysis.UpdateServiceAnalysis(serviceAnalysis);
                continue;
            }

            fileMultiAnalysis.AddServiceAnalysis(serviceAnalysis);
        }

        await _multiAnalysisRepository.UpdateAsync(fileMultiAnalysis);
    }
}