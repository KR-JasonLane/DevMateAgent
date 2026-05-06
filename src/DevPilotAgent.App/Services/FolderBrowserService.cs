namespace DevPilotAgent.App.Services;

using Microsoft.Win32;

/// <summary>
/// Windows 네이티브 폴더 선택 다이얼로그를 사용하는 <see cref="IFolderBrowserService"/> 구현체.
/// </summary>
public class FolderBrowserService : IFolderBrowserService
{
    /// <inheritdoc/>
    public string? BrowseFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "프로젝트 폴더 선택"
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}
