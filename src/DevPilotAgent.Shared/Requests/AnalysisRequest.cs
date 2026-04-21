namespace DevPilotAgent.Shared.Requests;

public record AnalysisRequest(
    string ProjectFolderPath,
    string ErrorLog
);
