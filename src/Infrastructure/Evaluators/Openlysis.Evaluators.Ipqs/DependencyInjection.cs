using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Evaluators.Ipqs.Adapters.EmailAddresses;
using Openlysis.Evaluators.Ipqs.Adapters.Phones;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Common;
using Openlysis.Evaluators.Ipqs.Core.Constants;
using Openlysis.Evaluators.Shared.Infrastructure.Client;
using Openlysis.Evaluators.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;

namespace Openlysis.Evaluators.Ipqs;

/// <summary>
/// Provides methods for adding IPQS Phone evaluator services to the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the IPQS Phone evaluator services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The IConfiguration to use for configuring the services.</param>
    public static void AddIpqsReputationEvaluators(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get options
        var secretOptionsSection = configuration
            .GetRequiredSection(IpqsSecretOptions.SectionName);
        var secretOptions = secretOptionsSection
            .Get<IpqsSecretOptions>();

        var evaluatorOptionsSection = configuration
            .GetRequiredSection(IpqsEvaluatorOptions.SectionName);
        var evaluatorOptions = evaluatorOptionsSection
            .Get<IpqsEvaluatorOptions>();

        ArgumentNullException.ThrowIfNull(secretOptions);
        ArgumentNullException.ThrowIfNull(evaluatorOptions);

        // Add rate quota service
        services.AddRateQuotaService<ReputationEndpointType>(
            configuration,
            KeyedServices.GlobalKey,
            evaluatorOptions.ServiceName);

        // Add options
        services.AddOptions<IpqsSecretOptions>()
            .Bind(secretOptionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptionsWithValidateOnStart<IpqsEvaluatorOptions>()
            .Bind(evaluatorOptionsSection)
            .ValidateDataAnnotations();

        // Add http client.
        services.ConfigureHttpClient(evaluatorOptions);

        // Add phone number evaluator.
        services.AddPhoneReputationEvaluator(configuration);

        // Email address evaluator.
        services.AddEmailAddressReputationEvaluator(configuration);
    }
}