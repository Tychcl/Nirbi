namespace nearby.ContentViews.Elements;

public partial class SettingsItem : ContentView
{
    public static readonly BindableProperty IconProperty =
    BindableProperty.Create(nameof(Icon), typeof(string), typeof(SettingsItem),
        default(string), BindingMode.OneWay);
    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty TextProperty =
    BindableProperty.Create(nameof(Text), typeof(string), typeof(SettingsItem),
        default(string), BindingMode.OneWay);
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public SettingsItem()
	{
		InitializeComponent();
	}
}