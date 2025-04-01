using Microsoft.Extensions.Options;

namespace Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Configuration;

/// <summary>
/// Validates <see cref="RateQuotaOptions"/>.
/// </summary>
public class RateQuotaOptionsValidator : IValidateOptions<RateQuotaOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, RateQuotaOptions options)
    {
        int total = options.MinuteRate + options.HourlyRate
            + options.DailyQuota + options.MonthlyQuota;

        if (total < 1)
        {
            return ValidateOptionsResult.Fail("There must be at least one rate or quota property higher than zero.");
        }

        if (options is { MinuteRate: > 0, HourlyRate: > 0 }
            && options.MinuteRate >= options.HourlyRate)
        {
            return ValidateOptionsResult.Fail("Minute rate must be less than hourly rate.");
        }

        if (options is { HourlyRate: > 0, DailyQuota: > 0 } &&
            options.HourlyRate >= options.DailyQuota)
        {
            return ValidateOptionsResult.Fail("Hourly rate must be less than daily quota.");
        }

        return options is { DailyQuota: > 0, MonthlyQuota: > 0 }
            && options.DailyQuota > options.MonthlyQuota
            ? ValidateOptionsResult.Fail("Daily quota must be less than monthly quota.")
            : ValidateOptionsResult.Success;
    }
}