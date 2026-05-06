namespace DevPilotAgent.Infrastructure.Persistence.Configurations;

using DevPilotAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// AnalysisRecord 엔티티의 EF Core 매핑 설정.
/// 테이블명, 필수 필드, 인덱스를 정의한다.
/// </summary>
public class AnalysisRecordConfiguration : IEntityTypeConfiguration<AnalysisRecord>
{
    /// <inheritdoc/>
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
