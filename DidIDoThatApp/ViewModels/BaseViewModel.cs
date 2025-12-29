using CommunityToolkit.Mvvm.ComponentModel;

namespace DidIDoThatApp.ViewModels;

/// <summary>
/// Base class for all ViewModels providing common functionality.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Executes an async operation with busy state management and error handling.
    /// Use allowReentry=true for RefreshView commands to avoid spinner lock.
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> operation, string? errorMessage = null, bool allowReentry = false)
    {
        if (IsBusy && !allowReentry)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await operation();
        }
        catch (Exception ex)
        {
            ErrorMessage = errorMessage ?? ex.Message;
            System.Diagnostics.Debug.WriteLine($"Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Executes an async operation with busy state management, error handling, and return value.
    /// Use allowReentry=true for RefreshView commands to avoid spinner lock.
    /// </summary>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? errorMessage = null, bool allowReentry = false)
    {
        if (IsBusy && !allowReentry)
            return default;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            return await operation();
        }
        catch (Exception ex)
        {
            ErrorMessage = errorMessage ?? ex.Message;
            System.Diagnostics.Debug.WriteLine($"Error: {ex}");
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
