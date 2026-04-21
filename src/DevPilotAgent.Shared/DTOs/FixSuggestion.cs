namespace DevPilotAgent.Shared.DTOs;

public record FixSuggestion(
    string TargetFilePath,
    string Description,
    string UnifiedDiff,
    string ModifiedContent,
    DateTime FileLastModifiedUtc
);
