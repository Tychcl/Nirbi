using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Views.Main;
using nearby.Classes;

namespace nearby.ViewModels
{

    public partial class SettingsViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<SettingsItem> settingsItems = new()
        {
            new SettingsItem
            {
                Icon = (string)ResourceManager.Get("Theme"),
                Text = "Внешний вид",
                Page = nameof(ThemeChangePage)
            },
            new SettingsItem
            {
                Icon = (string)ResourceManager.Get("Language"),
                Text = "Смена Языка",
                Page = nameof(ThemeChangePage)
            }
        };

        public SettingsViewModel()
        {
 
        }

        [RelayCommand]
        private async Task GoToPageAsync(string page)
        {
            if (string.IsNullOrEmpty(page))
                return;

            await Shell.Current.GoToAsync(page);
        }
    }
}