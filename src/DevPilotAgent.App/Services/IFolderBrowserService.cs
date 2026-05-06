namespace DevPilotAgent.App.Services;

/// <summary>
/// 네이티브 폴더 선택 다이얼로그를 추상화하는 인터페이스.
/// </summary>
public interface IFolderBrowserService
{
    /// <summary>
    /// 폴더 선택 다이얼로그를 표시하고 선택된 경로를 반환한다.
    /// </summary>
    /// <returns>선택된 폴더 경로. 취소 시 null.</returns>
    string? BrowseFolder();
}
