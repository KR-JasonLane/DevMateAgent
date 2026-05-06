namespace DevPilotAgent.Shared.Constants;

/// <summary>
/// SignalR Hub 메서드 이름 상수.
/// 서버와 클라이언트 간 메서드 이름 일관성을 보장한다.
/// </summary>
public static class HubMethods
{
    /// <summary>Agent 단계 진행 상황 알림 (서버 → 클라이언트).</summary>
    public const string AgentStepUpdate = "AgentStepUpdate";

    /// <summary>LLM 스트리밍 청크 전달 (서버 → 클라이언트).</summary>
    public const string StreamChunk = "StreamChunk";

    /// <summary>분석 완료 알림 (서버 → 클라이언트).</summary>
    public const string AnalysisCompleted = "AnalysisCompleted";

    /// <summary>분석 그룹 참가 요청 (클라이언트 → 서버).</summary>
    public const string JoinAnalysis = "JoinAnalysis";

    /// <summary>분석 그룹 탈퇴 요청 (클라이언트 → 서버).</summary>
    public const string LeaveAnalysis = "LeaveAnalysis";
}
