using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using nearby;
using nearby.Classes;

namespace nearby.ContentViews.Elements;

public partial class PopupMenu : Popup
{
    private ObservableCollection<PopupItem> _items;

    public PopupMenu(ObservableCollection<PopupItem> items)
    {
        InitializeComponent();
        _items = items;
        BuildMenu();
    }

    private void BuildMenu()
    {
        MenuContainer.Children.Clear();

        foreach (var item in _items)
        {
            var row = new VerticalStackLayout
            {
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(12, 8),
                Spacing = 0
            };

            var contentStack = new HorizontalStackLayout
            {
                Spacing = 6,
                InputTransparent = true
            };

            var image = new Image
            {
                WidthRequest = 16,
                HeightRequest = 16,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true
            };
            IconHelper.SetIcon(image, item.Icon);
            IconHelper.SetColor(image, "CTextPrimary");

            var label = new Label
            {
                Text = item.Text,
                VerticalOptions = LayoutOptions.Center,
                Style = (Style)Application.Current!.Resources["CommonLabel"],
                InputTransparent = true
            };

            contentStack.Children.Add(image);
            contentStack.Children.Add(label);

            var separator = new BoxView
            {
                Style = (Style)ResourceManager.Get("SplitLine"),
                Margin = new Thickness(0, 4, 0, 0),
                InputTransparent = true
            };

            row.Children.Add(contentStack);
            row.Children.Add(separator);

            if (item.Command != null)
            {
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => item.Command.Execute(null);
                row.GestureRecognizers.Add(tapGesture);
            }

            MenuContainer.Children.Add(row);
        }
    }
}