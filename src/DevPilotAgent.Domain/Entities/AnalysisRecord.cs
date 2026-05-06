namespace DevPilotAgent.Domain.Entities;

using DevPilotAgent.Domain.Enums;

/// <summary>
/// 에러 분석 결과를 저장하는 도메인 엔티티.
/// 7단계 Agent 파이프라인의 각 단계 결과를 JSON 컬럼으로 보관한다.
/// </summary>
public class AnalysisRecord
{
    /// <summary>분석 고유 식별자.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>분석 대상 프로젝트 폴더의 절대 경로.</summary>
    public string ProjectFolderPath { get; set; } = string.Empty;

    /// <summary>사용자가 입력한 에러 로그 원문.</summary>
    public string ErrorLog { get; set; } = string.Empty;

    /// <summary>에러 로그에서 추출된 키워드 목록 (JSON 배열).</summary>
    public string ExtractedKeywordsJson { get; set; } = "[]";

    /// <summary>키워드 기반으로 검색된 관련 파일 후보 목록 (JSON 배열).</summary>
    public string RelatedFilesJson { get; set; } = "[]";

    /// <summary>LLM이 생성한 근본 원인 분석 텍스트.</summary>
    public string RootCauseAnalysis { get; set; } = string.Empty;

    /// <summary>LLM이 제안한 수정안 목록 (JSON 배열).</summary>
    public string FixSuggestionsJson { get; set; } = "[]";

    /// <summary>LLM이 생성한 테스트 시나리오 목록 (JSON 배열).</summary>
    public string TestScenariosJson { get; set; } = "[]";

    /// <summary>LLM이 작성한 PR 설명문 초안 (Markdown).</summary>
    public string PrDescription { get; set; } = string.Empty;

    /// <summary>현재 분석 상태.</summary>
    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;

    /// <summary>분석 실패 시 오류 메시지. 성공 시 null.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>분석 레코드 생성 시각 (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>분석 완료 또는 실패 시각 (UTC). 진행 중이면 null.</summary>
    public DateTime? CompletedAt { get; set; }
}
