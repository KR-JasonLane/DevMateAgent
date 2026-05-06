namespace DevPilotAgent.Infrastructure.FileSystem;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Shared.Constants;
using Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="IFileSystemService"/> 구현체.
/// 파일 검색, 읽기, 패치 적용, 백업 관리를 수행한다.
/// </summary>
public class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<string>> SearchFilesAsync(string folderPath, List<string> keywords, CancellationToken ct = default)
    {
        var results = new List<(string Path, int Score)>();
        var normalizedKeywords = keywords.Select(k => k.ToLowerInvariant()).ToList();

        await Task.Run(() => SearchDirectory(folderPath, normalizedKeywords, results, 0), ct);

        return results
            .OrderByDescending(r => r.Score)
            .Take(FileConstants.MaxFileCandidates)
            .Select(r => r.Path)
            .ToList();
    }

    private void SearchDirectory(string dirPath, List<string> keywords, List<(string Path, int Score)> results, int depth)
    {
        if (depth > FileConstants.MaxSearchDepth) return;

        try
        {
            foreach (var dir in Directory.GetDirectories(dirPath))
            {
                var dirName = Path.GetFileName(dir);
                if (FileConstants.ExcludedFolders.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                    continue;

                SearchDirectory(dir, keywords, results, depth + 1);
            }

            foreach (var file in Directory.GetFiles(dirPath))
            {
                var ext = Path.GetExtension(file);
                if (!FileConstants.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                    continue;

                var fileInfo = new FileInfo(file);
                if (fileInfo.Length > FileConstants.MaxFileSizeBytes)
                    continue;

                var score = CalculateRelevanceScore(file, keywords);
                if (score > 0)
                    results.Add((file, score));
            }
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("접근 권한 없음: {Directory}", dirPath);
        }
    }

    private static int CalculateRelevanceScore(string filePath, List<string> keywords)
    {
        var score = 0;
        var fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();

        foreach (var keyword in keywords)
        {
            if (fileName.Contains(keyword))
                score += 2;
        }

        try
        {
            using var reader = new StreamReader(filePath);
            var lineCount = 0;
            while (reader.ReadLine() is { } line && lineCount < 100)
            {
                var lowerLine = line.ToLowerInvariant();
                foreach (var keyword in keywords)
                {
                    if (lowerLine.Contains(keyword))
                    {
                        score += 1;
                        break;
                    }
                }
                lineCount++;
            }
        }
        catch
        {
            // 파일 읽기 실패 시 파일명 매칭 점수만 사용
        }

        return score;
    }

    /// <inheritdoc/>
    public async Task<string> ReadFileAsync(string filePath, CancellationToken ct = default)
    {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > FileConstants.MaxFileSizeBytes)
        {
            var lines = new List<string>();
            using var reader = new StreamReader(filePath);
            for (var i = 0; i < FileConstants.MaxFileReadLines && await reader.ReadLineAsync(ct) is { } line; i++)
            {
                lines.Add(line);
            }
            return string.Join(Environment.NewLine, lines);
        }

        return await File.ReadAllTextAsync(filePath, ct);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, string>> ReadMultipleFilesAsync(List<string> filePaths, CancellationToken ct = default)
    {
        var result = new Dictionary<string, string>();
        var totalBytes = 0L;

        foreach (var filePath in filePaths.Take(FileConstants.MaxFilesToRead))
        {
            ct.ThrowIfCancellationRequested();

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) continue;

            totalBytes += Math.Min(fileInfo.Length, FileConstants.MaxFileSizeBytes);
            if (totalBytes > FileConstants.MaxTotalFileReadBytes)
                break;

            var content = await ReadFileAsync(filePath, ct);
            result[filePath] = content;
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<string> ApplyPatchAsync(string targetFilePath, string modifiedContent, string projectFolderPath, DateTime expectedLastModifiedUtc)
    {
        if (!IsPathWithinFolder(targetFilePath, projectFolderPath))
            throw new InvalidOperationException($"대상 파일이 프로젝트 폴더 외부에 있습니다: {targetFilePath}");

        if (!File.Exists(targetFilePath))
            throw new FileNotFoundException($"대상 파일을 찾을 수 없습니다: {targetFilePath}");

        var actualLastModified = File.GetLastWriteTimeUtc(targetFilePath);
        if (Math.Abs((actualLastModified - expectedLastModifiedUtc).TotalSeconds) > 1)
            throw new InvalidOperationException("파일이 분석 이후 변경되었습니다. 다시 분석해주세요.");

        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupPath = $"{targetFilePath}.{timestamp}{FileConstants.BackupExtension}";
        File.Copy(targetFilePath, backupPath, overwrite: true);

        await File.WriteAllTextAsync(targetFilePath, modifiedContent);

        CleanupOldBackups(targetFilePath);

        return backupPath;
    }

    /// <inheritdoc/>
    public bool ValidateFolderPath(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return false;

        if (folderPath.StartsWith(@"\\", StringComparison.Ordinal))
            return false;

        return Directory.Exists(folderPath);
    }

    /// <inheritdoc/>
    public bool IsPathWithinFolder(string filePath, string folderPath)
    {
        var fullFilePath = Path.GetFullPath(filePath);
        var fullFolderPath = Path.GetFullPath(folderPath);

        if (!fullFolderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            fullFolderPath += Path.DirectorySeparatorChar;

        return fullFilePath.StartsWith(fullFolderPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public void CleanupOldBackups(string targetFilePath, int maxBackups = 5)
    {
        var directory = Path.GetDirectoryName(targetFilePath);
        if (directory is null) return;

        var fileName = Path.GetFileName(targetFilePath);
        var backupPattern = $"{fileName}.*{FileConstants.BackupExtension}";

        var backups = Directory.GetFiles(directory, backupPattern)
            .OrderByDescending(f => f)
            .Skip(maxBackups)
            .ToList();

        foreach (var oldBackup in backups)
        {
            try
            {
                File.Delete(oldBackup);
                _logger.LogInformation("오래된 백업 삭제: {BackupPath}", oldBackup);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "백업 삭제 실패: {BackupPath}", oldBackup);
            }
        }
    }
}
