using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using nearby.Resources.Themes;

namespace nearby.Classes
{  
    public static class ThemeManager
    {
        public const string LightBlue = "LightBlue";
        public const string LightPurple = "LightPurple";
        public const string DarkOrange = "DarkOrange";
        public const string DarkGreen = "DarkGreen";

        public static ResourceDictionary GetTheme(string name)
        {
            return name switch
            {
                LightBlue => new LightBlueTheme(),
                LightPurple => new LightPurpleTheme(),
                DarkOrange => new DarkOrangeTheme(),
                DarkGreen => new DarkGreenTheme(),
                _ => new LightBlueTheme()
            };
        }

        public static void ApplyTheme(string name)
        {
            var merged = ResourceManager.MergedDictionaries;
            var oldThemes = merged.Where(md => md is ResourceDictionary rd && rd.Source?.OriginalString?.Contains("Theme") == true).ToList();
            foreach (var old in oldThemes)
                merged.Remove(old);
            var theme = GetTheme(name);
            merged.Add(theme);
            Preferences.Set("user_theme", name);
        }

        public static ThemeDescription GetDescription(string name)
        {
            return name switch
            {
                LightBlue => new ThemeDescription("Холодное море", true, "Синяя"),
                LightPurple => new ThemeDescription("Нежная лаванда", true, "Фиолетовая"),
                DarkOrange => new ThemeDescription("Шоколад с апельсином", false, "Оранжевая"),
                DarkGreen => new ThemeDescription("Хвойный лес", false, "Зеленая"),
                _ => new ThemeDescription()
            };
        }

        public static void LoadSavedTheme()
        {
            ApplyTheme(Preferences.Get("user_theme", LightBlue));
        }
    }
}
