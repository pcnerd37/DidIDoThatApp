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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings init failed: {ex}");
                return;
            }

            try
            {
                await vm.LoadSettingsCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings load failed: {ex}");
            }
        }
    }
}