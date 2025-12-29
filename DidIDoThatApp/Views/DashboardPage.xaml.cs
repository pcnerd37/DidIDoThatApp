using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is DashboardViewModel vm)
        {
            // Wait for database to be initialized before loading data
            await App.DatabaseInitializedTask;
            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
