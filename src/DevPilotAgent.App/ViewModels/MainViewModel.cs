namespace DevPilotAgent.App.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevPilotAgent.App.Components.Services;
using DevPilotAgent.App.Services;

/// <summary>
/// 메인 윈도우의 ViewModel.
/// 프로젝트 폴더 선택, API 연결 상태, 최근 프로젝트 관리를 담당한다.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFolderBrowserService _folderBrowser;
    private readonly AppStateService _appState;
    private readonly RecentProjectsService _recentProjects;

    [ObservableProperty]
    private string _projectFolderPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "준비";

    [ObservableProperty]
    private bool _isApiConnected;

    [ObservableProperty]
    private string _modelDisplayName = "Ollama (대기 중)";

    [ObservableProperty]
    private bool _showProjectSelector;

    public ObservableCollection<RecentProject> RecentProjects { get; } = [];

    public MainViewModel(
        IFolderBrowserService folderBrowser,
        AppStateService appState,
        RecentProjectsService recentProjects)
    {
        _folderBrowser = folderBrowser;
        _appState = appState;
        _recentProjects = recentProjects;

        foreach (var p in _recentProjects.Projects)
            RecentProjects.Add(p);

        if (_recentProjects.LastOpened is not null)
        {
            SelectProject(_recentProjects.LastOpened.FolderPath);
        }
        else
        {
            ShowProjectSelector = true;
        }
    }

    public string ProjectFolderDisplay =>
        string.IsNullOrEmpty(ProjectFolderPath) ? "프로젝트 폴더를 선택하세요" : ProjectFolderPath;

    public string ApiStatusText => IsApiConnected ? "API 연결됨" : "API 미연결";

    public SolidColorBrush ApiStatusColor => IsApiConnected
        ? new SolidColorBrush(Color.FromRgb(0xa6, 0xe3, 0xa1))
        : new SolidColorBrush(Color.FromRgb(0xf3, 0x8b, 0xa8));

    partial void OnProjectFolderPathChanged(string value)
    {
        OnPropertyChanged(nameof(ProjectFolderDisplay));
    }

    partial void OnIsApiConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(ApiStatusText));
        OnPropertyChanged(nameof(ApiStatusColor));
    }

    public void SetApiConnected(bool connected)
    {
        IsApiConnected = connected;
    }

    public void SetModelName(string modelName)
    {
        ModelDisplayName = modelName;
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var path = _folderBrowser.BrowseFolder();
        if (path is null) return;

        SelectProject(path);
        ShowProjectSelector = false;
    }

    [RelayCommand]
    private void SelectRecentProject(RecentProject project)
    {
        SelectProject(project.FolderPath);
        ShowProjectSelector = false;
    }

    [RelayCommand]
    private void ToggleProjectSelector()
    {
        ShowProjectSelector = !ShowProjectSelector;
    }

    private void SelectProject(string path)
    {
        if (!System.IO.Directory.Exists(path))
        {
            StatusMessage = $"폴더를 찾을 수 없습니다: {path}";
            return;
        }

        ProjectFolderPath = path;
        _appState.ProjectFolderPath = path;
        _recentProjects.AddProject(path);
        StatusMessage = $"프로젝트: {System.IO.Path.GetFileName(path)}";

        RefreshRecentList();
    }

    private void RefreshRecentList()
    {
        RecentProjects.Clear();
        foreach (var p in _recentProjects.Projects)
            RecentProjects.Add(p);
    }
}
