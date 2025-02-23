using System.Text.Json;

using Filescan.Client.Abstractions;

using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Filescan.Client.Services.Parsers;

/// <summary>
/// Creates <see cref="ServiceFileAnalysisId"/> objects from <see cref="JsonElement"/>.
/// </summary>
public class AnalysisIdParser : ModelParser<ServiceFileAnalysisId, JsonElement>
{
    /// <inheritdoc/>
    public override ServiceFileAnalysisId Parse(JsonElement rootElement)
    {
        string flowId = GetStringOrEmpty("flow_id", rootElement);

        return ServiceFileAnalysisId.Create(flowId);
    }
}