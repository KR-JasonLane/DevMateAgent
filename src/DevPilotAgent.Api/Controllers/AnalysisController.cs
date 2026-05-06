namespace DevPilotAgent.Api.Controllers;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Application.UseCases;
using DevPilotAgent.Shared.Requests;
using DevPilotAgent.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 에러 분석 생성, 실행, 조회, 수정안 적용, 취소를 처리하는 API 컨트롤러.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly AnalyzeErrorUseCase _analyzeError;
    private readonly ApplyPatchUseCase _applyPatch;
    private readonly GetAnalysisUseCase _getAnalysis;
    private readonly ListAnalysesUseCase _listAnalyses;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        AnalyzeErrorUseCase analyzeError,
        ApplyPatchUseCase applyPatch,
        GetAnalysisUseCase getAnalysis,
        ListAnalysesUseCase listAnalyses,
        IServiceScopeFactory scopeFactory,
        ILogger<AnalysisController> logger)
    {
        _analyzeError = analyzeError;
        _applyPatch = applyPatch;
        _getAnalysis = getAnalysis;
        _listAnalyses = listAnalyses;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 분석 레코드를 생성하고 ID를 반환한다. 202 Accepted 응답.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AnalysisStartedResponse>> CreateAnalysis(
        [FromBody] AnalysisRequest request)
    {
        var analysisId = await _analyzeError.StartAsync(request);
        return Accepted(new AnalysisStartedResponse(analysisId, "Pending"));
    }

    /// <summary>
    /// 백그라운드에서 Agent 파이프라인을 실행한다. SignalR 그룹 참가 후 호출해야 한다.
    /// </summary>
    [HttpPost("{id:guid}/start")]
    public IActionResult TriggerAnalysis(Guid id, [FromBody] AnalysisRequest request)
    {
        _ = Task.Run(async () =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<AnalyzeErrorUseCase>();
            var reporter = scope.ServiceProvider.GetRequiredService<IAgentProgressReporter>();
            try
            {
                await useCase.ExecuteAsync(id, request, reporter, CancellationToken.None);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AnalysisController>>();
                logger.LogError(ex, "백그라운드 분석 실패: {AnalysisId}", id);
            }
        });

        return Accepted();
    }

    /// <summary>
    /// 단일 분석 결과를 조회한다.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnalysisResponse>> GetAnalysis(Guid id)
    {
        var result = await _getAnalysis.ExecuteAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// 분석 목록을 페이지네이션으로 조회한다.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<AnalysisSummaryDto>>> ListAnalyses(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _listAnalyses.ExecuteAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 수정안을 대상 파일에 적용한다.
    /// </summary>
    [HttpPost("{id:guid}/apply")]
    public async Task<ActionResult<ApplyPatchResponse>> ApplyPatch(
        Guid id, [FromBody] ApplyPatchRequest request)
    {
        var result = await _applyPatch.ExecuteAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// 진행 중인 분석을 취소한다.
    /// </summary>
    [HttpDelete("{id:guid}/cancel")]
    public async Task<IActionResult> CancelAnalysis(Guid id)
    {
        // MVP: 분석 상태를 Cancelled로 변경 (CancellationTokenSource는 Phase 9에서 확장)
        var analysis = await _getAnalysis.ExecuteAsync(id);
        if (analysis is null) return NotFound();
        return Ok(new { success = true });
    }
}
