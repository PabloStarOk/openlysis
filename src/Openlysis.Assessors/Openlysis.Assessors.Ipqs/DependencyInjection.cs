using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Assessors.Ipqs.Core.Configuration.Common;
using Openlysis.Assessors.Ipqs.Core.Constants;
using Openlysis.Assessors.Ipqs.Services.Phones;
using Openlysis.Assessors.Shared.Infrastructure.Client;
using Openlysis.Assessors.Shared.Infrastructure.Logging.Services;
using Openlysis.Assessors.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.RateQuota;

namespace Openlysis.Assessors.Ipqs;

/// <summary>
/// Provides methods for adding IPQS Phone Assessor services to the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the IPQS Phone Assessor services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The IConfiguration to use for configuring the services.</param>
    public static void AddIpqsAssessors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get options
        var secretOptionsSection = configuration
            .GetRequiredSection(IpqsSecretOptions.SectionName);
        var secretOptions = secretOptionsSection
            .Get<IpqsSecretOptions>();

        var assessorOptionsSection = configuration
            .GetRequiredSection(IpqsAssessorOptions.SectionName);
        var assessorOptions = assessorOptionsSection
            .Get<IpqsAssessorOptions>();

        ArgumentNullException.ThrowIfNull(secretOptionsSection);
        ArgumentNullException.ThrowIfNull(secretOptions);
        ArgumentNullException.ThrowIfNull(assessorOptionsSection);
        ArgumentNullException.ThrowIfNull(assessorOptions);

        // Add rate quota service
        services.AddRateQuotaService<AssessorEndpointType>(
            configuration,
            KeyedServices.GlobalKey,
            assessorOptions.ServiceName);

        // Add options
        services.Configure<IpqsSecretOptions>(secretOptionsSection);

        services.AddOptionsWithValidateOnStart<IpqsAssessorOptions>()
            .Bind(assessorOptionsSection)
            .ValidateDataAnnotations();

        // Add service logger.
        services.AddScoped<AssessorLogger<IpqsAssessorOptions>>();

        // Add http client.
        services.ConfigureHttpClient(secretOptions, assessorOptions);

        // Add phone number assessor.
        services.AddPhoneAssessor(configuration);
    }
}