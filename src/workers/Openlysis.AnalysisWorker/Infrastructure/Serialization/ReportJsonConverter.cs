using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Reports;

namespace Openlysis.AnalysisWorker.Infrastructure.Serialization;

/// <summary>
/// Converts a <see cref="Report"/> object to and from JSON.
/// </summary>
public class ReportJsonConverter : JsonConverter<Report>
{
    private const string IdKey = "id";
    private const string VerdictKey = "verdict";
    private const string ThreatZoneKey = "threatzone";
    private const string ThreatLevelKey = "threatlevel";

    /// <inheritdoc/>
    public override Report Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string id = string.Empty;
        Verdict verdict = 0;
        ThreatZone threatZone = 0;
        float? threatLevel = null;
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.StartObject)
            {
                continue;
            }

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

                case VerdictKey:
                    verdict = Enum.Parse<Verdict>(reader.GetString() ?? string.Empty, ignoreCase: true);
                    break;

                case ThreatZoneKey:
                    threatZone = Enum.Parse<ThreatZone>(reader.GetString() ?? string.Empty, ignoreCase: true);
                    break;

                case ThreatLevelKey:
                    if (reader.TryGetDecimal(out decimal threatLevelDecimal))
                    {
                        threatLevel = (float)threatLevelDecimal;
                    }

                    break;
            }
        }

        return Report.Create(
            id,
            verdict,
            threatZone,
            threatLevel);
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        Report value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? nameof(Report.Id);
        string verdictKey = options.PropertyNamingPolicy?.ConvertName(VerdictKey) ?? nameof(Report.Verdict);
        string threatZoneKey = options.PropertyNamingPolicy?.ConvertName(ThreatZoneKey) ?? nameof(Report.ThreatZone);
        string threatLevelKey = options.PropertyNamingPolicy?.ConvertName(ThreatLevelKey) ?? nameof(Report.ThreatLevel);

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Id.Value);
        writer.WriteString(verdictKey, value.Verdict.ToString());
        writer.WriteString(threatZoneKey, value.ThreatZone.ToString());
        if (value.ThreatLevel is null)
        {
            writer.WriteNull(threatLevelKey);
        }
        else
        {
            writer.WriteNumber(threatLevelKey, (decimal)value.ThreatLevel);
        }

        writer.WriteEndObject();
    }
}