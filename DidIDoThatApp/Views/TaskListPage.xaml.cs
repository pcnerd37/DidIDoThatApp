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
            await App.DatabaseInitializedTask;
            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
