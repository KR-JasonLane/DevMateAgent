namespace DevPilotAgent.Tests.Unit;

using DevPilotAgent.Infrastructure.FileSystem;
using DevPilotAgent.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

public class FileSystemServiceTests : IClassFixture<TestProjectFixture>
{
    private readonly FileSystemService _service;
    private readonly TestProjectFixture _fixture;

    public FileSystemServiceTests(TestProjectFixture fixture)
    {
        _fixture = fixture;
        _service = new FileSystemService(Mock.Of<ILogger<FileSystemService>>());
    }

    [Fact]
    public async Task SearchFilesAsync_WithMatchingKeyword_ReturnsFiles()
    {
        var results = await _service.SearchFilesAsync(_fixture.TestFolderPath, ["UserService"]);

        results.Should().Contain(f => f.Contains("UserService.cs"));
    }

    [Fact]
    public async Task SearchFilesAsync_ExcludesBinFolder()
    {
        var results = await _service.SearchFilesAsync(_fixture.TestFolderPath, ["should_be_excluded"]);

        results.Should().NotContain(f => f.Contains("bin"));
    }

    [Fact]
    public async Task ReadFileAsync_ReturnsContent()
    {
        var filePath = Path.Combine(_fixture.TestFolderPath, "Services", "UserService.cs");

        var content = await _service.ReadFileAsync(filePath);

        content.Should().Contain("UserService");
        content.Should().Contain("GetUser");
    }

    [Fact]
    public async Task ApplyPatchAsync_CreatesBackupAndModifiesFile()
    {
        var targetPath = Path.Combine(_fixture.TestFolderPath, "Models", "User.cs");
        var lastModified = File.GetLastWriteTimeUtc(targetPath);
        var newContent = "// Modified content";

        var backupPath = await _service.ApplyPatchAsync(targetPath, newContent, _fixture.TestFolderPath, lastModified);

        File.Exists(backupPath).Should().BeTrue();
        (await File.ReadAllTextAsync(targetPath)).Should().Be(newContent);
    }

    [Fact]
    public async Task ApplyPatchAsync_RejectsPathOutsideFolder()
    {
        var act = () => _service.ApplyPatchAsync(
            @"C:\Windows\System32\test.cs", "content", _fixture.TestFolderPath, DateTime.UtcNow);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*프로젝트 폴더 외부*");
    }

    [Fact]
    public void ValidateFolderPath_WithExistingFolder_ReturnsTrue()
    {
        _service.ValidateFolderPath(_fixture.TestFolderPath).Should().BeTrue();
    }

    [Fact]
    public void ValidateFolderPath_WithNonExistingFolder_ReturnsFalse()
    {
        _service.ValidateFolderPath(@"C:\NonExistent\Path\12345").Should().BeFalse();
    }

    [Fact]
    public void ValidateFolderPath_WithUncPath_ReturnsFalse()
    {
        _service.ValidateFolderPath(@"\\server\share\folder").Should().BeFalse();
    }

    [Fact]
    public void IsPathWithinFolder_WithValidPath_ReturnsTrue()
    {
        var filePath = Path.Combine(_fixture.TestFolderPath, "Services", "UserService.cs");
        _service.IsPathWithinFolder(filePath, _fixture.TestFolderPath).Should().BeTrue();
    }

    [Fact]
    public void IsPathWithinFolder_WithTraversalPath_ReturnsFalse()
    {
        var traversalPath = Path.Combine(_fixture.TestFolderPath, "..", "..", "etc", "passwd");
        _service.IsPathWithinFolder(traversalPath, _fixture.TestFolderPath).Should().BeFalse();
    }
}
