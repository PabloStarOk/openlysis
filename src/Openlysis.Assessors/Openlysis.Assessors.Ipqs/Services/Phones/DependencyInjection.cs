using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Application.Phones.Requests;
using Openlysis.Application.Phones.Services;
using Openlysis.Assessors.Ipqs.Core.Configuration.Common;
using Openlysis.Assessors.Ipqs.Core.Configuration.Phones;
using Openlysis.Assessors.Ipqs.Core.Constants;
using Openlysis.Assessors.Ipqs.Core.Models;
using Openlysis.Assessors.Shared.Abstractions;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Infrastructure.Shared.Deserialization;

namespace Openlysis.Assessors.Ipqs.Services.Phones;

/// <summary>
/// Provides methods for registering IPQS Phone Assessor services with the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Adds the dependencies of <see cref="PhoneReputationAssessor"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance used to configure the services.</param>
    internal static void AddPhoneAssessor(
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
        services.AddServiceDeserializer<IpqsAssessorOptions>(
            KeyedServices.PhoneKey,
            () => new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            });

        // Add endpoint address factory.
        services.AddKeyedScoped<
            IEndpointAddressFactory<AssessPhoneNumber>,
            IpqsPhoneEndpointAddressFactory>(KeyedServices.PhoneKey);

        // Add verdict calculator
        services.AddScoped<
            IVerdictCalculator<ValidatePhoneResponse>,
            PhoneVerdictCalculator>();

        // Add response parser
        services.AddKeyedScoped<
            IResponseParser<PhoneServiceReputation>,
            IpqsPhoneResponseParser>(KeyedServices.PhoneKey);

        // Add assessor.
        services.AddScoped<
            IReputationAssessor<AssessPhoneNumber, PhoneServiceReputation>,
            PhoneReputationAssessor>();
    }
}