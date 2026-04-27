namespace DevPilotAgent.App;

using System.Windows;
using DevPilotAgent.App.Components.Services;
using DevPilotAgent.App.Services;
using DevPilotAgent.App.ViewModels;
using DevPilotAgent.App.Views;
using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        // WPF Blazor
        services.AddWpfBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        // Blazor 서비스
        services.AddSingleton<AppStateService>();
        services.AddSingleton<AnalysisHubService>();
        services.AddHttpClient<AnalysisApiService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        // WPF 서비스
        services.AddTransient<IFolderBrowserService, FolderBrowserService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        Services = services.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
