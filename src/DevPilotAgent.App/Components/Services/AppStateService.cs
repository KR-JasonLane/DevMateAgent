namespace DevPilotAgent.App.Components.Services;

public class AppStateService
{
    private string _projectFolderPath = string.Empty;

    public string ProjectFolderPath
    {
        get => _projectFolderPath;
        set
        {
            _projectFolderPath = value;
            OnProjectFolderChanged?.Invoke(value);
        }
    }

    public event Action<string>? OnProjectFolderChanged;
    public event Action? OnStateChanged;

    public void NotifyStateChanged() => OnStateChanged?.Invoke();
}
