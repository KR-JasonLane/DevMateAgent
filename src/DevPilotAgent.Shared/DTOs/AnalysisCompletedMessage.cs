namespace DevPilotAgent.Shared.DTOs;

/// <summary>
/// 분석 전체 완료(성공 또는 실패)를 SignalR로 전달하는 메시지 DTO.
/// </summary>
/// <param name="AnalysisId">완료된 분석 ID.</param>
/// <param name="Success">성공 여부.</param>
/// <param name="ErrorMessage">실패 시 오류 메시지. 성공 시 null.</param>
public record AnalysisCompletedMessage(
    Guid AnalysisId,
    bool Success,
    string? ErrorMessage
);
