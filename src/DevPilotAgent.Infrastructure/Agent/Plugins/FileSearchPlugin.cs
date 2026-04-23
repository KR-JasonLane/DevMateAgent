namespace DevPilotAgent.Infrastructure.Agent.Plugins;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Shared.DTOs;

public class FileSearchPlugin
{
    private readonly IFileSystemService _fileSystemService;

    public FileSearchPlugin(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

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
