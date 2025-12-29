using System.Text.Json;
using System.Text.Json.Serialization;
using DidIDoThatApp.Data;
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
}
