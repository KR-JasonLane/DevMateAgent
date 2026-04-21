namespace DevPilotAgent.Domain.Entities;

using DevPilotAgent.Domain.Enums;

public class AnalysisRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProjectFolderPath { get; set; } = string.Empty;
    public string ErrorLog { get; set; } = string.Empty;
    public string ExtractedKeywordsJson { get; set; } = "[]";
    public string RelatedFilesJson { get; set; } = "[]";
    public string RootCauseAnalysis { get; set; } = string.Empty;
    public string FixSuggestionsJson { get; set; } = "[]";
    public string TestScenariosJson { get; set; } = "[]";
    public string PrDescription { get; set; } = string.Empty;
    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
