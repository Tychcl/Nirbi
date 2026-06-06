using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Classes;
using nearby.ContentViews.Elements;
using nearby.Interfaces;
using nearby.Models;
using nearby.Services;
using nearby.Views.Main;

namespace nearby.ViewModels
{
    [QueryProperty(nameof(Task), "task")]
    public partial class TaskDetailViewModel : BaseViewModel, IDisposable
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private List<string> volunteerFilters = new(VolunteerStatusMap.Keys);

        private static readonly Dictionary<string, string> VolunteerStatusMap = new()
        {
            {"Ожидающие", "pending"},
            {"Принятые", "accepted"},
            {"Отклоненные", "rejected"}
        };

        [ObservableProperty]
        private string volunteerFilter;
        partial void OnVolunteerFilterChanged(string value)
        {
            UpdateVolunteersList();
        }
        [ObservableProperty]
        private ObservableCollection<TaskVolunteerInfo> volunteers = new();
        private List<TaskVolunteerInfo> _allVolunteersCache = new();

        [ObservableProperty]
        private View _creatorProfileView;

        [ObservableProperty]
        private ObservableCollection<PopupItem> _popupItems = new();

        [ObservableProperty]
        private User _creator = new();

        [ObservableProperty]
        private TaskItem _task = new();

        [ObservableProperty]
        private bool _headerMenuVisible;

        [ObservableProperty]
        private bool _isOwner;
        partial void OnIsOwnerChanged(bool value)
        {
            RefreshCommands();
            HeaderMenuVisible = IsOwner && !Completed;
        }

        [ObservableProperty]
        private bool _hasVolunteered;

        [ObservableProperty]
        private bool _canVolunteer;

        [ObservableProperty]
        private string _volunteerStatus = string.Empty;

        [ObservableProperty]
        private bool _inProgress;

        [ObservableProperty]
        private bool _isSearching;

        [ObservableProperty]
        private bool _completed;
        partial void OnCompletedChanged(bool value)
        {
            HeaderMenuVisible = IsOwner && !Completed;
        }

        private bool _isInitialized;

        private PopupMenu popupMenu;
        public TaskDetailViewModel(ITaskService taskService, IUserService userService, IServiceProvider sp, IChatService chatService)
        {
            _taskService = taskService;
            _taskService.TaskUpdated += TaskUpdated;
            _userService = userService;
            _serviceProvider = sp;
            _chatService = chatService;
            VolunteerFilter = volunteerFilters[0];
            PopupItems.Add(new((string)ResourceManager.Get("EditBox"), "Редактировать", EditCommand));
            PopupItems.Add(new((string)ResourceManager.Get("Delete"), "Удалить", DeleteCommand));

            popupMenu = PopupManager.Create(PopupItems, new Thickness(0, 15, 15, 0), LayoutOptions.End, LayoutOptions.Start);
            _chatService = chatService;
        }

        private async void TaskUpdated(object? sender, TaskItem task)
        {
            if (task.Id == Task.Id)
            {
                Task = task;
            }
        }

        partial void OnTaskChanged(TaskItem? value)
        {
            if (value == null) return;
            _ = InitializeAsync(value);
            PageTitle = value.Title;
            IsSearching = value.Status == "searching";
            InProgress = value.Status == "in_progress";
            Completed = value.Status == "completed";
        }

        private async Task InitializeAsync(TaskItem task)
        {
            if (_isInitialized) return;
            IsBusy = true;
            try
            {
                IsOwner = _userService.CurrentUserId == Task.CreatorId;
                if (IsOwner)
                {
                    CanVolunteer = false;
                    await LoadVolunteersAsync();
                }
                else
                {
                    await LoadMyVolunteerStatusAsync(Task.Id);
                }
                Creator = (await _userService.LoadUserByIdAsync(Task.CreatorId)).Data;
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                //await ShowErrorAsync(ex.Message);
                await GoBackCommand.ExecuteAsync(null);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateVolunteersList()
        {
            string statusValue = VolunteerStatusMap.GetValueOrDefault(VolunteerFilter ?? volunteerFilters[0], "");
            var filtered = string.IsNullOrEmpty(statusValue)
                ? _allVolunteersCache
                : _allVolunteersCache.Where(v => v.Status == statusValue).ToList();
            var order = new[] { "pending", "accepted", "rejected" };
            var sorted = filtered
                .OrderBy(v => Array.IndexOf(order, v.Status))
                .ThenBy(v => v.FullName)
                .ToList();
            Volunteers.Clear();
            foreach (var v in sorted)
                Volunteers.Add(v);
        }

        private async Task LoadVolunteersAsync()
        {
            try
            {
                var response = await _taskService.GetTaskVolunteersAsync(Task.Id);
                _allVolunteersCache.Clear();
                if (response.Data != null)
                {
                    foreach (var v in response.Data)
                    {
                        _allVolunteersCache.Add(v);
                    }
                }
                UpdateVolunteersList();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        private async Task LoadMyVolunteerStatusAsync(int taskId)
        {
            try
            {
                var response = await _taskService.GetMyVolunteerStatusAsync(taskId);
                (CanVolunteer, HasVolunteered, VolunteerStatus) = response.Data switch
                {
                    "pending" => (false, true, "Ожидание ответа"),
                    "accepted" => (false, true, "Вас приняли"),
                    "rejected" => (false, true, "Вам отказали"),
                    "cancelled" => (false, true, "Задача отменена"),
                    "completed" => (false, true, "Задача завершена"),
                    _ => (true, false, "")
                };
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
            
        }

        [RelayCommand(CanExecute = nameof(CanVolunteerExecute))]
        private async Task VolunteerAsync()
        {
            try
            {
                var response = await _taskService.VolunteerForTaskAsync(Task.Id);
                CanVolunteer = false;
                HasVolunteered = true;
                VolunteerStatus = "Ожидание ответа";
                await ShowMsgAsync("Успех", "Вы откликнулись на задачу", "OK");
            }
            catch(Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }
        private bool CanVolunteerExecute() => CanVolunteer && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanAcceptRejectExecute))]
        private async Task AcceptVolunteerAsync(Guid volunteerId)
        {
            try
            {
                var response = await _taskService.AcceptVolunteerAsync(Task.Id, volunteerId);
                await LoadVolunteersAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand(CanExecute = nameof(CanAcceptRejectExecute))]
        private async Task RejectVolunteerAsync(Guid volunteerId)
        {
            try
            {
                var response = await _taskService.RejectVolunteerAsync(Task.Id, volunteerId);
                await LoadVolunteersAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
}
        private bool CanAcceptRejectExecute() => IsOwner && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanStartTaskExecute))]
        private async Task StartTaskAsync()
        {
            try
            {
                var response = await _taskService.StartTaskAsync(Task.Id);
                Task.Status = "in_progress";
                IsSearching = false;
                InProgress = true;
                await ShowMsgAsync("Успех", "Задача начата", "OK");
            }
            catch (Exception e)
            {
                await ShowErrorAsync(e.Message);
            }
        }
        private bool CanStartTaskExecute() => IsOwner && Task?.Status == "searching" && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanCompleteTaskExecute))]
        private async Task CompleteTaskAsync()
        {
            try
            {
                var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Завершение", "Вы уверены, что задача выполнена?", "Да", "Нет");
                if (!confirm) return;
                var response = await _taskService.CompleteTaskAsync(Task.Id);
                Task.Status = "completed";
                InProgress = false;
                Completed = true;
                await ShowMsgAsync("Успех", "Задача завершена, награда начислена", "OK");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }
        private bool CanCompleteTaskExecute() => IsOwner && Task?.Status == "in_progress" && !IsBusy;

        [RelayCommand]
        private async Task EditAsync()
        {
            await popupMenu.CloseAsync();
            await Shell.Current.GoToAsync(nameof(TaskAddEditPage), new Dictionary<string, object?> { { "task", _task } });
        }

        [RelayCommand(CanExecute = nameof(CanDeleteTaskExecute))]
        private async Task DeleteAsync()
        {
            try
            {
                var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Удаление", "Удалить задачу?", "Да", "Нет");
                if (!confirm) return;
                var response = await _taskService.DeleteTaskAsync(Task.Id);
                Task.Id = 0;
                await Shell.Current.ClosePopupAsync();
                await ShowMsgAsync("Успех", "Задача удалена", "OK");
                await GoBackCommand.ExecuteAsync(null);
            }
            catch
            {
                await GoBackCommand.ExecuteAsync(null);
            }
        }
        private bool CanDeleteTaskExecute() => IsOwner && !IsBusy;

        [RelayCommand]
        private async Task GoToProfileAsync(int userId)
        {
            await Shell.Current.GoToAsync(nameof(ProfilePage), new Dictionary<string, object?> { { "id", userId } });
        }

        [RelayCommand]
        private async Task StartChatAsync()
        {
            try
            {
                var r = await _chatService.CreateChatAsync("personal", "", new() { Creator.Id, _userService.CurrentUser.Id});
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
        private async Task OpenPopupMenuAsync()
        {
            await PopupManager.Show(popupMenu);
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            if (Task.Id == 0) return;
            var response = await _taskService.GetTaskAsync(Task.Id);
            if (response.Data != null)
            {
                Task = response.Data;
                _isInitialized = false;
                await InitializeAsync(Task);
            }
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
            _taskService.TaskUpdated -= TaskUpdated;
        }
    }
}