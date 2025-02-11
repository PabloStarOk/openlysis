using Openlysis.Domain.Common.Hash;

namespace Openlysis.Domain.FileAnalyses.ValueObjects;

/// <summary>
/// Metadata of a file.
/// </summary>
public sealed record FileMetadata
{
    /// <summary>
    /// Gets the set of hash of the file.
    /// </summary>
    public HashSet HashSet { get; init; }

    /// <summary>
    /// Gets the information of the file.
    /// </summary>
    public FileGeneralInfo Information { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMetadata"/> class.
    /// </summary>
    /// <param name="hashSet">The hash set associated with the file.</param>
    /// <param name="information">The general information about the file.</param>
    public FileMetadata(HashSet hashSet, FileGeneralInfo information)
    {
        HashSet = hashSet;
        Information = information;
    }

    // For EF core.
#pragma warning disable CS8618
    private FileMetadata()
    {
    }
#pragma warning restore CS8618
}
