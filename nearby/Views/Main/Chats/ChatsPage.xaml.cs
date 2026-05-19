using System.Reflection;
using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class ChatsPage : ContentPage
{
    public ChatsPage(ChatsViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (BindingContext is ChatsViewModel vm && vm.Chats.Count == 0)
            {
                vm.LoadChatsCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex) 
        {
            var inner = ex.InnerException ?? ex;
            System.Diagnostics.Debug.WriteLine($"LoadChatsBaseAsync error: {inner.GetType()}: {inner.Message}");
            if (ex is TargetInvocationException tie)
                System.Diagnostics.Debug.WriteLine($"Real error: {tie.InnerException?.Message}");
            System.Diagnostics.Debug.WriteLine(inner.Message);
        }
    }
}