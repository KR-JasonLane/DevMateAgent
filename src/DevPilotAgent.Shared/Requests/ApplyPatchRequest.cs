namespace DevPilotAgent.Shared.Requests;

public record ApplyPatchRequest(
    Guid AnalysisId,
    int PatchIndex,
    string TargetFilePath
);
