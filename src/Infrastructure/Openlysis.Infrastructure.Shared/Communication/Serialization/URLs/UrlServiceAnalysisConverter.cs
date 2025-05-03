using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Infrastructure.Shared.Communication.Serialization.URLs;

/// <summary>
/// Converts JSON to and from <see cref="UrlServiceAnalysis"/> objects.
/// </summary>
internal class UrlServiceAnalysisConverter : JsonConverter<UrlServiceAnalysis>
{
    private const string IdKey = "id";
    private const string ServiceNameKey = "servicename";
    private const string StatusKey = "status";
    private const string VerdictKey = "verdict";
    private const string JobIdKey = "jobid";
    private const string ThreatScoreKey = "threatscore";

    /// <inheritdoc/>
    public override UrlServiceAnalysis Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string id = string.Empty;
        string serviceName = string.Empty;
        AnalysisStatus status = 0;
        Verdict verdict = 0;
        string? jobId = null;
        float? threatScore = null;
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            string key = reader.GetString()?.ToLowerInvariant() ?? string.Empty;
            reader.Read();
            switch (key)
            {
                case IdKey:
                    id = reader.GetString() ?? string.Empty;
                    break;

                case ServiceNameKey:
                    serviceName = reader.GetString() ?? string.Empty;
                    break;

                case StatusKey:
                    status = Enum.Parse<AnalysisStatus>(reader.GetString() ?? string.Empty, ignoreCase: true);
                    break;

                case VerdictKey:
                    verdict = Enum.Parse<Verdict>(reader.GetString() ?? string.Empty, ignoreCase: true);
                    break;

                case JobIdKey when reader.TokenType is JsonTokenType.String:
                    jobId = reader.GetString();
                    break;

                case ThreatScoreKey when reader.TokenType is JsonTokenType.Number:
                    threatScore = (float?)reader.GetDecimal();
                    break;
            }
        }

        return UrlServiceAnalysis.Create(
            id,
            serviceName,
            status,
            verdict,
            jobId,
            threatScore);
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        UrlServiceAnalysis value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? IdKey;
        string serviceNameKey = options.PropertyNamingPolicy?.ConvertName(ServiceNameKey) ?? nameof(UrlServiceAnalysis.ServiceName);
        string statusKey = options.PropertyNamingPolicy?.ConvertName(StatusKey) ?? nameof(UrlServiceAnalysis.Status);
        string verdictKey = options.PropertyNamingPolicy?.ConvertName(VerdictKey) ?? nameof(UrlServiceAnalysis.Verdict);
        string jobIdKey = options.PropertyNamingPolicy?.ConvertName(JobIdKey) ?? JobIdKey;
        string threatScoreKey = options.PropertyNamingPolicy?.ConvertName(ThreatScoreKey) ?? nameof(UrlServiceAnalysis.ThreatScore);

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Id.Primary.Value);
        writer.WriteString(serviceNameKey, value.ServiceName);
        writer.WriteString(statusKey, value.Status.ToString());
        writer.WriteString(verdictKey, value.Verdict.ToString());

        if (value.Id.Job is not null)
        {
            writer.WriteString(jobIdKey, value.Id.Job);
        }

        if (value.ThreatScore is not null)
        {
            writer.WriteNumber(threatScoreKey, (decimal)value.ThreatScore);
        }

        writer.WriteEndObject();
    }
}