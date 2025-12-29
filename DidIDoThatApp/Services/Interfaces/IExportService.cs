namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Result of an import operation.
/// </summary>
public record ImportResult(bool Success, string Message, int CategoriesImported = 0, int TasksImported = 0, int LogsImported = 0);

/// <summary>
/// Service for exporting and importing app data.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports all app data as a JSON file and prompts the user to save it.
    /// </summary>
    /// <returns>True if export was successful, false otherwise.</returns>
    Task<bool> ExportDataAsJsonAsync();

    /// <summary>
    /// Imports app data from a JSON file selected by the user.
    /// </summary>
    /// <returns>Result containing success status and details about imported items.</returns>
    Task<ImportResult> ImportDataFromJsonAsync();
}
