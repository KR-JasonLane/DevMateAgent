namespace DevPilotAgent.Application.UseCases;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Application.Mappers;
using DevPilotAgent.Shared.Responses;

/// <summary>
/// 분석 목록을 페이지네이션으로 조회하는 UseCase.
/// </summary>
public class ListAnalysesUseCase
{
    private readonly IAnalysisRepository _repository;
    private readonly AnalysisMapper _mapper;

    public ListAnalysesUseCase(IAnalysisRepository repository, AnalysisMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// 생성일 역순으로 정렬된 분석 요약 목록을 페이지 단위로 조회한다.
    /// </summary>
    /// <param name="page">페이지 번호 (1-based).</param>
    /// <param name="pageSize">페이지당 항목 수.</param>
    /// <returns>페이지네이션된 분석 요약 목록.</returns>
    public async Task<PagedResponse<AnalysisSummaryDto>> ExecuteAsync(int page, int pageSize)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize);
        var summaries = items.Select(_mapper.ToSummary).ToList();
        return new PagedResponse<AnalysisSummaryDto>(summaries, totalCount, page, pageSize);
    }
}
