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

        public App(ITokenService tokenService, IUserService userService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _tokenService = tokenService;
            _userService = userService;
            _serviceProvider = serviceProvider;
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
                var token = await _tokenService.GetTokenAsync();
                if(string.IsNullOrEmpty(token)) MainPage = _serviceProvider.GetRequiredService<AuthShell>();
                await _userService.LoadUserByIdAsync();
                if (_userService.CurrentUser is not null)
                {
                    MainPage = _serviceProvider.GetRequiredService<MainShell>();
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