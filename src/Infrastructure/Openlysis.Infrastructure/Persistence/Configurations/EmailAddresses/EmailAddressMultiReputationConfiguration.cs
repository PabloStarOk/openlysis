using System.Net.Mail;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.EmailAddresses.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.EmailAddresses;

/// <summary>
/// Provides the Entity Framework Core configuration for the <see cref="EmailAddressMultiReputation"/> entity.
/// </summary>
public class EmailAddressMultiReputationConfiguration : IEntityTypeConfiguration<EmailAddressMultiReputation>
{
    private const string SmallintType = "smallint";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<EmailAddressMultiReputation> builder)
    {
        ConfigureMultiReputations(builder);
        builder.OwnsMany(e => e.Reputations, ConfigureReputations);
    }

    /// <summary>
    /// Configures the entity type mapping for <see cref="EmailAddressMultiReputation"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="EntityTypeBuilder{TEntity}"/> used to configure the entity type.
    /// </param>
    private static void ConfigureMultiReputations(
        EntityTypeBuilder<EmailAddressMultiReputation> builder)
    {
        builder.ToTable("email_address_multi_reputations");

        builder.HasKey(e => e.Id);

        builder.Property(u => u.Id)
            .HasColumnName("email_address_multi_reputation_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(p => p.ReputationEvaluationDate)
            .HasColumnName("evaluation_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.FinalVerdict)
            .HasColumnName("final_verdict")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(p => p.FinalThreatZone)
            .HasColumnName("final_threat_zone")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(p => p.EmailAddress)
            .HasColumnName("email_address")
            .HasColumnType("varchar")
            .HasMaxLength(254)
            .IsRequired()
            .HasConversion(
                emailAddress => emailAddress.Address,
                dbValue => new MailAddress(dbValue));

        builder.Navigation(p => p.Reputations)
            .AutoInclude();
    }

    /// <summary>
    /// Configures the owned navigation property for <see cref="EmailAddressReputation"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}"/> used to configure the owned entity type.
    /// </param>
    private static void ConfigureReputations(
        OwnedNavigationBuilder<EmailAddressMultiReputation, EmailAddressReputation> builder)
    {
        builder.ToTable("email_address_reputations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("email_address_reputation_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

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

        builder.Property(p => p.IsDisposable)
            .HasColumnName("is_disposable")
            .HasColumnType("boolean");

        builder.Property(p => p.IsRiskyTld)
            .HasColumnName("is_risky_tld")
            .HasColumnType("boolean");

        builder.WithOwner()
            .HasForeignKey("email_address_multi_reputation_id");
    }
}