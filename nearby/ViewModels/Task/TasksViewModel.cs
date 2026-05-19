using System.Collections.ObjectModel;
using System.ComponentModel;
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

        [ObservableProperty]
        private List<string> priorityList = new() { "Все", "Низкий", "Средний", "Высокий" };

        private Dictionary<string, string> priorityMap = new()
        {
            {"Все", ""},
            {"Низкий", "low"},
            {"Средний", "medium"},
            {"Высокий", "high"}
        };

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _priorityFilter;
        async partial void OnPriorityFilterChanged(string value)
        {
            _priorityFilter = priorityMap.GetValueOrDefault(PriorityFilter ?? "Все", "");
            await Refresh();
        }

        [ObservableProperty]
        private string _cityFilter;
        async partial void OnCityFilterChanged(string value)
        {
            await Refresh();
        }

        [ObservableProperty]
        private ObservableCollection<TaskItem> _tasks = new();

        private int _currentPage = 1;
        private bool _hasMorePages = true;
        private const int PageSize = 10;


        public TasksViewModel(ITaskService taskService, IServiceProvider serviceProvider)
        {
            _taskService = taskService;
            _serviceProvider = serviceProvider;
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
                var response = await _taskService.GetTasksAsync(_currentPage, PageSize, "searching", PriorityFilter, CityFilter);
                if (response.Data != null && response.Data.Any())
                {
                    foreach (var task in response.Data)
                        Tasks.Add(task);
                    _currentPage++;
                    if (response.Data.Count < PageSize)
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
            PriorityFilter = string.Empty;
            CityFilter = string.Empty;
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskItem task)
        {
            try
            {
                var confirm = await Application.Current.MainPage.DisplayAlert("Удаление", $"Удалить задачу \"{task.Title}\"?", "Да", "Нет");
                if (!confirm) return;
                var success = await _taskService.DeleteTaskAsync(task.Id);
                Tasks.Remove(task);
                await Application.Current.MainPage.DisplayAlert("Успех", "Задача удалена", "OK");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }
    }
}