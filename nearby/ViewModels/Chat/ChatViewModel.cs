using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using nearby.Interfaces;
using nearby.Models;
using nearby.Views.Main;

namespace nearby.ViewModels
{
    public partial class ChatsViewModel : BaseViewModel
    {
        private readonly IChatService _chatService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserService _userService;

        [ObservableProperty]
        private ObservableCollection<Chat> _chats = new();

        [ObservableProperty]
        private bool _isRefreshing;
        partial void OnIsRefreshingChanged(bool value)
        {
            LoadMoreCommand.NotifyCanExecuteChanged();
        }

        protected override void OnBusyStateChanged(bool isBusy)
        {
            base.OnBusyStateChanged(isBusy);
            RefreshCommand.NotifyCanExecuteChanged();
            LoadMoreCommand.NotifyCanExecuteChanged();
        }

        public ChatsViewModel(IChatService chatService, IServiceProvider serviceProvider, IUserService userService)
        {
            _chatService = chatService;
            _serviceProvider = serviceProvider;
            _userService = userService;
        }

        [RelayCommand]
        private async Task LoadChatsAsync() => await LoadChatsBaseAsync(true);
        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task RefreshAsync() => await LoadChatsBaseAsync(true);
        [RelayCommand(CanExecute = nameof(CanLoadMore))]
        private async Task LoadMoreAsync() => await LoadChatsBaseAsync(false);
        private bool CanRefresh() => !IsBusy;
        private bool CanLoadMore() => !IsBusy;

        private async Task LoadChatsBaseAsync(bool reset)
        {
            try
            {
                if (IsBusy) return;
                IsBusy = true;
                if (reset)
                {
                    Chats.Clear();
                }
                IsRefreshing = true;
                var response = await _chatService.GetChatsAsync();
                var previews = await _chatService.GetMessagePreviewsAsync(response.Select(x => x.Id).ToList());
                response.ConvertAll(c => c.Preview = previews.FirstOrDefault(x => x.ChatId == c.Id));
                await GetChats(response);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException ?? ex;
                System.Diagnostics.Debug.WriteLine($"LoadChatsBaseAsync error: {inner.GetType()}: {inner.Message}");
                if (ex is TargetInvocationException tie)
                    System.Diagnostics.Debug.WriteLine($"Real error: {tie.InnerException?.Message}");
                await ShowErrorAsync(inner.Message);
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        private async Task GetChats(List<Chat>? chats)
        {
            if (chats != null && chats.Any())
            {
                foreach (var chat in chats)
                {
                    if (chat.IsPersonalChat)
                    {
                        Guid id = chat.ChatUsers.First(x => x != _userService.CurrentUserId);
                        var name = await _userService.GetUserFullNamesAsync(new List<Guid>() { id });
                        chat.Name = $"{name[0].SecondName} {name[0].FirstName} {name[0].LastName}";
                    }
                    Chats.Add(chat);
                }
            }
        }

        [RelayCommand]
        private async Task GoToChatDetailAsync(Chat chat)
        {
            await Shell.Current.GoToAsync(nameof(ChatDetailPage), new Dictionary<string, object?> { { "chat", chat } });
        }

        [RelayCommand]
        private async Task CreateChatAsync()
        {
            await Shell.Current.GoToAsync(nameof(CreateChatPage));
        }
    }
}