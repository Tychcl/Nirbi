using BruTile;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Microsoft.Maui.Devices.Sensors;
using nearby.Models;
using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class TaskDetailPage : ContentPage
{
    private Mapsui.Map? _map;
    private ILayer? _taskMarkerLayer;
    private bool _isListening;

    public TaskDetailPage(TaskDetailViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        taskMap.Map = _map;
        viewModel.UserLocationUpdated += UpdateLocation;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
        if (location is not null)
        {
            taskMap.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude));
            //var mPoint = SphericalMercator.FromLonLat(location.Longitude, location.Latitude).ToMPoint();
            //taskMap.Map.Navigator.CenterOnAndZoomTo(mPoint, 7);
        }
        if (BindingContext is TaskDetailViewModel vm)
        {
            await vm.StartLocationTrackingAsync();
            await vm.RefreshAsync();
            CreateMarker(new Location((double)vm.Task.Latitude, (double)vm.Task.Longitude));
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        (BindingContext as TaskDetailViewModel)?.Dispose();
    }

    private void UpdateLocation(double lat, double lon)
    {
        taskMap.MyLocationLayer.UpdateMyLocation(new Position(lat, lon));
        taskMap.Refresh();
    }

    private void CreateMarker(Location? location)
    {
        taskMap.Pins.Clear();
        if (location == null) return;
        var pin = new Pin(taskMap)
        {
            Label = "Задача",
            Color = Microsoft.Maui.Graphics.Color.FromRgb(0, 90, 185),
            Position = new Position(location.Latitude, location.Longitude),
            Type = PinType.Pin,
            Scale = 0.5f
        };
        pin.Callout.Anchor = new Point(0.5, 1);
        pin.Callout.RectRadius = 0;
        taskMap.Pins.Add(pin);
        taskMap.Refresh();
    }
}