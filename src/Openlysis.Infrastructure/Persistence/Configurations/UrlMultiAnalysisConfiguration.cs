using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration class for the UrlMultiAnalysis entity.
/// Implements IEntityTypeConfiguration to provide configuration for the UrlMultiAnalysis entity type.
/// </summary>
public class UrlMultiAnalysisConfiguration : IEntityTypeConfiguration<UrlMultiAnalysis>
{
    private const string VarcharType = "VARCHAR";
    private const string NvarcharType = "NVARCHAR";
    private const string TinyIntType = "TINYINT";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UrlMultiAnalysis> builder)
    {
        ConfigureMultiAnalysis(builder);
        builder.OwnsMany(u => u.ServiceAnalyses, ConfigureServiceAnalysis);
    }

    /// <summary>
    /// Configures the properties and relationships of the UrlMultiAnalysis entity.
    /// </summary>
    /// <param name="builder">The builder used to configure the UrlMultiAnalysis entity.</param>
    private static void ConfigureMultiAnalysis(EntityTypeBuilder<UrlMultiAnalysis> builder)
    {
        builder.ToTable("UrlMultiAnalyses");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("UrlMultiAnalysisId")
            .HasColumnType(VarcharType)
            .HasMaxLength(36)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => MultiAnalysisId.Create(dbValue));

        builder.Property(u => u.IsPrivate)
            .HasColumnName("IsPrivate")
            .HasColumnType("BIT")
            .IsRequired();

        builder.Property(u => u.StartedDate)
            .HasColumnName("StartedDate")
            .HasColumnType("DATETIME2")
            .IsRequired();

        builder.Property(u => u.Status)
            .HasColumnName("Status")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.AverageVerdict)
            .HasColumnName("AverageVerdict")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.AverageThreatZone)
            .HasColumnName("AverageThreatZone")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.AverageThreatScore)
            .HasColumnName("AverageThreatScore")
            .HasColumnType("FLOAT")
            .HasMaxLength(25);

        builder.Property(u => u.Url)
            .HasColumnName("Url")
            .HasColumnType(NvarcharType)
            .HasMaxLength(2083)
            .IsRequired()
            .HasConversion(
                u => u.AbsoluteUri,
                dbValue => new Uri(dbValue));

        builder.HasOne(u => u.UrlHashSet)
            .WithMany()
            .HasForeignKey("Sha256")
            .IsRequired();

        builder.Navigation(u => u.UrlHashSet)
            .AutoInclude();

        builder.Property(u => u.UserId)
            .HasColumnName("UserId")
            .HasColumnType(NvarcharType)
            .HasMaxLength(450)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => UserId.Create(dbValue));

        builder.HasIndex(u => u.UserId)
            .IsClustered();
    }

    /// <summary>
    /// Configures the properties and relationships of the UrlServiceAnalysis entity.
    /// </summary>
    /// <param name="builder">The builder used to configure the UrlServiceAnalysis entity.</param>
    private static void ConfigureServiceAnalysis(OwnedNavigationBuilder<UrlMultiAnalysis, UrlServiceAnalysis> builder)
    {
        builder.ToTable("UrlServiceAnalyses");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("UrlServiceAnalysisId")
            .HasColumnType(NvarcharType)
            .HasMaxLength(100)
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.ToString(),
                id => ComposedServiceAnalysisId.Create(id));

        builder.Property(u => u.ServiceName)
            .HasColumnName("ServiceName")
            .HasColumnType(NvarcharType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasColumnName("Status")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.Verdict)
            .HasColumnName("Verdict")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.ThreatZone)
            .HasColumnName("ThreatZone")
            .HasColumnType(TinyIntType)
            .IsRequired();

        builder.Property(u => u.ThreatScore)
            .HasColumnName("ThreatScore")
            .HasColumnType("FLOAT")
            .HasMaxLength(25);

        builder.WithOwner()
            .HasForeignKey("UrlMultiAnalysisId");
    }
}