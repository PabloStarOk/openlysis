using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Configurations.URLs;

/// <summary>
/// Configuration class for the UrlMultiAnalysis entity.
/// Implements IEntityTypeConfiguration to provide configuration for the UrlMultiAnalysis entity type.
/// </summary>
public class UrlMultiAnalysisConfiguration : IEntityTypeConfiguration<UrlMultiAnalysis>
{
    private const string SmallintType = "smallint";
    private const string VarcharType = "varchar";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UrlMultiAnalysis> builder)
    {
        ConfigureMultiAnalysis(builder);
        builder.OwnsMany(a => a.Analyses, ConfigureAnalyses);
    }

    /// <summary>
    /// Configures the properties and relationships of the UrlMultiAnalysis entity.
    /// </summary>
    /// <param name="builder">The builder used to configure the UrlMultiAnalysis entity.</param>
    private static void ConfigureMultiAnalysis(EntityTypeBuilder<UrlMultiAnalysis> builder)
    {
        builder.ToTable("url_multi_analyses");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("url_multi_analysis_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(u => u.IsPrivate)
            .HasColumnName("is_private")
            .HasColumnType("boolean")
            .IsRequired();

        builder.Property(u => u.StartedDate)
            .HasColumnName("started_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.OwnsOne(f => f.State, stateBuilder =>
        {
            stateBuilder.Property(s => s.Status)
                .HasColumnName("status")
                .HasColumnType(SmallintType)
                .IsRequired();

            stateBuilder.Property(s => s.Verdict)
                .HasColumnName("verdict")
                .HasColumnType(SmallintType)
                .IsRequired();

            stateBuilder.Property(s => s.ThreatZone)
                .HasColumnName("threat_zone")
                .HasColumnType(SmallintType)
                .IsRequired();
        });

        builder.Property(u => u.AverageThreatScore)
            .HasColumnName("average_threat_score")
            .HasColumnType("real")
            .HasMaxLength(25);

        builder.Property(u => u.Url)
            .HasColumnName("url")
            .HasColumnType(VarcharType)
            .HasMaxLength(2083)
            .IsRequired()
            .HasConversion(
                u => u.AbsoluteUri,
                dbValue => new Uri(dbValue));

        builder.HasOne(u => u.DataHashValues)
            .WithMany()
            .HasForeignKey("sha256")
            .IsRequired();

        builder.Navigation(u => u.DataHashValues)
            .AutoInclude();

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .HasColumnType(VarcharType)
            .HasMaxLength(450)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => UserId.Create(dbValue));

        builder.HasIndex(u => u.UserId);
    }

    /// <summary>
    /// Configures the owned collection of <see cref="UrlAnalysis"/> entities for a <see cref="UrlMultiAnalysis"/>.
    /// </summary>
    private static void ConfigureAnalyses(
        OwnedNavigationBuilder<UrlMultiAnalysis, UrlAnalysis> builder)
    {
        builder.ToTable("url_analyses");

        builder.WithOwner().HasForeignKey("url_multi_analysis_id");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("url_analysis_id")
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(u => u.ExternalId)
            .HasColumnName("external_id")
            .HasColumnType(VarcharType)
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(
                id => id.ToString(),
                id => ExternalAnalysisId.Parse(id));

        builder.Property(u => u.ServiceName)
            .HasColumnName("service_name")
            .HasColumnType(VarcharType)
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(u => u.State, stateBuilder =>
        {
            stateBuilder.Property(s => s.Status)
                .HasColumnName("status")
                .HasColumnType(SmallintType)
                .IsRequired();

            stateBuilder.Property(s => s.Verdict)
                .HasColumnName("verdict")
                .HasColumnType(SmallintType)
                .IsRequired();

            stateBuilder.Property(s => s.ThreatZone)
                .HasColumnName("threat_zone")
                .HasColumnType(SmallintType)
                .IsRequired();
        });

        builder.OwnsOne(r => r.ThreatScore, tsBuilder =>
        {
            tsBuilder.Ignore(t => t.NormalizedValue);

            tsBuilder.Property(t => t.RawValue)
                .HasColumnName("raw_threat_score")
                .HasColumnType("real");

            tsBuilder.Property(t => t.MaxPossibleRawValue)
                .HasColumnName("max_possible_threat_score")
                .HasColumnType("real");
        });
    }
}