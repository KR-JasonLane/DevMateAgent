namespace DevPilotAgent.Infrastructure.Persistence;

using DevPilotAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// SQLite 데이터베이스 접근을 위한 EF Core DbContext.
/// WAL 모드로 동시 읽기/쓰기를 지원한다.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>분석 레코드 테이블.</summary>
    public DbSet<AnalysisRecord> AnalysisRecords => Set<AnalysisRecord>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
