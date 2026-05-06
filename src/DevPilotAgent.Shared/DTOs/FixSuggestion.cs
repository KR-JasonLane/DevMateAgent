namespace DevPilotAgent.Shared.DTOs;

/// <summary>
/// LLM이 생성한 코드 수정 제안 DTO.
/// </summary>
/// <param name="TargetFilePath">수정 대상 파일 경로.</param>
/// <param name="Description">수정 내용에 대한 설명.</param>
/// <param name="UnifiedDiff">unified diff 형식의 변경 내용.</param>
/// <param name="ModifiedContent">수정 후 전체 파일 내용.</param>
/// <param name="FileLastModifiedUtc">분석 시점의 파일 최종 수정 시각 (충돌 감지용).</param>
public record FixSuggestion(
    string TargetFilePath,
    string Description,
    string UnifiedDiff,
    string ModifiedContent,
    DateTime FileLastModifiedUtc
);
