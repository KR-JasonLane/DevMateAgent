namespace DevPilotAgent.Application.Models;

using DevPilotAgent.Shared.DTOs;

public class AgentResult
{
    public List<string> ExtractedKeywords { get; set; } = [];
    public List<FileCandidate> RelatedFiles { get; set; } = [];
    public string RootCauseAnalysis { get; set; } = string.Empty;
    public List<FixSuggestion> FixSuggestions { get; set; } = [];
    public List<string> TestScenarios { get; set; } = [];
    public string PrDescription { get; set; } = string.Empty;
}
