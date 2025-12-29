using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Views;

public partial class AddEditTaskPage : ContentPage
{
    public AddEditTaskPage(AddEditTaskViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is AddEditTaskViewModel vm)
        {
            vm.LoadCategoriesCommand.Execute(null);
        }
    }
}
