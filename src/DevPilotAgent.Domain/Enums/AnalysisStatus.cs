namespace DevPilotAgent.Domain.Enums;

/// <summary>
/// 분석 작업의 생명주기 상태를 나타내는 열거형.
/// </summary>
public enum AnalysisStatus
{
    /// <summary>생성되었으나 아직 실행 전.</summary>
    Pending = 0,

    /// <summary>Agent 파이프라인 실행 중.</summary>
    Running = 1,

    /// <summary>모든 단계가 성공적으로 완료됨.</summary>
    Completed = 2,

    /// <summary>실행 중 오류 발생으로 실패.</summary>
    Failed = 3,

    /// <summary>사용자에 의해 취소됨.</summary>
    Cancelled = 4
}
