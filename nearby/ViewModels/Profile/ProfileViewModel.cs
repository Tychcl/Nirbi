using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using nearby.Interfaces;
using nearby.Models;
using nearby.Services;
using nearby.Views.Main;
using nearby.Views.Auth;
using nearby.Classes;

namespace nearby.ViewModels
{
    public enum TaskCategory { Created, InProgress, Completed }

    [QueryProperty(nameof(UserId), "id")] //-1 - current user
    public partial class ProfileViewModel : BaseViewModel, IDisposable
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly ITaskService _taskService;
        private readonly IChatService _chatService;
        private readonly IServiceProvider _serviceProvider;

        private const int TaskPageSize = 10;
        private int _currentTaskPage = 1;
        private bool _hasMoreTasks = true;

        [ObservableProperty]
        private int _userId = -99;
        async partial void OnUserIdChanged(int value)
        {
            await LoadData();
        }

        [ObservableProperty]
        private TaskCategory _selectedCategory = TaskCategory.Created;
        async partial void OnSelectedCategoryChanged(TaskCategory value)
        {
            _currentTaskPage = 1;
            _hasMoreTasks = true;
            await LoadUserTasksAsync(reset: true);
        }

        [ObservableProperty]
        private ObservableCollection<TaskItem> _userTasks = new();

        [ObservableProperty]
        private User _user = null!;

        [ObservableProperty]
        private bool _isOwnProfile;
        partial void OnIsOwnProfileChanged(bool value)
        {
            LogoutCommand.NotifyCanExecuteChanged();
            GoToEditCommand.NotifyCanExecuteChanged();
        }

        public ICommand SelectCategoryCommand { get; }
        public ProfileViewModel(
            IUserService userService,
            IAuthService authService,
            ITaskService taskService,
            IServiceProvider serviceProvider,
            IChatService chatService)
        {
            _userService = userService;
            _authService = authService;
            _taskService = taskService;
            _serviceProvider = serviceProvider;
            _chatService = chatService;

            SelectCategoryCommand = new Command<TaskCategory>(category => SelectedCategory = category);
            _userService.PropertyChanged += OnUserServicePropertyChanged;
            _chatService = chatService;
        }

        void OnUserServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsOwnProfile && e.PropertyName == nameof(IUserService.CurrentUser)) LoadUserData();
        }

        [RelayCommand]
        private async Task TaskSelectedAsync(TaskItem task)
        {
            await Shell.Current.GoToAsync(nameof(TaskDetailPage), new Dictionary<string, object?> { { "task", task } });
        }

        [RelayCommand]
        private async Task StartChatAsync()
        {
            try
            {
                var r = await _chatService.CreateChatAsync("personal", "", new() { User.Id, _userService.CurrentUser.Id });
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

        [RelayCommand(CanExecute = nameof(IsOwnProfile))]
        private async Task LogoutAsync()
        {
            bool confirm = await Application.Current!.MainPage!.DisplayAlert("Подтверждение", "Вы действительно хотите выйти?", "Да", "Нет");
            if (!confirm) return;
            await _authService.LogoutAsync();
            _userService.PropertyChanged -= OnUserServicePropertyChanged;
            _userService.CurrentUser = null;
            Application.Current.MainPage = _serviceProvider.GetRequiredService<AuthShell>();
        }

        [RelayCommand(CanExecute = nameof(IsOwnProfile))]
        private async Task GoToEditAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileVM] Вызов: {DateTime.Now}");
            if (Shell.Current != null)
                await Shell.Current.GoToAsync(nameof(EditProfilePage), true);
            else
            {
                var page = _serviceProvider.GetRequiredService<EditProfilePage>();
                var vm = _serviceProvider.GetRequiredService<EditProfileViewModel>();
                page.BindingContext = vm;
                await Application.Current.MainPage.Navigation.PushModalAsync(page);
            }
            System.Diagnostics.Debug.WriteLine($"[ProfileVM] Конец: {DateTime.Now}");
        }

        [RelayCommand]
        public async Task LoadUserTasksAsync(bool reset)
        {
            if (_user == null) return;
            if (reset)
            {
                _currentTaskPage = 1;
                _hasMoreTasks = true;
                UserTasks.Clear();
            }
            if (!_hasMoreTasks) return;
            try
            {
                string status = SelectedCategory switch
                {
                    TaskCategory.Created => "searching",
                    TaskCategory.InProgress => "in_progress",
                    TaskCategory.Completed => "completed",
                    _ => ""
                };

                var response = await _taskService.GetUserTasksAsync(_user.Id, status, _currentTaskPage, TaskPageSize);
                if (response?.Data == null) return;

                var tasks = response.Data;
                if (tasks.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        foreach (var task in tasks)
                            UserTasks.Add(task);
                    });
                    _currentTaskPage++;
                    if (tasks.Count < TaskPageSize)
                        _hasMoreTasks = false;
                }
                else
                {
                    _hasMoreTasks = false;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        private async Task LoadUserData()
        {
            try
            {
                IsOwnProfile = _userId == -1 || (_userService.CurrentUser != null && _userService.CurrentUser.Id == _userId);

                if (IsOwnProfile)
                {
                    if (_userService.CurrentUser == null) return;
                    User = _userService.CurrentUser;
                }
                else
                {
                    var response = await _userService.LoadUserByIdAsync((int)_userId);
                    User = response.Data;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        public async Task LoadData()
        {
            await LoadUserData();
            await LoadUserTasksAsync(reset: true);
        }

        public void Dispose()
        {
            _userService.PropertyChanged -= OnUserServicePropertyChanged;
        }
    }
}