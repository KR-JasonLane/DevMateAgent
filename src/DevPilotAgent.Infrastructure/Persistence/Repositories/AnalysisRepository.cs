namespace DevPilotAgent.Infrastructure.Persistence.Repositories;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Domain.Entities;
using DevPilotAgent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// EF Core 기반 <see cref="IAnalysisRepository"/> 구현체.
/// </summary>
public class AnalysisRepository : IAnalysisRepository
{
    private readonly AppDbContext _context;

    public AnalysisRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<AnalysisRecord> CreateAsync(AnalysisRecord record)
    {
        _context.AnalysisRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    /// <inheritdoc/>
    public async Task<AnalysisRecord?> GetByIdAsync(Guid id)
    {
        return await _context.AnalysisRecords.FindAsync(id);
    }

    /// <inheritdoc/>
    public async Task<(List<AnalysisRecord> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var totalCount = await _context.AnalysisRecords.CountAsync();

        var items = await _context.AnalysisRecords
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(AnalysisRecord record)
    {
        _context.AnalysisRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> HasRunningAnalysisAsync(string projectFolderPath)
    {
        return await _context.AnalysisRecords.AnyAsync(x =>
            x.ProjectFolderPath == projectFolderPath &&
            (x.Status == AnalysisStatus.Pending || x.Status == AnalysisStatus.Running));
    }
}
