using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is SettingsViewModel vm)
        {
            try
            {
                await App.DatabaseInitializedTask;
                await vm.LoadSettingsCommand.ExecuteAsync(null);
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
