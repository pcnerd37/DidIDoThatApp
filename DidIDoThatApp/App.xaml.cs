using DidIDoThatApp.Data;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DidIDoThatApp;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly TaskCompletionSource<bool> _databaseInitialized = new();
    private static string? _initializationError;

    /// <summary>
    /// Awaitable task that completes when the database is fully initialized.
    /// </summary>
    public static Task DatabaseInitializedTask => _databaseInitialized.Task;

    /// <summary>
    /// If database initialization failed, contains the error details.
    /// </summary>
    public static string? InitializationError => _initializationError;

    /// <summary>
    /// Data prefetch service for fast page loading.
    /// </summary>
    public static IDataPrefetchService? DataPrefetchService { get; private set; }

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        DataPrefetchService = serviceProvider.GetService<IDataPrefetchService>();

        // Start database initialization as a fire-and-forget async task.
        // This begins on the main thread (safe for Preferences/NSUserDefaults),
        // yields at the first await, and completes asynchronously.
        // Pages await DatabaseInitializedTask in OnAppearing which will
        // suspend until this completes — no deadlock because OnAppearing
        // also yields at its await.
        _ = InitializeDatabaseAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            // Yield immediately to let the UI startup continue.
            // This ensures CreateWindow/AppShell can proceed, and 
            // we resume on the main thread after the UI is set up.
            await Task.Yield();

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

            await dbContext.Database.EnsureCreatedAsync();
            await databaseInitializer.InitializeAsync();

            _databaseInitialized.TrySetResult(true);

            // Prefetch on background thread (no Preferences access, just DB reads)
            _ = Task.Run(async () =>
            {
                try
                {
                    if (DataPrefetchService != null)
                    {
                        await DataPrefetchService.PrefetchAllAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Prefetch error: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            _initializationError = $"{ex.GetType().Name}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex}");
            _databaseInitialized.TrySetException(ex);
        }
    }
}