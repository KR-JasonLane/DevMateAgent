namespace DevPilotAgent.Shared.DTOs;

/// <summary>
/// 키워드 기반 파일 검색에서 발견된 관련 파일 후보 DTO.
/// </summary>
/// <param name="FilePath">파일의 절대 경로.</param>
/// <param name="MatchReason">매칭 사유 설명.</param>
/// <param name="RelevanceScore">관련도 점수 (0.0 ~ 1.0).</param>
public record FileCandidate(
    string FilePath,
    string MatchReason,
    double RelevanceScore
);
