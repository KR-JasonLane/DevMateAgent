namespace DevPilotAgent.Application.Interfaces;

using DevPilotAgent.Application.Models;

/// <summary>
/// 7단계 Agent 파이프라인을 순차 실행하는 오케스트레이터 인터페이스.
/// </summary>
public interface IAgentOrchestrator
{
    /// <summary>
    /// 에러 분석 파이프라인을 실행한다.
    /// 키워드 추출 → 파일 검색 → 파일 읽기 → 원인 분석 → 수정안 → 테스트 → PR 설명문 순서로 진행한다.
    /// </summary>
    /// <param name="projectFolderPath">분석 대상 프로젝트 폴더 경로.</param>
    /// <param name="errorLog">분석할 에러 로그 원문.</param>
    /// <param name="analysisId">분석 레코드 ID (진행 보고용).</param>
    /// <param name="reporter">진행 상황 보고자.</param>
    /// <param name="cancellationToken">취소 토큰.</param>
    /// <returns>파이프라인 전체 실행 결과.</returns>
    Task<AgentResult> RunAsync(
        string projectFolderPath,
        string errorLog,
        Guid analysisId,
        IAgentProgressReporter reporter,
        CancellationToken cancellationToken = default);
}
