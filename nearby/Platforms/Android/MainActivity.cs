using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace nearby
{
    [Activity(Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density,
        WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.SetSoftInputMode(SoftInput.AdjustResize);
        }

        private List<Mapsui.UI.Maui.MapView> FindMapViews(Element element)
        {
            var result = new List<Mapsui.UI.Maui.MapView>();
            if (element is Mapsui.UI.Maui.MapView mapView)
                result.Add(mapView);
            if (element is IVisualTreeElement visualTreeElement)
            {
                var children = visualTreeElement.GetVisualChildren();
                foreach (var child in children)
                {
                    if (child is Element childElement)
                        result.AddRange(FindMapViews(childElement));
                }
            }
            return result;
        }
    }
}
