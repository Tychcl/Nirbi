using nearby.Interfaces;
using nearby.Services;
using nearby.Views;
using nearby.Views.Main;
using nearby.Views.Auth;
using nearby.Classes;

namespace nearby
{
    public partial class App : Application
    {

        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApiClient _apiClient;

        public App(ITokenService tokenService, IUserService userService, IServiceProvider serviceProvider, ApiClient apiClient)
        {
            InitializeComponent();
            _tokenService = tokenService;
            _userService = userService;
            _serviceProvider = serviceProvider;
            _apiClient = apiClient;
            ThemeManager.LoadSavedTheme();
            MainPage = _serviceProvider.GetRequiredService<LoadingPage>();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            try
            {
                await ResourceManager.Load<int>("PrimaryFontSize");
                await ResourceManager.Load<int>("SecondaryFontSize");
                var token = await _tokenService.GetTokenAsync(TokenService.TokenKey.Refresh);
                if(string.IsNullOrEmpty(token))
                {
                    MainPage = _serviceProvider.GetRequiredService<AuthShell>();
                    return;
                }
                var auth = await _apiClient.RefreshAsync(token);
                if (auth is null)
                {
                    MainPage = _serviceProvider.GetRequiredService<AuthShell>();
                    return;
                }
                _userService.CurrentUserId = auth!.UserId;
                await _tokenService.SetTokenAsync(TokenService.TokenKey.Access, auth.AccessToken);
                await _tokenService.SetTokenAsync(TokenService.TokenKey.Refresh, auth.RefreshToken);
                if (_userService.CurrentUserId is not null)
                {
                    await _userService.LoadUserByIdAsync(_userService.CurrentUserId);
                    MainPage = _serviceProvider.GetRequiredService<MainShell>();
                    return;
                }
                else
                {
                    MainPage = _serviceProvider.GetRequiredService<AuthShell>();
                    return;
                }
            }
            catch
            {
                MainPage = _serviceProvider.GetRequiredService<AuthShell>();
            }
        }
    }
}