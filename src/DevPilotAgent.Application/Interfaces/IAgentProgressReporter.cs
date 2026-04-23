namespace DevPilotAgent.Application.Interfaces;

using DevPilotAgent.Shared.DTOs;

public interface IAgentProgressReporter
{
    Task ReportStepAsync(AgentStepMessage message);
    Task ReportStreamChunkAsync(StreamChunkMessage message);
    Task ReportCompletedAsync(AnalysisCompletedMessage message);
}
