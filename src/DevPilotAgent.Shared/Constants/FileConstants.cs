namespace DevPilotAgent.Shared.Constants;

/// <summary>
/// 파일 시스템 접근에 사용되는 제한값과 허용 확장자 상수.
/// </summary>
public static class FileConstants
{
    /// <summary>검색 대상에 포함할 파일 확장자 목록.</summary>
    public static readonly string[] AllowedExtensions = [".cs", ".csproj", ".json", ".xml", ".config", ".razor", ".xaml"];

    /// <summary>파일 검색 시 건너뛸 폴더 이름 목록.</summary>
    public static readonly string[] ExcludedFolders = ["bin", "obj", "node_modules", ".git", ".vs", "packages"];

    /// <summary>단일 파일 최대 읽기 크기 (500KB).</summary>
    public const int MaxFileSizeBytes = 512_000;

    /// <summary>디렉터리 재귀 탐색 최대 깊이.</summary>
    public const int MaxSearchDepth = 10;

    /// <summary>파일 크기 초과 시 읽을 최대 줄 수.</summary>
    public const int MaxFileReadLines = 200;

    /// <summary>검색 결과로 반환할 최대 파일 후보 수.</summary>
    public const int MaxFileCandidates = 10;

    /// <summary>한 번에 읽을 수 있는 최대 파일 수.</summary>
    public const int MaxFilesToRead = 5;

    /// <summary>백업 파일의 확장자.</summary>
    public const string BackupExtension = ".bak";

    /// <summary>대상 파일당 보관할 최대 백업 수.</summary>
    public const int MaxBackupsPerFile = 5;

    /// <summary>다중 파일 읽기 시 총 최대 바이트 수 (1MB).</summary>
    public const int MaxTotalFileReadBytes = 1_048_576;
}
