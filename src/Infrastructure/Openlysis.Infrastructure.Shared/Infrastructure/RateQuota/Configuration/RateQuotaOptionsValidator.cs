using Microsoft.Extensions.Options;

namespace Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Configuration;

/// <summary>
/// Validates <see cref="RateQuotaEndpointOptions{TEnum}"/>.
/// </summary>
/// <typeparam name="TEnum">The enum type used for rate quota options.</typeparam>
public class RateQuotaOptionsValidator<TEnum>
    : IValidateOptions<RateQuotaEndpointOptions<TEnum>>
    where TEnum : Enum
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, RateQuotaEndpointOptions<TEnum> endpointOptions)
    {
        int total = endpointOptions.MinuteRate + endpointOptions.HourlyRate
            + endpointOptions.DailyQuota + endpointOptions.MonthlyQuota;

        if (total < 1)
        {
            return ValidateOptionsResult.Fail("There must be at least one rate or quota property higher than zero.");
        }

        if (endpointOptions is { MinuteRate: > 0, HourlyRate: > 0 }
            && endpointOptions.MinuteRate >= endpointOptions.HourlyRate)
        {
            return ValidateOptionsResult.Fail("Minute rate must be less than hourly rate.");
        }

        if (endpointOptions is { HourlyRate: > 0, DailyQuota: > 0 } &&
            endpointOptions.HourlyRate >= endpointOptions.DailyQuota)
        {
            return ValidateOptionsResult.Fail("Hourly rate must be less than daily quota.");
        }

        return endpointOptions is { DailyQuota: > 0, MonthlyQuota: > 0 }
            && endpointOptions.DailyQuota > endpointOptions.MonthlyQuota
            ? ValidateOptionsResult.Fail("Daily quota must be less than monthly quota.")
            : ValidateOptionsResult.Success;
    }
}