using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace nearby;

public static class IconHelper
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.CreateAttached(
            "Icon",                     
            typeof(string),             
            typeof(IconHelper),         
            default(string),            
            defaultValueCreator: null,
            propertyChanged: OnPropertyChanged);
    public static string GetIcon(BindableObject view)
        => (string)view.GetValue(IconProperty);
    public static void SetIcon(BindableObject view, string value)
        => view.SetValue(IconProperty, value);

    public static readonly BindableProperty ColorProperty =
        BindableProperty.CreateAttached(
            "Color",
            typeof(string),
            typeof(IconHelper),
            "CPrimary",
            defaultValueCreator: null,
            propertyChanged: OnPropertyChanged);
    public static string GetColor(BindableObject view)
        => (string)view.GetValue(ColorProperty);
    public static void SetColor(BindableObject view, string value)
        => view.SetValue(ColorProperty, value);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.CreateAttached(
            "Size",
            typeof(int),
            typeof(IconHelper),
            24,
            defaultValueCreator: null,
            propertyChanged: OnPropertyChanged);
    public static int GetSize(BindableObject view)
        => (int)view.GetValue(SizeProperty);
    public static void SetSize(BindableObject view, int value)
        => view.SetValue(SizeProperty, value);

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        string icon = GetIcon(bindable);
        string color = GetColor(bindable);
        if (string.IsNullOrEmpty(icon) || string.IsNullOrEmpty(color)) return;
        var fis = new FontImageSource
        {
            Glyph = icon,
            FontFamily = "Icons",
            Size = GetSize(bindable)
        };
        fis.SetDynamicResource(FontImageSource.ColorProperty, GetColor(bindable));
        switch (bindable)
        {
            case ShellContent sc:
                sc.Icon = fis;
                break;
            case Image img:
                img.Source = fis;
                break;
            case ImageButton imgbtn:
                imgbtn.Source = fis;
                break;
            case Button btn:
                btn.ImageSource = fis;
                break;
        }

    }

}
