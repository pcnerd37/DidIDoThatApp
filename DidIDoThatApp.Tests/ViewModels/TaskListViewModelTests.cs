using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services.Interfaces;
using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Tests.ViewModels;

public class TaskListViewModelTests
{
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly TaskListViewModel _sut;

    public TaskListViewModelTests()
    {
        _mockTaskService = new Mock<ITaskService>();
        _mockCategoryService = new Mock<ICategoryService>();
        _sut = new TaskListViewModel(_mockTaskService.Object, _mockCategoryService.Object);
    }

    [Fact]
    public void Constructor_SetsTitle()
    {
        // Assert
        _sut.Title.Should().Be("Tasks");
    }

    [Fact]
    public async Task LoadDataCommand_GroupsTasksByCategory()
    {
        // Arrange
        var category1 = new Category { Id = Guid.NewGuid(), Name = "Home", Icon = "🏠" };
        var category2 = new Category { Id = Guid.NewGuid(), Name = "Car", Icon = "🚗" };

        var categories = new List<Category> { category1, category2 };
        var tasks = new List<TaskItem>
        {
            CreateTaskItem("Task 1", category1),
            CreateTaskItem("Task 2", category1),
            CreateTaskItem("Task 3", category2)
        };

        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(categories);
        _mockTaskService.Setup(x => x.GetAllTasksAsync())
            .ReturnsAsync(tasks);

        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _sut.CategoryGroups.Should().HaveCount(2);
        _sut.CategoryGroups[0].Tasks.Should().HaveCount(2);
        _sut.CategoryGroups[1].Tasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadDataCommand_WhenNoTasks_SetsHasNoTasksTrue()
    {
        // Arrange
        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(new List<Category>());
        _mockTaskService.Setup(x => x.GetAllTasksAsync())
            .ReturnsAsync(new List<TaskItem>());

        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _sut.HasNoTasks.Should().BeTrue();
    }

    [Fact]
    public async Task LoadDataCommand_WhenTasksExist_SetsHasNoTasksFalse()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Home" };
        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(new List<Category> { category });
        _mockTaskService.Setup(x => x.GetAllTasksAsync())
            .ReturnsAsync(new List<TaskItem> { CreateTaskItem("Task", category) });

        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _sut.HasNoTasks.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteTaskCommand_CompletesTaskAndReloads()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Home" };
        var task = CreateTaskItem("Test", category);
        var taskVm = new TaskItemViewModel(task);

        _mockTaskService.Setup(x => x.CompleteTaskAsync(task.Id, null, null))
            .ReturnsAsync(new TaskLog { Id = Guid.NewGuid() });
        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(new List<Category>());
        _mockTaskService.Setup(x => x.GetAllTasksAsync())
            .ReturnsAsync(new List<TaskItem>());

        // Act
        await _sut.CompleteTaskCommand.ExecuteAsync(taskVm);

        // Assert
        _mockTaskService.Verify(x => x.CompleteTaskAsync(task.Id, null, null), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskCommand_WhenNull_DoesNothing()
    {
        // Act
        await _sut.DeleteTaskCommand.ExecuteAsync(null);

        // Assert
        _mockTaskService.Verify(x => x.DeleteTaskAsync(It.IsAny<Guid>()), Times.Never);
    }

    private static TaskItem CreateTaskItem(string name, Category category)
    {
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
