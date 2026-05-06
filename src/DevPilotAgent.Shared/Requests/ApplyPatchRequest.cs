namespace DevPilotAgent.Shared.Requests;

/// <summary>
/// 수정안 적용 요청 DTO.
/// </summary>
/// <param name="AnalysisId">대상 분석 레코드의 ID.</param>
/// <param name="PatchIndex">적용할 수정안의 인덱스 (0-based).</param>
/// <param name="TargetFilePath">수정 대상 파일의 절대 경로.</param>
public record ApplyPatchRequest(
    Guid AnalysisId,
    int PatchIndex,
    string TargetFilePath
);
