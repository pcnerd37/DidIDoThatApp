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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database init failed: {ex}");
                return;
            }

            try
            {
                await vm.LoadCategoriesCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddEditTaskPage load failed: {ex}");
            }
        }
    }
}