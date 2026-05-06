namespace DevPilotAgent.Shared.Constants;

/// <summary>
/// 7단계 Agent 파이프라인의 각 단계 이름 상수.
/// SignalR 메시지와 UI에서 단계를 식별하는 데 사용한다.
/// </summary>
public static class AgentStepNames
{
    /// <summary>1단계: 에러 로그에서 키워드 추출.</summary>
    public const string ExtractKeywords = "ExtractKeywords";

    /// <summary>2단계: 키워드 기반 관련 파일 검색.</summary>
    public const string SearchFiles = "SearchFiles";

    /// <summary>3단계: 관련 파일 내용 읽기.</summary>
    public const string ReadFiles = "ReadFiles";

    /// <summary>4단계: LLM 기반 근본 원인 분석.</summary>
    public const string AnalyzeRootCause = "AnalyzeRootCause";

    /// <summary>5단계: LLM 기반 수정안 생성.</summary>
    public const string GenerateFix = "GenerateFix";

    /// <summary>6단계: LLM 기반 테스트 시나리오 생성.</summary>
    public const string GenerateTests = "GenerateTests";

    /// <summary>7단계: LLM 기반 PR 설명문 생성.</summary>
    public const string GeneratePrDescription = "GeneratePrDescription";
}
