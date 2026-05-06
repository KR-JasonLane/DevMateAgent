namespace DevPilotAgent.Shared.DTOs;

/// <summary>
/// Blazor 채팅 패널에 표시되는 메시지 DTO.
/// </summary>
/// <param name="Type">메시지 유형.</param>
/// <param name="Content">메시지 본문.</param>
/// <param name="Timestamp">메시지 생성 시각.</param>
/// <param name="StepName">관련 Agent 단계 이름. 단계 메시지가 아니면 null.</param>
/// <param name="StepNumber">현재 단계 번호. 단계 메시지가 아니면 null.</param>
/// <param name="TotalSteps">전체 단계 수. 단계 메시지가 아니면 null.</param>
public record ChatMessageDto(
    ChatMessageType Type,
    string Content,
    DateTime Timestamp,
    string? StepName = null,
    int? StepNumber = null,
    int? TotalSteps = null);

/// <summary>
/// 채팅 메시지의 유형을 구분하는 열거형.
/// </summary>
public enum ChatMessageType
{
    /// <summary>사용자가 입력한 메시지.</summary>
    User,

    /// <summary>Agent가 생성한 완료 메시지.</summary>
    Agent,

    /// <summary>시스템 알림 메시지.</summary>
    System,

    /// <summary>Agent 파이프라인 단계 진행 메시지.</summary>
    Step,

    /// <summary>오류 메시지.</summary>
    Error,

    /// <summary>LLM 스트리밍 중인 실시간 메시지.</summary>
    Streaming
}
