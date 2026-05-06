namespace DevPilotAgent.Application.Interfaces;

using DevPilotAgent.Shared.DTOs;

/// <summary>
/// Agent 파이프라인의 실행 진행 상황을 외부에 보고하는 인터페이스.
/// Api 프로젝트에서 SignalR 구현체가 등록된다.
/// </summary>
public interface IAgentProgressReporter
{
    /// <summary>
    /// 파이프라인 단계의 시작/완료/실패를 보고한다.
    /// </summary>
    /// <param name="message">단계 진행 메시지.</param>
    Task ReportStepAsync(AgentStepMessage message);

    /// <summary>
    /// LLM 스트리밍 응답의 버퍼링된 청크를 전달한다.
    /// </summary>
    /// <param name="message">스트리밍 청크 메시지.</param>
    Task ReportStreamChunkAsync(StreamChunkMessage message);

    /// <summary>
    /// 분석 전체 완료(성공 또는 실패)를 알린다.
    /// </summary>
    /// <param name="message">완료 메시지.</param>
    Task ReportCompletedAsync(AnalysisCompletedMessage message);
}
