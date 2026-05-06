namespace DevPilotAgent.Shared.Constants;

/// <summary>
/// LLM 스트리밍 토큰 버퍼링 설정 상수.
/// 개별 토큰 전송을 방지하고 적절한 크기로 묶어 전송한다.
/// </summary>
public static class StreamBufferSettings
{
    /// <summary>버퍼 플러시를 위한 최소 토큰 수.</summary>
    public const int MinChunkSize = 5;

    /// <summary>버퍼 강제 플러시 간격 (밀리초).</summary>
    public const int FlushIntervalMs = 80;
}
