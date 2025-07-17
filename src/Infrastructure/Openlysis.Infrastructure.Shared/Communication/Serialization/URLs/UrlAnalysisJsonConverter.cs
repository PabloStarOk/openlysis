using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Infrastructure.Shared.Communication.Serialization.URLs;

/// <summary>
/// Converts JSON to and from <see cref="UrlAnalysis"/> objects.
/// </summary>
internal class UrlAnalysisJsonConverter : JsonConverter<UrlAnalysis>
{
    private const string IdKey = "id";
    private const string ExternalIdKey = "externalid";
    private const string ServiceNameKey = "servicename";
    private const string StatusKey = "status";
    private const string VerdictKey = "verdict";
    private const string ThreatScoreKey = "threatscore";

    /// <inheritdoc/>
    public override UrlAnalysis Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string id = string.Empty;
        string externalId = string.Empty;
        string serviceName = string.Empty;
        AnalysisStatus status = 0;
        Verdict verdict = 0;
        ThreatScore threatScore = ThreatScore.Create(null, null);
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

                case ExternalIdKey:
                    externalId = reader.GetString() ?? string.Empty;
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

                case ThreatScoreKey when reader.TokenType is JsonTokenType.StartObject:
                    threatScore = ReadThreatScore(ref reader, options);
                    break;
            }
        }

        return UrlAnalysis.CreateWithId(
            GlobalId.Parse(id),
            ExternalAnalysisId.Parse(externalId),
            serviceName,
            status,
            verdict,
            threatScore);
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        UrlAnalysis value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? IdKey;
        string externalIdKey = options.PropertyNamingPolicy?.ConvertName(ExternalIdKey) ?? ExternalIdKey;
        string serviceNameKey = options.PropertyNamingPolicy?.ConvertName(ServiceNameKey) ?? ServiceNameKey;
        string statusKey = options.PropertyNamingPolicy?.ConvertName(StatusKey) ?? StatusKey;
        string verdictKey = options.PropertyNamingPolicy?.ConvertName(VerdictKey) ?? VerdictKey;

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Id.ToString());
        writer.WriteString(externalIdKey, value.ExternalId.ToString());
        writer.WriteString(serviceNameKey, value.ServiceName);
        writer.WriteString(statusKey, value.State.Status.ToString());
        writer.WriteString(verdictKey, value.State.Verdict.ToString());

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