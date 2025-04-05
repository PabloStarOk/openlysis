using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.Phones;

/// <summary>
/// Configuration class for the PhoneMultiValidation entity.
/// </summary>
public class PhoneMultiValidationConfiguration : IEntityTypeConfiguration<PhoneMultiValidation>
{
    private const string TinyintType = "TINYINT";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PhoneMultiValidation> builder)
    {
        ConfigureMultiValidation(builder);
        builder.OwnsMany(p => p.ServicesValidations, ConfigureServicesValidations);
    }

    /// <summary>
    /// Configures the properties and relationships of the PhoneMultiValidation entity.
    /// </summary>
    /// <param name="builder">The builder used to configure the PhoneMultiValidation entity.</param>
    private static void ConfigureMultiValidation(
        EntityTypeBuilder<PhoneMultiValidation> builder)
    {
        builder.ToTable("PhoneMultiValidations");

        builder.HasKey(p => p.Id);

        builder.Property(u => u.Id)
            .HasColumnName("PhoneMultiValidationId")
            .HasColumnType("CHAR")
            .HasMaxLength(36)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => Id.Create(dbValue));

        builder.Property(p => p.ValidationDate)
            .HasColumnName("ValidationDate")
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

        builder.Navigation(p => p.ServicesValidations)
            .AutoInclude();
    }

    /// <summary>
    /// Configures the properties and relationships of the PhoneServiceValidation entity.
    /// </summary>
    /// <param name="builder">The builder used to configure the PhoneServiceValidation entity.</param>
    private static void ConfigureServicesValidations(
        OwnedNavigationBuilder<PhoneMultiValidation, PhoneServiceValidation> builder)
    {
        builder.ToTable("PhoneServicesValidations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PhoneServiceValidationId")
            .HasColumnType("CHAR")
            .HasMaxLength(36)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => Id.Create(dbValue));

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
            .HasForeignKey("PhoneMultiValidationId");
    }
}
