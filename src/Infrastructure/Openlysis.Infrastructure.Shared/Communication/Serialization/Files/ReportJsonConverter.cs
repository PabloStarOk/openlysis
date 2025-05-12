using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files.Entities;

namespace Openlysis.Infrastructure.Shared.Communication.Serialization.Files;

/// <summary>
/// Converts a <see cref="FileReport"/> object to and from JSON.
/// </summary>
internal class ReportJsonConverter : JsonConverter<FileReport>
{
    private const string IdKey = "id";
    private const string VerdictKey = "verdict";
    private const string ThreatZoneKey = "threatzone";
    private const string ThreatLevelKey = "threatlevel";

    /// <inheritdoc/>
    public override FileReport Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
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
                    if (reader.TokenType is not JsonTokenType.Null
                        && reader.TryGetDecimal(out decimal threatLevelDecimal))
                    {
                        threatLevel = (float)threatLevelDecimal;
                    }

                    break;
            }
        }

        return FileReport.Create(
            id,
            verdict,
            threatZone,
            threatLevel);
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        FileReport value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? nameof(FileReport.Id);
        string verdictKey = options.PropertyNamingPolicy?.ConvertName(VerdictKey) ?? nameof(FileReport.Verdict);
        string threatZoneKey = options.PropertyNamingPolicy?.ConvertName(ThreatZoneKey) ?? nameof(FileReport.ThreatZone);
        string threatLevelKey = options.PropertyNamingPolicy?.ConvertName(ThreatLevelKey) ?? nameof(FileReport.ThreatScore);

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Id.Value);
        writer.WriteString(verdictKey, value.Verdict.ToString());
        writer.WriteString(threatZoneKey, value.ThreatZone.ToString());
        if (value.ThreatScore is null)
        {
            writer.WriteNull(threatLevelKey);
        }
        else
        {
            writer.WriteNumber(threatLevelKey, (decimal)value.ThreatScore);
        }

        writer.WriteEndObject();
    }
}