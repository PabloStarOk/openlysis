namespace Openlysis.Evaluators.Ipqs.Core.Constants;

/// <summary>
/// Contains constant addresses for various validation endpoints.
/// </summary>
internal static class Addresses
{
    /// <summary>
    /// Endpoint for email address verification.
    /// </summary>
    internal const string EmailAddressVerification = "email/{0}/{1}";

    /// <summary>
    /// Endpoint for phone number validation.
    /// </summary>
    internal const string PhoneNumberValidation = "phone/{0}/{1}";
}