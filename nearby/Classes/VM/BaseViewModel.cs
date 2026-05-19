using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace nearby
{
    public abstract partial class BaseViewModel : ObservableValidator
    {
        [ObservableProperty]
        private string _pageTitle = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            protected set => SetProperty(ref _errorMessage, value);
        }

        protected BaseViewModel()
        {
            ErrorsChanged += OnErrorsChanged;
        }

        protected virtual void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            var allErrors = GetErrors()
                .SelectMany(err => err.ErrorMessage != null ? new[] { err.ErrorMessage } : Array.Empty<string>())
                .Distinct();
            ErrorMessage = string.Join(Environment.NewLine, allErrors);
            OnPropertyChanged(nameof(HasErrors));
        }

        protected void ValidateAndNotify(string propertyName)
        {
            ValidateProperty(propertyName);
        }

        protected virtual Task ShowInnerErrorsAsync()
            => ShowErrorAsync(ErrorMessage);

        protected virtual Task ShowErrorAsync(string message)
            => ShowMsgAsync("Ошибка", message, "OK");

        protected virtual Task ShowSuccessfulAsync(string message)
            => ShowMsgAsync("Успешно", message, "OK");

        protected virtual Task ShowMsgAsync(string title, string message, string cancel = "OK")
            => Application.Current!.MainPage!.DisplayAlert(title, message, cancel);

        [RelayCommand]
        private static async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

        partial void OnIsBusyChanged(bool value)
        {
            OnBusyStateChanged(value);
        }

        protected virtual void OnBusyStateChanged(bool isBusy)
        {

        }
    }
}