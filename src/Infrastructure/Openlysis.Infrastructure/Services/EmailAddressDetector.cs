using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Infrastructure.Persistence;

using DataType = Openlysis.Domain.Messages.Enums.DataType;

namespace Openlysis.Infrastructure.Services;

/// <summary>
/// A service that detects email addresses in a given input string.
/// </summary>
internal class EmailAddressDetector : DataDetector
{
    /// <inheritdoc/>
    public override DataType DetectableData => DataType.EmailAddress;

    private readonly EmailAddressAttribute _emailAddressAttribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressDetector"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging within the detector.</param>
    public EmailAddressDetector(ILogger<EmailAddressDetector> logger)
        : base(logger)
    {
        _emailAddressAttribute = new EmailAddressAttribute();
    }

    /// <inheritdoc/>
    protected override Regex GetDetectionPattern() => RegularExpressions.EmailAddress();

    /// <inheritdoc/>
    protected override IEnumerable<Regex> GetExclusionPatterns()
        => [];

    /// <inheritdoc/>
    protected override string OnCleanInput(string input)
    {
        return input
            .Replace("[", string.Empty)
            .Replace("]", string.Empty);
    }

    /// <inheritdoc/>
    protected override bool ValidateDetection(string detection)
    {
        return _emailAddressAttribute.IsValid(detection);
    }
}