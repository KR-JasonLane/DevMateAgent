namespace DevPilotAgent.Infrastructure.Persistence.Configurations;

using DevPilotAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AnalysisRecordConfiguration : IEntityTypeConfiguration<AnalysisRecord>
{
    public void Configure(EntityTypeBuilder<AnalysisRecord> builder)
    {
        builder.ToTable("AnalysisRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectFolderPath).IsRequired();
        builder.Property(x => x.ErrorLog).IsRequired();
        builder.Property(x => x.ExtractedKeywordsJson).IsRequired();
        builder.Property(x => x.RelatedFilesJson).IsRequired();
        builder.Property(x => x.RootCauseAnalysis).IsRequired();
        builder.Property(x => x.FixSuggestionsJson).IsRequired();
        builder.Property(x => x.TestScenariosJson).IsRequired();
        builder.Property(x => x.PrDescription).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.HasIndex(x => x.CreatedAt)
            .IsDescending()
            .HasDatabaseName("IX_AnalysisRecords_CreatedAt");
    }
}
