namespace DevPilotAgent.Infrastructure;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Infrastructure.FileSystem;
using DevPilotAgent.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Infrastructure 계층의 DI 등록 확장 메서드 모음.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Infrastructure 계층의 리포지토리와 파일 시스템 서비스를 DI 컨테이너에 등록한다.
    /// </summary>
    /// <param name="services">서비스 컬렉션.</param>
    /// <param name="configuration">앱 설정.</param>
    /// <returns>체이닝을 위한 서비스 컬렉션.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAnalysisRepository, AnalysisRepository>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        return services;
    }
}
