using DidIDoThatApp.Data;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services;
using Microsoft.EntityFrameworkCore;

namespace DidIDoThatApp.Tests.Services;

public class CategoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new CategoryService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetAllCategoriesAsync Tests

    [Fact]
    public async Task GetAllCategoriesAsync_WhenNoCategories_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetAllCategoriesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WithCategories_ReturnsOrderedByName()
    {
        // Arrange
        await SeedCategoriesAsync("Zebra", "Alpha", "Middle");

        // Act
        var result = await _sut.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Middle");
        result[2].Name.Should().Be("Zebra");
    }

    #endregion

    #region GetCategoryByIdAsync Tests

    [Fact]
    public async Task GetCategoryByIdAsync_WhenCategoryExists_ReturnsCategory()
    {
        // Arrange
        var category = await CreateCategoryAsync("Test Category");

        // Act
        var result = await _sut.GetCategoryByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WhenCategoryDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _sut.GetCategoryByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateCategoryAsync Tests

    [Fact]
    public async Task CreateCategoryAsync_WithValidName_CreatesCategory()
    {
        // Act
        var result = await _sut.CreateCategoryAsync("New Category", "🏠");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New Category");
        result.Icon.Should().Be("🏠");
        result.IsDefault.Should().BeFalse();

        var saved = await _context.Categories.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCategoryAsync_TrimsName()
    {
        // Act
        var result = await _sut.CreateCategoryAsync("  Trimmed Name  ");

        // Assert
        result.Name.Should().Be("Trimmed Name");
    }

    #endregion

    #region UpdateCategoryAsync Tests

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryExists_UpdatesCategory()
    {
        // Arrange
        var category = await CreateCategoryAsync("Original Name", "🏠");

        // Act
        var result = await _sut.UpdateCategoryAsync(category.Id, "Updated Name", "🚗");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Icon.Should().Be("🚗");
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _sut.UpdateCategoryAsync(Guid.NewGuid(), "Name", "Icon");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteCategoryAsync Tests

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var category = await CreateCategoryAsync("To Delete");

        // Act
        var result = await _sut.DeleteCategoryAsync(category.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Categories.FindAsync(category.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteCategoryAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryIsDefault_ReturnsFalse()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Default Category",
            IsDefault = true,
            CreatedDate = DateTime.UtcNow
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.DeleteCategoryAsync(category.Id);

        // Assert
        result.Should().BeFalse();
        var stillExists = await _context.Categories.FindAsync(category.Id);
        stillExists.Should().NotBeNull();
    }

    #endregion

    #region CategoryExistsAsync Tests

    [Fact]
    public async Task CategoryExistsAsync_WhenCategoryExists_ReturnsTrue()
    {
        // Arrange
        await CreateCategoryAsync("Existing");

        // Act
        var result = await _sut.CategoryExistsAsync("Existing");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CategoryExistsAsync_IsCaseInsensitive()
    {
        // Arrange
        await CreateCategoryAsync("ExistingCategory");

        // Act
        var result = await _sut.CategoryExistsAsync("existingcategory");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CategoryExistsAsync_WhenCategoryDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _sut.CategoryExistsAsync("NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private async Task<Category> CreateCategoryAsync(string name, string? icon = null)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = icon,
            CreatedDate = DateTime.UtcNow,
            IsDefault = false
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    private async Task SeedCategoriesAsync(params string[] names)
    {
        foreach (var name in names)
        {
            await CreateCategoryAsync(name);
        }
    }

    #endregion
}
