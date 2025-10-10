namespace Openlysis.Domain.Users.ValueObjects;

/// <summary>
/// An user's email address.
/// </summary>
public sealed record EmailAddress(string Value)
{
    private const char AtSign = '@';

    /// <summary>
    /// Gets the normalized form of the email address, with the domain part in lowercase.
    /// </summary>
    public string NormalizedValue
    {
        get
        {
            var parts = Value.Trim().Split(AtSign);
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid email address format.", nameof(Value));
            }

            return $"{parts[0]}{AtSign}{parts[1].ToLowerInvariant()}";
        }
    }

}