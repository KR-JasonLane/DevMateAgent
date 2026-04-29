namespace DevPilotAgent.Tests.Unit;

using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Application.Models;
using DevPilotAgent.Application.UseCases;
using DevPilotAgent.Domain.Entities;
using DevPilotAgent.Domain.Enums;
using DevPilotAgent.Shared.DTOs;
using DevPilotAgent.Shared.Requests;
using FluentAssertions;
using Moq;

public class AnalyzeErrorUseCaseTests
{
    private readonly Mock<IAgentOrchestrator> _orchestrator = new();
    private readonly Mock<IAnalysisRepository> _repository = new();
    private readonly Mock<IFileSystemService> _fileSystem = new();
    private readonly Mock<IAgentProgressReporter> _reporter = new();
    private readonly AnalyzeErrorUseCase _useCase;

    public AnalyzeErrorUseCaseTests()
    {
        _useCase = new AnalyzeErrorUseCase(_orchestrator.Object, _repository.Object, _fileSystem.Object);
    }

    [Fact]
    public async Task StartAsync_WithValidRequest_ReturnsGuid()
    {
        _fileSystem.Setup(f => f.ValidateFolderPath(It.IsAny<string>())).Returns(true);
        _repository.Setup(r => r.HasRunningAnalysisAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repository.Setup(r => r.CreateAsync(It.IsAny<AnalysisRecord>()))
            .ReturnsAsync((AnalysisRecord r) => r);

        var request = new AnalysisRequest(@"C:\project", "NullReferenceException at line 42");

        var id = await _useCase.StartAsync(request);

        id.Should().NotBeEmpty();
        _repository.Verify(r => r.CreateAsync(It.Is<AnalysisRecord>(
            rec => rec.Status == AnalysisStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithInvalidFolder_ThrowsArgumentException()
    {
        _fileSystem.Setup(f => f.ValidateFolderPath(It.IsAny<string>())).Returns(false);

        var request = new AnalysisRequest(@"C:\invalid", "error");

        var act = () => _useCase.StartAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*유효하지 않은*");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CompletesSuccessfully()
    {
        var analysisId = Guid.NewGuid();
        var record = new AnalysisRecord { Id = analysisId, Status = AnalysisStatus.Pending };
        _repository.Setup(r => r.GetByIdAsync(analysisId)).ReturnsAsync(record);
        _orchestrator.Setup(o => o.RunAsync(It.IsAny<string>(), It.IsAny<string>(),
                analysisId, _reporter.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentResult
            {
                ExtractedKeywords = ["NullReferenceException"],
                RootCauseAnalysis = "Null check missing"
            });

        var request = new AnalysisRequest(@"C:\project", "error");

        await _useCase.ExecuteAsync(analysisId, request, _reporter.Object, CancellationToken.None);

        _repository.Verify(r => r.UpdateAsync(It.Is<AnalysisRecord>(
            rec => rec.Status == AnalysisStatus.Completed)), Times.AtLeastOnce);
        _reporter.Verify(r => r.ReportCompletedAsync(It.Is<AnalysisCompletedMessage>(
            m => m.Success)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrchestratorFails_SavesFailedStatus()
    {
        var analysisId = Guid.NewGuid();
        var record = new AnalysisRecord { Id = analysisId, Status = AnalysisStatus.Pending };
        _repository.Setup(r => r.GetByIdAsync(analysisId)).ReturnsAsync(record);
        _orchestrator.Setup(o => o.RunAsync(It.IsAny<string>(), It.IsAny<string>(),
                analysisId, _reporter.Object, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("LLM API 실패"));

        var request = new AnalysisRequest(@"C:\project", "error");

        var act = () => _useCase.ExecuteAsync(analysisId, request, _reporter.Object, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        _repository.Verify(r => r.UpdateAsync(It.Is<AnalysisRecord>(
            rec => rec.Status == AnalysisStatus.Failed && rec.ErrorMessage == "LLM API 실패")), Times.AtLeastOnce);
    }
}
