namespace DevPilotAgent.Api.Hubs;

using Microsoft.AspNetCore.SignalR;

public class AnalysisHub : Hub
{
    public async Task JoinAnalysis(Guid analysisId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, analysisId.ToString());
    }

    public async Task LeaveAnalysis(Guid analysisId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, analysisId.ToString());
    }
}
