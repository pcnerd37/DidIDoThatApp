using System.Text.Json;
using DidIDoThatApp.Data;
using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services;
using Microsoft.EntityFrameworkCore;

namespace DidIDoThatApp.Tests.Services;

public class ExportServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ExportService _sut;

    public ExportServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new ExportService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GatherExportDataAsync Tests

    [Fact]
    public async Task GatherExportDataAsync_WhenDatabaseEmpty_ReturnsEmptyLists()
    {
        // Act
        var result = await _sut.GatherExportDataAsync("1.0.0");

        // Assert
        result.Should().NotBeNull();
        result.Categories.Should().BeEmpty();
        result.Tasks.Should().BeEmpty();
        result.TaskLogs.Should().BeEmpty();
        result.AppVersion.Should().Be("1.0.0");
    }

    [Fact]
    public async Task GatherExportDataAsync_WithCategories_IncludesAllCategories()
    {
        // Arrange
        var category1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            Icon = "🏠",
            CreatedDate = DateTime.UtcNow,
            IsDefault = true
        };
        var category2 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Car",
            Icon = "🚗",
            CreatedDate = DateTime.UtcNow,
            IsDefault = false
        };

        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GatherExportDataAsync("1.0.0");

        // Assert
        result.Categories.Should().HaveCount(2);
        
        var exportedCategory1 = result.Categories.First(c => c.Id == category1.Id);
        exportedCategory1.Name.Should().Be("Home");
        exportedCategory1.Icon.Should().Be("🏠");
        exportedCategory1.IsDefault.Should().BeTrue();

        var exportedCategory2 = result.Categories.First(c => c.Id == category2.Id);
        exportedCategory2.Name.Should().Be("Car");
        exportedCategory2.Icon.Should().Be("🚗");
        exportedCategory2.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task GatherExportDataAsync_WithTasks_IncludesAllTasks()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            CreatedDate = DateTime.UtcNow
        };
        _context.Categories.Add(category);

        var task1 = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Change HVAC Filter",
            Description = "Replace the air filter",
            FrequencyValue = 3,
            FrequencyUnit = FrequencyUnit.Months,
            IsReminderEnabled = true,
            CreatedDate = DateTime.UtcNow
        };
        var task2 = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Check Smoke Detectors",
            FrequencyValue = 6,
            FrequencyUnit = FrequencyUnit.Months,
            IsReminderEnabled = false,
            CreatedDate = DateTime.UtcNow
        };

        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GatherExportDataAsync("1.0.0");

        // Assert
        result.Tasks.Should().HaveCount(2);

        var exportedTask1 = result.Tasks.First(t => t.Id == task1.Id);
        exportedTask1.Name.Should().Be("Change HVAC Filter");
        exportedTask1.Description.Should().Be("Replace the air filter");
        exportedTask1.FrequencyValue.Should().Be(3);
        exportedTask1.FrequencyUnit.Should().Be("Months");
        exportedTask1.IsReminderEnabled.Should().BeTrue();
        exportedTask1.CategoryId.Should().Be(category.Id);

        var exportedTask2 = result.Tasks.First(t => t.Id == task2.Id);
        exportedTask2.Name.Should().Be("Check Smoke Detectors");
        exportedTask2.Description.Should().BeNull();
        exportedTask2.FrequencyValue.Should().Be(6);
        exportedTask2.IsReminderEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task GatherExportDataAsync_WithTaskLogs_IncludesAllLogs()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            CreatedDate = DateTime.UtcNow
        };
        _context.Categories.Add(category);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Test Task",
            FrequencyValue = 1,
            FrequencyUnit = FrequencyUnit.Months,
            CreatedDate = DateTime.UtcNow
        };
        _context.Tasks.Add(task);

        var log1 = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = task.Id,
            CompletedDate = DateTime.UtcNow.AddDays(-30),
            Notes = "First completion"
        };
        var log2 = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = task.Id,
            CompletedDate = DateTime.UtcNow,
            Notes = null
        };

        _context.TaskLogs.AddRange(log1, log2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GatherExportDataAsync("1.0.0");

        // Assert
        result.TaskLogs.Should().HaveCount(2);

        var exportedLog1 = result.TaskLogs.First(l => l.Id == log1.Id);
        exportedLog1.TaskItemId.Should().Be(task.Id);
        exportedLog1.Notes.Should().Be("First completion");

        var exportedLog2 = result.TaskLogs.First(l => l.Id == log2.Id);
        exportedLog2.TaskItemId.Should().Be(task.Id);
        exportedLog2.Notes.Should().BeNull();
    }

    [Fact]
    public async Task GatherExportDataAsync_SetsExportedAtTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = await _sut.GatherExportDataAsync("1.0.0");

        // Assert
        var after = DateTime.UtcNow;
        result.ExportedAt.Should().BeOnOrAfter(before);
        result.ExportedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task GatherExportDataAsync_WithCompleteData_ExportsAllRelationships()
    {
        // Arrange - Create a complete data set
        var homeCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Home",
            Icon = "🏠",
            IsDefault = true,
            CreatedDate = DateTime.UtcNow
        };
        var carCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Car",
            Icon = "🚗",
            IsDefault = false,
            CreatedDate = DateTime.UtcNow
        };
        _context.Categories.AddRange(homeCategory, carCategory);

        var homeTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = homeCategory.Id,
            Name = "HVAC Filter",
            FrequencyValue = 3,
            FrequencyUnit = FrequencyUnit.Months,
            CreatedDate = DateTime.UtcNow
        };
        var carTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = carCategory.Id,
            Name = "Oil Change",
            FrequencyValue = 5000,
            FrequencyUnit = FrequencyUnit.Days, // Represents miles approximated
            CreatedDate = DateTime.UtcNow
        };
        _context.Tasks.AddRange(homeTask, carTask);

        var homeLog = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = homeTask.Id,
            CompletedDate = DateTime.UtcNow.AddMonths(-2),
        };
        var carLog = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = carTask.Id,
            CompletedDate = DateTime.UtcNow.AddMonths(-1),
        };
        _context.TaskLogs.AddRange(homeLog, carLog);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GatherExportDataAsync("1.0.0");

        // Assert
        result.Categories.Should().HaveCount(2);
        result.Tasks.Should().HaveCount(2);
        result.TaskLogs.Should().HaveCount(2);

        // Verify relationships are maintained by IDs
        result.Tasks.Should().Contain(t => t.CategoryId == homeCategory.Id);
        result.Tasks.Should().Contain(t => t.CategoryId == carCategory.Id);
        result.TaskLogs.Should().Contain(l => l.TaskItemId == homeTask.Id);
        result.TaskLogs.Should().Contain(l => l.TaskItemId == carTask.Id);
    }

    #endregion

    #region SerializeExportData Tests

    [Fact]
    public void SerializeExportData_ProducesValidJson()
    {
        // Arrange
        var exportData = new ExportData
        {
            ExportedAt = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            AppVersion = "1.0.0 (1)",
            Categories = [new CategoryExport { Id = Guid.NewGuid(), Name = "Test" }],
            Tasks = [],
            TaskLogs = []
        };

        // Act
        var json = ExportService.SerializeExportData(exportData);

        // Assert
        json.Should().NotBeNullOrEmpty();
        
        // Verify it's valid JSON by deserializing
        var deserialized = JsonSerializer.Deserialize<JsonDocument>(json);
        deserialized.Should().NotBeNull();
    }

    [Fact]
    public void SerializeExportData_UsesCamelCase()
    {
        // Arrange
        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [new CategoryExport { Id = Guid.NewGuid(), Name = "Test", IsDefault = true }],
            Tasks = [],
            TaskLogs = []
        };

        // Act
        var json = ExportService.SerializeExportData(exportData);

        // Assert
        json.Should().Contain("\"exportedAt\"");
        json.Should().Contain("\"appVersion\"");
        json.Should().Contain("\"isDefault\"");
        json.Should().NotContain("\"ExportedAt\"");
        json.Should().NotContain("\"AppVersion\"");
        json.Should().NotContain("\"IsDefault\"");
    }

    [Fact]
    public void SerializeExportData_OmitsNullValues()
    {
        // Arrange
        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [new CategoryExport { Id = Guid.NewGuid(), Name = "Test", Icon = null }],
            Tasks = [new TaskExport { Id = Guid.NewGuid(), Name = "Task", Description = null }],
            TaskLogs = []
        };

        // Act
        var json = ExportService.SerializeExportData(exportData);

        // Assert
        // When Icon or Description is null, it should be omitted from JSON
        json.Should().NotContain("\"icon\":null");
        json.Should().NotContain("\"description\":null");
    }

    [Fact]
    public void SerializeExportData_IsIndented()
    {
        // Arrange
        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [],
            Tasks = [],
            TaskLogs = []
        };

        // Act
        var json = ExportService.SerializeExportData(exportData);

        // Assert - indented JSON contains newlines
        json.Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void SerializeExportData_IncludesAllFrequencyUnits()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [],
            Tasks =
            [
                new TaskExport { Id = Guid.NewGuid(), CategoryId = categoryId, Name = "Daily", FrequencyUnit = "Days" },
                new TaskExport { Id = Guid.NewGuid(), CategoryId = categoryId, Name = "Weekly", FrequencyUnit = "Weeks" },
                new TaskExport { Id = Guid.NewGuid(), CategoryId = categoryId, Name = "Monthly", FrequencyUnit = "Months" }
            ],
            TaskLogs = []
        };

        // Act
        var json = ExportService.SerializeExportData(exportData);

        // Assert
        json.Should().Contain("\"frequencyUnit\": \"Days\"");
        json.Should().Contain("\"frequencyUnit\": \"Weeks\"");
        json.Should().Contain("\"frequencyUnit\": \"Months\"");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task GatherAndSerialize_ProducesCompleteExportFile()
    {
        // Arrange - Set up realistic data
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Home Maintenance",
            Icon = "🏠",
            IsDefault = true,
            CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _context.Categories.Add(category);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Replace HVAC Filter",
            Description = "Change the air filter in the HVAC system",
            FrequencyValue = 3,
            FrequencyUnit = FrequencyUnit.Months,
            IsReminderEnabled = true,
            CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _context.Tasks.Add(task);

        var log = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = task.Id,
            CompletedDate = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            Notes = "Used MERV 13 filter"
        };
        _context.TaskLogs.Add(log);
        await _context.SaveChangesAsync();

        // Act
        var exportData = await _sut.GatherExportDataAsync("1.0.0 (1)");
        var json = ExportService.SerializeExportData(exportData);

        // Assert - Verify JSON contains all expected data
        json.Should().Contain("\"appVersion\": \"1.0.0 (1)\"");
        json.Should().Contain("\"name\": \"Home Maintenance\"");
        // Icon may be Unicode escaped in JSON, so check for the icon key
        json.Should().Contain("\"icon\":");
        json.Should().Contain("\"name\": \"Replace HVAC Filter\"");
        json.Should().Contain("\"description\": \"Change the air filter in the HVAC system\"");
        json.Should().Contain("\"frequencyValue\": 3");
        json.Should().Contain("\"frequencyUnit\": \"Months\"");
        json.Should().Contain("\"notes\": \"Used MERV 13 filter\"");
    }

    #endregion
}
