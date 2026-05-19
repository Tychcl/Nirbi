using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Classes;
using nearby.Views.Main;

namespace nearby.ViewModels
{

    public partial class ThemeChangeViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int primaryFontSize = (int)ResourceManager.Get("PrimaryFontSize");
        async partial void OnPrimaryFontSizeChanged(int value)
        {
            await ResourceManager.SetSave<int>("PrimaryFontSize", primaryFontSize);
        }

        [ObservableProperty]
        private int secondaryFontSize = (int)ResourceManager.Get("SecondaryFontSize");
        async partial void OnSecondaryFontSizeChanged(int value)
        {
            await ResourceManager.SetSave<int>("SecondaryFontSize", SecondaryFontSize);
        }

        [ObservableProperty]
        private List<string> resources = new();

        public ThemeChangeViewModel()
        {
            var themes = typeof(ThemeManager).GetFields();
            foreach (var t in themes)
            {
                Resources.Add(t.Name);
            }
        }
    }
}