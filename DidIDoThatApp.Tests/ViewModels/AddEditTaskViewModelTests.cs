using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services.Interfaces;
using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Tests.ViewModels;

public class AddEditTaskViewModelTests
{
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly AddEditTaskViewModel _sut;

    public AddEditTaskViewModelTests()
    {
        _mockTaskService = new Mock<ITaskService>();
        _mockCategoryService = new Mock<ICategoryService>();
        _sut = new AddEditTaskViewModel(_mockTaskService.Object, _mockCategoryService.Object);
    }

    [Fact]
    public void Constructor_InitializesDefaults()
    {
        // Assert
        _sut.FrequencyValue.Should().Be(1);
        _sut.FrequencyUnit.Should().Be(FrequencyUnit.Months);
        _sut.IsReminderEnabled.Should().BeTrue();
        _sut.IsEditMode.Should().BeFalse();
        _sut.FrequencyUnits.Should().HaveCount(3);
    }

    [Fact]
    public void OnTaskIdChanged_WhenValidGuid_SetsEditMode()
    {
        // Act
        _sut.TaskId = Guid.NewGuid();

        // Assert
        _sut.IsEditMode.Should().BeTrue();
        _sut.Title.Should().Be("Edit Task");
    }

    [Fact]
    public void OnTaskIdChanged_WhenNull_SetsNewMode()
    {
        // First set to a value so it triggers change, then set to null
        _sut.TaskId = Guid.NewGuid();
        
        // Act
        _sut.TaskId = null;

        // Assert
        _sut.IsEditMode.Should().BeFalse();
        _sut.Title.Should().Be("New Task");
    }

    [Fact]
    public void CanSave_WhenNameEmptyAndCategoryNull_ReturnsFalse()
    {
        // Arrange
        _sut.Name = "";
        _sut.SelectedCategory = null;

        // Assert
        _sut.CanSave.Should().BeFalse();
    }

    [Fact]
    public void CanSave_WhenNameSetAndCategorySelected_ReturnsTrue()
    {
        // Arrange
        _sut.Name = "Test Task";
        _sut.SelectedCategory = new Category { Id = Guid.NewGuid(), Name = "Home" };

        // Assert
        _sut.CanSave.Should().BeTrue();
    }

    [Fact]
    public void CanSave_WhenNameWhitespaceOnly_ReturnsFalse()
    {
        // Arrange
        _sut.Name = "   ";
        _sut.SelectedCategory = new Category { Id = Guid.NewGuid(), Name = "Home" };

        // Assert
        _sut.CanSave.Should().BeFalse();
    }

    [Fact]
    public async Task LoadCategoriesCommand_LoadsCategoriesFromService()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Home" },
            new() { Id = Guid.NewGuid(), Name = "Car" }
        };

        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        await _sut.LoadCategoriesCommand.ExecuteAsync(null);

        // Assert
        _sut.Categories.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadCategoriesCommand_WhenNotEditMode_SelectsFirstCategory()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Home" },
            new() { Id = Guid.NewGuid(), Name = "Car" }
        };

        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        await _sut.LoadCategoriesCommand.ExecuteAsync(null);

        // Assert
        _sut.SelectedCategory.Should().NotBeNull();
        _sut.SelectedCategory!.Name.Should().Be("Home");
    }

    [Fact]
    public async Task LoadTaskCommand_WhenTaskExists_PopulatesFields()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Home" };
        var task = new TaskItem
        {
            Id = taskId,
            CategoryId = categoryId,
            Category = category,
            Name = "Test Task",
            Description = "Description",
            FrequencyValue = 3,
            FrequencyUnit = FrequencyUnit.Months,
            IsReminderEnabled = false,
            TaskLogs = new List<TaskLog>()
        };

        // Setup mocks
        _mockTaskService.Setup(x => x.GetTaskByIdAsync(taskId))
            .ReturnsAsync(task);
        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(new List<Category> { category });

        // Pre-load categories first (to avoid IsBusy guard issue in nested ExecuteAsync)
        await _sut.LoadCategoriesCommand.ExecuteAsync(null);
        
        // Set up for edit mode
        var taskIdField = typeof(AddEditTaskViewModel).GetField("_taskId", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        taskIdField?.SetValue(_sut, taskId);
        
        // Act - explicitly call the command
        await _sut.LoadTaskCommand.ExecuteAsync(null);

        // Assert
        _sut.Name.Should().Be("Test Task");
        _sut.Description.Should().Be("Description");
        _sut.FrequencyValue.Should().Be(3);
        _sut.FrequencyUnit.Should().Be(FrequencyUnit.Months);
        _sut.IsReminderEnabled.Should().BeFalse();
        _sut.SelectedCategory.Should().NotBeNull();
        _sut.SelectedCategory!.Id.Should().Be(categoryId);
    }
}
