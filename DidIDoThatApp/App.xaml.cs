using DidIDoThatApp.Data;
using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly TaskCompletionSource<bool> _databaseInitialized = new();

        /// <summary>
        /// Awaitable task that completes when the database is fully initialized.
        /// </summary>
        public static Task DatabaseInitializedTask => _databaseInitialized.Task;

        /// <summary>
        /// Data prefetch service for fast page loading.
        /// </summary>
        public static IDataPrefetchService? DataPrefetchService { get; private set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // Get the prefetch service for static access
            DataPrefetchService = serviceProvider.GetService<IDataPrefetchService>();

            // Initialize database on a background thread BEFORE any pages load.
            // Do NOT use MainThread.BeginInvokeOnMainThread here — that posts to the
            // message queue which runs AFTER CreateWindow, causing a deadlock when
            // pages await DatabaseInitializedTask on the main thread.
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
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

                await dbContext.Database.EnsureCreatedAsync();
                await databaseInitializer.InitializeAsync();

                _databaseInitialized.TrySetResult(true);

                // Prefetch data in the same background context for fast page loads.
                // This runs AFTER the TrySetResult so pages can start loading immediately
                // while prefetch populates the cache.
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                _databaseInitialized.TrySetException(ex);
            }
        }
    }
}