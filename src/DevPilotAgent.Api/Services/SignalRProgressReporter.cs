namespace DevPilotAgent.Api.Services;

using DevPilotAgent.Api.Hubs;
using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Shared.Constants;
using DevPilotAgent.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR를 통해 Agent 파이프라인 진행 상황을 클라이언트에 전달하는 <see cref="IAgentProgressReporter"/> 구현체.
/// Api 프로젝트에 위치하여 Infrastructure의 Api 의존을 방지한다.
/// </summary>
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
