namespace DevPilotAgent.Application.UseCases;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Application.Mappers;
using DevPilotAgent.Shared.Responses;

public class GetAnalysisUseCase
{
    private readonly IAnalysisRepository _repository;
    private readonly AnalysisMapper _mapper;

    public GetAnalysisUseCase(IAnalysisRepository repository, AnalysisMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AnalysisResponse?> ExecuteAsync(Guid id)
    {
        var record = await _repository.GetByIdAsync(id);
        return record is null ? null : _mapper.ToResponse(record);
    }
}
