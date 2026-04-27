namespace DevPilotAgent.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevPilotAgent.App.Components.Services;
using DevPilotAgent.App.Services;

public partial class MainViewModel : ObservableObject
{
    private readonly IFolderBrowserService _folderBrowser;
    private readonly AppStateService _appState;

    [ObservableProperty]
    private string _projectFolderPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "준비";

    public MainViewModel(IFolderBrowserService folderBrowser, AppStateService appState)
    {
        _folderBrowser = folderBrowser;
        _appState = appState;
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var path = _folderBrowser.BrowseFolder();
        if (path is null) return;

        ProjectFolderPath = path;
        _appState.ProjectFolderPath = path;
        StatusMessage = $"프로젝트: {path}";
    }
}
