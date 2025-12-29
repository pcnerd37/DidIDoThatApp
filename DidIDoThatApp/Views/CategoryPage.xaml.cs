using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Views;

public partial class CategoryPage : ContentPage
{
    public CategoryPage(CategoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is CategoryViewModel vm)
        {
            vm.LoadDataCommand.Execute(null);
        }
    }
}
