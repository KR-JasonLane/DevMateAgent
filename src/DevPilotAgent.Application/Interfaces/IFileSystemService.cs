namespace DevPilotAgent.Application.Interfaces;

public interface IFileSystemService
{
    Task<List<string>> SearchFilesAsync(string folderPath, List<string> keywords, CancellationToken ct = default);
    Task<string> ReadFileAsync(string filePath, CancellationToken ct = default);
    Task<Dictionary<string, string>> ReadMultipleFilesAsync(List<string> filePaths, CancellationToken ct = default);
    Task<string> ApplyPatchAsync(string targetFilePath, string modifiedContent, string projectFolderPath, DateTime expectedLastModifiedUtc);
    bool ValidateFolderPath(string folderPath);
    bool IsPathWithinFolder(string filePath, string folderPath);
    void CleanupOldBackups(string targetFilePath, int maxBackups = 5);
}
