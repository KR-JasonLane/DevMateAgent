namespace DevPilotAgent.App.Services;

using System.IO;
using System.Text.Json;
using Serilog;

/// <summary>
/// 최근 열어본 프로젝트 폴더 목록을 로컬 JSON 파일로 관리하는 서비스.
/// 최대 10개 항목을 유지하며 최근 사용 순으로 정렬한다.
/// </summary>
public class RecentProjectsService
{
    private const int MaxRecentProjects = 10;
    private readonly string _filePath;
    private List<RecentProject> _projects = [];

    public RecentProjectsService()
    {
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevPilotAgent");
        Directory.CreateDirectory(appDataDir);
        _filePath = Path.Combine(appDataDir, "recent-projects.json");
        Load();
    }

    public IReadOnlyList<RecentProject> Projects => _projects.AsReadOnly();

    public RecentProject? LastOpened => _projects.FirstOrDefault();

    public void AddProject(string folderPath)
    {
        var existing = _projects.FindIndex(p =>
            string.Equals(p.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase));

        if (existing >= 0)
        {
            _projects.RemoveAt(existing);
        }

        var projectName = Path.GetFileName(folderPath) ?? folderPath;
        _projects.Insert(0, new RecentProject(folderPath, projectName, DateTime.Now));

        if (_projects.Count > MaxRecentProjects)
        {
            _projects = _projects.Take(MaxRecentProjects).ToList();
        }

        Save();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _projects = JsonSerializer.Deserialize<List<RecentProject>>(json) ?? [];
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "최근 프로젝트 목록 로드 실패");
            _projects = [];
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_projects, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "최근 프로젝트 목록 저장 실패");
        }
    }
}

/// <summary>
/// 최근 프로젝트 항목.
/// </summary>
/// <param name="FolderPath">프로젝트 폴더 절대 경로.</param>
/// <param name="ProjectName">폴더명에서 추출한 프로젝트 이름.</param>
/// <param name="LastOpenedAt">마지막으로 열어본 시각.</param>
public record RecentProject(string FolderPath, string ProjectName, DateTime LastOpenedAt);
