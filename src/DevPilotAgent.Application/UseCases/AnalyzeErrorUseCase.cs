namespace DevPilotAgent.Application.UseCases;

using System.Text.Json;
using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Domain.Entities;
using DevPilotAgent.Domain.Enums;
using DevPilotAgent.Shared.DTOs;
using DevPilotAgent.Shared.Requests;

public class AnalyzeErrorUseCase
{
    private readonly IAgentOrchestrator _orchestrator;
    private readonly IAnalysisRepository _repository;
    private readonly IFileSystemService _fileSystemService;

    public AnalyzeErrorUseCase(
        IAgentOrchestrator orchestrator,
        IAnalysisRepository repository,
        IFileSystemService fileSystemService)
    {
        _orchestrator = orchestrator;
        _repository = repository;
        _fileSystemService = fileSystemService;
    }

    public async Task<Guid> StartAsync(AnalysisRequest request)
    {
        if (!_fileSystemService.ValidateFolderPath(request.ProjectFolderPath))
            throw new ArgumentException($"유효하지 않은 프로젝트 폴더 경로입니다: {request.ProjectFolderPath}");

        if (string.IsNullOrWhiteSpace(request.ErrorLog))
            throw new ArgumentException("에러 로그를 입력해주세요.");

        if (request.ErrorLog.Length > 50_000)
            throw new ArgumentException("에러 로그는 50,000자 이내여야 합니다.");

        if (await _repository.HasRunningAnalysisAsync(request.ProjectFolderPath))
            throw new InvalidOperationException("해당 프로젝트 폴더에 이미 진행 중인 분석이 있습니다.");

        var record = new AnalysisRecord
        {
            ProjectFolderPath = request.ProjectFolderPath,
            ErrorLog = request.ErrorLog,
            Status = AnalysisStatus.Pending
        };

        await _repository.CreateAsync(record);
        return record.Id;
    }

    public async Task ExecuteAsync(
        Guid analysisId,
        AnalysisRequest request,
        IAgentProgressReporter reporter,
        CancellationToken ct)
    {
        var record = await _repository.GetByIdAsync(analysisId)
            ?? throw new InvalidOperationException("분석 기록을 찾을 수 없습니다.");

        record.Status = AnalysisStatus.Running;
        await _repository.UpdateAsync(record);

        try
        {
            var result = await _orchestrator.RunAsync(
                request.ProjectFolderPath, request.ErrorLog, analysisId, reporter, ct);

            record.ExtractedKeywordsJson = JsonSerializer.Serialize(result.ExtractedKeywords);
            record.RelatedFilesJson = JsonSerializer.Serialize(result.RelatedFiles);
            record.RootCauseAnalysis = result.RootCauseAnalysis;
            record.FixSuggestionsJson = JsonSerializer.Serialize(result.FixSuggestions);
            record.TestScenariosJson = JsonSerializer.Serialize(result.TestScenarios);
            record.PrDescription = result.PrDescription;
            record.Status = AnalysisStatus.Completed;
            record.CompletedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(record);

            await reporter.ReportCompletedAsync(new AnalysisCompletedMessage(analysisId, true, null));
        }
        catch (OperationCanceledException)
        {
            record.Status = AnalysisStatus.Cancelled;
            record.CompletedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(record);

            await reporter.ReportCompletedAsync(new AnalysisCompletedMessage(analysisId, false, "분석이 취소되었습니다."));
        }
        catch (Exception ex)
        {
            record.Status = AnalysisStatus.Failed;
            record.ErrorMessage = ex.Message;
            record.CompletedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(record);

            await reporter.ReportCompletedAsync(new AnalysisCompletedMessage(analysisId, false, ex.Message));
            throw;
        }
    }
}
