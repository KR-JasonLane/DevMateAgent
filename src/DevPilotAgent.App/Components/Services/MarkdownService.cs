namespace DevPilotAgent.App.Components.Services;

using Markdig;

/// <summary>
/// Markdown 텍스트를 HTML로 변환하는 서비스.
/// Markdig의 고급 확장(테이블, 코드 하이라이팅 등)을 활성화한다.
/// </summary>
public class MarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        return Markdown.ToHtml(markdown, _pipeline);
    }
}
