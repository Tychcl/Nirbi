using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;
using nearby.ContentViews.Elements;

namespace nearby.Classes
{
    public static class PopupManager
    {
        public static readonly Point MaxDIU = new Point(DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density, DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
        public static PopupOptions options = new PopupOptions
        {
            PageOverlayColor = Colors.Transparent,
            CanBeDismissedByTappingOutsideOfPopup = true,
            Shape = null,
            Shadow = null
        };

        public static INavigation navigation => Application.Current?.Windows[0]?.Page?.Navigation;

        public static PopupMenu Create(ObservableCollection<PopupItem> list, Thickness margin = default(Thickness), LayoutOptions? horizontal = null, LayoutOptions? vertical = null)
        {
            var popup = new PopupMenu(list);
            popup.HorizontalOptions = horizontal ?? LayoutOptions.Start;
            popup.VerticalOptions = vertical ?? LayoutOptions.Start;
            popup.Margin = margin;
            return popup;
        }

        public static async Task Show(PopupMenu popup, INavigation? nav = null)
        {
            INavigation n = nav ?? navigation;
            await n.ShowPopupAsync(popup, options);
        }

        public static async Task Show(PopupMenu popup, double x, double y, INavigation? nav = null)
        {
            //Минимальные отступы слева и сверху
            double minLeft = 50;
            double minTop = 75;
            //Вычисление финальной возможной позиции
            double finalLeft = Math.Min(MaxDIU.X - 200, x);
            double finalTop = Math.Min(MaxDIU.Y - 300, y);
            popup.Margin = new Thickness(
                Math.Max(minLeft, finalLeft) / 2, 
                Math.Max(minTop, finalTop) / 2, 0, 0);
            //отображение контекстного меню
            await Show(popup, nav);
        }
    }
}
