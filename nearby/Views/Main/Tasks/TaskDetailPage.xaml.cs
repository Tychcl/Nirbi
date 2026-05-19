using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class TaskDetailPage : ContentPage
{
    public TaskDetailPage(TaskDetailViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            if (BindingContext is TaskDetailViewModel vm)
            {
                await vm.RefreshAsync();
            }
        }
        catch { }
    }
}