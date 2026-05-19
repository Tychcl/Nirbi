using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Classes.Validation;
using nearby.Interfaces;
using nearby.Services;
using nearby.Views.Main;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace nearby.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IServiceProvider _serviceProvider;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Логин не может быть пустым")]
    [ValidateWithValidator(validatorName: nameof(Validate.EmailOrPhoneValidator), ErrorMessage = "Неверный логин")]
    private string _login;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Пароль не может быть пустым")]
    private string _password;
    public LoginViewModel(IAuthService authService, IUserService userService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _userService = userService;
        _serviceProvider = serviceProvider;

        ValidateAllProperties();
        ErrorsChanged += OnErrorsChanged;
    }
    protected override void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        base.OnErrorsChanged(sender, e);
        LoginCommand.NotifyCanExecuteChanged();
    }
    private bool CanLogin() => !HasErrors;
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            await ShowInnerErrorsAsync();
            return;
        }
        try
        {
            var success = await _authService.LoginAsync(Login, Password);
            _userService.CurrentUser = success.Data;
            Application.Current.MainPage = _serviceProvider.GetRequiredService<MainShell>();
        }
        catch (Exception e)
        {
            await ShowErrorAsync(e.Message);
        }
    }
}