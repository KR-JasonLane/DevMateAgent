namespace DevPilotAgent.Shared.Responses;

using DevPilotAgent.Shared.DTOs;

public record AnalysisResponse(
    Guid Id,
    string ProjectFolderPath,
    string ErrorLog,
    List<string> ExtractedKeywords,
    List<FileCandidate> RelatedFiles,
    string RootCauseAnalysis,
    List<FixSuggestion> FixSuggestions,
    List<string> TestScenarios,
    string PrDescription,
    string Status,
    DateTime CreatedAt
);
