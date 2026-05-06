namespace DevPilotAgent.Application.UseCases;

using System.Text.Json;
using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Domain.Entities;
using DevPilotAgent.Domain.Enums;
using DevPilotAgent.Shared.DTOs;
using DevPilotAgent.Shared.Requests;

/// <summary>
/// 에러 분석을 생성하고 Agent 파이프라인을 실행하는 UseCase.
/// 분석의 생성(StartAsync)과 실행(ExecuteAsync)이 분리되어
/// 클라이언트가 SignalR 그룹에 먼저 참가할 수 있도록 한다.
/// </summary>
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

    /// <summary>
    /// 분석 레코드를 생성하고 ID를 반환한다. 실제 실행은 하지 않는다.
    /// </summary>
    /// <param name="request">분석 요청.</param>
    /// <returns>생성된 분석 레코드의 ID.</returns>
    /// <exception cref="ArgumentException">폴더 경로가 유효하지 않거나 에러 로그가 비어 있는 경우.</exception>
    /// <exception cref="InvalidOperationException">해당 폴더에 이미 진행 중인 분석이 있는 경우.</exception>
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

    /// <summary>
    /// Agent 파이프라인을 실행하고 결과를 분석 레코드에 저장한다.
    /// 취소 또는 실패 시 레코드 상태를 적절히 갱신한다.
    /// </summary>
    /// <param name="analysisId">실행할 분석 ID.</param>
    /// <param name="request">분석 요청 (에러 로그와 프로젝트 경로).</param>
    /// <param name="reporter">진행 상황 보고자.</param>
    /// <param name="ct">취소 토큰.</param>
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
