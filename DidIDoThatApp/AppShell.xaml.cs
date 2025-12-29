using DidIDoThatApp.Helpers;
using DidIDoThatApp.Views;

namespace DidIDoThatApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute(Constants.Routes.TaskDetail, typeof(TaskDetailPage));
            Routing.RegisterRoute(Constants.Routes.AddEditTask, typeof(AddEditTaskPage));
        }
    }
}
