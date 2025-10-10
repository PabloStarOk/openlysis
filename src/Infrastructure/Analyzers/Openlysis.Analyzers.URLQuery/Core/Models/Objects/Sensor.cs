using System.Text.Json.Serialization;

using Openlysis.Analyzers.URLQuery.Core.Models.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Objects;

/// <summary>
/// Defines a base sensor with a specific type and associated alerts.
/// </summary>
/// <typeparam name="TAlert">An implementation of <see cref="Alert"/>.</typeparam>
/// <param name="Type">A <see cref="SensorType"/>.</param>
/// <param name="Alerts">An <see cref="Array"/> of alerts created by the sensor.</param>
public abstract record Sensor<TAlert>(
    [property: JsonIgnore] SensorType Type,
    [property: JsonPropertyName("alerts")] TAlert[]? Alerts)
    where TAlert : Alert;
