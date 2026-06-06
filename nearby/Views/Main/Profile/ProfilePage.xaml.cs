using System.Diagnostics;
using System.Threading.Tasks;
using nearby.Services;
using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class ProfilePage : ContentPage
{
    private IUserService _userService;
    public ProfilePage(ProfileViewModel viewModel, IUserService us)
    {
        BindingContext = viewModel;
        _userService = us;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if(BindingContext is ProfileViewModel vm && vm.UserId == null)
        {
            vm.UserId = _userService.CurrentUserId;
        }
    }

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    (BindingContext as IDisposable)?.Dispose();
    //}
}