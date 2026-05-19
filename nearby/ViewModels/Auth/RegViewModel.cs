using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using nearby.Classes.Validation;
using nearby.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace nearby.ViewModels
{
    public partial class RegViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "ФИО не может быть пустым")]
        [ValidateWithValidator(validatorName: nameof(Validate.FIOValidator), ErrorMessage = "Неверный формат ФИО")]
        private string _fullName;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Номер телефона не может быть пустым")]
        [ValidateWithValidator(validatorName: nameof(Validate.PhoneValidator), ErrorMessage = "Неверный формат телефона")]
        private string _phone;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Почта не может быть пустым")]
        [ValidateWithValidator(validatorName: nameof(Validate.EmailValidator), ErrorMessage = "Неверный формат почты")]
        private string _email;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Пароль не может быть пустым")]
        [ValidateWithValidator(validatorName: nameof(Validate.PasswordValidator), ErrorMessage = "Неверный формат gароля")]
        private string _password;
        partial void OnPasswordChanged(string value)
        {
            ValidateProperty(Confirm, nameof(Confirm));
        }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Подтверждение пароля не может быть пустым")]
        [ValidateWithValidator(validatorName: nameof(Validate.PasswordValidator), ErrorMessage = "Неверный формат подтверждения пароля")]
        [property: Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        private string _confirm;

        public RegViewModel(IAuthService authService)
        {
            _authService = authService;
            ValidateAllProperties();
            ErrorsChanged += OnErrorsChanged;
        }

        protected override void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            base.OnErrorsChanged(sender, e);
            RegisterCommand.NotifyCanExecuteChanged();
        }

        private bool CanReg() => !HasErrors;
        [RelayCommand(CanExecute = nameof(CanReg))]
        private async Task RegisterAsync()
        {
            ValidateAllProperties();
            if (HasErrors)
            {
                await ShowInnerErrorsAsync();
                return;
            }
            try
            {
                var success = await _authService.RegisterAsync(FullName, Phone, Email, Password);
                await ShowMsgAsync("Успех", "Регистрация прошла успешно. Теперь войдите.", "OK");
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
            
        }
    }
}