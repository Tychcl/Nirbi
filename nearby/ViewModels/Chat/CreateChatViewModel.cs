using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Interfaces;
using nearby.Models;
using nearby.Services;
using nearby.Views.Main;

namespace nearby.ViewModels
{
    public partial class CreateChatViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        [ObservableProperty]
        private string? _firstMessage;

        [ObservableProperty]
        private string? _searchQuery;

        [ObservableProperty]
        private bool _searchResultsVisibilitty;

        [ObservableProperty]
        private ObservableCollection<User> _searchResults = new();

        private CancellationTokenSource? _debounceCts;

        public CreateChatViewModel(IUserService userService, IChatService chatService)
        {
            _userService = userService;
            _chatService = chatService;
            PageTitle = "Новый чат";
        }

        partial void OnSearchQueryChanged(string? value)
        {
            _ = DebouncedSearchAsync(value);
        }

        private async Task DebouncedSearchAsync(string? query)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;
            try
            {
                await Task.Delay(300, token);
                if (string.IsNullOrWhiteSpace(query))
                {
                    SearchResultsVisibilitty = false;
                    SearchResults.Clear();
                    return;
                }
                SearchResultsVisibilitty = true;
                await ExecuteSearchAsync(query, token);
            }
            catch (TaskCanceledException) { }
        }

        private async Task ExecuteSearchAsync(string query, CancellationToken token)
        {
            IsBusy = true;
            try
            {
                var response = await _userService.GetUsersAsync(search: query, fields: new List<string>() { "firstName", "secondName", "lastName", "username", "email", "phone" });
                if (response == null) return;
                SearchResults.Clear();
                SearchResults = response.ToObservableCollection();
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async void AddUser(User user)
        {
            try
            {
                if (user == null) return;
                var chats = await _chatService.GetChatsAsync();
                var chat = chats.FirstOrDefault(x => x.ChatUsers.Contains(user.Id));
                if (chat is null)
                {
                    if (string.IsNullOrEmpty(FirstMessage))
                    {
                        await ShowErrorAsync("Нужно написать первое сообщение");
                        return;
                    }
                    await _chatService.SendPrivateMessageAsync(user.Id, FirstMessage.Trim());
                    chats = await _chatService.GetChatsAsync();
                    chat = chats.FirstOrDefault(x => x.ChatUsers.Contains(user.Id));
                }
                chat.Name = user.FullName;
                await GoBackCommand.ExecuteAsync(null);
                await Shell.Current.GoToAsync(nameof(ChatDetailPage), new Dictionary<string, object?> { { "chat", chat } });
            } 
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
            
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery = string.Empty;
        }

        private bool CanCreateChat() => !IsBusy;
    }
}