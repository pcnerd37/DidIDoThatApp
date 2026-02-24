using DidIDoThatApp.Data;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DidIDoThatApp
{
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
            
            _ = Task.Run(async () => await InitializeDatabaseAsync());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                // Use a dedicated scope for initialization only
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                // Ensure database is created
                await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);

                // Seed default data
                var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                await databaseInitializer.InitializeAsync().ConfigureAwait(false);
                
                _databaseInitialized.TrySetResult(true);

                // Prefetch in background after init completes
                try
                {
                    if (DataPrefetchService != null)
                    {
                        await DataPrefetchService.PrefetchAllAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Prefetch error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _initializationError = $"{ex.GetType().Name}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex}");
                _databaseInitialized.TrySetException(ex);
            }
        }
    }
}