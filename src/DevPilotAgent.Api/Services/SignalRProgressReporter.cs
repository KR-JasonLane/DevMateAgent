namespace DevPilotAgent.Api.Services;

using DevPilotAgent.Api.Hubs;
using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Shared.Constants;
using DevPilotAgent.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

public class SignalRProgressReporter : IAgentProgressReporter
{
    private readonly IHubContext<AnalysisHub> _hubContext;

    public SignalRProgressReporter(IHubContext<AnalysisHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task ReportStepAsync(AgentStepMessage message)
    {
        await _hubContext.Clients.Group(message.AnalysisId.ToString())
            .SendAsync(HubMethods.AgentStepUpdate, message);
    }

    public async Task ReportStreamChunkAsync(StreamChunkMessage message)
    {
        await _hubContext.Clients.Group(message.AnalysisId.ToString())
            .SendAsync(HubMethods.StreamChunk, message);
    }

    public async Task ReportCompletedAsync(AnalysisCompletedMessage message)
    {
        await _hubContext.Clients.Group(message.AnalysisId.ToString())
            .SendAsync(HubMethods.AnalysisCompleted, message);
    }
}
