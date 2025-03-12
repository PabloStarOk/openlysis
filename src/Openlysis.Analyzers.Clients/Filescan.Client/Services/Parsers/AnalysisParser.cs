using System.Text.Json;

using Filescan.Client.Abstractions;
using Filescan.Client.Constants.Common;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.FileAnalyses.Entities;

namespace Filescan.Client.Services.Parsers;

/// <summary>
/// Creates <see cref="ServiceFileAnalysis"/> objects from <see cref="JsonElement"/>.
/// </summary>
public class AnalysisParser : ModelParser<ServiceFileAnalysis, JsonElement>
{
    private static readonly Dictionary<string, AnalysisStatus> AnalysisStatusMap = new ()
    {
        { "CREATED", AnalysisStatus.Queued },
        { "QUEUED", AnalysisStatus.Queued },
        { "SCANNING", AnalysisStatus.InProgress },
        { "FINISHED", AnalysisStatus.Completed },
    };

    private readonly ModelParser<Report, JsonProperty> _reportParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisParser"/> class.
    /// </summary>
    /// <param name="reportParser">A <see cref="ModelParser{TModel,TJsonElement}"/>.</param>
    public AnalysisParser(ModelParser<Report, JsonProperty> reportParser)
    {
        _reportParser = reportParser;
    }

    /// <inheritdoc/>
    public override ServiceFileAnalysis Parse(JsonElement rootElement)
    {
        string flowId = GetStringOrEmpty("flowId", rootElement);

        string statusString = GetStringOrEmpty("state", rootElement);
        AnalysisStatus status = ParseAnalysisStatus(statusString);

        Report[] reports = [];
        if (rootElement.TryGetProperty("reports", out JsonElement reportsElement))
        {
            reports = ParseReports(reportsElement);
        }

        return ServiceFileAnalysis.Create(
            flowId,
            ServiceConstants.ServiceName,
            status,
            reports.ToList());
    }

    /// <summary>
    /// Parses a status string into an <see cref="AnalysisStatus"/> enum value.
    /// </summary>
    /// <param name="statusString">The status string to parse.</param>
    /// <returns>The corresponding <see cref="AnalysisStatus"/> enum value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status string does not match any known status.</exception>
    private static AnalysisStatus ParseAnalysisStatus(string statusString)
    {
        statusString = statusString.Trim().Replace(" ", string.Empty).ToUpper();
        return AnalysisStatusMap[statusString];
    }

    /// <summary>
    /// Parses a JSON element into an array of <see cref="Report"/> objects.
    /// </summary>
    /// <param name="element">A <see cref="JsonElement"/> containing the reports.</param>
    /// <returns>An array of <see cref="Report"/> objects.</returns>
    private Report[] ParseReports(JsonElement element)
    {
        var objectEnumerator = element.EnumerateObject();
        List<Report> reports = [];

        while (objectEnumerator.MoveNext())
        {
            var report = _reportParser.Parse(objectEnumerator.Current);
            reports.Add(report);
        }

        return reports.ToArray();
    }
}