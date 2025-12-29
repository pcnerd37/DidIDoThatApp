using DidIDoThatApp.ViewModels;

namespace DidIDoThatApp.Views;

public partial class TaskDetailPage : ContentPage
{
    public TaskDetailPage(TaskDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
