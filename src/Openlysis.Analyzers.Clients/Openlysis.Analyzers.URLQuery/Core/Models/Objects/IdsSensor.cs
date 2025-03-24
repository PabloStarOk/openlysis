using Openlysis.Analyzers.URLQuery.Core.Models.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Objects;

/// <summary>
/// Represents an intrusion detection system sensor.
/// </summary>
/// <param name="Alerts">An array of alerts associated with the sensor.</param>
public record IdsSensor(
    Alert[]? Alerts)
    : Sensor<Alert>(SensorType.IntrusionDetectionSystem, Alerts);