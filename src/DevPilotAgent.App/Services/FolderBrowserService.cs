namespace DevPilotAgent.App.Services;

using Microsoft.Win32;

public class FolderBrowserService : IFolderBrowserService
{
    public string? BrowseFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "프로젝트 폴더 선택"
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}
