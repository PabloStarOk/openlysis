using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Application.Phones.Contracts.Requests;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Evaluators.Ipqs.Adapters.Common;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Common;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Phones;
using Openlysis.Evaluators.Ipqs.Core.Constants;
using Openlysis.Evaluators.Ipqs.Core.Models.Responses;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;
using Openlysis.Evaluators.Shared.Infrastructure.Logging.Services;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization;

namespace Openlysis.Evaluators.Ipqs.Adapters.Phones;

/// <summary>
/// Provides methods for registering a client to use phone reputation validation services of IPQS with the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Adds the dependencies of <see cref="PhoneReputationEvaluator"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance used to configure the services.</param>
    internal static void AddPhoneReputationEvaluator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get options.
        var calculationOptionsSection = configuration
            .GetRequiredSection(PhoneVerdictCalculationOptions.SectionName);

        ArgumentNullException.ThrowIfNull(calculationOptionsSection);

        // Add options.
        services.AddOptionsWithValidateOnStart<PhoneVerdictCalculationOptions>()
            .Bind(calculationOptionsSection)
            .ValidateDataAnnotations();

        services.AddSingleton<
            IValidateOptions<PhoneVerdictCalculationOptions>,
            PhoneVerdictCalculationOptionsValidator>();

        // Add service deserializer.
        services.AddServiceDeserializer<IpqsEvaluatorOptions>(
            KeyedServices.PhoneKey,
            () => new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            });

        // Add service logger.
        services.AddScoped<
            IServiceLogger<PhoneReputationEvaluator>,
            ReputationEvaluatorLogger<PhoneReputationEvaluator>>();

        // Add endpoint address factory.
        services.AddKeyedScoped<
            IEndpointAddressFactory<EvaluatePhoneReputation>,
            EndpointAddressFactory>(KeyedServices.PhoneKey);

        // Add verdict calculator
        services.AddScoped<
            IVerdictCalculator<ValidatePhoneResponse>,
            PhoneVerdictCalculator>();

        // Add response parser
        services.AddKeyedScoped<
            IResponseParser<PhoneServiceReputation>,
            PhoneResponseParser>(KeyedServices.PhoneKey);

        // Add evaluator.
        services.AddScoped<
            IReputationEvaluator<EvaluatePhoneReputation, PhoneServiceReputation>,
            PhoneReputationEvaluator>();
    }
}