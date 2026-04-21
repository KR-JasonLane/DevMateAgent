namespace DevPilotAgent.Shared.Responses;

public record ApplyPatchResponse(
    bool Success,
    string Message,
    string BackupFilePath
);
