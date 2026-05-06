namespace DevPilotAgent.Infrastructure.Agent.Plugins;

using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

/// <summary>
/// 에러 로그에서 핵심 키워드(파일명, 클래스명, 예외 타입 등)를 LLM으로 추출하는 플러그인.
/// Singleton으로 등록되며 상태를 보유하지 않는다.
/// </summary>
public class ErrorParserPlugin
{
    private readonly Kernel _kernel;
    private readonly ILogger<ErrorParserPlugin> _logger;

    public ErrorParserPlugin(Kernel kernel, ILogger<ErrorParserPlugin> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    /// <summary>
    /// 에러 로그에서 키워드를 추출한다.
    /// </summary>
    /// <param name="errorLog">분석할 에러 로그 원문.</param>
    /// <param name="ct">취소 토큰.</param>
    /// <returns>추출된 키워드 문자열 목록.</returns>
    public async Task<List<string>> ExtractKeywordsAsync(string errorLog, CancellationToken ct = default)
    {
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            에러 로그에서 다음 키워드를 추출하라: 파일명, 클래스명, 메서드명, 예외 타입, 네임스페이스.
            JSON 문자열 배열로만 응답하라. 다른 텍스트는 포함하지 마라.
            예시: ["NullReferenceException", "UserService", "GetUser"]
            """);
        chatHistory.AddUserMessage(errorLog);

        var response = await chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var content = response.Content ?? "[]";

        return ParseJsonArray(content);
    }

    private List<string> ParseJsonArray(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(content) ?? [];
        }
        catch (JsonException)
        {
            _logger.LogWarning("JSON 파싱 실패, 코드블록 제거 후 재시도");
            var cleaned = Regex.Replace(content, @"```(?:json)?\s*|\s*```", "").Trim();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(cleaned) ?? [];
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "키워드 추출 JSON 파싱 최종 실패");
                return [];
            }
        }
    }
}
