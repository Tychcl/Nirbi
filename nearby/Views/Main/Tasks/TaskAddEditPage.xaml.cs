using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.UI.Objects;
using Microsoft.Maui.Devices.Sensors;
using nearby.ViewModels;
using SkiaSharp;

namespace nearby.Views.Main;

public partial class TaskAddEditPage : ContentPage
{
    private Mapsui.Layers.MyLocationLayer? _myLocationLayer;
    private ILayer? _markerLayer;
    private Mapsui.Map? _map;

    public TaskAddEditPage(TaskAddEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _map = new Mapsui.Map();
        _map.Layers.Add(OpenStreetMap.CreateTileLayer());
        taskMap.Map = _map;
        viewModel.UserLocationUpdated += UpdateLocation;
        //viewModel.PropertyChanged += (s, e) =>
        //{
        //    if (e.PropertyName == nameof(TaskAddEditViewModel.SelectedLocation))
        //        UpdateMarkerAndCenter(viewModel.SelectedLocation);
        //};
        taskMap.MapClicked += (s, e) =>
        {
            var point = e.Point;
            var location = new Location(point.Latitude, point.Longitude);
            if (BindingContext is TaskAddEditViewModel vm)
                vm.MapClickedCommand.Execute(location);
            CreateMarker(location);
        };
    }

    private void CreateMarker(Location? location)
    {
        taskMap.Pins.Clear();
        if (location == null) return;
        var pin = new Pin(taskMap)
        {
            Label = "Задача",
            Color = Microsoft.Maui.Graphics.Color.FromRgb(0,90,185),
            Position = new Position(location.Latitude, location.Longitude),
            Type = PinType.Pin,
            Scale = 0.5f
        };
        pin.Callout.Anchor = new Point(0.5, 1);
        pin.Callout.RectRadius = 0;
        taskMap.Pins.Add(pin);
        taskMap.Refresh();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
        if (location is not null)
        {
            taskMap.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude));
            var mPoint = SphericalMercator.FromLonLat(location.Longitude, location.Latitude).ToMPoint();
            taskMap.Map.Navigator.CenterOnAndZoomTo(mPoint, 7);
        }
        if (BindingContext is TaskAddEditViewModel vm)
        {
            await vm.StartLocationTrackingAsync();
            if (vm.Task.Id == Guid.Empty && vm.SelectedLocation == null)
            {
                // новая задача, можно запросить местоположение
            }
            else if (vm.SelectedLocation != null)
            {
                CreateMarker(vm.SelectedLocation);
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        (BindingContext as TaskAddEditViewModel)?.Dispose();
    }

    private void UpdateLocation(double lat, double lon)
    {
        taskMap.MyLocationLayer.UpdateMyLocation(new Position(lat, lon));
        taskMap.Refresh();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        BorderMap.IsVisible = !BorderMap.IsVisible;
        OpenCloseMapBtn.Text = BorderMap.IsVisible ? "Скрыть карту" : "Показать карту";
    }
}