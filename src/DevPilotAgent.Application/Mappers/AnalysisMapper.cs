namespace DevPilotAgent.Application.Mappers;

using System.Text.Json;
using DevPilotAgent.Domain.Entities;
using DevPilotAgent.Shared.DTOs;
using DevPilotAgent.Shared.Responses;

/// <summary>
/// AnalysisRecord 도메인 엔티티를 응답 DTO로 변환하는 매퍼.
/// JSON 컬럼의 역직렬화를 포함한다.
/// </summary>
public class AnalysisMapper
{
    /// <summary>
    /// AnalysisRecord를 전체 결과 응답 DTO로 변환한다.
    /// </summary>
    /// <param name="record">변환할 분석 레코드.</param>
    /// <returns>분석 응답 DTO.</returns>
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

    /// <summary>
    /// AnalysisRecord를 목록 표시용 요약 DTO로 변환한다.
    /// </summary>
    /// <param name="record">변환할 분석 레코드.</param>
    /// <returns>분석 요약 DTO.</returns>
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
