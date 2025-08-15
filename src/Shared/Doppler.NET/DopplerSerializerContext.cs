using System.Text.Json.Serialization;

using Doppler.NET.Models;

namespace Doppler.NET;

/// <summary>
/// Provides a source generation context for System.Text.Json serialization of Doppler models.
/// </summary>
[JsonSerializable(typeof(DopplerSecret))]
[JsonSerializable(typeof(DopplerSecretValue))]
internal partial class DopplerSerializerContext : JsonSerializerContext
{
}