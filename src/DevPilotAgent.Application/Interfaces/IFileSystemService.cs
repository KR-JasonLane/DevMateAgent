namespace DevPilotAgent.Application.Interfaces;

/// <summary>
/// 파일 시스템 접근(검색, 읽기, 패치 적용)을 추상화하는 서비스 인터페이스.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// 폴더 내에서 키워드와 관련도가 높은 파일을 검색한다.
    /// </summary>
    /// <param name="folderPath">검색 시작 폴더 경로.</param>
    /// <param name="keywords">검색 키워드 목록.</param>
    /// <param name="ct">취소 토큰.</param>
    /// <returns>관련도 순으로 정렬된 파일 경로 목록.</returns>
    Task<List<string>> SearchFilesAsync(string folderPath, List<string> keywords, CancellationToken ct = default);

    /// <summary>
    /// 단일 파일의 내용을 읽는다. 파일 크기가 제한을 초과하면 앞부분만 반환한다.
    /// </summary>
    /// <param name="filePath">읽을 파일의 절대 경로.</param>
    /// <param name="ct">취소 토큰.</param>
    /// <returns>파일 내용 문자열.</returns>
    Task<string> ReadFileAsync(string filePath, CancellationToken ct = default);

    /// <summary>
    /// 여러 파일의 내용을 한 번에 읽는다. 총 크기 제한을 적용한다.
    /// </summary>
    /// <param name="filePaths">읽을 파일 경로 목록.</param>
    /// <param name="ct">취소 토큰.</param>
    /// <returns>파일 경로를 키, 내용을 값으로 하는 딕셔너리.</returns>
    Task<Dictionary<string, string>> ReadMultipleFilesAsync(List<string> filePaths, CancellationToken ct = default);

    /// <summary>
    /// 수정안을 대상 파일에 적용한다. 원본 파일은 백업한다.
    /// </summary>
    /// <param name="targetFilePath">수정 대상 파일 경로.</param>
    /// <param name="modifiedContent">수정 후 전체 파일 내용.</param>
    /// <param name="projectFolderPath">프로젝트 폴더 경로 (경로 검증용).</param>
    /// <param name="expectedLastModifiedUtc">분석 시점의 파일 수정 시각 (충돌 감지용).</param>
    /// <returns>생성된 백업 파일의 경로.</returns>
    /// <exception cref="InvalidOperationException">파일이 분석 이후 변경되었거나 프로젝트 폴더 외부인 경우.</exception>
    Task<string> ApplyPatchAsync(string targetFilePath, string modifiedContent, string projectFolderPath, DateTime expectedLastModifiedUtc);

    /// <summary>
    /// 폴더 경로가 유효한지 검증한다 (존재 여부, UNC 경로 차단).
    /// </summary>
    /// <param name="folderPath">검증할 폴더 경로.</param>
    /// <returns>유효하면 true.</returns>
    bool ValidateFolderPath(string folderPath);

    /// <summary>
    /// 파일 경로가 지정된 폴더 내에 있는지 확인한다 (Path Traversal 방지).
    /// </summary>
    /// <param name="filePath">확인할 파일 경로.</param>
    /// <param name="folderPath">기준 폴더 경로.</param>
    /// <returns>폴더 내에 있으면 true.</returns>
    bool IsPathWithinFolder(string filePath, string folderPath);

    /// <summary>
    /// 대상 파일의 오래된 백업 파일을 정리한다.
    /// </summary>
    /// <param name="targetFilePath">원본 파일 경로.</param>
    /// <param name="maxBackups">보관할 최대 백업 수.</param>
    void CleanupOldBackups(string targetFilePath, int maxBackups = 5);
}
