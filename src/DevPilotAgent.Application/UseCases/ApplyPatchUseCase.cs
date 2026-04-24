namespace DevPilotAgent.Application.UseCases;

using System.Text.Json;
using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Domain.Enums;
using DevPilotAgent.Shared.DTOs;
using DevPilotAgent.Shared.Requests;
using DevPilotAgent.Shared.Responses;

public class ApplyPatchUseCase
{
    private readonly IAnalysisRepository _repository;
    private readonly IFileSystemService _fileSystemService;

    public ApplyPatchUseCase(IAnalysisRepository repository, IFileSystemService fileSystemService)
    {
        _repository = repository;
        _fileSystemService = fileSystemService;
    }

    public async Task<ApplyPatchResponse> ExecuteAsync(ApplyPatchRequest request)
    {
        var record = await _repository.GetByIdAsync(request.AnalysisId)
            ?? throw new InvalidOperationException("분석 기록을 찾을 수 없습니다.");

        if (record.Status != AnalysisStatus.Completed)
            throw new InvalidOperationException("완료된 분석에 대해서만 수정안을 적용할 수 있습니다.");

        var fixSuggestions = JsonSerializer.Deserialize<List<FixSuggestion>>(record.FixSuggestionsJson)
            ?? throw new InvalidOperationException("수정 제안을 파싱할 수 없습니다.");

        if (request.PatchIndex < 0 || request.PatchIndex >= fixSuggestions.Count)
            throw new ArgumentException($"유효하지 않은 PatchIndex입니다: {request.PatchIndex}");

        var fix = fixSuggestions[request.PatchIndex];

        if (!_fileSystemService.IsPathWithinFolder(request.TargetFilePath, record.ProjectFolderPath))
            throw new InvalidOperationException("대상 파일이 프로젝트 폴더 외부에 있습니다.");

        var backupPath = await _fileSystemService.ApplyPatchAsync(
            request.TargetFilePath, fix.ModifiedContent, record.ProjectFolderPath, fix.FileLastModifiedUtc);

        return new ApplyPatchResponse(true, "수정안이 적용되었습니다.", backupPath);
    }
}
