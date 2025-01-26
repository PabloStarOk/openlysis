using Openlysis.Domain.Common.Hash;

namespace Openlysis.Domain.FileReports.ValueObjects;

/// <summary>
/// A single file with metadata.
/// </summary>
public sealed record File(HashSet HashSet, FileGeneralInfo Information);
