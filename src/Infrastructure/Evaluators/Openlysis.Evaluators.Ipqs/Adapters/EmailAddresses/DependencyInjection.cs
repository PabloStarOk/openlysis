using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Application.EmailAddresses.Contracts.Requests;
using Openlysis.Domain.EmailAddresses.Entities;
using Openlysis.Evaluators.Ipqs.Adapters.Common;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Common;
using Openlysis.Evaluators.Ipqs.Core.Configuration.EmailAddresses;
using Openlysis.Evaluators.Ipqs.Core.Constants;
using Openlysis.Evaluators.Ipqs.Core.Models.Responses;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;
using Openlysis.Evaluators.Shared.Infrastructure.Logging.Services;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization;

namespace Openlysis.Evaluators.Ipqs.Adapters.EmailAddresses;

/// <summary>
/// Provides methods for registering dependencies related to the Email Address Reputation Evaluator.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers services and configurations required for the Email Address Reputation Evaluator.
    /// </summary>
    /// <param name="services">The service collection to which the dependencies will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve settings.</param>
    internal static void AddEmailAddressReputationEvaluator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get options.
        var verificationOptionsSection = configuration
            .GetRequiredSection(EmailAddressVerificationOptions.SectionName);

        var verdictCalculationOptionsSection = configuration
            .GetRequiredSection(EmailVerdictCalculationOptions.SectionName);

        var verdictCalculationOptions = verdictCalculationOptionsSection
            .Get<EmailVerdictCalculationOptions>();

        ArgumentNullException.ThrowIfNull(verificationOptionsSection);
        ArgumentNullException.ThrowIfNull(verdictCalculationOptionsSection);
        ArgumentNullException.ThrowIfNull(verdictCalculationOptions);

        // Add options.
        services.AddOptionsWithValidateOnStart<EmailAddressVerificationOptions>()
            .Bind(verificationOptionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptionsWithValidateOnStart<EmailVerdictCalculationOptions>()
            .Bind(verdictCalculationOptionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<EmailVerdictCalculationOptions>,
            EmailVerdictCalculationOptionsValidator>();

        // Add service deserializer.
        services.AddServiceDeserializer<IpqsEvaluatorOptions>(
            KeyedServices.EmailAddressKey,
            () => new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            });

        // Add service logger.
        services.AddScoped<
            IServiceLogger<EmailAddressReputationEvaluator>,
            ReputationEvaluatorLogger<EmailAddressReputationEvaluator>>();

        // Add endpoint address factory.
        services.AddKeyedScoped<
            IEndpointAddressFactory<EvaluateEmailAddressReputation>,
            EndpointAddressFactory>(KeyedServices.EmailAddressKey);

        // Add verdict calculator.
        services.AddScoped<
            IVerdictCalculator<VerifyEmailAddressResponse>,
            EmailAddressVerdictCalculator>();

        // Add response parser.
        services.AddKeyedScoped<
            IResponseParser<EmailAddressServiceReputation>,
            EmailAddressResponseParser>(KeyedServices.EmailAddressKey);

        // Add reputation evaluator.
        services.AddScoped<
            IReputationEvaluator<EvaluateEmailAddressReputation, EmailAddressServiceReputation>,
            EmailAddressReputationEvaluator>();
    }
}