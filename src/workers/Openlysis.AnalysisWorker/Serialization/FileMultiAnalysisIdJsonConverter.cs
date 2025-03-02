using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.AnalysisWorker.Serialization;

/// <summary>
/// Converts a <see cref="FileMultiAnalysisId"/> object to and from JSON.
/// </summary>
public class FileMultiAnalysisIdJsonConverter : JsonConverter<FileMultiAnalysisId>
{
    private const string IdKey = "id";

    /// <inheritdoc/>
    public override FileMultiAnalysisId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Read start object.
        reader.Read();

        // Read ID.
        reader.Read();
        var id = reader.GetString();
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        // Create ID object.
        Guid guid = Guid.Parse(id);
        var fileMultiAnalysisId = FileMultiAnalysisId.Create(guid);

        // Read end of object
        reader.Read();
        return fileMultiAnalysisId;
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        FileMultiAnalysisId value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? IdKey;

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Value);
        writer.WriteEndObject();
    }
}