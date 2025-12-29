using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services.Interfaces;
using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Tests.ViewModels;

public class DashboardViewModelTests
{
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly DashboardViewModel _sut;

    public DashboardViewModelTests()
    {
        _mockTaskService = new Mock<ITaskService>();
        _sut = new DashboardViewModel(_mockTaskService.Object);
    }

    [Fact]
    public void Constructor_SetsTitle()
    {
        // Assert
        _sut.Title.Should().Be("Dashboard");
    }

    [Fact]
    public async Task LoadDataCommand_LoadsOverdueTasks()
    {
        // Arrange
        var overdueTasks = new List<TaskItem>
        {
            CreateTaskItem("Overdue 1"),
            CreateTaskItem("Overdue 2")
        };

        _mockTaskService.Setup(x => x.GetOverdueTasksAsync())
            .ReturnsAsync(overdueTasks);
        _mockTaskService.Setup(x => x.GetDueSoonTasksAsync())
            .ReturnsAsync(new List<TaskItem>());
        _mockTaskService.Setup(x => x.GetRecentlyCompletedTasksAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<TaskItem>());

        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _sut.OverdueTasks.Should().HaveCount(2);
        _sut.HasOverdueTasks.Should().BeTrue();
    }

    [Fact]
    public async Task LoadDataCommand_LoadsDueSoonTasks()
    {
        // Arrange
        var dueSoonTasks = new List<TaskItem>
        {
            CreateTaskItem("Due Soon 1")
        };

        _mockTaskService.Setup(x => x.GetOverdueTasksAsync())
            .ReturnsAsync(new List<TaskItem>());
        _mockTaskService.Setup(x => x.GetDueSoonTasksAsync())
            .ReturnsAsync(dueSoonTasks);
        _mockTaskService.Setup(x => x.GetRecentlyCompletedTasksAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<TaskItem>());

        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _sut.DueSoonTasks.Should().HaveCount(1);
        _sut.HasDueSoonTasks.Should().BeTrue();
    }

    [Fact]
    public async Task LoadDataCommand_LoadsRecentlyCompletedTasks()
    {
        // Arrange
        var recentTasks = new List<TaskItem>
        {
            CreateTaskItem("Recent 1"),
            CreateTaskItem("Recent 2"),
            CreateTaskItem("Recent 3")
        };

        _mockTaskService.Setup(x => x.GetOverdueTasksAsync())
            .ReturnsAsync(new List<TaskItem>());
        _mockTaskService.Setup(x => x.GetDueSoonTasksAsync())
            .ReturnsAsync(new List<TaskItem>());
        _mockTaskService.Setup(x => x.GetRecentlyCompletedTasksAsync(5))
            .ReturnsAsync(recentTasks);

        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _sut.RecentlyCompletedTasks.Should().HaveCount(3);
        _sut.HasRecentlyCompletedTasks.Should().BeTrue();
    }

    [Fact]
    public async Task LoadDataCommand_WhenNoTasks_HasFlagsAreFalse()
    {
        // Arrange
        _mockTaskService.Setup(x => x.GetOverdueTasksAsync())
            .ReturnsAsync(new List<TaskItem>());
        _mockTaskService.Setup(x => x.GetDueSoonTasksAsync())
            .ReturnsAsync(new List<TaskItem>());
        _mockTaskService.Setup(x => x.GetRecentlyCompletedTasksAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<TaskItem>());

        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _sut.HasOverdueTasks.Should().BeFalse();
        _sut.HasDueSoonTasks.Should().BeFalse();
        _sut.HasRecentlyCompletedTasks.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteTaskCommand_CompletesTaskAndReloadsData()
    {
        // Arrange
        var task = CreateTaskItem("Test");
        var taskVm = new TaskItemViewModel(task);

        // Use It.IsAny for all parameters to match default parameter values
        _mockTaskService.Setup(x => x.CompleteTaskAsync(
                It.IsAny<Guid>(), 
                It.IsAny<DateTime?>(), 
                It.IsAny<string?>()))
            .ReturnsAsync(new TaskLog { Id = Guid.NewGuid(), TaskItemId = task.Id });
        _mockTaskService.Setup(x => x.GetOverdueTasksAsync())
            .ReturnsAsync(new List<TaskItem>());
        _mockTaskService.Setup(x => x.GetDueSoonTasksAsync())
            .ReturnsAsync(new List<TaskItem>());
        _mockTaskService.Setup(x => x.GetRecentlyCompletedTasksAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<TaskItem>());

        // Act
        await _sut.CompleteTaskCommand.ExecuteAsync(taskVm);

        // Assert
        _mockTaskService.Verify(x => x.CompleteTaskAsync(
            task.Id, 
            It.IsAny<DateTime?>(), 
            It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task CompleteTaskCommand_WithNullTask_DoesNothing()
    {
        // Act
        await _sut.CompleteTaskCommand.ExecuteAsync(null);

        // Assert
        _mockTaskService.Verify(x => x.CompleteTaskAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<string?>()), Times.Never);
    }

    private static TaskItem CreateTaskItem(string name)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Icon = "🏠"
        };

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Category = category,
            Name = name,
            FrequencyValue = 7,
            FrequencyUnit = FrequencyUnit.Days,
            IsReminderEnabled = true,
            CreatedDate = DateTime.UtcNow,
            TaskLogs = new List<TaskLog>()
        };
    }
}
