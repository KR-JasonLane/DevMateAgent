namespace DevPilotAgent.Infrastructure.Agent.Plugins;

using DevPilotAgent.Application.Interfaces;

public class FileReaderPlugin
{
    private readonly IFileSystemService _fileSystemService;

    public FileReaderPlugin(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public async Task<Dictionary<string, string>> ReadFilesAsync(
        List<string> filePaths,
        CancellationToken ct = default)
    {
        return await _fileSystemService.ReadMultipleFilesAsync(filePaths, ct);
    }
}
