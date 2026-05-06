namespace DevPilotAgent.Infrastructure.Agent.Plugins;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Shared.DTOs;

/// <summary>
/// 키워드 기반으로 프로젝트 내 관련 파일을 검색하고 관련도 점수를 부여하는 플러그인.
/// </summary>
public class FileSearchPlugin
{
    private readonly IFileSystemService _fileSystemService;

    public FileSearchPlugin(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    /// <summary>
    /// 프로젝트 폴더에서 키워드와 매칭되는 파일을 검색한다.
    /// </summary>
    /// <param name="projectFolderPath">검색 대상 프로젝트 폴더 경로.</param>
    /// <param name="keywords">검색 키워드 목록.</param>
    /// <param name="ct">취소 토큰.</param>
    /// <returns>관련도 순으로 정렬된 파일 후보 목록.</returns>
    public async Task<List<FileCandidate>> SearchFilesAsync(
        string projectFolderPath,
        List<string> keywords,
        CancellationToken ct = default)
    {
        var filePaths = await _fileSystemService.SearchFilesAsync(projectFolderPath, keywords, ct);

        return filePaths.Select((path, index) => new FileCandidate(
            FilePath: path,
            MatchReason: $"키워드 매칭: {string.Join(", ", keywords.Take(3))}",
            RelevanceScore: Math.Round(1.0 - (index * 0.1), 2)
        )).ToList();
    }
}
