namespace DevPilotAgent.App.Components.Services;

/// <summary>
/// WPF와 Blazor 간 공유 상태를 관리하는 싱글톤 서비스.
/// 프로젝트 폴더 경로 등 공통 상태와 변경 이벤트를 제공한다.
/// </summary>
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
