using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.Phones;

/// <summary>
/// Provides the Entity Framework Core configuration for the <see cref="PhoneMultiReputation"/> entity.
/// </summary>
public class PhoneMultiReputationConfiguration : IEntityTypeConfiguration<PhoneMultiReputation>
{
    private const string VarcharType = "varchar";
    private const string SmallintType = "smallint";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PhoneMultiReputation> builder)
    {
        ConfigureMultiReputation(builder);
        builder.OwnsMany(p => p.Reputations, ConfigureReputations);
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
            .HasColumnName("evaluation_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.PhoneNumber)
            .HasColumnName("phone_number")
            .HasColumnType(VarcharType)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(p => p.FinalVerdict)
            .HasColumnName("final_verdict")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(p => p.FinalThreatZone)
            .HasColumnName("final_threat_zone")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Navigation(p => p.Reputations)
            .AutoInclude();
    }

    /// <summary>
    /// Configures the properties and relationships of the PhoneServiceReputation entity.
    /// </summary>
    /// <param name="builder">The builder used to configure the PhoneServiceReputation entity.</param>
    private static void ConfigureReputations(
        OwnedNavigationBuilder<PhoneMultiReputation, PhoneReputation> builder)
    {
        builder.ToTable("phone_reputations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("phone_reputation_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue));

        builder.Property(p => p.ServiceName)
            .HasColumnName("service_name")
            .HasColumnType(VarcharType)
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
                .HasColumnType(VarcharType)
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
                .HasColumnType(VarcharType)
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.WithOwner()
            .HasForeignKey("phone_multi_reputation_id");
    }
}
