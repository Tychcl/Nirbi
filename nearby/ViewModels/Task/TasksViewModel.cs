using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Classes;

using nearby.Interfaces;
using nearby.Models;
using nearby.Views.Main;


namespace nearby.ViewModels
{
    public partial class TasksViewModel : BaseViewModel
    {
        private readonly ITaskService _taskService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGeocodingService _geocodingService;

        [ObservableProperty]
        private List<string> sortOptions = new()
        {
            "Сначала новые",
            "По вознаграждению",
            "По волонтёрам"
        };
        [ObservableProperty]
        private string selectedSort = "Сначала новые";
        partial void OnSelectedSortChanged(string value)
        {
            sort();
        }

        [ObservableProperty]
        private string? cityFilter;
        private CancellationTokenSource? _debounceCts;
        async partial void OnCityFilterChanged(string? value)
        {
            _ = DebouncedApplyFiltersAsync();
        }

        private async Task DebouncedApplyFiltersAsync()
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            try
            {
                await Task.Delay(300, token);
                sort();
            }
            catch (TaskCanceledException){}
        }

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private ObservableCollection<TaskItem> _tasks = new();
        private ObservableCollection<TaskItem> _allTasks = new();

        private int _currentPage = 1;
        private bool _hasMorePages = true;
        private const int PageSize = 20;


        public TasksViewModel(ITaskService taskService, IServiceProvider serviceProvider, IGeocodingService geocodingService)
        {
            _taskService = taskService;
            _serviceProvider = serviceProvider;
            _geocodingService = geocodingService;
        }

        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task LoadTasks() => await LoadTasksAsync(true);

        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task Refresh() => await LoadTasksAsync(true);
        private bool CanRefresh() => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanLoadMore))]
        private async Task LoadMore() => await LoadTasksAsync(false);
        private bool CanLoadMore() => !IsBusy && _hasMorePages;

        private void ResetPagination(bool reset)
        {
            if (reset)
            {
                _currentPage = 1;
                _hasMorePages = true;
                Tasks.Clear();
                _allTasks.Clear();
            }
        }
        private async Task LoadTasksAsync(bool reset)
        {
            if (IsBusy) return;

            ResetPagination(reset);

            if (!_hasMorePages) return;

            IsBusy = true;
            try
            {
                var response = await _taskService.GetTasksAsync(offset: (_currentPage - 1) * PageSize, limit: PageSize * _currentPage);
                if (response != null && response.Any())
                {
                    foreach (var task in response)
                    {
                        if (string.IsNullOrEmpty(task.Location))
                        {
                            task.Location = await _geocodingService.GetAddressAsync((double)task.Latitude, (double)task.Longitude) ?? "Адрес недоступен";
                        }
                        if (task.Status == "В поиске волонтеров")
                        {
                            _allTasks.Add(task);
                        }
                    }
                    sort();
                    _currentPage++;
                    if (response.Count < PageSize)
                        _hasMorePages = false;
                }
                else
                {
                    _hasMorePages = false;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
                RefreshCommand.NotifyCanExecuteChanged();
                LoadMoreCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private async Task GoToDetailAsync(TaskItem task)
        {
            await Shell.Current.GoToAsync(nameof(TaskDetailPage), new Dictionary<string, object?> { { "task", task } });
        }

        [RelayCommand]
        private async Task GoToCreateAsync()
        {
            await Shell.Current.GoToAsync(nameof(TaskAddEditPage));
        }

        [RelayCommand]
        private void FilterClear()
        {
            SelectedSort = "Сначала новые";
            CityFilter = string.Empty;
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskItem task)
        {
            try
            {
                var confirm = await Application.Current.MainPage.DisplayAlert("Удаление", $"Удалить задачу \"{task.Title}\"?", "Да", "Нет");
                if (!confirm) return;
                await _taskService.DeleteTaskAsync(task.Id);
                Tasks.Remove(task);
                await Application.Current.MainPage.DisplayAlert("Успех", "Задача удалена", "OK");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        private void sort()
        {
            IEnumerable<TaskItem> sorted = _allTasks;

            switch (SelectedSort)
            {
                case "По вознаграждению":
                    sorted = _allTasks.OrderByDescending(t => t.Reward);
                    break;
                case "По волонтёрам":
                    sorted = _allTasks.OrderByDescending(t => t.NeededVolunteers);
                    break;
                    // "Сначала новые" или default – оставляем исходный порядок
            }
            if (!string.IsNullOrEmpty(CityFilter))
            {
                sorted = sorted.Where(x => x.Location != null &&
                                           x.Location.Contains(CityFilter, StringComparison.OrdinalIgnoreCase));
            }
            Tasks.Clear();
            foreach (var task in sorted)
                Tasks.Add(task);
        }
    }
}