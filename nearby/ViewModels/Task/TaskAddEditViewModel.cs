using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices.Sensors;
using nearby.Interfaces;
using nearby.Models;
using nearby.Models.Task;
using nearby.Services;

namespace nearby.ViewModels;

[QueryProperty(nameof(Task), "task")]
public partial class TaskAddEditViewModel : BaseViewModel, IDisposable
{
    private readonly ITaskService _taskService;
    private readonly IUserService _userService;
    private readonly IGeocodingService _geocodingService;

    public event Action<double, double>? UserLocationUpdated;
    public async Task StartLocationTrackingAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await ShowErrorAsync("Нет разрешения на геолокацию");
                return;
            }
        }
        var request = new GeolocationListeningRequest(GeolocationAccuracy.Best)
        {
            MinimumTime = TimeSpan.FromSeconds(3)
        };
        try
        {
            Geolocation.LocationChanged += OnLocationChanged;
            Geolocation.ListeningFailed += OnListeningFailed;
            if (!Geolocation.IsListeningForeground)
            {
                var success = await Geolocation.StartListeningForegroundAsync(request);
            }   
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Ошибка: {ex.Message}");
        }
    }

    private async Task StopLocationTrackingAsync()
    {
        Geolocation.LocationChanged -= OnLocationChanged;
        Geolocation.ListeningFailed -= OnListeningFailed;
        Geolocation.StopListeningForeground();
    }

    private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var location = e.Location;
            if (location != null)
            {
                UserLocationUpdated?.Invoke(location.Latitude, location.Longitude);
            }
        });
    }

    private void OnListeningFailed(object? sender, GeolocationListeningFailedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await ShowErrorAsync($"Ошибка при прослушивании: {e.Error}");
        });
    }

    [ObservableProperty]
    private string? searchAddressQuery;

    [ObservableProperty]
    private ObservableCollection<Position> searchResults = new();

    [ObservableProperty]
    private Position? selectedSearchResult;

    [ObservableProperty]
    private bool isSearchResultsVisible;

    [ObservableProperty]
    private Location? selectedLocation = null;

    [ObservableProperty]
    private TaskItem _task = new TaskItem();

    partial void OnTaskChanged(TaskItem item)
    {
        if (item == null) return;
        if (item.Latitude.HasValue && item.Longitude.HasValue)
        {
            SelectedLocation = new Location(item.Latitude.Value, item.Longitude.Value);
            _ = UpdateAddressFromCoordinatesAsync(item.Latitude.Value, item.Longitude.Value);
        }
        Title = item.Title;
        Description = item.Description ?? string.Empty;
        NeededVolunteers = item.NeededVolunteers;
        LocationAddress = item.Location ?? string.Empty;
        Reward = item.Reward;
        ValidateAllProperties();
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Название обязательно")]
    [MaxLength(100, ErrorMessage = "Максимальная длина 100 символов")]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _descriptionLength;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(300, ErrorMessage = "Максимальная длина 300 символов")]
    [Required(ErrorMessage = "Описание обязательно")]
    private string _description = string.Empty;
    partial void OnDescriptionChanged(string value)
    {
        Task.Description = value;
        DescriptionLength = value.Length;
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(1, 10, ErrorMessage = "Количество волонтёров должно быть от 1 до 10")]
    private int _neededVolunteers = 1;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Приоритет обязателен")]
    private string _priority = "medium";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Дата окончания обязательна")]
    private DateTime _deadline = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private int _locationLength;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Адрес обязателен")]
    [MaxLength(150, ErrorMessage = "Максимальная длина 150 символов")]
    private string? _locationAddress;
    partial void OnLocationAddressChanged(string value)
    {
        Task.Location = value;
        LocationLength = value.Length;
    }

    [ObservableProperty]
    private decimal _reward;

    public TaskAddEditViewModel(ITaskService ts, IUserService us, IGeocodingService gs)
    {
        _taskService = ts;
        _userService = us;
        _geocodingService = gs;
        ValidateAllProperties();
    }

    [RelayCommand]
    private async Task SearchAddressAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchAddressQuery)) return;
        var results = await _geocodingService.SearchAddressAsync(SearchAddressQuery);
        IsSearchResultsVisible = searchResults.Count() > 0;
        if (isSearchResultsVisible)
        {
            SearchResults = new ObservableCollection<Position>(results);
        }
    }

    [RelayCommand]
    private async Task SelectSearchResultAsync(Position? address)
    {
        if (address == null) return;
        if (double.TryParse(address.Lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
            double.TryParse(address.Lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
        {
            SelectedLocation = new Location(lat, lon);
            LocationAddress = address.DisplayName;
            Task.Latitude = lat;
            Task.Longitude = lon;
        }
        IsSearchResultsVisible = false;
        SearchAddressQuery = string.Empty;
    }

    [RelayCommand]
    private async Task OnMapClickedAsync(Location location)
    {
        SelectedLocation = location;
        Task.Latitude = location.Latitude;
        Task.Longitude = location.Longitude;
        var addr = await _geocodingService.GetAddressAsync(location.Latitude, location.Longitude);
        LocationAddress = addr ?? "Адрес не определён";
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            ValidateAllProperties();
            if (HasErrors) return;
            _task.Title = _title;
            _task.Description = _description;
            _task.NeededVolunteers = _neededVolunteers;
            _task.Location = _locationAddress;
            _task.Reward = _reward;
            bool isNewTask = _task.Id == Guid.Empty;
            if (isNewTask)
            {
                var request = new CreateMinorTaskRequest
                {
                    name = _task.Title,
                    description = _task.Description,
                    latitude = _task.Latitude ?? 0,
                    longitude = _task.Longitude ?? 0,
                    numberVolunteers = _task.NeededVolunteers,
                    encouragement = (double)_task.Reward,
                    //images = null
                };
                var createdTask = await _taskService.CreateTaskAsync(request);
                _task.Id = createdTask.Id;
            }
            else
            {
                var request = new UpdateMinorTaskRequest
                {
                    name = _task.Title,
                    description = _task.Description,
                    latitude = _task.Latitude ?? 0,
                    longitude = _task.Longitude ?? 0,
                    numberVolunteers = _task.NeededVolunteers,
                    encouragement = (double)_task.Reward
                };
                await _taskService.UpdateTaskAsync(_task.Id, request);
            }
            await ShowSuccessfulAsync(isNewTask ? "Задача создана" : "Задача обновлена");
            await GoBackCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    private async Task UpdateAddressFromCoordinatesAsync(double lat, double lon)
    {
        var addr = await _geocodingService.GetAddressAsync(lat, lon);
        LocationAddress = addr ?? "Адрес не определён";
    }

    public void Dispose()
    {
        StopLocationTrackingAsync();
    }
}