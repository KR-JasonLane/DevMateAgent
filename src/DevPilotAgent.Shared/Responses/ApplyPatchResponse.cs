namespace DevPilotAgent.Shared.Responses;

/// <summary>
/// 수정안 적용 결과를 반환하는 응답 DTO.
/// </summary>
/// <param name="Success">적용 성공 여부.</param>
/// <param name="Message">결과 메시지.</param>
/// <param name="BackupFilePath">원본 파일의 백업 경로.</param>
public record ApplyPatchResponse(
    bool Success,
    string Message,
    string BackupFilePath
);
