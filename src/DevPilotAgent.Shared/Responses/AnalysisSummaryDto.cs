namespace DevPilotAgent.Shared.Responses;

/// <summary>
/// 분석 목록 조회 시 사용하는 요약 DTO.
/// </summary>
/// <param name="Id">분석 고유 ID.</param>
/// <param name="ProjectFolderPath">프로젝트 폴더 경로.</param>
/// <param name="ErrorLogPreview">에러 로그의 앞부분 미리보기 (최대 100자).</param>
/// <param name="Status">분석 상태 문자열.</param>
/// <param name="CreatedAt">분석 생성 시각.</param>
public record AnalysisSummaryDto(
    Guid Id,
    string ProjectFolderPath,
    string ErrorLogPreview,
    string Status,
    DateTime CreatedAt
);
