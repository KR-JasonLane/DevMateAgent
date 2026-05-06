namespace DevPilotAgent.Shared.DTOs;

/// <summary>
/// Agent 파이프라인의 각 단계 진행 상황을 SignalR로 전달하는 메시지 DTO.
/// </summary>
/// <param name="AnalysisId">대상 분석 ID.</param>
/// <param name="StepNumber">단계 번호 (1~7).</param>
/// <param name="StepName">단계 이름 (AgentStepNames 상수 참조).</param>
/// <param name="Status">단계 상태 (Started, Completed, Failed).</param>
/// <param name="Message">사용자에게 표시할 진행 메시지.</param>
/// <param name="DataJson">단계 결과 데이터 (JSON). 없으면 null.</param>
/// <param name="Timestamp">메시지 생성 시각 (UTC).</param>
public record AgentStepMessage(
    Guid AnalysisId,
    int StepNumber,
    string StepName,
    string Status,
    string Message,
    string? DataJson,
    DateTime Timestamp
);
