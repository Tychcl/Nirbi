using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Classes;
using nearby.ContentViews.Elements;
using nearby.Interfaces;
using nearby.Models;
using nearby.Services;
using nearby.Views.Main;

namespace nearby.ViewModels;

[QueryProperty(nameof(Task), "task")]
public partial class TaskDetailViewModel : BaseViewModel, IDisposable
{
    private readonly ITaskService _taskService;
    private readonly IUserService _userService;
    private readonly IChatService _chatService;
    private readonly IConfirmationService _confirmationService;

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
    private List<string> _candidateStatuses = new() {"Ожидающие", "Принятые", "Отклонёные" };
    [ObservableProperty]
    private string _selectedCandidateStatus;
    private List<Candidate> _allCandidates = new();
    partial void OnSelectedCandidateStatusChanged(string value)
    {
        ApplyCandidateFilter();
    }

    private void ApplyCandidateFilter()
    {
        var filtered = _allCandidates.AsEnumerable();
        switch (SelectedCandidateStatus)
        {
            case "Ожидающие":
                filtered = filtered.Where(c => c.IsPending == true);
                break;
            case "Принятые":
                filtered = filtered.Where(c => c.IsAccepted == true);
                break;
            case "Отклонёные":
                filtered = filtered.Where(c => c.IsRejected == true);
                break;
        }
        Volunteers = new ObservableCollection<Candidate>(filtered);
    }

    [ObservableProperty]
    private TaskItem _task = new();

    partial void OnTaskChanged(TaskItem? value)
    {
        if (value == null) return;
        _ = InitializeAsync(value);
        PageTitle = value.Title;
        IsSearching = value.Status == "В поиске волонтеров";
        InProgress = value.Status == "Выполняется";
        Completed = value.Status == "Выполнен";
    }

    [ObservableProperty]
    private bool _isOwner;
    partial void OnIsOwnerChanged(bool value) => RefreshCommands();
    [ObservableProperty]
    private string _volonteerButtonText;
    [ObservableProperty]
    private bool _statusVisible = false;
    [ObservableProperty]
    private bool _inProgress;
    [ObservableProperty]
    private bool _isSearching;
    [ObservableProperty]
    private bool _completed;
    [ObservableProperty]
    private User _creator = new();
    [ObservableProperty]
    private bool _canVolunteer;
    [ObservableProperty]
    private bool _hasVolunteered;
    [ObservableProperty]
    private string _volunteerStatus = string.Empty;
    [ObservableProperty]
    private ObservableCollection<Candidate> _volunteers = new();
    [ObservableProperty]
    private bool _headerMenuVisible;
    [ObservableProperty]
    private ObservableCollection<PopupItem> _popupItems = new();
    private PopupMenu popupMenu;

    private bool _isInitialized;

    public TaskDetailViewModel(
        ITaskService taskService,
        IUserService userService,
        IChatService chatService,
        IConfirmationService confirmationService)
    {
        _taskService = taskService;
        _userService = userService;
        _chatService = chatService;
        _confirmationService = confirmationService;

        PopupItems.Add(new((string)ResourceManager.Get("EditBox"), "Редактировать", EditCommand));
        PopupItems.Add(new((string)ResourceManager.Get("Delete"), "Удалить", DeleteCommand));

        popupMenu = PopupManager.Create(PopupItems, new Thickness(0, 15, 15, 0), LayoutOptions.End, LayoutOptions.Start);
    }

    private async Task InitializeAsync(TaskItem task)
    {
        if (_isInitialized) return;
        IsBusy = true;
        try
        {
            IsOwner = _userService.CurrentUserId == task.CreatorId;
            Creator = await _userService.LoadUserByIdAsync(task.CreatorId);

            if (IsOwner)
            {

                var candidates = await _confirmationService.GetTaskCandidates(task.Id);
                _allCandidates = candidates;
                SelectedCandidateStatus = "Ожидающие";
                //Volunteers = new ObservableCollection<Candidate>(candidates);
                CanVolunteer = false;
                HeaderMenuVisible = true;
            }
            else
            {
                string? status = await _confirmationService.GetTaskConfirmationStatus(task.Id);
                switch (status)
                {
                    case null:
                        CanVolunteer = true;
                        StatusVisible = false;
                        VolonteerButtonText = "Откликнуться";
                        break;
                    case "Created":
                        CanVolunteer = false;
                        StatusVisible = true;
                        VolunteerStatus = "Ожидание ответа";
                        break;
                    case "Accepted":
                        HasVolunteered = true;
                        CanVolunteer = false;
                        VolunteerStatus = "Вас приняли";
                        break;
                    case "Rejected":
                        CanVolunteer = false;
                        StatusVisible = true;
                        VolonteerButtonText = "Откликнуться повторно";
                        VolunteerStatus = "Вам отказали";
                        break;
                }
            }
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            var t = ex.Message;
            await ShowErrorAsync(ex.Message);
            await GoBackCommand.ExecuteAsync(null);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanVolunteerExecute))]
    private async Task VolunteerAsync()
    {
        try
        {
            await _confirmationService.CreateConfirmationAsync(new CreateConfirmationRequest
            {
                ConfirmationType = "Respond to minor task",
                EntityId = Task.Id,
                ExpirationHours = 72,
                MetaData = new Metadata() { ApplicantUsername = _userService.CurrentUser.Email, TaskName = Task.Title },
                ReviewerId = Task.CreatorId
            });
            CanVolunteer = false;
            HasVolunteered = true;
            VolunteerStatus = "Ожидание ответа";
            await ShowMsgAsync("Успех", "Вы откликнулись на задачу", "OK");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }
    private bool CanVolunteerExecute() => CanVolunteer && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanAcceptRejectExecute))]
    private async Task AcceptVolunteerAsync(Guid confirmationId)
    {
        try
        {
            await _confirmationService.RespondToConfirmationAsync(confirmationId, new RespondToConfirmationRequest
            {
                IsAccepted = true,
                RejectionReason = null
            });
            await RefreshVolunteersAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanAcceptRejectExecute))]
    private async Task RejectVolunteerAsync(Guid confirmationId)
    {
        try
        {
            await _confirmationService.RespondToConfirmationAsync(confirmationId, new RespondToConfirmationRequest
            {
                IsAccepted = false,
                RejectionReason = "Отклонено"
            });
            await RefreshVolunteersAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }
    private bool CanAcceptRejectExecute() => IsOwner && !IsBusy;

    [RelayCommand]
    private async Task StartTaskAsync()
    {
        try
        {
            await _taskService.UpdateTaskStatusAsync(Task.Id, new Guid("8449b004-3f18-4906-b31a-4687605a49e6"));
            Task.Status = "Выполняется";
            IsSearching = false;
            InProgress = true;
            await ShowMsgAsync("Успех", "Задача начата", "OK");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    [RelayCommand]
    private async Task CompleteTaskAsync()
    {
        try
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert("Завершение", "Вы уверены?", "Да", "Нет");
            if (!confirm) return;
            await _taskService.UpdateTaskStatusAsync(Task.Id, new Guid("b3dd4e86-0a4a-403f-8f36-6bf311b3f52f"));
            Task.Status = "Выполнен";
            InProgress = false;
            Completed = true;
            await ShowMsgAsync("Успех", "Задача завершена", "OK");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        await popupMenu.CloseAsync();
        await Shell.Current.GoToAsync(nameof(TaskAddEditPage), new Dictionary<string, object?> { { "task", Task } });
    }

    [RelayCommand(CanExecute = nameof(CanDeleteTaskExecute))]
    private async Task DeleteAsync()
    {
        try
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert("Удаление", "Удалить задачу?", "Да", "Нет");
            if (!confirm) return;
            await _taskService.DeleteTaskAsync(Task.Id);
            await GoBackCommand.ExecuteAsync(null);
            await ShowMsgAsync("Успех", "Задача удалена", "OK");
        }
        catch
        {
            await GoBackCommand.ExecuteAsync(null);
        }
    }
    private bool CanDeleteTaskExecute() => IsOwner && !IsBusy;

    [RelayCommand]
    private async Task GoToProfileAsync(Guid? userId)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage), new Dictionary<string, object?> { { "id", userId.ToString() } });
    }

    [RelayCommand]
    private async Task StartChatAsync()
    {
        try
        {
            var r = await _chatService.CreateChatAsync("personal", "", new() { Creator.Id, _userService.CurrentUser.Id });
            if (r is ApiResponse<int>)
            {
                await Shell.Current.GoToAsync(nameof(ChatDetailPage), new Dictionary<string, object?> { { "id", r.Data } });
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    [RelayCommand]
    private async Task OpenPopupMenuAsync() => await PopupManager.Show(popupMenu);

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (Task.Id == Guid.Empty) return;
        var task = await _taskService.GetTaskAsync(Task.Id);
        Task = task;
        _isInitialized = false;
        await InitializeAsync(task);
    }

    private async Task RefreshVolunteersAsync()
    {
        if (!IsOwner) return;
        var candidates = await _confirmationService.GetTaskCandidates(Task.Id);
        Volunteers = new ObservableCollection<Candidate>(candidates);
    }

    private void RefreshCommands()
    {
        StartTaskCommand.NotifyCanExecuteChanged();
        CompleteTaskCommand.NotifyCanExecuteChanged();
        AcceptVolunteerCommand.NotifyCanExecuteChanged();
        RejectVolunteerCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        VolunteerCommand.NotifyCanExecuteChanged();
    }

    protected override void OnBusyStateChanged(bool isBusy)
    {
        base.OnBusyStateChanged(isBusy);
        RefreshCommands();
    }

    public void Dispose()
    {
        StopLocationTrackingAsync();
    }
}