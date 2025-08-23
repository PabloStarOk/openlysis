using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.Application.Common.Models;

/// <summary>
/// Represents a file that has been processed, including its metadata and hash values.
/// </summary>
/// <param name="Metadata">Metadata describing the processed file, such as name, size, and type.</param>
/// <param name="HashValues">The computed hash values for the file.</param>
/// <param name="StorageFileName">The name used to store the file in the storage system.</param>
public sealed record ProcessedFile(
    FileMetadata Metadata,
    HashValues HashValues,
    string StorageFileName);