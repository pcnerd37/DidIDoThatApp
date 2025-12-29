using DidIDoThatApp.Data;
using DidIDoThatApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DidIDoThatApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            
            // Initialize database on startup
            Task.Run(async () => await InitializeDatabaseAsync());
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

                // Ensure database is created and migrated
                await dbContext.Database.EnsureCreatedAsync();

                // Seed initial data
                await databaseInitializer.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
            }
        }
    }
}