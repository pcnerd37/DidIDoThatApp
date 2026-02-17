using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Views;

public partial class CategoryPage : ContentPage
{
    public CategoryPage(CategoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is CategoryViewModel vm)
        {
            try
            {
                await App.DatabaseInitializedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database init failed: {ex}");
                await DisplayAlert("Error", 
                    "The database failed to initialize. Please restart the app.", "OK");
                return;  // Don't try to load data
            }

            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
