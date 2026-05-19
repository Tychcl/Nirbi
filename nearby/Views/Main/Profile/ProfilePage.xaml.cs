using System.Diagnostics;
using System.Threading.Tasks;
using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class ProfilePage : ContentPage
{

    public ProfilePage(ProfileViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if(BindingContext is ProfileViewModel vm && vm.UserId == -99)
        {
            vm.UserId = -1;
        }
    }

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    (BindingContext as IDisposable)?.Dispose();
    //}
}