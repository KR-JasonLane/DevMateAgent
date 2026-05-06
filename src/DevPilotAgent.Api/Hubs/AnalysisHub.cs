namespace DevPilotAgent.Api.Hubs;

using Microsoft.AspNetCore.SignalR;

/// <summary>
/// 분석 실시간 스트리밍을 위한 SignalR Hub.
/// 클라이언트는 분석 ID 기반 그룹에 참가하여 해당 분석의 진행 상황을 수신한다.
/// </summary>
public class AnalysisHub : Hub
{
    /// <summary>
    /// 지정된 분석의 SignalR 그룹에 참가한다.
    /// 분석 시작 전에 호출되어야 경쟁 조건을 방지할 수 있다.
    /// </summary>
    /// <param name="analysisId">참가할 분석 ID.</param>
    public async Task JoinAnalysis(Guid analysisId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, analysisId.ToString());
    }

    /// <summary>
    /// 지정된 분석의 SignalR 그룹에서 탈퇴한다.
    /// </summary>
    /// <param name="analysisId">탈퇴할 분석 ID.</param>
    public async Task LeaveAnalysis(Guid analysisId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, analysisId.ToString());
    }
}
