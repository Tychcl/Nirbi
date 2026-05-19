using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
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
        private string? _searchQuery;

        [ObservableProperty]
        private bool _isGroup;

        [ObservableProperty]
        private bool _searchResultsVisibilitty;

        [ObservableProperty]
        private ObservableCollection<User> _searchResults = new();

        [ObservableProperty]
        private ObservableCollection<User> _selectedUsers = new();

        [ObservableProperty]
        private string? _chatName;

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
                var response = await _userService.SearchUsersAsync(query, limit: 20);
                if (response.Data == null) return;

                var users = response.Data;
                SearchResults.Clear();
                foreach (var user in users)
                {
                    if (!SelectedUsers.Any(u => u.Id == user.Id) && user.Id != _userService.CurrentUser.Id)
                        SearchResults.Add(user);
                }
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
        private void AddUser(User user)
        {
            if (user == null) return;
            if (SelectedUsers.Any(u => u.Id == user.Id)) return;
            SelectedUsers.Add(user);
            IsGroup = SelectedUsers.Count > 1;
            SearchResults.Remove(user);
            CreateChatCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void RemoveUser(User user)
        {
            if (user == null) return;
            SelectedUsers.Remove(user);
            IsGroup = SelectedUsers.Count > 1;
            if (!string.IsNullOrWhiteSpace(SearchQuery) &&
                (user.FullName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                 user.Phone?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                 user.Email?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true))
            {
                SearchResults.Add(user);
            }
            CreateChatCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery = string.Empty;
        }

        [RelayCommand(CanExecute = nameof(CanCreateChat))]
        private async Task CreateChat()
        {
            if (SelectedUsers.Count == 0) return;
            IsBusy = true;
            try
            {
                string type = SelectedUsers.Count == 1 ? "personal" : "group";
                string? name = type == "group" ? ChatName?.Trim() : null;

                if (type == "group" && string.IsNullOrWhiteSpace(name))
                    throw new Exception("Введите название группы");

                var userIds = SelectedUsers.Select(u => u.Id).ToList();
                var response = await _chatService.CreateChatAsync(type, name, userIds);
                await Shell.Current.GoToAsync($"{nameof(ChatDetailPage)}?id={response.Data}");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private bool CanCreateChat() => SelectedUsers.Count > 0 && !IsBusy;
    }
}