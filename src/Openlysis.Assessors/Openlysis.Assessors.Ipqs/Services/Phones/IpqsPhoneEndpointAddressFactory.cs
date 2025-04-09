using Microsoft.Extensions.Options;

using Openlysis.Application.Phones.Requests;
using Openlysis.Assessors.Ipqs.Core.Configuration.Common;
using Openlysis.Assessors.Ipqs.Core.Constants;
using Openlysis.Assessors.Shared.Abstractions;

namespace Openlysis.Assessors.Ipqs.Services.Phones;

/// <summary>
/// Factory class for creating endpoint address to validate a phone number using IPQualityScore service.
/// </summary>
/// <seealso cref="IEndpointAddressFactory{AssessPhoneNumber}"/>
public class IpqsPhoneEndpointAddressFactory : IEndpointAddressFactory<AssessPhoneNumber>
{
    private readonly IOptionsSnapshot<IpqsSecretOptions> _secretOptions;
    private readonly IOptionsSnapshot<IpqsAssessorOptions> _assessorOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="IpqsPhoneEndpointAddressFactory"/> class.
    /// </summary>
    /// <param name="secretOptions">The options snapshot for IPQS secret options.</param>
    /// <param name="assessorOptions">The options snapshot for IPQS assessor options.</param>
    public IpqsPhoneEndpointAddressFactory(
        IOptionsSnapshot<IpqsSecretOptions> secretOptions,
        IOptionsSnapshot<IpqsAssessorOptions> assessorOptions)
    {
        _secretOptions = secretOptions;
        _assessorOptions = assessorOptions;
    }

    /// <inheritdoc/>
    public Uri Create(AssessPhoneNumber data)
    {
        string formattedUrl = string.Format(
            Addresses.PhoneNumberValidation,
            _secretOptions.Value.ApiKey,
            data.Value);

        return new Uri(
            _assessorOptions.Value.BaseAddress,
            formattedUrl);
    }
}