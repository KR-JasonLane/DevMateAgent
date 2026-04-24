namespace DevPilotAgent.Application;

using DevPilotAgent.Application.Mappers;
using DevPilotAgent.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
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
