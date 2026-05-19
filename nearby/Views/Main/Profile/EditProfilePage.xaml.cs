using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class EditProfilePage : ContentPage
{
	public EditProfilePage(EditProfileViewModel viewModel)
	{
        BindingContext = viewModel;
        InitializeComponent();
    }
}