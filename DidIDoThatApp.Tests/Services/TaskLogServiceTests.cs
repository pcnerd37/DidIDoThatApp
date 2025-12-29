using DidIDoThatApp.Data;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services;
using Microsoft.EntityFrameworkCore;

namespace DidIDoThatApp.Tests.Services;

public class TaskLogServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskLogService _sut;
    private readonly Category _testCategory;
    private readonly TaskItem _testTask;

    public TaskLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new TaskLogService(_context);

        // Set up test data
        _testCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            CreatedDate = DateTime.UtcNow
        };
        _context.Categories.Add(_testCategory);

        _testTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = _testCategory.Id,
            Name = "Test Task",
            FrequencyValue = 7,
            FrequencyUnit = Models.Enums.FrequencyUnit.Days,
            CreatedDate = DateTime.UtcNow
        };
        _context.Tasks.Add(_testTask);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetLogsForTaskAsync Tests

    [Fact]
    public async Task GetLogsForTaskAsync_WhenNoLogs_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetLogsForTaskAsync(_testTask.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLogsForTaskAsync_ReturnsLogsOrderedByDateDescending()
    {
        // Arrange
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 1));
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 15));
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 8));

        // Act
        var result = await _sut.GetLogsForTaskAsync(_testTask.Id);

        // Assert
        result.Should().HaveCount(3);
        result[0].CompletedDate.Should().Be(new DateTime(2024, 1, 15));
        result[1].CompletedDate.Should().Be(new DateTime(2024, 1, 8));
        result[2].CompletedDate.Should().Be(new DateTime(2024, 1, 1));
    }

    #endregion

    #region GetMostRecentLogAsync Tests

    [Fact]
    public async Task GetMostRecentLogAsync_WhenNoLogs_ReturnsNull()
    {
        // Act
        var result = await _sut.GetMostRecentLogAsync(_testTask.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMostRecentLogAsync_ReturnsMostRecentLog()
    {
        // Arrange
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 1));
        var mostRecent = await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 15));
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 8));

        // Act
        var result = await _sut.GetMostRecentLogAsync(_testTask.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(mostRecent.Id);
        result.CompletedDate.Should().Be(new DateTime(2024, 1, 15));
    }

    #endregion

    #region CreateLogAsync Tests

    [Fact]
    public async Task CreateLogAsync_CreatesLogWithCorrectData()
    {
        // Arrange
        var completedDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var notes = "Test notes";

        // Act
        var result = await _sut.CreateLogAsync(_testTask.Id, completedDate, notes);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.TaskItemId.Should().Be(_testTask.Id);
        result.CompletedDate.Should().Be(completedDate);
        result.Notes.Should().Be(notes);

        var saved = await _context.TaskLogs.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateLogAsync_WithoutNotes_CreatesLogWithNullNotes()
    {
        // Act
        var result = await _sut.CreateLogAsync(_testTask.Id, DateTime.Now);

        // Assert
        result.Notes.Should().BeNull();
    }

    #endregion

    #region DeleteLogAsync Tests

    [Fact]
    public async Task DeleteLogAsync_WhenLogExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var log = await CreateLogAsync(_testTask.Id, DateTime.Now);

        // Act
        var result = await _sut.DeleteLogAsync(log.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.TaskLogs.FindAsync(log.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteLogAsync_WhenLogDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteLogAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetLogsInRangeAsync Tests

    [Fact]
    public async Task GetLogsInRangeAsync_ReturnsLogsWithinRange()
    {
        // Arrange
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 5)); // In range
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 15)); // In range
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 1)); // Before range
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 20)); // After range

        var start = new DateTime(2024, 1, 5);
        var end = new DateTime(2024, 1, 15);

        // Act
        var result = await _sut.GetLogsInRangeAsync(start, end);

        // Assert
        result.Should().HaveCount(2);
        result.All(l => l.CompletedDate >= start && l.CompletedDate <= end).Should().BeTrue();
    }

    [Fact]
    public async Task GetLogsInRangeAsync_IncludesTaskItemNavigation()
    {
        // Arrange
        await CreateLogAsync(_testTask.Id, new DateTime(2024, 1, 10));

        // Act
        var result = await _sut.GetLogsInRangeAsync(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31));

        // Assert
        result.Should().HaveCount(1);
        result[0].TaskItem.Should().NotBeNull();
        result[0].TaskItem!.Name.Should().Be("Test Task");
    }

    #endregion

    #region Helper Methods

    private async Task<TaskLog> CreateLogAsync(Guid taskId, DateTime completedDate, string? notes = null)
    {
        var log = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId,
            CompletedDate = completedDate,
            Notes = notes
        };
        _context.TaskLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    #endregion
}
