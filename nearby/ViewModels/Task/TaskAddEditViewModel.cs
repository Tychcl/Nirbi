using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices.Sensors;
using nearby.Interfaces;
using nearby.Models;
using nearby.Services;
using nearby.Views.Main;

namespace nearby.ViewModels
{
    [QueryProperty(nameof(Task), "task")]
    public partial class TaskAddEditViewModel : BaseViewModel
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private DateTime minimumDate = DateTime.Today;

        [ObservableProperty]
        private TaskItem _task = new TaskItem();
        partial void OnTaskChanged(TaskItem item)
        {
            Title = _task.Title;
            Description = _task.Description ?? string.Empty;
            NeededVolunteers = _task.NeededVolunteers;
            Priority = _task.Priority;
            Location = _task.Location ?? string.Empty;
            Reward = _task.Reward;
            Deadline = _task.Deadline;
            minimumDate = _task.Deadline;
            ValidateAllProperties();
        }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Название обязательно")]
        [MaxLength(100, ErrorMessage = "Максимальная длина 100 символов")]
        private string _title;

        [ObservableProperty]
        private int _descriptionLength;
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [MaxLength(300, ErrorMessage = "Максимальная длина 300 символов")]
        [Required(ErrorMessage = "Описание обязательно")]
        private string _description;
        partial void OnDescriptionChanged(string value)
        {
            _task.Description = value;
            DescriptionLength = value.Length;
        }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(1, 10, ErrorMessage = "Количество волонтёров должно быть от 1 до 10")]
        private int _neededVolunteers;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Приоритет обязателен")]
        private string _priority;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Дата окончания обязательна")]
        private DateTime _deadline;

        [ObservableProperty]
        private int _locationLength;
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Адресс обязателен")]
        [MaxLength(150, ErrorMessage = "Максимальная длина 150 символов")]
        private string _location;
        partial void OnLocationChanged(string value)
        {
            _task.Location = value;
            LocationLength = value.Length;
        }

        [ObservableProperty]
        private decimal _reward;

        public TaskAddEditViewModel(ITaskService ts, IUserService us)
        {
            _taskService = ts;
            _userService = us;
            ValidateAllProperties();
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
                _task.Priority = _priority;
                _task.Deadline = _deadline;
                _task.Location = _location;
                _task.Reward = _reward;
                var response = Task.Id != 0 ? await _taskService.UpdateTaskAsync(_task.Id, _task) : await _taskService.CreateTaskAsync(_task);
                await ShowSuccessfulAsync(Task.Id != 0 ? "Задача была обновлена" : "Задача была создана");
                await GoBackCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }            
        }
    }
}