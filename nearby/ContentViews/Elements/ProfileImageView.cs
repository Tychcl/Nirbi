

using Microsoft.Maui.Controls.Shapes;

namespace nearby.ContentViews.Elements;

public class ProfileImageView : ContentView
{
	private const int ImageSizeConst = 70;
	private const double CornerRadiusConst = 12.0;

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(ProfileImageView), CornerRadiusConst, propertyChanged: OnImageSizeChanged);
    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly BindableProperty ImageSizeProperty = 
        BindableProperty.Create(nameof(ImageSize), typeof(int), typeof(ProfileImageView), ImageSizeConst, propertyChanged: OnImageSizeChanged);
    public int ImageSize
    {
        get => (int)GetValue(ImageSizeProperty);
        set => SetValue(ImageSizeProperty, value);
    }
    private static void OnImageSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        (bindable as ProfileImageView).UpdateSize();
    }
    private void UpdateSize()
    {
        double cornerRadius;
        if (ImageSize != ImageSizeConst && CornerRadius == CornerRadiusConst)
        {
            cornerRadius = CornerRadiusConst * ((double)ImageSize / ImageSizeConst);
            cornerRadius = Math.Ceiling(cornerRadius);
        }
        else
        {
            cornerRadius = CornerRadius;
        }
        border.StrokeShape = new RoundRectangle { CornerRadius = cornerRadius};
        avatar.Clip = new RoundRectangleGeometry(new CornerRadius(cornerRadius), new Rect(0, 0, ImageSize, ImageSize));
    }

    public static readonly BindableProperty ImageProperty = 
        BindableProperty.Create(nameof(Image), typeof(string), typeof(ProfileImageView), "test_profile_image.jpg", propertyChanged: OnImageChanged);
    public string Image
    {
        get => (string)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }
    private static void OnImageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        (bindable as ProfileImageView).Updateimage();
    }
    private void Updateimage()
    {
        if(avatar is not null)
        {
            avatar.Source = Image;
        }
    }

    private Image avatar;
    private Border border;
    public ProfileImageView()
	{
        avatar = new Image
        {
            Aspect = Aspect.AspectFill
        };
        Updateimage();

        border = new Border
        {
            StrokeThickness = 2,
            Margin = new Thickness(0),
            Content = avatar
        };
        border.SetDynamicResource(Border.StrokeProperty, "CBorder");
        UpdateSize();

        Content = border;

        border.SetBinding(WidthRequestProperty, new Binding(nameof(ImageSize), source: this));
        border.SetBinding(HeightRequestProperty, new Binding(nameof(ImageSize), source: this));
    }
}