namespace DevPilotAgent.Shared.Responses;

public record AnalysisStartedResponse(
    Guid AnalysisId,
    string Status
);
