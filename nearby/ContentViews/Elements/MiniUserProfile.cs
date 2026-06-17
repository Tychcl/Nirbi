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
        BindableProperty.Create(nameof(User), typeof(object), typeof(MiniUserProfile), null,
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

    public object? User
    {
        get => (object?)GetValue(UserProperty);
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
    private readonly VerticalStackLayout _detailedInfo;
    private readonly VerticalStackLayout _shortInfo;
    private readonly Label _labelName;
    private readonly Label _labelSurname;
    private readonly Label _labelBirthDate;
    private readonly Label _labelFullName;
    private readonly Label _labelEmail;
    private readonly Label _labelPhone;
    private readonly ContentView _extraContentSlot;
    private readonly ContentView _userDataContainer;

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

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(User))
                UpdateUserData();
        };

        UpdateUserData();
    }

    private void UpdateUserData()
    {
        if (User == null)
        {
            _labelName.Text = string.Empty;
            _labelSurname.Text = string.Empty;
            _labelBirthDate.Text = string.Empty;
            _labelFullName.Text = string.Empty;
            _labelEmail.Text = string.Empty;
            _labelPhone.Text = string.Empty;
            _piv.SetBinding(ProfileImageView.ImageProperty, new Binding("User.ProfilePicture", source: this) { TargetNullValue = "test_profile_image.jpg" });
            return;
        }

        bool isCandidate = User is Candidate;
        bool isUser = User is User;

        string fullName = string.Empty;
        string? profilePicture = null;

        if (isUser)
        {
            var u = (User)User;
            fullName = u.FullName ?? string.Empty;
            //profilePicture = u.;
            _labelName.Text = u.Name ?? string.Empty;
            _labelSurname.Text = u.Surname ?? string.Empty;
            _labelBirthDate.Text = u.BirthDate?.ToString("dd.MM.yyyy") ?? string.Empty;
            _labelEmail.Text = u.Email ?? string.Empty;
            _labelPhone.Text = u.Phone ?? string.Empty;
        }
        else if (isCandidate)
        {
            var c = (Candidate)User;
            fullName = $"{c.SecondName} {c.FirstName} {c.LastName}" ?? string.Empty;
            _labelName.Text = c.FirstName ?? string.Empty;
            _labelSurname.Text = c.SecondName ?? string.Empty;
            _labelBirthDate.Text = string.Empty;
            _labelEmail.Text = string.Empty;
            _labelPhone.Text = string.Empty;
        }

        _labelFullName.Text = fullName;

        _labelBirthDate.IsVisible = !OnlyFullNameMode && !string.IsNullOrEmpty(_labelBirthDate.Text);
        _labelEmail.IsVisible = !OnlyFullNameMode && !string.IsNullOrEmpty(_labelEmail.Text);
        _labelPhone.IsVisible = !OnlyFullNameMode && !string.IsNullOrEmpty(_labelPhone.Text);
        _piv.Image = "test_profile_image.jpg";
        //if (profilePicture != null)
        //{
        //    _piv.Image = ImageSource.FromUri(new Uri(profilePicture));
        //}
        //else
        //{
        //    _piv.Image = "test_profile_image.jpg";
        //}
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
        _labelEmail.IsVisible = !OnlyFullNameMode && !string.IsNullOrEmpty(_labelEmail.Text);
        _labelPhone.IsVisible = !OnlyFullNameMode && !string.IsNullOrEmpty(_labelPhone.Text);
    }
}