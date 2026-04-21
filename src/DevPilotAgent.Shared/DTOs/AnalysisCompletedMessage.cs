namespace DevPilotAgent.Shared.DTOs;

public record AnalysisCompletedMessage(
    Guid AnalysisId,
    bool Success,
    string? ErrorMessage
);
