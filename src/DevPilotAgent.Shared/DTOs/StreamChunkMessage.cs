namespace DevPilotAgent.Shared.DTOs;

/// <summary>
/// LLM 스트리밍 응답의 버퍼링된 청크를 SignalR로 전달하는 메시지 DTO.
/// </summary>
/// <param name="AnalysisId">대상 분석 ID.</param>
/// <param name="StepNumber">현재 단계 번호.</param>
/// <param name="StepName">현재 단계 이름.</param>
/// <param name="Chunk">버퍼링된 토큰 텍스트. 완료 시 빈 문자열.</param>
/// <param name="IsComplete">해당 단계의 스트리밍이 완료되었는지 여부.</param>
public record StreamChunkMessage(
    Guid AnalysisId,
    int StepNumber,
    string StepName,
    string Chunk,
    bool IsComplete
);
