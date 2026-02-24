using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Views;

public partial class AddEditTaskPage : ContentPage
{
    public AddEditTaskPage(AddEditTaskViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is AddEditTaskViewModel vm)
        {
            try
            {
                await App.DatabaseInitializedTask;
                await vm.LoadCategoriesCommand.ExecuteAsync(null);
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
