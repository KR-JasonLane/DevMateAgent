namespace DevPilotAgent.Application.Interfaces;

using DevPilotAgent.Domain.Entities;

/// <summary>
/// 분석 레코드의 영속성 접근을 추상화하는 리포지토리 인터페이스.
/// </summary>
public interface IAnalysisRepository
{
    /// <summary>
    /// 새 분석 레코드를 생성하고 저장한다.
    /// </summary>
    /// <param name="record">저장할 분석 레코드.</param>
    /// <returns>저장된 분석 레코드.</returns>
    Task<AnalysisRecord> CreateAsync(AnalysisRecord record);

    /// <summary>
    /// ID로 분석 레코드를 조회한다.
    /// </summary>
    /// <param name="id">조회할 분석 ID.</param>
    /// <returns>분석 레코드. 존재하지 않으면 null.</returns>
    Task<AnalysisRecord?> GetByIdAsync(Guid id);

    /// <summary>
    /// 생성일 역순으로 페이지네이션된 분석 목록을 조회한다.
    /// </summary>
    /// <param name="page">페이지 번호 (1-based).</param>
    /// <param name="pageSize">페이지당 항목 수.</param>
    /// <returns>해당 페이지의 항목 목록과 전체 항목 수.</returns>
    Task<(List<AnalysisRecord> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// 기존 분석 레코드를 갱신한다.
    /// </summary>
    /// <param name="record">갱신할 분석 레코드.</param>
    Task UpdateAsync(AnalysisRecord record);

    /// <summary>
    /// 지정된 프로젝트 폴더에 대해 진행 중(Pending/Running)인 분석이 있는지 확인한다.
    /// </summary>
    /// <param name="projectFolderPath">확인할 프로젝트 폴더 경로.</param>
    /// <returns>진행 중인 분석이 있으면 true.</returns>
    Task<bool> HasRunningAnalysisAsync(string projectFolderPath);
}
