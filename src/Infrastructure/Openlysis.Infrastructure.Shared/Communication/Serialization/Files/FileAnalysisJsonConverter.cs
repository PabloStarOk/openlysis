using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;

namespace Openlysis.Infrastructure.Shared.Communication.Serialization.Files;

/// <summary>
/// Converts a <see cref="FileAnalysis"/> object to and from JSON.
/// </summary>
internal class FileAnalysisJsonConverter : JsonConverter<FileAnalysis>
{
    private const string IdKey = "id";
    private const string ServiceNameKey = "servicename";
    private const string StatusKey = "status";
    private const string VerdictKey = "verdict";
    private const string ThreatScoreKey = "threatscore";
    private const string ReportsKey = "reports";

    /// <inheritdoc/>
    public override FileAnalysis Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string id = string.Empty;
        string serviceName = string.Empty;
        AnalysisStatus status = 0;
        Verdict verdict = 0;
        List<FileReport> reports = [];
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

                case ServiceNameKey:
                    serviceName = reader.GetString() ?? string.Empty;
                    break;

                case StatusKey:
                    status = Enum.Parse<AnalysisStatus>(
                        reader.GetString() ?? string.Empty,
                        ignoreCase: true);
                    break;

                case VerdictKey:
                    verdict = Enum.Parse<Verdict>(
                        reader.GetString() ?? string.Empty,
                        ignoreCase: true);
                    break;

                case ThreatScoreKey when reader.TokenType is JsonTokenType.StartObject:
                    threatScore = ReadThreatScore(ref reader, options);
                    break;

                case ReportsKey:
                    reports = ConvertReports(ref reader, options);
                    break;
            }
        }

        return FileAnalysis.Create(
            id,
            serviceName,
            status,
            verdict,
            reports,
            threatScore: threatScore);
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        FileAnalysis value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? nameof(FileAnalysis.Id);
        string serviceNameKey = options.PropertyNamingPolicy?.ConvertName(ServiceNameKey) ?? nameof(FileAnalysis.ServiceName);
        string statusKey = options.PropertyNamingPolicy?.ConvertName(StatusKey) ?? nameof(FileAnalysis.State.Status);
        string verdictKey = options.PropertyNamingPolicy?.ConvertName(VerdictKey) ?? nameof(FileAnalysis.State.Verdict);
        string reportsKey = options.PropertyNamingPolicy?.ConvertName(ReportsKey) ?? nameof(FileAnalysis.Reports);

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Id.ToString());
        writer.WriteString(serviceNameKey, value.ServiceName);
        writer.WriteString(statusKey, value.State.Status.ToString());
        writer.WriteString(verdictKey, value.State.Verdict.ToString());
        var threatScoreConverter =
            (JsonConverter<ThreatScore>)options.GetConverter(typeof(ThreatScore));
        threatScoreConverter.Write(writer, value.ThreatScore, options);

        writer.WriteStartArray(reportsKey);
        var reportJsonConverter = (JsonConverter<FileReport>)options.Converters.Single(c => c.Type == typeof(FileReport));
        foreach (var report in value.Reports)
        {
            reportJsonConverter.Write(writer, report, options);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    /// <summary>
    /// Converts a JSON array to a list of <see cref="FileReport"/> objects.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
    /// <returns>A list of <see cref="FileReport"/> objects.</returns>
    private static List<FileReport> ConvertReports(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options)
    {
        var reportJsonConverter = (JsonConverter<FileReport>)options.Converters
            .Single(c => c.Type == typeof(FileReport));
        List<FileReport> reports = [];
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.StartArray)
            {
                continue;
            }

            if (reader.TokenType is JsonTokenType.EndArray)
            {
                break;
            }

            FileReport? report = reportJsonConverter.Read(ref reader, typeof(FileReport), options);

            if (report is not null)
            {
                reports.Add(report);
            }
        }

        return reports;
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