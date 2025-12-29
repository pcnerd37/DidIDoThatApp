using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;
using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Tests.ViewModels;

public class CategoryViewModelTests
{
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly CategoryViewModel _sut;

    public CategoryViewModelTests()
    {
        _mockCategoryService = new Mock<ICategoryService>();
        _mockTaskService = new Mock<ITaskService>();
        _sut = new CategoryViewModel(_mockCategoryService.Object, _mockTaskService.Object);
    }

    [Fact]
    public void Constructor_SetsTitle()
    {
        // Assert
        _sut.Title.Should().Be("Categories");
    }

    [Fact]
    public async Task LoadDataCommand_LoadsCategoriesWithTaskCounts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var categories = new List<Category>
        {
            new() { Id = categoryId, Name = "Home", Icon = "🏠" },
            new() { Id = Guid.NewGuid(), Name = "Car", Icon = "🚗" }
        };

        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), CategoryId = categoryId, Name = "Task1", TaskLogs = new List<TaskLog>() },
            new() { Id = Guid.NewGuid(), CategoryId = categoryId, Name = "Task2", TaskLogs = new List<TaskLog>() }
        };

        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(categories);
        _mockTaskService.Setup(x => x.GetAllTasksAsync())
            .ReturnsAsync(tasks);

        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _sut.Categories.Should().HaveCount(2);
        _sut.Categories[0].TaskCount.Should().Be(2); // Home has 2 tasks
        _sut.Categories[1].TaskCount.Should().Be(0); // Car has 0 tasks
    }

    [Fact]
    public void StartAddCategoryCommand_SetsIsAddingCategory()
    {
        // Act
        _sut.StartAddCategoryCommand.Execute(null);

        // Assert
        _sut.IsAddingCategory.Should().BeTrue();
        _sut.NewCategoryName.Should().BeEmpty();
        _sut.NewCategoryIcon.Should().BeEmpty();
    }

    [Fact]
    public void CancelAddCategoryCommand_ClearsAddState()
    {
        // Arrange
        _sut.IsAddingCategory = true;
        _sut.NewCategoryName = "Test";
        _sut.NewCategoryIcon = "🏠";

        // Act
        _sut.CancelAddCategoryCommand.Execute(null);

        // Assert
        _sut.IsAddingCategory.Should().BeFalse();
        _sut.NewCategoryName.Should().BeEmpty();
        _sut.NewCategoryIcon.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveNewCategoryCommand_WhenNameEmpty_DoesNotCreate()
    {
        // Arrange
        _sut.NewCategoryName = "";

        // Act
        await _sut.SaveNewCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockCategoryService.Verify(
            x => x.CreateCategoryAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveNewCategoryCommand_WhenCategoryExists_DoesNotCreate()
    {
        // Arrange
        _sut.NewCategoryName = "ExistingCategory";
        _mockCategoryService.Setup(x => x.CategoryExistsAsync("ExistingCategory"))
            .ReturnsAsync(true);

        // Act
        await _sut.SaveNewCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockCategoryService.Verify(
            x => x.CreateCategoryAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void StartEditCategoryCommand_SetsEditingState()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Home", Icon = "🏠" };
        var categoryVm = new CategoryItemViewModel(category, 5);

        // Act
        _sut.StartEditCategoryCommand.Execute(categoryVm);

        // Assert
        _sut.EditingCategory.Should().Be(categoryVm);
        _sut.EditCategoryName.Should().Be("Home");
        _sut.EditCategoryIcon.Should().Be("🏠");
    }

    [Fact]
    public void CancelEditCategoryCommand_ClearsEditState()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Home" };
        _sut.EditingCategory = new CategoryItemViewModel(category, 0);
        _sut.EditCategoryName = "Test";
        _sut.EditCategoryIcon = "🏠";

        // Act
        _sut.CancelEditCategoryCommand.Execute(null);

        // Assert
        _sut.EditingCategory.Should().BeNull();
        _sut.EditCategoryName.Should().BeEmpty();
        _sut.EditCategoryIcon.Should().BeEmpty();
    }

    [Fact]
    public void CategoryItemViewModel_TaskCountText_SingleTask_ReturnsSingular()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Home" };
        var vm = new CategoryItemViewModel(category, 1);

        // Assert
        vm.TaskCountText.Should().Be("1 task");
    }

    [Fact]
    public void CategoryItemViewModel_TaskCountText_MultipleTasks_ReturnsPlural()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Home" };
        var vm = new CategoryItemViewModel(category, 5);

        // Assert
        vm.TaskCountText.Should().Be("5 tasks");
    }

    [Fact]
    public void CategoryItemViewModel_DisplayName_IncludesIconAndName()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Home", Icon = "🏠" };
        var vm = new CategoryItemViewModel(category, 0);

        // Assert
        vm.DisplayName.Should().Be("🏠 Home");
    }
}
