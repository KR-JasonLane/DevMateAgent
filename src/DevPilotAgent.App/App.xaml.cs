namespace DevPilotAgent.App;

using System.IO;
using System.Windows;
using System.Windows.Threading;
using DevPilotAgent.App.Components.Services;
using DevPilotAgent.App.Services;
using DevPilotAgent.App.ViewModels;
using DevPilotAgent.App.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

/// <summary>
/// WPF 애플리케이션 진입점.
/// API 서버 프로세스 관리, Serilog 초기화, DI 컨테이너 구성, 전역 예외 처리를 수행한다.
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    private ApiProcessManager? _apiProcessManager;
    private IConfiguration _configuration = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        InitializeLogging();
        RegisterGlobalExceptionHandlers();

        Log.Information("DevPilot Agent 시작");

        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var apiBaseUrl = _configuration["Api:BaseUrl"] ?? "http://localhost:5000";
        var hubPath = _configuration["Api:HubPath"] ?? "/hubs/analysis";

        var apiStarted = await StartApiServerAsync(apiBaseUrl);
        if (!apiStarted)
        {
            Log.Warning("API 서버 시작 실패 — UI만 표시합니다.");
        }

        var services = new ServiceCollection();

        // WPF Blazor
        services.AddWpfBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        // Blazor 서비스
        services.AddSingleton<MarkdownService>();
        services.AddSingleton<AppStateService>();
        services.AddSingleton(new AnalysisHubService($"{apiBaseUrl}{hubPath}"));
        services.AddHttpClient<AnalysisApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        // WPF 서비스
        services.AddSingleton<RecentProjectsService>();
        services.AddTransient<IFolderBrowserService, FolderBrowserService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        Services = services.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        if (mainWindow.DataContext is MainViewModel viewModel)
        {
            viewModel.SetApiConnected(apiStarted);
        }

        Log.Information("MainWindow 표시 완료");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("DevPilot Agent 종료");
        _apiProcessManager?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private async Task<bool> StartApiServerAsync(string apiBaseUrl)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var apiProjectPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "DevPilotAgent.Api"));

        if (!Directory.Exists(apiProjectPath))
        {
            Log.Error("API 프로젝트 경로를 찾을 수 없습니다: {Path}", apiProjectPath);
            return false;
        }

        _apiProcessManager = new ApiProcessManager(apiProjectPath, apiBaseUrl);
        return await _apiProcessManager.StartAsync();
    }

    private static void InitializeLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File(
                "logs/devpilotagent-app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private void RegisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "UI 스레드 미처리 예외");
        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "비동기 작업 미처리 예외");
        e.SetObserved();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            Log.Fatal(ex, "치명적 미처리 예외");
    }
}
