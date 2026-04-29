namespace DevPilotAgent.Tests.Integration;

using DevPilotAgent.Domain.Entities;
using DevPilotAgent.Domain.Enums;
using DevPilotAgent.Infrastructure.Persistence;
using DevPilotAgent.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class AnalysisRepositoryTests : IAsyncLifetime
{
    private AppDbContext _context = null!;
    private AnalysisRepository _repository = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new AnalysisRepository(_context);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_SavesRecord()
    {
        var record = new AnalysisRecord
        {
            ProjectFolderPath = @"C:\project",
            ErrorLog = "test error"
        };

        var saved = await _repository.CreateAsync(record);

        saved.Id.Should().NotBeEmpty();
        var found = await _context.AnalysisRecords.FindAsync(saved.Id);
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsRecord()
    {
        var record = new AnalysisRecord { ProjectFolderPath = @"C:\test", ErrorLog = "error" };
        _context.AnalysisRecords.Add(record);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(record.Id);

        result.Should().NotBeNull();
        result!.ProjectFolderPath.Should().Be(@"C:\test");
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPaginatedResults()
    {
        for (var i = 0; i < 5; i++)
        {
            _context.AnalysisRecords.Add(new AnalysisRecord
            {
                ProjectFolderPath = $@"C:\project{i}",
                ErrorLog = $"error {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        var (items, totalCount) = await _repository.GetPagedAsync(1, 3);

        totalCount.Should().Be(5);
        items.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesRecord()
    {
        var record = new AnalysisRecord { ProjectFolderPath = @"C:\test", ErrorLog = "error" };
        _context.AnalysisRecords.Add(record);
        await _context.SaveChangesAsync();

        record.Status = AnalysisStatus.Completed;
        record.RootCauseAnalysis = "Found the bug";
        await _repository.UpdateAsync(record);

        var updated = await _context.AnalysisRecords.FindAsync(record.Id);
        updated!.Status.Should().Be(AnalysisStatus.Completed);
        updated.RootCauseAnalysis.Should().Be("Found the bug");
    }

    [Fact]
    public async Task HasRunningAnalysisAsync_WithRunning_ReturnsTrue()
    {
        _context.AnalysisRecords.Add(new AnalysisRecord
        {
            ProjectFolderPath = @"C:\active",
            ErrorLog = "error",
            Status = AnalysisStatus.Running
        });
        await _context.SaveChangesAsync();

        var result = await _repository.HasRunningAnalysisAsync(@"C:\active");

        result.Should().BeTrue();
    }
}
