using Openlysis.Analyzers.URLQuery.Core.Models.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Objects;

/// <summary>
/// Represents a threat detection system sensor.
/// </summary>
/// <param name="Alerts">An array of alerts associated with the sensor.</param>
public record TdsSensor(
    AnalyzerAlert[]? Alerts)
    : Sensor<AnalyzerAlert>(SensorType.ThreatDetectionSystem, Alerts);