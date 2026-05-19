using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Behaviors;
using nearby.Classes;

namespace nearby.ContentViews.Elements;

public partial class ThemeMiniView : ContentView
{
    public static readonly BindableProperty ThemeProperty =
        BindableProperty.Create(
            nameof(Theme),
            typeof(string),
            typeof(ThemeMiniView),
            default(string),
            propertyChanged: OnThemeChanged
        );

    public string Theme
    {
        get => (string)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    public ThemeMiniView()
    {
        InitializeComponent();
    }

    private static void OnThemeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ThemeMiniView)bindable;
        view.UpdateThemeResources((string)newValue);
    }

    private void UpdateThemeResources(string themeName)
    {
        if (string.IsNullOrEmpty(themeName))
            return;
        ThemeDescription ThemeDesc = ThemeManager.GetDescription(themeName);
        Name.Text = ThemeDesc.Name;
        Desc.Text = $"{ThemeDesc.Mode} • {ThemeDesc.Color}";
        Resources.Clear();
        Resources.Add(ThemeManager.GetTheme(themeName));
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (!string.IsNullOrEmpty(Theme))
            UpdateThemeResources(Theme);
    }

    private void OnThemeCardTapped(object sender, TappedEventArgs e)
    {
        ThemeManager.ApplyTheme(Theme);
    }
}