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

        private int _currentPage = 1;
        private bool _hasMorePages = true;
        private const int PageSize = 20;

        public ChatsViewModel(IChatService chatService, IServiceProvider serviceProvider)
        {
            _chatService = chatService;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task LoadChatsAsync() => await LoadChatsBaseAsync(true);
        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task RefreshAsync() => await LoadChatsBaseAsync(true);
        [RelayCommand(CanExecute = nameof(CanLoadMore))]
        private async Task LoadMoreAsync() => await LoadChatsBaseAsync(false);
        private bool CanRefresh() => !IsBusy;
        private bool CanLoadMore() => !IsBusy && _hasMorePages;

        private async Task LoadChatsBaseAsync(bool reset)
        {
            try
            {
                if (IsBusy) return;
                IsBusy = true;
                if (reset)
                {
                    _currentPage = 1;
                    _hasMorePages = true;
                    Chats.Clear();
                }

                if (!_hasMorePages) return;

                IsRefreshing = true;
                var response = await _chatService.GetChatsAsync(_currentPage, PageSize);
                await GetChats(response.Data);
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
                    Chats.Add(chat);
                _currentPage++;
                if (chats.Count < PageSize)
                    _hasMorePages = false;
            }
            else
            {
                _hasMorePages = false;
            }
        }

        [RelayCommand]
        private async Task GoToChatDetailAsync(Chat chat)
        {
            await Shell.Current.GoToAsync(nameof(ChatDetailPage), new Dictionary<string, object?> { { "id", chat.Id } });
        }

        [RelayCommand]
        private async Task CreateChatAsync()
        {
            await Shell.Current.GoToAsync(nameof(CreateChatPage));
        }
    }
}