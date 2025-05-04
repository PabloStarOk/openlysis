namespace Openlysis.Domain.Common.Entities;

/// <summary>
/// Represents a set of hash values for a given entity.
/// </summary>
/// <param name="Md5">The MD5 hash value.</param>
/// <param name="Sha1">The SHA-1 hash value.</param>
/// <param name="Sha256">The SHA-256 hash value.</param>
/// <param name="Sha512">The SHA-512 hash value.</param>
public sealed record HashValues(string Md5, string Sha1, string Sha256, string Sha512);
