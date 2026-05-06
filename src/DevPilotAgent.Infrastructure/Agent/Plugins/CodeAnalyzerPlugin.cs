namespace DevPilotAgent.Infrastructure.Agent.Plugins;

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Shared.Constants;
using DevPilotAgent.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

/// <summary>
/// LLM을 활용하여 근본 원인 분석, 수정안 생성, 테스트 시나리오, PR 설명문을 생성하는 플러그인.
/// 모든 LLM 응답은 스트리밍으로 처리되며 버퍼링을 통해 SignalR로 전달된다.
/// </summary>
public class CodeAnalyzerPlugin
{
    private readonly Kernel _kernel;
    private readonly ILogger<CodeAnalyzerPlugin> _logger;

    public CodeAnalyzerPlugin(Kernel kernel, ILogger<CodeAnalyzerPlugin> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    /// <summary>
    /// 에러 로그와 소스 코드를 분석하여 근본 원인을 스트리밍으로 생성한다.
    /// </summary>
    public async Task<string> AnalyzeRootCauseStreamingAsync(
        string errorLog,
        Dictionary<string, string> fileContents,
        Guid analysisId,
        int stepNumber,
        IAgentProgressReporter reporter,
        CancellationToken ct)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            당신은 시니어 소프트웨어 엔지니어입니다.
            에러 로그와 관련 소스 코드를 분석하여 근본 원인을 설명하세요.
            한국어로 상세하게 답변하세요.
            """);

        var trimmedContents = TrimFileContentsToTokenLimit(fileContents, errorLog);
        chatHistory.AddUserMessage($"에러 로그:\n{errorLog}\n\n소스 코드:\n{FormatFileContents(trimmedContents)}");

        return await StreamResponseAsync(chatHistory, analysisId, stepNumber, AgentStepNames.AnalyzeRootCause, reporter, ct);
    }

    /// <summary>
    /// 근본 원인 분석을 바탕으로 코드 수정안을 스트리밍으로 생성한다.
    /// </summary>
    public async Task<List<FixSuggestion>> GenerateFixSuggestionsStreamingAsync(
        string errorLog,
        string rootCauseAnalysis,
        Dictionary<string, string> fileContents,
        Guid analysisId,
        int stepNumber,
        IAgentProgressReporter reporter,
        CancellationToken ct)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            당신은 시니어 소프트웨어 엔지니어입니다.
            에러의 근본 원인과 소스 코드를 바탕으로 수정안을 제안하세요.

            반드시 다음 JSON 배열 형식으로만 응답하세요:
            [
              {
                "targetFilePath": "파일 경로",
                "description": "수정 설명",
                "unifiedDiff": "unified diff 형식",
                "modifiedContent": "수정 후 전체 파일 내용"
              }
            ]
            """);
        chatHistory.AddUserMessage(
            $"에러 로그:\n{errorLog}\n\n원인 분석:\n{rootCauseAnalysis}\n\n소스 코드:\n{FormatFileContents(fileContents)}");

        var responseText = await StreamResponseAsync(chatHistory, analysisId, stepNumber, AgentStepNames.GenerateFix, reporter, ct);

        return ParseFixSuggestions(responseText);
    }

    /// <summary>
    /// 원인 분석과 수정안을 바탕으로 테스트 시나리오를 스트리밍으로 생성한다.
    /// </summary>
    public async Task<List<string>> GenerateTestScenariosStreamingAsync(
        string rootCauseAnalysis,
        List<FixSuggestion> fixSuggestions,
        Guid analysisId,
        int stepNumber,
        IAgentProgressReporter reporter,
        CancellationToken ct)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            당신은 QA 엔지니어입니다.
            에러 원인과 수정안을 바탕으로 테스트 시나리오를 작성하세요.

            반드시 JSON 문자열 배열 형식으로만 응답하세요:
            ["테스트 시나리오 1", "테스트 시나리오 2", ...]
            """);
        chatHistory.AddUserMessage(
            $"원인 분석:\n{rootCauseAnalysis}\n\n수정안:\n{JsonSerializer.Serialize(fixSuggestions.Select(f => f.Description))}");

        var responseText = await StreamResponseAsync(chatHistory, analysisId, stepNumber, AgentStepNames.GenerateTests, reporter, ct);

        return ParseJsonStringArray(responseText);
    }

    /// <summary>
    /// 분석 결과 전체를 바탕으로 PR 설명문 초안을 스트리밍으로 생성한다.
    /// </summary>
    public async Task<string> GeneratePrDescriptionStreamingAsync(
        string rootCauseAnalysis,
        List<FixSuggestion> fixSuggestions,
        List<string> testScenarios,
        Guid analysisId,
        int stepNumber,
        IAgentProgressReporter reporter,
        CancellationToken ct)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            당신은 시니어 개발자입니다.
            원인 분석, 수정안, 테스트 시나리오를 바탕으로 PR 설명문 초안을 Markdown으로 작성하세요.
            섹션: ## 문제, ## 원인, ## 수정 내용, ## 테스트
            """);
        chatHistory.AddUserMessage(
            $"원인 분석:\n{rootCauseAnalysis}\n\n수정안:\n{JsonSerializer.Serialize(fixSuggestions.Select(f => new { f.TargetFilePath, f.Description }))}\n\n테스트 시나리오:\n{JsonSerializer.Serialize(testScenarios)}");

        return await StreamResponseAsync(chatHistory, analysisId, stepNumber, AgentStepNames.GeneratePrDescription, reporter, ct);
    }

    private async Task<string> StreamResponseAsync(
        ChatHistory chatHistory,
        Guid analysisId,
        int stepNumber,
        string stepName,
        IAgentProgressReporter reporter,
        CancellationToken ct)
    {
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var fullResponse = new StringBuilder();
        var chunkBuffer = new StringBuilder();
        var lastFlush = DateTime.UtcNow;

        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory, cancellationToken: ct))
        {
            if (chunk.Content is null) continue;

            fullResponse.Append(chunk.Content);
            chunkBuffer.Append(chunk.Content);

            if (chunkBuffer.Length >= StreamBufferSettings.MinChunkSize
                || (DateTime.UtcNow - lastFlush).TotalMilliseconds >= StreamBufferSettings.FlushIntervalMs)
            {
                await reporter.ReportStreamChunkAsync(new StreamChunkMessage(
                    analysisId, stepNumber, stepName, chunkBuffer.ToString(), false));
                chunkBuffer.Clear();
                lastFlush = DateTime.UtcNow;
            }
        }

        if (chunkBuffer.Length > 0)
        {
            await reporter.ReportStreamChunkAsync(new StreamChunkMessage(
                analysisId, stepNumber, stepName, chunkBuffer.ToString(), false));
        }

        await reporter.ReportStreamChunkAsync(new StreamChunkMessage(
            analysisId, stepNumber, stepName, "", true));

        return fullResponse.ToString();
    }

    private Dictionary<string, string> TrimFileContentsToTokenLimit(
        Dictionary<string, string> fileContents, string errorLog)
    {
        const int maxInputTokens = 100_000;
        const int charsPerToken = 4;
        var availableChars = (maxInputTokens * charsPerToken) - errorLog.Length - 500;

        if (availableChars <= 0)
            return new Dictionary<string, string>();

        var result = new Dictionary<string, string>();
        var usedChars = 0;

        foreach (var (path, content) in fileContents)
        {
            if (usedChars + content.Length > availableChars)
            {
                var remaining = availableChars - usedChars;
                if (remaining > 200)
                {
                    result[path] = content[..Math.Min(content.Length, remaining)] + "\n... (토큰 제한으로 잘림)";
                }
                break;
            }

            result[path] = content;
            usedChars += content.Length;
        }

        if (result.Count < fileContents.Count)
            _logger.LogWarning("토큰 제한으로 {Trimmed}개 파일 내용이 축소됨", fileContents.Count - result.Count);

        return result;
    }

    private static string FormatFileContents(Dictionary<string, string> fileContents)
    {
        var sb = new StringBuilder();
        foreach (var (path, content) in fileContents)
        {
            sb.AppendLine($"--- {path} ---");
            sb.AppendLine(content);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private List<FixSuggestion> ParseFixSuggestions(string responseText)
    {
        var json = CleanJsonResponse(responseText);
        try
        {
            var items = JsonSerializer.Deserialize<List<FixSuggestionRaw>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return items?.Select(i => new FixSuggestion(
                i.TargetFilePath ?? "",
                i.Description ?? "",
                i.UnifiedDiff ?? "",
                i.ModifiedContent ?? "",
                DateTime.UtcNow
            )).ToList() ?? [];
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "수정 제안 JSON 파싱 실패, 원문 보존");
            return [new FixSuggestion("", "파싱 실패 - 원문 참조", responseText, "", DateTime.UtcNow)];
        }
    }

    private List<string> ParseJsonStringArray(string responseText)
    {
        var json = CleanJsonResponse(responseText);
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "테스트 시나리오 JSON 파싱 실패, 원문 보존");
            return [responseText];
        }
    }

    private static string CleanJsonResponse(string text)
    {
        var cleaned = Regex.Replace(text, @"```(?:json)?\s*|\s*```", "").Trim();

        var start = cleaned.IndexOf('[');
        var end = cleaned.LastIndexOf(']');
        if (start >= 0 && end > start)
            return cleaned[start..(end + 1)];

        return cleaned;
    }

    private record FixSuggestionRaw(
        string? TargetFilePath,
        string? Description,
        string? UnifiedDiff,
        string? ModifiedContent);
}
