namespace DevPilotAgent.Shared.Responses;

/// <summary>
/// 분석 생성 직후 반환되는 응답 DTO.
/// </summary>
/// <param name="AnalysisId">생성된 분석의 고유 ID.</param>
/// <param name="Status">초기 상태 문자열 (Pending).</param>
public record AnalysisStartedResponse(
    Guid AnalysisId,
    string Status
);
