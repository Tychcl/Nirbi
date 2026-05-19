using nearby.ViewModels;

namespace nearby.Views.Auth;

public partial class RegPage : ContentPage
{
	public RegPage(RegViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}