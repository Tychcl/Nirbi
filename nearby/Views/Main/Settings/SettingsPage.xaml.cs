using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel vm)
	{
        BindingContext = vm;
        InitializeComponent();
	}
}