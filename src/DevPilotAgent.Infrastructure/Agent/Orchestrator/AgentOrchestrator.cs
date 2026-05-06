namespace DevPilotAgent.Infrastructure.Agent.Orchestrator;

using System.Text.Json;
using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Application.Models;
using DevPilotAgent.Infrastructure.Agent.Plugins;
using DevPilotAgent.Shared.Constants;
using DevPilotAgent.Shared.DTOs;
using Microsoft.Extensions.Logging;

/// <summary>
/// 7단계 Agent 파이프라인을 순차적으로 실행하는 오케스트레이터.
/// 각 단계의 시작/완료를 <see cref="IAgentProgressReporter"/>를 통해 보고한다.
/// </summary>
public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly ErrorParserPlugin _errorParser;
    private readonly FileSearchPlugin _fileSearch;
    private readonly FileReaderPlugin _fileReader;
    private readonly CodeAnalyzerPlugin _codeAnalyzer;
    private readonly ILogger<AgentOrchestrator> _logger;

    public AgentOrchestrator(
        ErrorParserPlugin errorParser,
        FileSearchPlugin fileSearch,
        FileReaderPlugin fileReader,
        CodeAnalyzerPlugin codeAnalyzer,
        ILogger<AgentOrchestrator> logger)
    {
        _errorParser = errorParser;
        _fileSearch = fileSearch;
        _fileReader = fileReader;
        _codeAnalyzer = codeAnalyzer;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AgentResult> RunAsync(
        string projectFolderPath,
        string errorLog,
        Guid analysisId,
        IAgentProgressReporter reporter,
        CancellationToken ct)
    {
        var result = new AgentResult();

        // Step 1: 키워드 추출
        await ReportStepStarted(reporter, analysisId, 1, AgentStepNames.ExtractKeywords, "에러 로그에서 키워드를 추출하고 있습니다...");
        result.ExtractedKeywords = await _errorParser.ExtractKeywordsAsync(errorLog, ct);
        await ReportStepCompleted(reporter, analysisId, 1, AgentStepNames.ExtractKeywords,
            $"{result.ExtractedKeywords.Count}개 키워드 추출 완료",
            JsonSerializer.Serialize(result.ExtractedKeywords));

        // Step 2: 파일 검색
        await ReportStepStarted(reporter, analysisId, 2, AgentStepNames.SearchFiles, "관련 파일을 검색하고 있습니다...");
        result.RelatedFiles = await _fileSearch.SearchFilesAsync(projectFolderPath, result.ExtractedKeywords, ct);
        await ReportStepCompleted(reporter, analysisId, 2, AgentStepNames.SearchFiles,
            $"{result.RelatedFiles.Count}개 관련 파일 발견",
            JsonSerializer.Serialize(result.RelatedFiles));

        // Step 3: 파일 읽기
        await ReportStepStarted(reporter, analysisId, 3, AgentStepNames.ReadFiles, "파일 내용을 읽고 있습니다...");
        var fileContents = await _fileReader.ReadFilesAsync(
            result.RelatedFiles.Select(f => f.FilePath).ToList(), ct);
        await ReportStepCompleted(reporter, analysisId, 3, AgentStepNames.ReadFiles,
            $"{fileContents.Count}개 파일 읽기 완료", null);

        // Step 4: LLM 원인 분석 (스트리밍)
        await ReportStepStarted(reporter, analysisId, 4, AgentStepNames.AnalyzeRootCause, "원인을 분석하고 있습니다...");
        result.RootCauseAnalysis = await _codeAnalyzer.AnalyzeRootCauseStreamingAsync(
            errorLog, fileContents, analysisId, 4, reporter, ct);
        await ReportStepCompleted(reporter, analysisId, 4, AgentStepNames.AnalyzeRootCause, "원인 분석 완료", null);

        // Step 5: 수정 제안 (스트리밍)
        await ReportStepStarted(reporter, analysisId, 5, AgentStepNames.GenerateFix, "수정 코드를 생성하고 있습니다...");
        result.FixSuggestions = await _codeAnalyzer.GenerateFixSuggestionsStreamingAsync(
            errorLog, result.RootCauseAnalysis, fileContents, analysisId, 5, reporter, ct);
        await ReportStepCompleted(reporter, analysisId, 5, AgentStepNames.GenerateFix,
            $"{result.FixSuggestions.Count}개 수정 제안 생성", null);

        // Step 6: 테스트 시나리오 (스트리밍)
        await ReportStepStarted(reporter, analysisId, 6, AgentStepNames.GenerateTests, "테스트 시나리오를 작성하고 있습니다...");
        result.TestScenarios = await _codeAnalyzer.GenerateTestScenariosStreamingAsync(
            result.RootCauseAnalysis, result.FixSuggestions, analysisId, 6, reporter, ct);
        await ReportStepCompleted(reporter, analysisId, 6, AgentStepNames.GenerateTests,
            $"{result.TestScenarios.Count}개 시나리오 생성", null);

        // Step 7: PR 설명문 (스트리밍)
        await ReportStepStarted(reporter, analysisId, 7, AgentStepNames.GeneratePrDescription, "PR 설명문을 작성하고 있습니다...");
        result.PrDescription = await _codeAnalyzer.GeneratePrDescriptionStreamingAsync(
            result.RootCauseAnalysis, result.FixSuggestions, result.TestScenarios,
            analysisId, 7, reporter, ct);
        await ReportStepCompleted(reporter, analysisId, 7, AgentStepNames.GeneratePrDescription, "PR 설명문 작성 완료", null);

        return result;
    }

    private static Task ReportStepStarted(IAgentProgressReporter reporter, Guid analysisId, int step, string stepName, string message)
    {
        return reporter.ReportStepAsync(new AgentStepMessage(
            analysisId, step, stepName, "Started", message, null, DateTime.UtcNow));
    }

    private static Task ReportStepCompleted(IAgentProgressReporter reporter, Guid analysisId, int step, string stepName, string message, string? dataJson)
    {
        return reporter.ReportStepAsync(new AgentStepMessage(
            analysisId, step, stepName, "Completed", message, dataJson, DateTime.UtcNow));
    }
}
