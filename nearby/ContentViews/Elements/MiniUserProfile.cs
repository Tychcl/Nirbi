using nearby.Models;
using Microsoft.Maui.Controls.Shapes;
using System.Globalization;
using nearby.Classes;
using nearby.Classes.Interface.Converters;

namespace nearby.ContentViews.Elements;

[ContentProperty(nameof(ExtraContent))]
public class MiniUserProfile : ContentView
{
    public static readonly BindableProperty UserProperty =
        BindableProperty.Create(nameof(User), typeof(User), typeof(MiniUserProfile), null,
            propertyChanged: OnUserChanged);

    public static readonly BindableProperty ImageSizeProperty =
        BindableProperty.Create(nameof(ImageSize), typeof(int), typeof(MiniUserProfile), 70);

    public static readonly BindableProperty ImageIsVisibleProperty =
        BindableProperty.Create(nameof(ImageIsVisible), typeof(bool), typeof(MiniUserProfile), true);

    public static readonly BindableProperty IsUseBorderProperty =
        BindableProperty.Create(nameof(IsUseBorder), typeof(bool), typeof(MiniUserProfile), false);

    public static readonly BindableProperty IsFullNameModeProperty =
        BindableProperty.Create(nameof(IsFullNameMode), typeof(bool), typeof(MiniUserProfile), false,
            propertyChanged: OnFullNameModeChanged);

    public static readonly BindableProperty OnlyFullNameModeProperty =
        BindableProperty.Create(nameof(OnlyFullNameMode), typeof(bool), typeof(MiniUserProfile), false,
            propertyChanged: OnFullNameModeChanged);

    public static readonly BindableProperty ExtraContentProperty =
        BindableProperty.Create(nameof(ExtraContent), typeof(View), typeof(MiniUserProfile), null,
            propertyChanged: OnExtraContentChanged);

    public User? User
    {
        get => (User?)GetValue(UserProperty);
        set => SetValue(UserProperty, value);
    }
    public int ImageSize
    {
        get => (int)GetValue(ImageSizeProperty);
        set => SetValue(ImageSizeProperty, value);
    }
    public bool ImageIsVisible
    {
        get => (bool)GetValue(ImageIsVisibleProperty);
        set => SetValue(ImageIsVisibleProperty, value);
    }
    public bool IsFullNameMode
    {
        get => (bool)GetValue(IsFullNameModeProperty);
        set => SetValue(IsFullNameModeProperty, value);
    }
    public bool OnlyFullNameMode
    {
        get => (bool)GetValue(OnlyFullNameModeProperty);
        set => SetValue(OnlyFullNameModeProperty, value);
    }
    public bool IsUseBorder
    {
        get => (bool)GetValue(IsUseBorderProperty);
        set => SetValue(IsUseBorderProperty, value);
    }
    public View? ExtraContent
    {
        get => (View?)GetValue(ExtraContentProperty);
        set => SetValue(ExtraContentProperty, value);
    }

    private readonly Grid _rootGrid;
    private readonly ProfileImageView _piv;
    private readonly VerticalStackLayout _detailedInfo;   // Имя, Фамилия, ДР
    private readonly VerticalStackLayout _shortInfo;      // ФИО, Email, Телефон
    private readonly Label _labelName, _labelSurname, _labelBirthDate;
    private readonly Label _labelFullName, _labelEmail, _labelPhone;
    private readonly ContentView _extraContentSlot;
    private readonly ContentView _userDataContainer;      // контейнер с BindingContext = User

    public MiniUserProfile()
    {
        Style BCL = (Style)ResourceManager.Get("BoldCommonLabel");

        _piv = new ProfileImageView();
        _piv.ImageSize = ImageSize;

        _labelName = new Label { Style = BCL, Margin = new Thickness(0) };
        _labelSurname = new Label { Style = BCL, Margin = new Thickness(0) };
        _labelBirthDate = new Label
        {
            Style = BCL,
            TextColor = (Color)ResourceManager.Get("Gray500"),
            Margin = new Thickness(0)
        };
        _labelBirthDate.SetDynamicResource(Label.FontSizeProperty, "PrimaryFontSize");

        _detailedInfo = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0),
            Children = { _labelName, _labelSurname, _labelBirthDate }
        };

        _labelFullName = new Label { Style = BCL, Margin = new Thickness(0) };
        _labelEmail = new Label
        {
            Style = BCL,
            TextColor = (Color)ResourceManager.Get("Gray500"),
            Margin = new Thickness(0)
        };
        _labelEmail.SetDynamicResource(Label.FontSizeProperty, "PrimaryFontSize");

        _labelPhone = new Label
        {
            Style = BCL,
            TextColor = (Color)ResourceManager.Get("Gray500"),
            Margin = new Thickness(0)
        };
        _labelPhone.SetDynamicResource(Label.FontSizeProperty, "PrimaryFontSize");

        _shortInfo = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0),
            Children = { _labelFullName, _labelEmail, _labelPhone }
        };

        _userDataContainer = new ContentView
        {
            VerticalOptions = LayoutOptions.Center,
            Content = _detailedInfo 
        };
        _userDataContainer.SetBinding(BindableObject.BindingContextProperty, new Binding(nameof(User), source: this));

        _extraContentSlot = new ContentView { VerticalOptions = LayoutOptions.Center };
        _extraContentSlot.SetBinding(ContentView.ContentProperty, new Binding(nameof(ExtraContent), source: this));

        _rootGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8,
            Padding = new Thickness(0),
            Margin = new Thickness(0)
        };

        _rootGrid.Add(_piv, 0, 0);
        _rootGrid.Add(_userDataContainer, 1, 0);
        _rootGrid.Add(_extraContentSlot, 2, 0);

        Content = _rootGrid;

        _piv.SetBinding(IsVisibleProperty, new Binding(nameof(ImageIsVisible), source: this));
        _piv.SetBinding(ProfileImageView.ImageSizeProperty, new Binding(nameof(ImageSize), source: this));
        _piv.SetBinding(ProfileImageView.ImageProperty, new Binding("User.ProfilePicture", source: this) { TargetNullValue = "test_profile_image.jpg" });

        _labelName.SetBinding(Label.TextProperty, new Binding("Name"));
        _labelSurname.SetBinding(Label.TextProperty, new Binding("Surname"));
        _labelBirthDate.SetBinding(Label.TextProperty, new Binding("BirthDate", stringFormat: "{0:dd.MM.yyyy}"));

        _labelFullName.SetBinding(Label.TextProperty, new Binding("FullName"));
        _labelEmail.SetBinding(Label.TextProperty, new Binding("Email"));
        _labelPhone.SetBinding(Label.TextProperty, new Binding("Phone"));

        UpdateDetailMode();
    }

    private static void OnUserChanged(BindableObject bindable, object oldValue, object newValue)
    {
        
    }

    private static void OnFullNameModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MiniUserProfile)bindable;
        control.UpdateDetailMode();
    }

    private static void OnExtraContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        
    }

    private void UpdateDetailMode()
    {
        _userDataContainer.Content = IsFullNameMode || OnlyFullNameMode ? _shortInfo : _detailedInfo;
        _labelEmail.IsVisible = !OnlyFullNameMode;
        _labelPhone.IsVisible = !OnlyFullNameMode;
    }
}