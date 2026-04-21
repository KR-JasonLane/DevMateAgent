namespace DevPilotAgent.Shared.Constants;

public static class FileConstants
{
    public static readonly string[] AllowedExtensions = [".cs", ".csproj", ".json", ".xml", ".config", ".razor", ".xaml"];
    public static readonly string[] ExcludedFolders = ["bin", "obj", "node_modules", ".git", ".vs", "packages"];
    public const int MaxFileSizeBytes = 512_000;
    public const int MaxSearchDepth = 10;
    public const int MaxFileReadLines = 200;
    public const int MaxFileCandidates = 10;
    public const int MaxFilesToRead = 5;
    public const string BackupExtension = ".bak";
    public const int MaxBackupsPerFile = 5;
    public const int MaxTotalFileReadBytes = 1_048_576;
}
