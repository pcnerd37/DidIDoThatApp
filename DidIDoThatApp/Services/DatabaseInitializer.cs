using DidIDoThatApp.Data;
using DidIDoThatApp.Helpers;
using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DidIDoThatApp.Services;

/// <summary>
/// Service for initializing the database with default data.
/// </summary>
public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly AppDbContext _context;
    private readonly ISettingsService _settingsService;

    public DatabaseInitializer(AppDbContext context, ISettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
    }

    public async Task InitializeAsync()
    {
        // Note: EnsureCreatedAsync is already called by App.xaml.cs before this.

        // Seed default data if this is the first launch
        if (!_settingsService.IsFirstLaunchComplete)
        {
            await SeedDefaultDataAsync();
            _settingsService.IsFirstLaunchComplete = true;
        }
    }

    public async Task SeedDefaultDataAsync()
    {
        // Check if categories already exist
        if (await _context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new Dictionary<string, Category>();

        // Create default categories
        foreach (var (name, icon) in Constants.DefaultCategories.Categories)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Icon = icon,
                CreatedDate = DateTime.UtcNow,
                IsDefault = true
            };
            categories[name] = category;
            _context.Categories.Add(category);
        }

        await _context.SaveChangesAsync();

        // Add example tasks for each category
        await SeedExampleTasksAsync(categories);
    }

    private async Task SeedExampleTasksAsync(Dictionary<string, Category> categories)
    {
        var exampleTasks = new List<(string CategoryName, string TaskName, string Description, int FreqValue, FrequencyUnit FreqUnit)>
        {
            ("Home", "Change HVAC filter", "Replace the air filter in your heating/cooling system", 3, FrequencyUnit.Months),
            ("Home", "Clean gutters", "Remove debris from roof gutters", 6, FrequencyUnit.Months),
            ("Home", "Test smoke detectors", "Press test button on all smoke detectors", 1, FrequencyUnit.Months),
            ("Home", "Deep clean refrigerator", "Clean shelves, drawers, and coils", 3, FrequencyUnit.Months),

            ("Car", "Oil change", "Change engine oil and filter", 3, FrequencyUnit.Months),
            ("Car", "Tire rotation", "Rotate tires for even wear", 6, FrequencyUnit.Months),
            ("Car", "Check tire pressure", "Verify all tires are properly inflated", 1, FrequencyUnit.Months),
            ("Car", "Replace wiper blades", "Install new windshield wiper blades", 6, FrequencyUnit.Months),

            ("Personal", "Haircut", "Schedule and get a haircut", 6, FrequencyUnit.Weeks),
            ("Personal", "Doctor checkup", "Annual physical examination", 12, FrequencyUnit.Months),
            ("Personal", "Dental cleaning", "Professional teeth cleaning", 6, FrequencyUnit.Months),
            ("Personal", "Eye exam", "Annual vision checkup", 12, FrequencyUnit.Months),

            ("Pet", "Vet visit", "Annual wellness checkup", 12, FrequencyUnit.Months),
            ("Pet", "Flea treatment", "Apply flea prevention medication", 1, FrequencyUnit.Months),
            ("Pet", "Nail trim", "Trim pet's nails", 4, FrequencyUnit.Weeks),
            ("Pet", "Grooming", "Full grooming session", 2, FrequencyUnit.Months),

            ("Business", "Review finances", "Review budget and expenses", 1, FrequencyUnit.Months),
            ("Business", "Backup data", "Backup important files and documents", 1, FrequencyUnit.Weeks),
            ("Business", "Update passwords", "Change important account passwords", 3, FrequencyUnit.Months),
            ("Business", "Review subscriptions", "Audit and cancel unused subscriptions", 3, FrequencyUnit.Months)
        };

        foreach (var (categoryName, taskName, description, freqValue, freqUnit) in exampleTasks)
        {
            if (categories.TryGetValue(categoryName, out var category))
            {
                var task = new TaskItem
                {
                    Id = Guid.NewGuid(),
                    CategoryId = category.Id,
                    Name = taskName,
                    Description = description,
                    FrequencyValue = freqValue,
                    FrequencyUnit = freqUnit,
                    IsReminderEnabled = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Tasks.Add(task);
            }
        }

        await _context.SaveChangesAsync();
    }
}