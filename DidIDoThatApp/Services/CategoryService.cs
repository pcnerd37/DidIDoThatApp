using DidIDoThatApp.Data;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DidIDoThatApp.Services;

/// <summary>
/// Service for managing categories.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.Tasks)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Category> CreateCategoryAsync(string name, string? icon = null)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Icon = icon,
            CreatedDate = DateTime.UtcNow,
            IsDefault = false
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(Guid id, string name, string? icon = null)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return null;
        }

        category.Name = name.Trim();
        category.Icon = icon;

        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return false;
        }

        // Prevent deletion of default categories
        if (category.IsDefault)
        {
            return false;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CategoryExistsAsync(string name)
    {
        return await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == name.Trim().ToLower());
    }
}
