namespace DevPilotAgent.Application.Models;

using DevPilotAgent.Shared.DTOs;

/// <summary>
/// 7단계 Agent 파이프라인의 전체 실행 결과를 담는 모델.
/// </summary>
public class AgentResult
{
    /// <summary>1단계에서 추출된 키워드 목록.</summary>
    public List<string> ExtractedKeywords { get; set; } = [];

    /// <summary>2단계에서 검색된 관련 파일 후보 목록.</summary>
    public List<FileCandidate> RelatedFiles { get; set; } = [];

    /// <summary>4단계에서 생성된 근본 원인 분석 텍스트.</summary>
    public string RootCauseAnalysis { get; set; } = string.Empty;

    /// <summary>5단계에서 생성된 수정 제안 목록.</summary>
    public List<FixSuggestion> FixSuggestions { get; set; } = [];

    /// <summary>6단계에서 생성된 테스트 시나리오 목록.</summary>
    public List<string> TestScenarios { get; set; } = [];

    /// <summary>7단계에서 생성된 PR 설명문 초안.</summary>
    public string PrDescription { get; set; } = string.Empty;
}
