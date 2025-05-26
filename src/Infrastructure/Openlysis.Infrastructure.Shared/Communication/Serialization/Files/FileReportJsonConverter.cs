using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;

namespace Openlysis.Infrastructure.Shared.Communication.Serialization.Files;

/// <summary>
/// Converts a <see cref="FileReport"/> object to and from JSON.
/// </summary>
internal class FileReportJsonConverter : JsonConverter<FileReport>
{
    private const string IdKey = "id";
    private const string VerdictKey = "verdict";
    private const string ThreatZoneKey = "threatzone";
    private const string ThreatScoreKey = "threatscore";

    /// <inheritdoc/>
    public override FileReport Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string id = string.Empty;
        Verdict verdict = 0;
        ThreatZone threatZone = 0;
        ThreatScore threatScore = ThreatScore.Create(null, null);
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

                case ThreatScoreKey when reader.TokenType is JsonTokenType.StartObject:
                    threatScore = ReadThreatScore(ref reader, options);
                    break;
            }
        }

        return FileReport.Create(
            id,
            verdict,
            threatZone,
            threatScore);
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

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Id.Value);
        writer.WriteString(verdictKey, value.Verdict.ToString());
        writer.WriteString(threatZoneKey, value.ThreatZone.ToString());
        var threatScoreConverter =
            (JsonConverter<ThreatScore>)options.GetConverter(typeof(ThreatScore));
        threatScoreConverter.Write(writer, value.ThreatScore, options);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads a <see cref="ThreatScore"/> value from the JSON reader using the provided serializer options.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
    /// <returns>The deserialized <see cref="ThreatScore"/>.</returns>
    /// <exception cref="JsonException">Thrown if the ThreatScore value is missing or invalid.</exception>
    private static ThreatScore ReadThreatScore(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options)
    {
        var threatScoreConverter =
            (JsonConverter<ThreatScore>)options.GetConverter(typeof(ThreatScore));
        ThreatScore? threatScore =
            threatScoreConverter.Read(ref reader, typeof(ThreatScore), options);

        if (threatScore is null)
        {
            throw new JsonException("ThreatScore value is missing or invalid.");
        }

        return threatScore;
    }
}