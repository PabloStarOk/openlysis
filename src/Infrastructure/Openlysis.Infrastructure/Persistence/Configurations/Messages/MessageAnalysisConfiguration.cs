using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Entities;

namespace Openlysis.Infrastructure.Persistence.Configurations.Messages;

/// <summary>
/// Configuration class for the <see cref="MessageAnalysis"/> entity.
/// Implements the <see cref="IEntityTypeConfiguration{TEntity}"/> interface to configure the entity's schema.
/// </summary>
public class MessageAnalysisConfiguration : IEntityTypeConfiguration<MessageAnalysis>
{
    private const string SmallintType = "smallint";
    private const string VarcharType = "varchar";
    private const string MessageAnalysisIdColumnName = "message_analysis_id";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<MessageAnalysis> builder)
    {
        ConfigureMessageAnalysis(builder);

        builder.OwnsMany(d => d.AttachedFilesIndicators, nestedBuilder => ConfigureIndicator(
            nestedBuilder,
            tableName: "attached_file_results",
            idColumnName: "attached_file_result_id",
            valueColumnName: "file_name",
            resultIdColumnName: "file_multi_analysis_id"));

        builder.OwnsMany(d => d.DetectedUrlsIndicators, nestedBuilder => ConfigureIndicator(
            nestedBuilder,
            tableName: "detected_url_results",
            idColumnName: "detected_url_results_id",
            valueColumnName: "url",
            resultIdColumnName: "url_multi_analysis_id"));

        builder.OwnsMany(d => d.DetectedEmailAddressesIndicators, nestedBuilder => ConfigureIndicator(
            nestedBuilder,
            tableName: "detected_email_address_results",
            idColumnName: "detected_email_address_results_id",
            valueColumnName: "email_address",
            resultIdColumnName: "email_address_multi_reputation_id"));

        builder.OwnsMany(d => d.DetectedPhoneNumbersIndicators, nestedBuilder => ConfigureIndicator(
            nestedBuilder,
            tableName: "detected_phone_number_results",
            idColumnName: "detected_phone_number_results_id",
            valueColumnName: "phone_number",
            resultIdColumnName: "phone_multi_reputation_id"));
    }

    /// <summary>
    /// Configures the schema for the <see cref="MessageAnalysis"/> entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="EntityTypeBuilder{TEntity}"/> used to configure the entity.
    /// </param>
    private static void ConfigureMessageAnalysis(
        EntityTypeBuilder<MessageAnalysis> builder)
    {
        builder.ToTable("message_analysis");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName(MessageAnalysisIdColumnName)
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(m => m.IsPrivate)
            .HasColumnName("is_private")
            .HasColumnType("boolean")
            .IsRequired();

        builder.Property(m => m.StartedDate)
            .HasColumnName("started_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.OwnsOne(m => m.Message, messageBuilder =>
        {
            messageBuilder.Property(m => m.Type)
                .HasColumnName("message_type")
                .HasColumnType(SmallintType)
                .IsRequired();

            messageBuilder.Property(m => m.Sender)
                .HasColumnName("message_sender")
                .HasColumnType(VarcharType)
                .HasMaxLength(254)
                .IsRequired();

            messageBuilder.Property(m => m.Subject)
                .HasColumnName("message_subject")
                .HasColumnType(VarcharType)
                .HasMaxLength(998);

            messageBuilder.Property(m => m.Content)
                .HasColumnName("message_content")
                .HasColumnType("text")
                .IsRequired();

            messageBuilder
                .HasOne(m => m.HashValues)
                .WithMany()
                .HasForeignKey("sha256")
                .IsRequired();

            messageBuilder.Property("sha256")
                .HasColumnName("sha256")
                .IsRequired();

            messageBuilder
                .Navigation(m => m.HashValues)
                .AutoInclude();
        });

        builder.OwnsOne(m => m.State, stateBuilder =>
        {
           stateBuilder.Property(m => m.Status)
               .HasColumnName("status")
               .HasColumnType(SmallintType)
               .IsRequired();

           stateBuilder.Property(m => m.Verdict)
               .HasColumnName("verdict")
               .HasColumnType(SmallintType)
               .IsRequired();

           stateBuilder.Property(m => m.ThreatZone)
               .HasColumnName("threat_zone")
               .HasColumnType(SmallintType)
               .IsRequired();
        });

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Navigation(m => m.Message)
            .AutoInclude();

        builder.Navigation(m => m.State)
            .AutoInclude();
    }

    private static void ConfigureIndicator(
        OwnedNavigationBuilder<MessageAnalysis, Indicator> builder,
        string tableName,
        string idColumnName,
        string valueColumnName,
        string resultIdColumnName)
    {
        builder.ToTable(tableName);

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName(idColumnName)
            .HasColumnType("uuid")
            .IsRequired()
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.Property(a => a.Type)
            .HasColumnName("data_type")
            .HasColumnType(SmallintType)
            .IsRequired();

        builder.Property(u => u.Value)
            .HasColumnName(valueColumnName)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.ResultId)
            .HasColumnName(resultIdColumnName)
            .HasColumnType("uuid")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                dbValue => GlobalId.Parse(dbValue.ToString()));

        builder.OwnsOne(a => a.State, stateBuilder =>
            {
                stateBuilder.Property(s => s.Status)
                    .HasColumnName("status")
                    .HasColumnType(SmallintType)
                    .IsRequired();

                stateBuilder.Property(s => s.Verdict)
                    .HasColumnName("verdict")
                    .HasColumnType(SmallintType)
                    .IsRequired();

                stateBuilder.Ignore(s => s.ThreatZone);
                stateBuilder.Ignore(s => s.CanBeUpdated);
            });

        builder.WithOwner()
            .HasForeignKey(MessageAnalysisIdColumnName);
    }
}