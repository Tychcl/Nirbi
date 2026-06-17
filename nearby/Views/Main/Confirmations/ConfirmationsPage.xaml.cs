using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class ConfirmationsPage : ContentPage
{
    public ConfirmationsPage(ConfirmationsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ConfirmationsViewModel vm)
            await vm.LoadDataCommand.ExecuteAsync(null);
    }
}