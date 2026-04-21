namespace DevPilotAgent.Shared.DTOs;

public record StreamChunkMessage(
    Guid AnalysisId,
    int StepNumber,
    string StepName,
    string Chunk,
    bool IsComplete
);
