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
                var errorDetail = App.InitializationError ?? ex.Message;
                await DisplayAlert("Error", 
                    $"The database failed to initialize: {errorDetail}", "OK");
                return;
            }

            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}