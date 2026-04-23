namespace DevPilotAgent.Application.Interfaces;

using DevPilotAgent.Application.Models;

public interface IAgentOrchestrator
{
    Task<AgentResult> RunAsync(
        string projectFolderPath,
        string errorLog,
        Guid analysisId,
        IAgentProgressReporter reporter,
        CancellationToken cancellationToken = default);
}
