namespace DevPilotAgent.Application;

using DevPilotAgent.Application.Mappers;
using DevPilotAgent.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Application 계층의 DI 등록 확장 메서드 모음.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Application 계층의 UseCase와 Mapper를 DI 컨테이너에 등록한다.
    /// </summary>
    /// <param name="services">서비스 컬렉션.</param>
    /// <returns>체이닝을 위한 서비스 컬렉션.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<AnalyzeErrorUseCase>();
        services.AddScoped<ApplyPatchUseCase>();
        services.AddScoped<GetAnalysisUseCase>();
        services.AddScoped<ListAnalysesUseCase>();
        services.AddSingleton<AnalysisMapper>();
        return services;
    }
}
