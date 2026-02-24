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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database init failed: {ex}");
                return;
            }

            try
            {
                await vm.LoadDataCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TaskListPage load failed: {ex}");
            }
        }
    }
}