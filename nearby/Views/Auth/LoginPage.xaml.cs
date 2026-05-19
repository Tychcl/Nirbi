using nearby.ViewModels;

namespace nearby.Views.Auth;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
        BindingContext = viewModel;
        InitializeComponent();   
    }
}