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
            
            // Initialize database and prefetch data
            MainThread.BeginInvokeOnMainThread(async () => await InitializeDatabaseAsync());
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
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                // Signal failure as an exception so awaiting code knows it failed
                _databaseInitialized.TrySetException(ex);
            }
        }
    }
}