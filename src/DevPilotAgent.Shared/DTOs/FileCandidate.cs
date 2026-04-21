namespace DevPilotAgent.Shared.DTOs;

public record FileCandidate(
    string FilePath,
    string MatchReason,
    double RelevanceScore
);
