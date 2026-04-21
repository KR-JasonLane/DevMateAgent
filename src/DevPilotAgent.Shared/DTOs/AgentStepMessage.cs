namespace DevPilotAgent.Shared.DTOs;

public record AgentStepMessage(
    Guid AnalysisId,
    int StepNumber,
    string StepName,
    string Status,
    string Message,
    string? DataJson,
    DateTime Timestamp
);
