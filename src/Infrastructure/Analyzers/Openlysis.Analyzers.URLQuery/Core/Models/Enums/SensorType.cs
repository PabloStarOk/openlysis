namespace Openlysis.Analyzers.URLQuery.Core.Models.Enums;

/// <summary>
/// Represents the type of sensor.
/// </summary>
public enum SensorType
{
    /// <summary>
    /// Intrusion Detection System sensors, returned in JSON as "ids".
    /// </summary>
    IntrusionDetectionSystem, // Returned in JSON as "ids"

    /// <summary>
    /// Threat Detection System sensors, returned in JSON as "analyzer".
    /// </summary>
    ThreatDetectionSystem, // Returned in JSON as "analyzer"
}