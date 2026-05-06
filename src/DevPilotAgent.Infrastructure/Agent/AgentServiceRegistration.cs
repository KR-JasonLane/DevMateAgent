namespace DevPilotAgent.Infrastructure.Agent;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Infrastructure.Agent.Orchestrator;
using DevPilotAgent.Infrastructure.Agent.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

/// <summary>
/// Semantic Kernel과 Agent 플러그인의 DI 등록.
/// OpenAI API 키가 없으면 즉시 실패한다 (fail-fast).
/// </summary>
public static class AgentServiceRegistration
{
    /// <summary>
    /// Semantic Kernel, 플러그인, 오케스트레이터를 DI 컨테이너에 등록한다.
    /// </summary>
    /// <param name="services">서비스 컬렉션.</param>
    /// <param name="configuration">앱 설정 (OpenAI:ApiKey 필수).</param>
    /// <returns>체이닝을 위한 서비스 컬렉션.</returns>
    /// <exception cref="InvalidOperationException">API 키가 설정되지 않은 경우.</exception>
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o";
        var endpoint = configuration["OpenAI:Endpoint"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "OpenAI API 키가 설정되지 않았습니다. appsettings.Development.json 또는 User Secrets에 'OpenAI:ApiKey'를 설정하세요.");

        var builder = Kernel.CreateBuilder();

        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(endpoint) };
            builder.AddOpenAIChatCompletion(modelId, apiKey, httpClient: httpClient);
        }
        else
        {
            builder.AddOpenAIChatCompletion(modelId, apiKey);
        }

        var kernel = builder.Build();

        services.AddSingleton(kernel);

        // Plugins (stateless - Singleton 안전)
        services.AddSingleton<ErrorParserPlugin>();
        services.AddSingleton<FileSearchPlugin>();
        services.AddSingleton<FileReaderPlugin>();
        services.AddSingleton<CodeAnalyzerPlugin>();

        // Orchestrator
        services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

        // IAgentProgressReporter는 Api 프로젝트에서 등록 (레이어 역의존 방지)

        return services;
    }
}
