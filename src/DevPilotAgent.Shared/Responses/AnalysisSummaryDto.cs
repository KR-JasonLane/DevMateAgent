namespace DevPilotAgent.Shared.Responses;

public record AnalysisSummaryDto(
    Guid Id,
    string ProjectFolderPath,
    string ErrorLogPreview,
    string Status,
    DateTime CreatedAt
);
