using System.Reflection;
using nearby.Classes;
using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class ThemeChangePage : ContentPage
{
	public ThemeChangePage(ThemeChangeViewModel vm)
	{
        BindingContext = vm;
		InitializeComponent();
	}
}