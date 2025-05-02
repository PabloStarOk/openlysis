using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files.Entities;

namespace Openlysis.Infrastructure.Shared.Messaging.Serialization.Files;

/// <summary>
/// Converts a <see cref="FileServiceAnalysis"/> object to and from JSON.
/// </summary>
internal class ServiceFileAnalysisJsonConverter : JsonConverter<FileServiceAnalysis>
{
    private const string IdKey = "id";
    private const string ServiceNameKey = "servicename";
    private const string StatusKey = "status";
    private const string ReportsKey = "reports";

    /// <inheritdoc/>
    public override FileServiceAnalysis Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string id = string.Empty;
        string serviceName = string.Empty;
        AnalysisStatus status = 0;
        List<Report> reports = [];
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

                case ReportsKey:
                    reports = ConvertReports(ref reader, options);
                    break;
            }
        }

        return FileServiceAnalysis.Create(id, serviceName, status, reports);
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        FileServiceAnalysis value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? nameof(FileServiceAnalysis.Id);
        string serviceNameKey = options.PropertyNamingPolicy?.ConvertName(ServiceNameKey) ?? nameof(FileServiceAnalysis.ServiceName);
        string statusKey = options.PropertyNamingPolicy?.ConvertName(StatusKey) ?? nameof(FileServiceAnalysis.Status);
        string reportsKey = options.PropertyNamingPolicy?.ConvertName(ReportsKey) ?? nameof(FileServiceAnalysis.Reports);

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Id.ToString());
        writer.WriteString(serviceNameKey, value.ServiceName);
        writer.WriteString(statusKey, value.Status.ToString());

        writer.WriteStartArray(reportsKey);
        var reportJsonConverter = (JsonConverter<Report>)options.Converters.Single(c => c.Type == typeof(Report));
        foreach (var report in value.Reports)
        {
            reportJsonConverter.Write(writer, report, options);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    /// <summary>
    /// Converts a JSON array to a list of <see cref="Report"/> objects.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
    /// <returns>A list of <see cref="Report"/> objects.</returns>
    private static List<Report> ConvertReports(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options)
    {
        var reportJsonConverter = (JsonConverter<Report>)options.Converters
            .Single(c => c.Type == typeof(Report));
        List<Report> reports = [];
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

            Report? report = reportJsonConverter.Read(ref reader, typeof(Report), options);

            if (report is not null)
            {
                reports.Add(report);
            }
        }

        return reports;
    }
}