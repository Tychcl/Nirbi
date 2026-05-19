using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using nearby.Classes;
using nearby.ContentViews.Elements;
using nearby.Interfaces;
using nearby.Services;
using nearby.ViewModels;
using nearby.Views;
using nearby.Views.Auth;
using nearby.Views.Main;

namespace nearby
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialSymbols-Rounded.ttf", "Icons");
                });
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                if (view is NoUnderlineEditor)
                {
                    #if ANDROID
                        handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                    #endif
                }
            });
            Microsoft.Maui.Handlers.ToolbarHandler.Mapper.AppendToMapping("CustomTitleViewInsets", (handler, view) =>
            {
                #if ANDROID
                    var toolbar = handler.PlatformView;
                    toolbar.SetContentInsetsAbsolute(0, 0);
                    toolbar.ContentInsetStartWithNavigation = 0;
                    toolbar.ContentInsetEndWithActions = 0;
                    toolbar.SetPadding(0, toolbar.PaddingTop, 0, toolbar.PaddingBottom);
                #endif
            });
#if DEBUG
            builder.Logging.AddDebug();
#endif
            //====================
            //Сервисы
            //====================
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<ITokenService, TokenService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddSingleton<ITaskService, TaskService>();
            builder.Services.AddSingleton<IChatService, ChatService>();

            builder.Services.AddTransient<CreateChatViewModel>();
            builder.Services.AddTransient<CreateChatPage>();

            //=====================
            //ВиевМодели
            //=====================
            //аунтефикация
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegViewModel>();

            builder.Services.AddTransient<ChatsViewModel>();
            builder.Services.AddTransient<ChatDetailViewModel>();
            builder.Services.AddTransient<EditProfileViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<TasksViewModel>();
            builder.Services.AddTransient<TaskDetailViewModel>();
            builder.Services.AddTransient<TaskAddEditViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<ThemeChangeViewModel>();

            //=====================
            //Страницы
            //=====================
            builder.Services.AddTransient<LoadingPage>();
            //аунтефикация
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegPage>();
            builder.Services.AddTransient<AuthShell>();
            //основные
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<TasksPage>();
            builder.Services.AddTransient<MainShell>();
            builder.Services.AddTransient<ChatsPage>();
            builder.Services.AddTransient<SettingsPage>();
            //дополнительные
            builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<TaskDetailPage>();
            builder.Services.AddTransient<TaskAddEditPage>();
            builder.Services.AddTransient<ChatDetailPage>();
            //настройки
            builder.Services.AddTransient<ThemeChangePage>();
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                System.Diagnostics.Debug.WriteLine($"ГЛОБАЛЬНАЯ ОШИБКА: {ex}");
            };
            return builder.Build();
        }
    }
}
