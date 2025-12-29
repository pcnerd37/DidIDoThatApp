using DidIDoThatApp.Models;

namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for managing categories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories.
    /// </summary>
    Task<IReadOnlyList<Category>> GetAllCategoriesAsync();

    /// <summary>
    /// Gets a category by its ID.
    /// </summary>
    Task<Category?> GetCategoryByIdAsync(Guid id);

    /// <summary>
    /// Creates a new category.
    /// </summary>
    Task<Category> CreateCategoryAsync(string name, string? icon = null);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    Task<Category?> UpdateCategoryAsync(Guid id, string name, string? icon = null);

    /// <summary>
    /// Deletes a category. Cannot delete default categories.
    /// </summary>
    Task<bool> DeleteCategoryAsync(Guid id);

    /// <summary>
    /// Checks if a category with the given name exists.
    /// </summary>
    Task<bool> CategoryExistsAsync(string name);
}
