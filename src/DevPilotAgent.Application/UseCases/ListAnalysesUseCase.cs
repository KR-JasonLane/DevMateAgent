namespace DevPilotAgent.Application.UseCases;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Application.Mappers;
using DevPilotAgent.Shared.Responses;

public class ListAnalysesUseCase
{
    private readonly IAnalysisRepository _repository;
    private readonly AnalysisMapper _mapper;

    public ListAnalysesUseCase(IAnalysisRepository repository, AnalysisMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<AnalysisSummaryDto>> ExecuteAsync(int page, int pageSize)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize);
        var summaries = items.Select(_mapper.ToSummary).ToList();
        return new PagedResponse<AnalysisSummaryDto>(summaries, totalCount, page, pageSize);
    }
}
