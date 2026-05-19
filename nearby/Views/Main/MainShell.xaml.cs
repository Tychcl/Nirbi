
namespace nearby.Views.Main;
public partial class MainShell : Shell
{
    public MainShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));

        Routing.RegisterRoute(nameof(TaskDetailPage), typeof(TaskDetailPage));
        Routing.RegisterRoute(nameof(TaskAddEditPage), typeof(TaskAddEditPage));

        Routing.RegisterRoute(nameof(CreateChatPage), typeof(CreateChatPage));
        Routing.RegisterRoute(nameof(ChatDetailPage), typeof(ChatDetailPage));
        //настройки
        Routing.RegisterRoute(nameof(ThemeChangePage), typeof(ThemeChangePage));
    }
}
