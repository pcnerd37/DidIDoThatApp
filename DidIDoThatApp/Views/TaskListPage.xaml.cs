using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Views;

public partial class TaskListPage : ContentPage
{
    public TaskListPage(TaskListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is TaskListViewModel vm)
        {
            try
            {
                await App.DatabaseInitializedTask;
                await vm.LoadDataCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database init failed: {ex}");
                var errorDetail = App.InitializationError ?? ex.Message;
                await DisplayAlert("Error", 
                    $"The database failed to initialize: {errorDetail}", "OK");
                return;
            }
        }
    }
}
