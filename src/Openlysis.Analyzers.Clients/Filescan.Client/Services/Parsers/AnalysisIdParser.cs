using System.Text.Json;

using Filescan.Client.Abstractions;

using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Filescan.Client.Services.Parsers;

/// <summary>
/// Creates <see cref="ServiceAnalysisId"/> objects from <see cref="JsonElement"/>.
/// </summary>
public class AnalysisIdParser : ModelParser<ServiceAnalysisId, JsonElement>
{
    /// <inheritdoc/>
    public override ServiceAnalysisId Parse(JsonElement rootElement)
    {
        string flowId = GetStringOrEmpty("flow_id", rootElement);

        return ServiceAnalysisId.Create(flowId);
    }
}