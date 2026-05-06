namespace DevPilotAgent.Shared.Responses;

using DevPilotAgent.Shared.DTOs;

/// <summary>
/// 완료된 분석의 전체 결과를 반환하는 응답 DTO.
/// </summary>
/// <param name="Id">분석 고유 ID.</param>
/// <param name="ProjectFolderPath">분석 대상 프로젝트 폴더 경로.</param>
/// <param name="ErrorLog">원본 에러 로그.</param>
/// <param name="ExtractedKeywords">추출된 키워드 목록.</param>
/// <param name="RelatedFiles">관련 파일 후보 목록.</param>
/// <param name="RootCauseAnalysis">근본 원인 분석 텍스트.</param>
/// <param name="FixSuggestions">수정 제안 목록.</param>
/// <param name="TestScenarios">테스트 시나리오 목록.</param>
/// <param name="PrDescription">PR 설명문 초안.</param>
/// <param name="Status">분석 상태 문자열.</param>
/// <param name="CreatedAt">분석 생성 시각.</param>
public record AnalysisResponse(
    Guid Id,
    string ProjectFolderPath,
    string ErrorLog,
    List<string> ExtractedKeywords,
    List<FileCandidate> RelatedFiles,
    string RootCauseAnalysis,
    List<FixSuggestion> FixSuggestions,
    List<string> TestScenarios,
    string PrDescription,
    string Status,
    DateTime CreatedAt
);
