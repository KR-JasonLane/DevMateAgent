namespace DevPilotAgent.Shared.Requests;

/// <summary>
/// 에러 분석 요청 DTO.
/// </summary>
/// <param name="ProjectFolderPath">분석 대상 프로젝트 폴더의 절대 경로.</param>
/// <param name="ErrorLog">분석할 에러 로그 원문.</param>
public record AnalysisRequest(
    string ProjectFolderPath,
    string ErrorLog
);
