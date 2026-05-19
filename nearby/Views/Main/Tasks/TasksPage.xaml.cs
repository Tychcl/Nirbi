using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class TasksPage : ContentPage
{
    public TasksPage(TasksViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TasksViewModel vm && vm.Tasks.Count == 0)
        {
            await vm.LoadTasksCommand.ExecuteAsync(true);
        }
    }
}