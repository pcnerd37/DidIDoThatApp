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
            await App.DatabaseInitializedTask;
            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
