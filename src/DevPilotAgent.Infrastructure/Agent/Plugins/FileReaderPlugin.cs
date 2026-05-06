namespace DevPilotAgent.Infrastructure.Agent.Plugins;

using DevPilotAgent.Application.Interfaces;

/// <summary>
/// 여러 파일의 내용을 일괄 읽기하는 플러그인.
/// </summary>
public class FileReaderPlugin
{
    private readonly IFileSystemService _fileSystemService;

    public FileReaderPlugin(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    /// <summary>
    /// 지정된 파일들의 내용을 읽어 딕셔너리로 반환한다.
    /// </summary>
    /// <param name="filePaths">읽을 파일 경로 목록.</param>
    /// <param name="ct">취소 토큰.</param>
    /// <returns>파일 경로를 키, 내용을 값으로 하는 딕셔너리.</returns>
    public async Task<Dictionary<string, string>> ReadFilesAsync(
        List<string> filePaths,
        CancellationToken ct = default)
    {
        return await _fileSystemService.ReadMultipleFilesAsync(filePaths, ct);
    }
}
