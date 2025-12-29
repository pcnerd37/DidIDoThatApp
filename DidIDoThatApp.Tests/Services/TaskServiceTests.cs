using DidIDoThatApp.Data;
using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TaskStatus = DidIDoThatApp.Models.Enums.TaskStatus;

namespace DidIDoThatApp.Tests.Services;

public class TaskServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskLogService _taskLogService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly TaskService _sut;
    private readonly Category _testCategory;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _taskLogService = new TaskLogService(_context);
        _mockNotificationService = new Mock<INotificationService>();
        _sut = new TaskService(_context, _taskLogService, _mockNotificationService.Object);

        // Set up test category
        _testCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            CreatedDate = DateTime.UtcNow
        };
        _context.Categories.Add(_testCategory);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetAllTasksAsync Tests

    [Fact]
    public async Task GetAllTasksAsync_WhenNoTasks_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetAllTasksAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsTasks_OrderedByCategoryThenName()
    {
        // Arrange
        var category2 = new Category { Id = Guid.NewGuid(), Name = "ZCategory", CreatedDate = DateTime.UtcNow };
        _context.Categories.Add(category2);
        await _context.SaveChangesAsync();

        await CreateTaskAsync("ZTask", _testCategory.Id);
        await CreateTaskAsync("ATask", _testCategory.Id);
        await CreateTaskAsync("BTask", category2.Id);

        // Act
        var result = await _sut.GetAllTasksAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("ATask");
        result[1].Name.Should().Be("ZTask");
        result[2].Name.Should().Be("BTask");
    }

    #endregion

    #region GetTasksByCategoryAsync Tests

    [Fact]
    public async Task GetTasksByCategoryAsync_ReturnsOnlyTasksForCategory()
    {
        // Arrange
        var category2 = new Category { Id = Guid.NewGuid(), Name = "Other", CreatedDate = DateTime.UtcNow };
        _context.Categories.Add(category2);
        await _context.SaveChangesAsync();

        await CreateTaskAsync("Task1", _testCategory.Id);
        await CreateTaskAsync("Task2", _testCategory.Id);
        await CreateTaskAsync("Task3", category2.Id);

        // Act
        var result = await _sut.GetTasksByCategoryAsync(_testCategory.Id);

        // Assert
        result.Should().HaveCount(2);
        result.All(t => t.CategoryId == _testCategory.Id).Should().BeTrue();
    }

    #endregion

    #region GetOverdueTasksAsync Tests

    [Fact]
    public async Task GetOverdueTasksAsync_ReturnsOnlyOverdueTasks()
    {
        // Arrange
        var overdueTask = await CreateTaskAsync("Overdue");
        // No completion = overdue

        var upToDateTask = await CreateTaskAsync("UpToDate");
        await CompleteTaskAsync(upToDateTask.Id, DateTime.Now.AddDays(-1)); // Completed yesterday, 7-day freq = not overdue

        // Act
        var result = await _sut.GetOverdueTasksAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Overdue");
    }

    #endregion

    #region GetDueSoonTasksAsync Tests

    [Fact]
    public async Task GetDueSoonTasksAsync_ReturnsOnlyDueSoonTasks()
    {
        // Arrange
        var overdueTask = await CreateTaskAsync("Overdue");
        // No completion = overdue

        // Task with 10-day frequency, completed 9 days ago = due soon (within 20% of 10 = 2 days)
        var dueSoonTask = await CreateTaskAsync("DueSoon", frequencyValue: 10);
        await CompleteTaskAsync(dueSoonTask.Id, DateTime.Now.AddDays(-9));

        // Task with 10-day frequency, completed 2 days ago = up to date
        var upToDateTask = await CreateTaskAsync("UpToDate", frequencyValue: 10);
        await CompleteTaskAsync(upToDateTask.Id, DateTime.Now.AddDays(-2));

        // Act
        var result = await _sut.GetDueSoonTasksAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("DueSoon");
    }

    #endregion

    #region CreateTaskAsync Tests

    [Fact]
    public async Task CreateTaskAsync_CreatesTaskWithCorrectData()
    {
        // Act
        var result = await _sut.CreateTaskAsync(
            _testCategory.Id,
            "New Task",
            "Description",
            3,
            FrequencyUnit.Months,
            true);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CategoryId.Should().Be(_testCategory.Id);
        result.Name.Should().Be("New Task");
        result.Description.Should().Be("Description");
        result.FrequencyValue.Should().Be(3);
        result.FrequencyUnit.Should().Be(FrequencyUnit.Months);
        result.IsReminderEnabled.Should().BeTrue();

        var saved = await _context.Tasks.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTaskAsync_TrimsNameAndDescription()
    {
        // Act
        var result = await _sut.CreateTaskAsync(
            _testCategory.Id,
            "  Trimmed Name  ",
            "  Trimmed Description  ",
            1,
            FrequencyUnit.Days);

        // Assert
        result.Name.Should().Be("Trimmed Name");
        result.Description.Should().Be("Trimmed Description");
    }

    #endregion

    #region UpdateTaskAsync Tests

    [Fact]
    public async Task UpdateTaskAsync_WhenTaskExists_UpdatesTask()
    {
        // Arrange
        var task = await CreateTaskAsync("Original");

        // Act
        var result = await _sut.UpdateTaskAsync(
            task.Id,
            "Updated",
            "New Description",
            14,
            FrequencyUnit.Weeks,
            false);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        result.Description.Should().Be("New Description");
        result.FrequencyValue.Should().Be(14);
        result.FrequencyUnit.Should().Be(FrequencyUnit.Weeks);
        result.IsReminderEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTaskAsync_WhenTaskDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _sut.UpdateTaskAsync(
            Guid.NewGuid(),
            "Name",
            null,
            1,
            FrequencyUnit.Days,
            true);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTaskAsync_WhenReminderDisabled_CancelsNotification()
    {
        // Arrange
        var task = await CreateTaskAsync("Task", isReminderEnabled: true);
        await CompleteTaskAsync(task.Id, DateTime.Now);

        // Act
        await _sut.UpdateTaskAsync(task.Id, "Task", null, 7, FrequencyUnit.Days, false);

        // Assert
        _mockNotificationService.Verify(
            x => x.CancelTaskNotificationAsync(task.Id),
            Times.Once);
    }

    #endregion

    #region DeleteTaskAsync Tests

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var task = await CreateTaskAsync("ToDelete");

        // Act
        var result = await _sut.DeleteTaskAsync(task.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Tasks.FindAsync(task.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTaskAsync_CancelsNotification()
    {
        // Arrange
        var task = await CreateTaskAsync("ToDelete");

        // Act
        await _sut.DeleteTaskAsync(task.Id);

        // Assert
        _mockNotificationService.Verify(
            x => x.CancelTaskNotificationAsync(task.Id),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteTaskAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CompleteTaskAsync Tests

    [Fact]
    public async Task CompleteTaskAsync_CreatesTaskLog()
    {
        // Arrange
        var task = await CreateTaskAsync("ToComplete");

        // Act
        var result = await _sut.CompleteTaskAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result.TaskItemId.Should().Be(task.Id);
        result.CompletedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));

        var logs = await _context.TaskLogs.Where(l => l.TaskItemId == task.Id).ToListAsync();
        logs.Should().HaveCount(1);
    }

    [Fact]
    public async Task CompleteTaskAsync_WithCustomDate_UsesProvidedDate()
    {
        // Arrange
        var task = await CreateTaskAsync("ToComplete");
        var customDate = new DateTime(2024, 1, 15);

        // Act
        var result = await _sut.CompleteTaskAsync(task.Id, customDate, "Notes");

        // Assert
        result.CompletedDate.Should().Be(customDate);
        result.Notes.Should().Be("Notes");
    }

    [Fact]
    public async Task CompleteTaskAsync_WithReminderEnabled_SchedulesNotification()
    {
        // Arrange
        var task = await CreateTaskAsync("ToComplete", isReminderEnabled: true);

        // Act
        await _sut.CompleteTaskAsync(task.Id);

        // Assert
        _mockNotificationService.Verify(
            x => x.ScheduleTaskNotificationAsync(It.IsAny<TaskItem>(), It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteTaskAsync_WhenTaskDoesNotExist_ThrowsArgumentException()
    {
        // Act
        var act = async () => await _sut.CompleteTaskAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region GetTaskStatusAsync Tests

    [Fact]
    public async Task GetTaskStatusAsync_WhenNeverCompleted_ReturnsOverdue()
    {
        // Arrange
        var task = await CreateTaskAsync("NeverCompleted");

        // Act
        var result = await _sut.GetTaskStatusAsync(task.Id);

        // Assert
        result.Should().Be(TaskStatus.Overdue);
    }

    [Fact]
    public async Task GetTaskStatusAsync_WhenRecentlyCompleted_ReturnsUpToDate()
    {
        // Arrange
        var task = await CreateTaskAsync("RecentlyCompleted", frequencyValue: 30);
        await CompleteTaskAsync(task.Id, DateTime.Now.AddDays(-1));

        // Act
        var result = await _sut.GetTaskStatusAsync(task.Id);

        // Assert
        result.Should().Be(TaskStatus.UpToDate);
    }

    [Fact]
    public async Task GetTaskStatusAsync_WhenTaskDoesNotExist_ReturnsOverdue()
    {
        // Act
        var result = await _sut.GetTaskStatusAsync(Guid.NewGuid());

        // Assert
        result.Should().Be(TaskStatus.Overdue);
    }

    #endregion

    #region GetDueDateAsync Tests

    [Fact]
    public async Task GetDueDateAsync_WhenNeverCompleted_ReturnsNull()
    {
        // Arrange
        var task = await CreateTaskAsync("NeverCompleted");

        // Act
        var result = await _sut.GetDueDateAsync(task.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDueDateAsync_WhenCompleted_ReturnsCorrectDueDate()
    {
        // Arrange
        var task = await CreateTaskAsync("Completed", frequencyValue: 7);
        var completedDate = new DateTime(2024, 1, 1);
        await CompleteTaskAsync(task.Id, completedDate);

        // Act
        var result = await _sut.GetDueDateAsync(task.Id);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 8));
    }

    #endregion

    #region Helper Methods

    private async Task<TaskItem> CreateTaskAsync(
        string name = "Test Task",
        Guid? categoryId = null,
        int frequencyValue = 7,
        FrequencyUnit frequencyUnit = FrequencyUnit.Days,
        bool isReminderEnabled = true)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId ?? _testCategory.Id,
            Name = name,
            FrequencyValue = frequencyValue,
            FrequencyUnit = frequencyUnit,
            IsReminderEnabled = isReminderEnabled,
            CreatedDate = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    private async Task CompleteTaskAsync(Guid taskId, DateTime completedDate)
    {
        var log = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId,
            CompletedDate = completedDate
        };
        _context.TaskLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    #endregion
}
