namespace DevPilotAgent.Application.Mappers;

using System.Text.Json;
using DevPilotAgent.Domain.Entities;
using DevPilotAgent.Shared.DTOs;
using DevPilotAgent.Shared.Responses;

public class AnalysisMapper
{
    public AnalysisResponse ToResponse(AnalysisRecord record)
    {
        return new AnalysisResponse(
            Id: record.Id,
            ProjectFolderPath: record.ProjectFolderPath,
            ErrorLog: record.ErrorLog,
            ExtractedKeywords: Deserialize<List<string>>(record.ExtractedKeywordsJson),
            RelatedFiles: Deserialize<List<FileCandidate>>(record.RelatedFilesJson),
            RootCauseAnalysis: record.RootCauseAnalysis,
            FixSuggestions: Deserialize<List<FixSuggestion>>(record.FixSuggestionsJson),
            TestScenarios: Deserialize<List<string>>(record.TestScenariosJson),
            PrDescription: record.PrDescription,
            Status: record.Status.ToString(),
            CreatedAt: record.CreatedAt
        );
    }

    public AnalysisSummaryDto ToSummary(AnalysisRecord record)
    {
        var preview = record.ErrorLog.Length > 100
            ? record.ErrorLog[..100] + "..."
            : record.ErrorLog;

        return new AnalysisSummaryDto(
            Id: record.Id,
            ProjectFolderPath: record.ProjectFolderPath,
            ErrorLogPreview: preview,
            Status: record.Status.ToString(),
            CreatedAt: record.CreatedAt
        );
    }

    private static T Deserialize<T>(string json) where T : new()
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch
        {
            return new T();
        }
    }
}
