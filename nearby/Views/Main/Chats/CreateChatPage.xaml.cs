using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class CreateChatPage : ContentPage
{
	public CreateChatPage(CreateChatViewModel vm)
	{
		BindingContext = vm;
		InitializeComponent();
	}
}