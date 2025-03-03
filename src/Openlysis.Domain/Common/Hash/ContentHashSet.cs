namespace Openlysis.Domain.Common.Hash;

/// <summary>
/// A set of different hashes.
/// </summary>
public sealed record ContentHashSet(string Md5, string Sha1, string Sha256, string Sha512);
