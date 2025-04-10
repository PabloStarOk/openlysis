using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.MultiAnalyzer.Infrastructure.Serialization;

/// <summary>
/// Converts a <see cref="MultiAnalysisId"/> object to and from JSON.
/// </summary>
public class MultiAnalysisIdConverter : JsonConverter<MultiAnalysisId>
{
    private const string IdKey = "id";

    /// <inheritdoc/>
    public override MultiAnalysisId Read(
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
        var multiAnalysisId = MultiAnalysisId.Create(guid);

        // Read end of object
        reader.Read();
        return multiAnalysisId;
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        MultiAnalysisId value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? IdKey;

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Value);
        writer.WriteEndObject();
    }
}