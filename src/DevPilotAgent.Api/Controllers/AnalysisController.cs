namespace DevPilotAgent.Api.Controllers;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Application.UseCases;
using DevPilotAgent.Shared.Requests;
using DevPilotAgent.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<ActionResult<AnalysisStartedResponse>> CreateAnalysis(
        [FromBody] AnalysisRequest request)
    {
        var analysisId = await _analyzeError.StartAsync(request);
        return Accepted(new AnalysisStartedResponse(analysisId, "Pending"));
    }

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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnalysisResponse>> GetAnalysis(Guid id)
    {
        var result = await _getAnalysis.ExecuteAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AnalysisSummaryDto>>> ListAnalyses(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _listAnalyses.ExecuteAsync(page, pageSize);
        return Ok(result);
    }

    [HttpPost("{id:guid}/apply")]
    public async Task<ActionResult<ApplyPatchResponse>> ApplyPatch(
        Guid id, [FromBody] ApplyPatchRequest request)
    {
        var result = await _applyPatch.ExecuteAsync(request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/cancel")]
    public async Task<IActionResult> CancelAnalysis(Guid id)
    {
        // MVP: 분석 상태를 Cancelled로 변경 (CancellationTokenSource는 Phase 9에서 확장)
        var analysis = await _getAnalysis.ExecuteAsync(id);
        if (analysis is null) return NotFound();
        return Ok(new { success = true });
    }
}
