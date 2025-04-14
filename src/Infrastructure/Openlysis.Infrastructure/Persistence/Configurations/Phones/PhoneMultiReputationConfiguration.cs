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
    private const string SmallintType = "smallint";

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
        builder.ToTable("phone_multi_reputations");

        builder.HasKey(p => p.Id);

        builder.Property(u => u.Id)
            .HasColumnName("phone_multi_reputation_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(p => p.ReputationEvaluationDate)
            .HasColumnName("assessment_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.AverageVerdict)
            .HasColumnName("average_verdict")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(p => p.AverageThreatZone)
            .HasColumnName("average_threat_zone")
            .HasColumnType(SmallintType)
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
        builder.ToTable("phone_services_reputations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("phone_service_reputation_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(p => p.ServiceName)
            .HasColumnName("service_name")
            .HasColumnType("varchar")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Verdict)
            .HasColumnName("verdict")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(p => p.ThreatZone)
            .HasColumnName("threat_zone")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.OwnsOne(p => p.PhoneInfo, phoneBuilder =>
        {
            phoneBuilder.Property(p => p.LocalFormat)
                .HasColumnName("phone_local_format")
                .HasColumnType("varchar")
                .HasMaxLength(30)
                .IsRequired();

            phoneBuilder.Property(p => p.CountryCode)
                .HasColumnName("phone_country_code")
                .HasColumnType("char")
                .HasMaxLength(2)
                .IsRequired();

            phoneBuilder.Property(p => p.DialingCode)
                .HasColumnName("phone_dialing_code")
                .HasColumnType(SmallintType)
                .IsRequired();

            phoneBuilder.Property(p => p.LineType)
                .HasColumnName("phone_line_type")
                .HasColumnType("varchar")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.WithOwner()
            .HasForeignKey("phone_multi_reputation_id");
    }
}
