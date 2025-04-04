using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Models;
using Openlysis.Domain.Common.ServiceAnalyses.Mappings;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Domain.Phones;

/// <summary>
/// Represents a multi-validation process for phone services.
/// </summary>
public class PhoneMultiValidation : AggregateRoot<Id>
{
    private readonly List<PhoneServiceValidation> _servicesValidations = [];

    /// <summary>
    /// Gets the date of the validation.
    /// </summary>
    public DateTimeOffset ValidationDate { get; }

    /// <summary>
    /// Gets the average verdict of the validation.
    /// </summary>
    public Verdict AverageVerdict { get; private set; }

    /// <summary>
    /// Gets the average threat zone of the validation.
    /// </summary>
    public ThreatZone AverageThreatZone { get; private set; }

    /// <summary>
    /// Gets the list of service validations.
    /// </summary>
    public IReadOnlyList<PhoneServiceValidation> ServicesValidations => _servicesValidations;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneMultiValidation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the validation.</param>
    /// <param name="validationDate">The date of the validation.</param>
    /// <param name="averageVerdict">The average verdict of the validation.</param>
    /// <param name="averageThreatZone">The average threat zone of the validation.</param>
    private PhoneMultiValidation(
        Id id,
        DateTimeOffset validationDate,
        Verdict averageVerdict,
        ThreatZone averageThreatZone)
        : base(id)
    {
        ValidationDate = validationDate;
        AverageVerdict = averageVerdict;
        AverageThreatZone = averageThreatZone;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PhoneMultiValidation"/> with the specified validation date.
    /// </summary>
    /// <param name="validationDate">The date of the validation.</param>
    /// <returns>A new instance of <see cref="PhoneMultiValidation"/>.</returns>
    public static PhoneMultiValidation Create(
        DateTimeOffset validationDate)
    {
        Id id = Id.CreateUnique();
        return new PhoneMultiValidation(
            id,
            validationDate,
            Verdict.Unknown,
            ThreatZone.Unknown);
    }

    /// <summary>
    /// Adds a new service validation to the collection.
    /// </summary>
    /// <param name="serviceValidation">The service validation to add.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the service validation already exists in the collection.
    /// </exception>
    public void AddServiceValidation(PhoneServiceValidation serviceValidation)
    {
        if (_servicesValidations.Contains(serviceValidation))
        {
            throw new ArgumentException(
                "Given PhoneServiceValidation already exists in the collection.",
                nameof(serviceValidation));
        }

        _servicesValidations.Add(serviceValidation);
        UpdateVerdict();
        UpdateThreatZone();
    }

    /// <summary>
    /// Updates the average verdict based on the service validations.
    /// </summary>
    private void UpdateVerdict()
    {
        if (_servicesValidations.Count is 0)
        {
            AverageVerdict = Verdict.Unknown;
            return;
        }

        var verdictCounts = _servicesValidations
            .GroupBy(s => s.Verdict)
            .ToDictionary(g => g.Key, g => g.Count());

        AverageVerdict = verdictCounts
            .OrderByDescending(pair => pair.Value)
            .ThenByDescending(pair => pair.Key)
            .First().Key;
    }

    /// <summary>
    /// Updates the average threat zone based on the average verdict.
    /// </summary>
    private void UpdateThreatZone()
    {
        AverageThreatZone = ThreatZoneMapping.Map[AverageVerdict];
    }
}