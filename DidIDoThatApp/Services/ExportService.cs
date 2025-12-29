using System.Text.Json;
using System.Text.Json.Serialization;
using DidIDoThatApp.Data;
using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DidIDoThatApp.Services;

/// <summary>
/// Data transfer object for exporting app data.
/// </summary>
public class ExportData
{
    /// <summary>
    /// The date and time this export was created.
    /// </summary>
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The app version that created this export.
    /// </summary>
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// All categories in the database.
    /// </summary>
    public List<CategoryExport> Categories { get; set; } = [];

    /// <summary>
    /// All tasks in the database.
    /// </summary>
    public List<TaskExport> Tasks { get; set; } = [];

    /// <summary>
    /// All task completion logs in the database.
    /// </summary>
    public List<TaskLogExport> TaskLogs { get; set; } = [];
}

/// <summary>
/// Category data for export.
/// </summary>
public class CategoryExport
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Task data for export.
/// </summary>
public class TaskExport
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int FrequencyValue { get; set; }
    public string FrequencyUnit { get; set; } = string.Empty;
    public bool IsReminderEnabled { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Task log data for export.
/// </summary>
public class TaskLogExport
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public DateTime CompletedDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Service for exporting app data.
/// </summary>
public class ExportService : IExportService
{
    private readonly AppDbContext _context;

    public ExportService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gathers all app data into an ExportData object.
    /// This method is public for testability.
    /// </summary>
    public async Task<ExportData> GatherExportDataAsync(string? appVersion = null)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .ToListAsync();

        var tasks = await _context.Tasks
            .AsNoTracking()
            .ToListAsync();

        var taskLogs = await _context.TaskLogs
            .AsNoTracking()
            .ToListAsync();

        return new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            AppVersion = appVersion ?? $"{AppInfo.VersionString} ({AppInfo.BuildString})",
            Categories = categories.Select(c => new CategoryExport
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon,
                CreatedDate = c.CreatedDate,
                IsDefault = c.IsDefault
            }).ToList(),
            Tasks = tasks.Select(t => new TaskExport
            {
                Id = t.Id,
                CategoryId = t.CategoryId,
                Name = t.Name,
                Description = t.Description,
                FrequencyValue = t.FrequencyValue,
                FrequencyUnit = t.FrequencyUnit.ToString(),
                IsReminderEnabled = t.IsReminderEnabled,
                CreatedDate = t.CreatedDate
            }).ToList(),
            TaskLogs = taskLogs.Select(l => new TaskLogExport
            {
                Id = l.Id,
                TaskItemId = l.TaskItemId,
                CompletedDate = l.CompletedDate,
                Notes = l.Notes
            }).ToList()
        };
    }

    /// <summary>
    /// Serializes export data to JSON string.
    /// This method is public for testability.
    /// </summary>
    public static string SerializeExportData(ExportData exportData)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        return JsonSerializer.Serialize(exportData, jsonOptions);
    }

    /// <inheritdoc />
    public async Task<bool> ExportDataAsJsonAsync()
    {
        try
        {
            var exportData = await GatherExportDataAsync();
            var json = SerializeExportData(exportData);

            // Generate filename with timestamp
            var fileName = $"DidIDoThat_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            // Save to a temporary file first
            var tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(tempFilePath, json);

            // Use FileSaver or Share to let user save/share the file
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Export Data",
                File = new ShareFile(tempFilePath)
            });

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Export failed: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Deserializes JSON string to ExportData object.
    /// This method is public for testability.
    /// </summary>
    public static ExportData? DeserializeExportData(string json)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<ExportData>(json, jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ImportResult> ImportDataFromJsonAsync()
    {
        try
        {
            // Let user pick a JSON file
            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select Did I Do That? Export File",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.iOS, new[] { "public.json" } },
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.json" } }
                })
            });

            if (fileResult == null)
            {
                return new ImportResult(false, "No file selected.");
            }

            // Read the file
            using var stream = await fileResult.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            // Deserialize
            var exportData = DeserializeExportData(json);
            if (exportData == null)
            {
                return new ImportResult(false, "Invalid file format. Could not parse the export file.");
            }

            // Validate basic structure
            if (exportData.Categories == null || exportData.Tasks == null || exportData.TaskLogs == null)
            {
                return new ImportResult(false, "Invalid file format. Missing required data sections.");
            }

            // Import data
            return await ImportDataAsync(exportData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Import failed: {ex}");
            return new ImportResult(false, $"Import failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Imports the export data into the database.
    /// Merges with existing data - skips items that already exist (by ID).
    /// </summary>
    private async Task<ImportResult> ImportDataAsync(ExportData exportData)
    {
        int categoriesImported = 0;
        int tasksImported = 0;
        int logsImported = 0;

        // Get existing IDs to avoid duplicates
        var existingCategoryIds = await _context.Categories.Select(c => c.Id).ToListAsync();
        var existingTaskIds = await _context.Tasks.Select(t => t.Id).ToListAsync();
        var existingLogIds = await _context.TaskLogs.Select(l => l.Id).ToListAsync();

        // Import categories first (tasks depend on them)
        foreach (var categoryExport in exportData.Categories)
        {
            if (existingCategoryIds.Contains(categoryExport.Id))
                continue;

            var category = new Category
            {
                Id = categoryExport.Id,
                Name = categoryExport.Name,
                Icon = categoryExport.Icon,
                CreatedDate = categoryExport.CreatedDate,
                IsDefault = categoryExport.IsDefault
            };

            _context.Categories.Add(category);
            categoriesImported++;
        }

        // Save categories first so foreign key constraints work
        if (categoriesImported > 0)
        {
            await _context.SaveChangesAsync();
        }

        // Get updated category list for validation
        var allCategoryIds = await _context.Categories.Select(c => c.Id).ToListAsync();

        // Import tasks
        foreach (var taskExport in exportData.Tasks)
        {
            if (existingTaskIds.Contains(taskExport.Id))
                continue;

            // Skip if the category doesn't exist
            if (!allCategoryIds.Contains(taskExport.CategoryId))
                continue;

            if (!Enum.TryParse<FrequencyUnit>(taskExport.FrequencyUnit, true, out var frequencyUnit))
            {
                frequencyUnit = FrequencyUnit.Days;
            }

            var task = new TaskItem
            {
                Id = taskExport.Id,
                CategoryId = taskExport.CategoryId,
                Name = taskExport.Name,
                Description = taskExport.Description,
                FrequencyValue = taskExport.FrequencyValue,
                FrequencyUnit = frequencyUnit,
                IsReminderEnabled = taskExport.IsReminderEnabled,
                CreatedDate = taskExport.CreatedDate
            };

            _context.Tasks.Add(task);
            tasksImported++;
        }

        // Save tasks before logs
        if (tasksImported > 0)
        {
            await _context.SaveChangesAsync();
        }

        // Get updated task list for validation
        var allTaskIds = await _context.Tasks.Select(t => t.Id).ToListAsync();

        // Import task logs
        foreach (var logExport in exportData.TaskLogs)
        {
            if (existingLogIds.Contains(logExport.Id))
                continue;

            // Skip if the task doesn't exist
            if (!allTaskIds.Contains(logExport.TaskItemId))
                continue;

            var log = new TaskLog
            {
                Id = logExport.Id,
                TaskItemId = logExport.TaskItemId,
                CompletedDate = logExport.CompletedDate,
                Notes = logExport.Notes
            };

            _context.TaskLogs.Add(log);
            logsImported++;
        }

        // Save logs
        if (logsImported > 0)
        {
            await _context.SaveChangesAsync();
        }

        var message = $"Successfully imported {categoriesImported} categories, {tasksImported} tasks, and {logsImported} completion records.";
        if (categoriesImported == 0 && tasksImported == 0 && logsImported == 0)
        {
            message = "No new data to import. All items already exist in the app.";
        }

        return new ImportResult(true, message, categoriesImported, tasksImported, logsImported);
    }
}
