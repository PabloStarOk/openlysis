using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.Phones;

/// <summary>
/// Configuration class for the PhoneMultiReputation entity.
/// </summary>
public class PhoneMultiReputationConfiguration : IEntityTypeConfiguration<PhoneMultiReputation>
{
    private const string TinyintType = "TINYINT";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PhoneMultiReputation> builder)
    {
        ConfigureMultiReputation(builder);
        builder.OwnsMany(p => p.ServicesReputations, ConfigureServicesReputations);
    }

    /// <summary>
    /// Configures the properties and relationships of the PhoneMultiReputation entity.
    /// </summary>
    /// <param name="builder">The builder used to configure the PhoneMultiReputation entity.</param>
    private static void ConfigureMultiReputation(
        EntityTypeBuilder<PhoneMultiReputation> builder)
    {
        builder.ToTable("PhoneMultiReputations");

        builder.HasKey(p => p.Id);

        builder.Property(u => u.Id)
            .HasColumnName("PhoneMultiReputationId")
            .HasColumnType("CHAR")
            .HasMaxLength(36)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(p => p.AssessmentDate)
            .HasColumnName("AssessmentDate")
            .HasColumnType("DATETIME2")
            .IsRequired();

        builder.Property(p => p.AverageVerdict)
            .HasColumnName("AverageVerdict")
            .HasColumnType(TinyintType)
            .IsRequired();

        builder.Property(p => p.AverageThreatZone)
            .HasColumnName("AverageThreatZone")
            .HasColumnType(TinyintType)
            .IsRequired();

        builder.Navigation(p => p.ServicesReputations)
            .AutoInclude();
    }

    /// <summary>
    /// Configures the properties and relationships of the PhoneServiceReputation entity.
    /// </summary>
    /// <param name="builder">The builder used to configure the PhoneServiceReputation entity.</param>
    private static void ConfigureServicesReputations(
        OwnedNavigationBuilder<PhoneMultiReputation, PhoneServiceReputation> builder)
    {
        builder.ToTable("PhoneServicesReputations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PhoneServiceReputationId")
            .HasColumnType("CHAR")
            .HasMaxLength(36)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(p => p.ServiceName)
            .HasColumnName("ServiceName")
            .HasColumnType("VARCHAR")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Verdict)
            .HasColumnName("Verdict")
            .HasColumnType(TinyintType)
            .IsRequired();

        builder.Property(p => p.ThreatZone)
            .HasColumnName("ThreatZone")
            .HasColumnType(TinyintType)
            .IsRequired();

        builder.OwnsOne(p => p.PhoneInfo, phoneBuilder =>
        {
            phoneBuilder.Property(p => p.LocalFormat)
                .HasColumnName("PhoneLocalFormat")
                .HasColumnType("VARCHAR")
                .HasMaxLength(30)
                .IsRequired();

            phoneBuilder.Property(p => p.CountryCode)
                .HasColumnName("PhoneCountryCode")
                .HasColumnType("CHAR")
                .HasMaxLength(2)
                .IsRequired();

            phoneBuilder.Property(p => p.DialingCode)
                .HasColumnName("PhoneDialingCode")
                .HasColumnType(TinyintType)
                .IsRequired();

            phoneBuilder.Property(p => p.LineType)
                .HasColumnName("PhoneLineType")
                .HasColumnType("VARCHAR")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.WithOwner()
            .HasForeignKey("PhoneMultiReputationId");
    }
}
