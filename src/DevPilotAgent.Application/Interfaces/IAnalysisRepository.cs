namespace DevPilotAgent.Application.Interfaces;

using DevPilotAgent.Domain.Entities;

public interface IAnalysisRepository
{
    Task<AnalysisRecord> CreateAsync(AnalysisRecord record);
    Task<AnalysisRecord?> GetByIdAsync(Guid id);
    Task<(List<AnalysisRecord> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task UpdateAsync(AnalysisRecord record);
    Task<bool> HasRunningAnalysisAsync(string projectFolderPath);
}
