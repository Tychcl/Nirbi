using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class TaskAddEditPage : ContentPage
{
	public TaskAddEditPage(TaskAddEditViewModel vm)
	{
		BindingContext = vm;
		InitializeComponent();
	}
}