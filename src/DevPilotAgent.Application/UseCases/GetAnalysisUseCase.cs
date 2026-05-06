namespace DevPilotAgent.Application.UseCases;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Application.Mappers;
using DevPilotAgent.Shared.Responses;

/// <summary>
/// 단일 분석 결과를 조회하는 UseCase.
/// </summary>
public class GetAnalysisUseCase
{
    private readonly IAnalysisRepository _repository;
    private readonly AnalysisMapper _mapper;

    public GetAnalysisUseCase(IAnalysisRepository repository, AnalysisMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// ID로 분석 결과를 조회하여 응답 DTO로 변환한다.
    /// </summary>
    /// <param name="id">조회할 분석 ID.</param>
    /// <returns>분석 응답 DTO. 존재하지 않으면 null.</returns>
    public async Task<AnalysisResponse?> ExecuteAsync(Guid id)
    {
        var record = await _repository.GetByIdAsync(id);
        return record is null ? null : _mapper.ToResponse(record);
    }
}
