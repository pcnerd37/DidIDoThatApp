namespace DidIDoThatApp.Services.Interfaces;

/// <summary>
/// Service for exporting app data.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports all app data as a JSON file and prompts the user to save it.
    /// </summary>
    /// <returns>True if export was successful, false otherwise.</returns>
    Task<bool> ExportDataAsJsonAsync();
}
