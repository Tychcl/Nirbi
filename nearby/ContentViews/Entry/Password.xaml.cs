using nearby.Classes;

namespace nearby.ContentViews.Entry;

public partial class Password : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Password),
            default(string), BindingMode.TwoWay);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Password),
            default(string));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public Password()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        PEntry.IsPassword = !PEntry.IsPassword;
        if (ToggleButton.Source is FontImageSource fis)
        {
            fis.Glyph = PEntry.IsPassword ? (string)ResourceManager.Get("VisibleOff") : (string)ResourceManager.Get("VisibleOn");
        }
    }
}