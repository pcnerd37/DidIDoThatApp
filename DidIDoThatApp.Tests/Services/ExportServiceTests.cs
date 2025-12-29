using System.Text.Json;
using DidIDoThatApp.Data;
using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services;
using DidIDoThatApp.Services.Interfaces;
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

    #region DeserializeExportData Tests

    [Fact]
    public void DeserializeExportData_WithValidJson_ReturnsExportData()
    {
        // Arrange
        var json = """
        {
            "exportedAt": "2025-01-15T12:00:00Z",
            "appVersion": "1.0.0",
            "categories": [
                {
                    "id": "11111111-1111-1111-1111-111111111111",
                    "name": "Home",
                    "icon": "🏠",
                    "createdDate": "2025-01-01T00:00:00Z",
                    "isDefault": true
                }
            ],
            "tasks": [],
            "taskLogs": []
        }
        """;

        // Act
        var result = ExportService.DeserializeExportData(json);

        // Assert
        result.Should().NotBeNull();
        result!.AppVersion.Should().Be("1.0.0");
        result.Categories.Should().HaveCount(1);
        result.Categories[0].Name.Should().Be("Home");
        result.Categories[0].Icon.Should().Be("🏠");
        result.Categories[0].IsDefault.Should().BeTrue();
    }

    [Fact]
    public void DeserializeExportData_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var json = "{ this is not valid json }";

        // Act
        var result = ExportService.DeserializeExportData(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DeserializeExportData_IsCaseInsensitive()
    {
        // Arrange - Use PascalCase instead of camelCase
        var json = """
        {
            "ExportedAt": "2025-01-15T12:00:00Z",
            "AppVersion": "1.0.0",
            "Categories": [],
            "Tasks": [],
            "TaskLogs": []
        }
        """;

        // Act
        var result = ExportService.DeserializeExportData(json);

        // Assert
        result.Should().NotBeNull();
        result!.AppVersion.Should().Be("1.0.0");
    }

    [Fact]
    public void DeserializeExportData_WithCompleteData_ParsesAllFields()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var logId = Guid.NewGuid();

        var json = $$"""
        {
            "exportedAt": "2025-01-15T12:00:00Z",
            "appVersion": "1.0.0 (5)",
            "categories": [
                {
                    "id": "{{categoryId}}",
                    "name": "Car Maintenance",
                    "icon": "🚗",
                    "createdDate": "2025-01-01T00:00:00Z",
                    "isDefault": false
                }
            ],
            "tasks": [
                {
                    "id": "{{taskId}}",
                    "categoryId": "{{categoryId}}",
                    "name": "Oil Change",
                    "description": "Change engine oil every 5000 miles",
                    "frequencyValue": 90,
                    "frequencyUnit": "Days",
                    "isReminderEnabled": true,
                    "createdDate": "2025-01-05T00:00:00Z"
                }
            ],
            "taskLogs": [
                {
                    "id": "{{logId}}",
                    "taskItemId": "{{taskId}}",
                    "completedDate": "2025-01-10T14:30:00Z",
                    "notes": "Used synthetic oil"
                }
            ]
        }
        """;

        // Act
        var result = ExportService.DeserializeExportData(json);

        // Assert
        result.Should().NotBeNull();
        
        result!.Categories.Should().HaveCount(1);
        result.Categories[0].Id.Should().Be(categoryId);
        result.Categories[0].Name.Should().Be("Car Maintenance");
        
        result.Tasks.Should().HaveCount(1);
        result.Tasks[0].Id.Should().Be(taskId);
        result.Tasks[0].CategoryId.Should().Be(categoryId);
        result.Tasks[0].Name.Should().Be("Oil Change");
        result.Tasks[0].Description.Should().Be("Change engine oil every 5000 miles");
        result.Tasks[0].FrequencyValue.Should().Be(90);
        result.Tasks[0].FrequencyUnit.Should().Be("Days");
        result.Tasks[0].IsReminderEnabled.Should().BeTrue();
        
        result.TaskLogs.Should().HaveCount(1);
        result.TaskLogs[0].Id.Should().Be(logId);
        result.TaskLogs[0].TaskItemId.Should().Be(taskId);
        result.TaskLogs[0].Notes.Should().Be("Used synthetic oil");
    }

    #endregion

    #region Import Tests (using ImportDataAsync via reflection or public wrapper)

    [Fact]
    public async Task Import_WithNewCategories_AddsCategoriesToDatabase()
    {
        // Arrange
        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories =
            [
                new CategoryExport
                {
                    Id = Guid.NewGuid(),
                    Name = "Imported Category",
                    Icon = "📦",
                    CreatedDate = DateTime.UtcNow,
                    IsDefault = false
                }
            ],
            Tasks = [],
            TaskLogs = []
        };

        // Serialize and deserialize to simulate real import
        var json = ExportService.SerializeExportData(exportData);
        var parsed = ExportService.DeserializeExportData(json);

        // Act - Use the private import method via a test wrapper
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.CategoriesImported.Should().Be(1);
        
        var categories = await _context.Categories.ToListAsync();
        categories.Should().HaveCount(1);
        categories[0].Name.Should().Be("Imported Category");
    }

    [Fact]
    public async Task Import_WithExistingCategory_SkipsCategory()
    {
        // Arrange - Add existing category
        var existingId = Guid.NewGuid();
        _context.Categories.Add(new Category
        {
            Id = existingId,
            Name = "Existing",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories =
            [
                new CategoryExport
                {
                    Id = existingId, // Same ID as existing
                    Name = "Different Name",
                    CreatedDate = DateTime.UtcNow,
                    IsDefault = false
                }
            ],
            Tasks = [],
            TaskLogs = []
        };

        var json = ExportService.SerializeExportData(exportData);
        var parsed = ExportService.DeserializeExportData(json);

        // Act
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.CategoriesImported.Should().Be(0);
        
        var categories = await _context.Categories.ToListAsync();
        categories.Should().HaveCount(1);
        categories[0].Name.Should().Be("Existing"); // Not overwritten
    }

    [Fact]
    public async Task Import_WithNewTask_AddsTaskToDatabase()
    {
        // Arrange - Need existing category for the task
        var categoryId = Guid.NewGuid();
        _context.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Home",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [], // Category already exists
            Tasks =
            [
                new TaskExport
                {
                    Id = Guid.NewGuid(),
                    CategoryId = categoryId,
                    Name = "Imported Task",
                    Description = "Test description",
                    FrequencyValue = 30,
                    FrequencyUnit = "Days",
                    IsReminderEnabled = true,
                    CreatedDate = DateTime.UtcNow
                }
            ],
            TaskLogs = []
        };

        var json = ExportService.SerializeExportData(exportData);
        var parsed = ExportService.DeserializeExportData(json);

        // Act
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.TasksImported.Should().Be(1);
        
        var tasks = await _context.Tasks.ToListAsync();
        tasks.Should().HaveCount(1);
        tasks[0].Name.Should().Be("Imported Task");
        tasks[0].FrequencyValue.Should().Be(30);
        tasks[0].FrequencyUnit.Should().Be(FrequencyUnit.Days);
    }

    [Fact]
    public async Task Import_WithTaskForMissingCategory_SkipsTask()
    {
        // Arrange - No category exists
        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [],
            Tasks =
            [
                new TaskExport
                {
                    Id = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(), // Non-existent category
                    Name = "Orphan Task",
                    FrequencyValue = 30,
                    FrequencyUnit = "Days",
                    CreatedDate = DateTime.UtcNow
                }
            ],
            TaskLogs = []
        };

        var json = ExportService.SerializeExportData(exportData);
        var parsed = ExportService.DeserializeExportData(json);

        // Act
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.TasksImported.Should().Be(0);
        
        var tasks = await _context.Tasks.ToListAsync();
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task Import_WithNewTaskLog_AddsLogToDatabase()
    {
        // Arrange - Need existing category and task
        var categoryId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        
        _context.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Home",
            CreatedDate = DateTime.UtcNow
        });
        _context.Tasks.Add(new TaskItem
        {
            Id = taskId,
            CategoryId = categoryId,
            Name = "Existing Task",
            FrequencyValue = 30,
            FrequencyUnit = FrequencyUnit.Days,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [],
            Tasks = [],
            TaskLogs =
            [
                new TaskLogExport
                {
                    Id = Guid.NewGuid(),
                    TaskItemId = taskId,
                    CompletedDate = DateTime.UtcNow.AddDays(-5),
                    Notes = "Imported log entry"
                }
            ]
        };

        var json = ExportService.SerializeExportData(exportData);
        var parsed = ExportService.DeserializeExportData(json);

        // Act
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.LogsImported.Should().Be(1);
        
        var logs = await _context.TaskLogs.ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].Notes.Should().Be("Imported log entry");
    }

    [Fact]
    public async Task Import_WithCompleteDataSet_ImportsAllEntities()
    {
        // Arrange - Full export with category, task, and log
        var categoryId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var logId = Guid.NewGuid();

        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories =
            [
                new CategoryExport
                {
                    Id = categoryId,
                    Name = "Full Import Test",
                    Icon = "🧪",
                    CreatedDate = DateTime.UtcNow,
                    IsDefault = false
                }
            ],
            Tasks =
            [
                new TaskExport
                {
                    Id = taskId,
                    CategoryId = categoryId,
                    Name = "Test Task",
                    Description = "Full test",
                    FrequencyValue = 7,
                    FrequencyUnit = "Weeks",
                    IsReminderEnabled = true,
                    CreatedDate = DateTime.UtcNow
                }
            ],
            TaskLogs =
            [
                new TaskLogExport
                {
                    Id = logId,
                    TaskItemId = taskId,
                    CompletedDate = DateTime.UtcNow.AddDays(-7),
                    Notes = "First completion"
                }
            ]
        };

        var json = ExportService.SerializeExportData(exportData);
        var parsed = ExportService.DeserializeExportData(json);

        // Act
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.CategoriesImported.Should().Be(1);
        result.TasksImported.Should().Be(1);
        result.LogsImported.Should().Be(1);
        result.Message.Should().Contain("1 categories");
        result.Message.Should().Contain("1 tasks");
        result.Message.Should().Contain("1 completion records");
    }

    [Fact]
    public async Task Import_WhenAllDataExists_ReturnsNoNewDataMessage()
    {
        // Arrange - Add data to database first
        var categoryId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var logId = Guid.NewGuid();

        _context.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Existing",
            CreatedDate = DateTime.UtcNow
        });
        _context.Tasks.Add(new TaskItem
        {
            Id = taskId,
            CategoryId = categoryId,
            Name = "Existing Task",
            FrequencyValue = 30,
            FrequencyUnit = FrequencyUnit.Days,
            CreatedDate = DateTime.UtcNow
        });
        _context.TaskLogs.Add(new TaskLog
        {
            Id = logId,
            TaskItemId = taskId,
            CompletedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Try to import the same IDs
        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [new CategoryExport { Id = categoryId, Name = "Different", CreatedDate = DateTime.UtcNow }],
            Tasks = [new TaskExport { Id = taskId, CategoryId = categoryId, Name = "Different", FrequencyValue = 1, FrequencyUnit = "Days", CreatedDate = DateTime.UtcNow }],
            TaskLogs = [new TaskLogExport { Id = logId, TaskItemId = taskId, CompletedDate = DateTime.UtcNow }]
        };

        var json = ExportService.SerializeExportData(exportData);
        var parsed = ExportService.DeserializeExportData(json);

        // Act
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.CategoriesImported.Should().Be(0);
        result.TasksImported.Should().Be(0);
        result.LogsImported.Should().Be(0);
        result.Message.Should().Contain("No new data to import");
    }

    [Fact]
    public async Task Import_WithInvalidFrequencyUnit_DefaultsToDays()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _context.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Home",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0",
            Categories = [],
            Tasks =
            [
                new TaskExport
                {
                    Id = Guid.NewGuid(),
                    CategoryId = categoryId,
                    Name = "Task with Invalid Unit",
                    FrequencyValue = 30,
                    FrequencyUnit = "InvalidUnit", // Invalid
                    CreatedDate = DateTime.UtcNow
                }
            ],
            TaskLogs = []
        };

        var json = ExportService.SerializeExportData(exportData);
        var parsed = ExportService.DeserializeExportData(json);

        // Act
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.TasksImported.Should().Be(1);
        
        var task = await _context.Tasks.FirstAsync();
        task.FrequencyUnit.Should().Be(FrequencyUnit.Days); // Defaults to Days
    }

    /// <summary>
    /// Helper to invoke the private ImportDataAsync method for testing.
    /// In production, this is called by ImportDataFromJsonAsync after file picking.
    /// </summary>
    private async Task<ImportResult> ImportDataViaServiceAsync(ExportData exportData)
    {
        // Use reflection to call the private ImportDataAsync method
        var methodInfo = typeof(ExportService).GetMethod("ImportDataAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var task = (Task<ImportResult>)methodInfo!.Invoke(_sut, [exportData])!;
        return await task;
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public async Task ExportThenImport_PreservesAllData()
    {
        // Arrange - Create complete dataset
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Round Trip Test",
            Icon = "🔄",
            CreatedDate = DateTime.UtcNow,
            IsDefault = false
        };
        _context.Categories.Add(category);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Round Trip Task",
            Description = "Testing full export/import cycle",
            FrequencyValue = 14,
            FrequencyUnit = FrequencyUnit.Days,
            IsReminderEnabled = true,
            CreatedDate = DateTime.UtcNow
        };
        _context.Tasks.Add(task);

        var log = new TaskLog
        {
            Id = Guid.NewGuid(),
            TaskItemId = task.Id,
            CompletedDate = DateTime.UtcNow.AddDays(-7),
            Notes = "Round trip log"
        };
        _context.TaskLogs.Add(log);
        await _context.SaveChangesAsync();

        // Act - Export
        var exportData = await _sut.GatherExportDataAsync("1.0.0");
        var json = ExportService.SerializeExportData(exportData);

        // Clear database to simulate new device
        _context.TaskLogs.RemoveRange(_context.TaskLogs);
        _context.Tasks.RemoveRange(_context.Tasks);
        _context.Categories.RemoveRange(_context.Categories);
        await _context.SaveChangesAsync();

        // Verify empty
        (await _context.Categories.CountAsync()).Should().Be(0);

        // Act - Import
        var parsed = ExportService.DeserializeExportData(json);
        var result = await ImportDataViaServiceAsync(parsed!);

        // Assert
        result.Success.Should().BeTrue();
        result.CategoriesImported.Should().Be(1);
        result.TasksImported.Should().Be(1);
        result.LogsImported.Should().Be(1);

        // Verify data integrity
        var importedCategory = await _context.Categories.FirstAsync();
        importedCategory.Id.Should().Be(category.Id);
        importedCategory.Name.Should().Be("Round Trip Test");
        importedCategory.Icon.Should().Be("🔄");

        var importedTask = await _context.Tasks.FirstAsync();
        importedTask.Id.Should().Be(task.Id);
        importedTask.Name.Should().Be("Round Trip Task");
        importedTask.FrequencyValue.Should().Be(14);
        importedTask.FrequencyUnit.Should().Be(FrequencyUnit.Days);

        var importedLog = await _context.TaskLogs.FirstAsync();
        importedLog.Id.Should().Be(log.Id);
        importedLog.Notes.Should().Be("Round trip log");
    }

    #endregion
}
