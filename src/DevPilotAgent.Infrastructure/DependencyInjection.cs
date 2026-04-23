namespace DevPilotAgent.Infrastructure;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Infrastructure.FileSystem;
using DevPilotAgent.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAnalysisRepository, AnalysisRepository>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        return services;
    }
}
