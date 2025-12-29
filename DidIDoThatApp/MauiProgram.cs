using DidIDoThatApp.Data;
using DidIDoThatApp.Services;
using DidIDoThatApp.Services.Interfaces;
using DidIDoThatApp.ViewModels;
using DidIDoThatApp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using IAppNotificationService = DidIDoThatApp.Services.Interfaces.INotificationService;
using IDataPrefetchService = DidIDoThatApp.Services.Interfaces.IDataPrefetchService;

#if ANDROID
using DidIDoThatApp.Platforms.Android.Services;
#endif

namespace DidIDoThatApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register database context
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={Helpers.Constants.DatabasePath}");
            });

            // Register services
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<IDataPrefetchService, DataPrefetchService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<ITaskLogService, TaskLogService>();
            builder.Services.AddScoped<IAppNotificationService, NotificationService>();
            builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
            builder.Services.AddScoped<IExportService, ExportService>();
            
            // Register platform-specific background task service
#if ANDROID
            builder.Services.AddSingleton<IBackgroundTaskService, AndroidBackgroundTaskService>();
#else
            builder.Services.AddSingleton<IBackgroundTaskService, BackgroundTaskService>();
#endif

            // Register ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<TaskListViewModel>();
            builder.Services.AddTransient<TaskDetailViewModel>();
            builder.Services.AddTransient<AddEditTaskViewModel>();
            builder.Services.AddTransient<CategoryViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Register Views
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<TaskListPage>();
            builder.Services.AddTransient<TaskDetailPage>();
            builder.Services.AddTransient<AddEditTaskPage>();
            builder.Services.AddTransient<CategoryPage>();
            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
