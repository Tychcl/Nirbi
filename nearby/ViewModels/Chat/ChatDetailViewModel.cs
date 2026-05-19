using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Classes;
using nearby.ContentViews.Elements;
using nearby.Interfaces;
using nearby.Models;
using nearby.Services;

namespace nearby.ViewModels
{
    public enum ChatAction
    {
        Edit, Reply
    }
    [QueryProperty(nameof(ChatId), "id")]
    public partial class ChatDetailViewModel : BaseViewModel
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

        private const int PageSize = 50;
        private int _currentPage = 1;
        private bool _hasMoreMessages = true;
        public PopupMenu MessageOwnerPopup;
        private ObservableCollection<PopupItem> MessageOwnerPopupItems;
        public PopupMenu MessageNotOwnerPopup;
        private ObservableCollection<PopupItem> MessageNotOwnerPopupItems = new();
        public CollectionView? CV;

        [ObservableProperty]
        private ChatAction? _currentAction;
        partial void OnCurrentActionChanged(ChatAction? value)
        {
            switch (_currentAction)
            {
                case ChatAction.Edit:
                    CurrentActionName = "Редактирование";
                    CurrentActionDescription = SelectedMessage.Content;
                    CurrentActionIcon = (string)ResourceManager.Get("Edit");
                    break;
                case ChatAction.Reply:
                    CurrentActionName = "Ответ";
                    CurrentActionDescription = SelectedMessage.Content;
                    CurrentActionIcon = (string)ResourceManager.Get("Reply");
                    break;
            }
        }

        [ObservableProperty]
        private string? _currentActionName;
        [ObservableProperty]
        private string? _currentActionDescription;
        [ObservableProperty]
        private string? _currentActionIcon;


        [ObservableProperty]
        private int _curentUserId;

        [ObservableProperty]
        private int _chatId;

        [ObservableProperty]
        private Chat? _chat;

        [ObservableProperty]
        private ObservableCollection<User> _participants = new();

        [ObservableProperty]
        private ObservableCollection<Message> _messages = new();

        [ObservableProperty]
        private string _newMessageText = string.Empty;
        [ObservableProperty]
        private string _newMessageTextSave = string.Empty;

        [ObservableProperty]
        private Message? _selectedMessage;

        public int CurrentUserId => _userService.CurrentUser?.Id ?? 0;

        public ChatDetailViewModel(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
            CurentUserId = _userService.CurrentUser.Id;

            MessageNotOwnerPopupItems.Add(new((string)ResourceManager.Get("Reply"), "Ответить", ReplyMessageCommand));
            MessageNotOwnerPopupItems.Add(new((string)ResourceManager.Get("Copy"), "Копировать", CopyMessageCommand));
            
            MessageOwnerPopupItems = new(MessageNotOwnerPopupItems)
            {
                new((string)ResourceManager.Get("EditBox"), "Редактировать", EditMessageCommand),
                new((string)ResourceManager.Get("Delete"), "Удалить", DeleteMessageCommand)
            };

            MessageNotOwnerPopup = PopupManager.Create(MessageNotOwnerPopupItems, new Thickness(0));
            MessageOwnerPopup = PopupManager.Create(MessageOwnerPopupItems, new Thickness(0));
        }

        async partial void OnChatIdChanged(int value)
        {
            if (value > 0)
            {
                IsBusy = true;
                try
                {
                    await LoadChatDetailAsync();
                    await LoadMessagesBaseAsync(true);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        partial void OnChatChanged(Chat? value)
        {
            if (value != null)
                PageTitle = value.OtherUser?.FullName ?? "Чат";
        }
        protected override void OnBusyStateChanged(bool isBusy)
        {
            base.OnBusyStateChanged(isBusy);
            RefreshCommands();
        }
        partial void OnNewMessageTextChanged(string value) => SendMessageCommand.NotifyCanExecuteChanged();

        private async Task LoadChatDetailAsync()
        {
            try
            {
                var response = await _chatService.GetChatByIdAsync(ChatId, _currentPage, PageSize);
                if (response.Data != null)
                {
                    Chat = response.Data.Chat;
                    Participants.Clear();
                    if (response.Data.Participants != null)
                        foreach (var p in response.Data.Participants)
                            Participants.Add(p);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand]
        private async Task LoadMessages() => await LoadMessagesBaseAsync(true);

        [RelayCommand]
        private async Task LoadMoreMessages() => await LoadMessagesBaseAsync(false);

        private async Task LoadMessagesBaseAsync(bool reset)
        {
            try
            {
                if (reset)
                {
                    _currentPage = 1;
                    _hasMoreMessages = true;
                    Messages.Clear();
                }

                if (!_hasMoreMessages) return;

                var response = await _chatService.GetMessagesAsync(ChatId, _currentPage, PageSize);

                if (response.Data?.Data != null && response.Data.Data.Any())
                {
                    var newMessages = response.Data.Data.OrderBy(m => m.CreatedAt).ToList();
                    foreach (var msg in newMessages)
                    {
                        msg.ChatType = Chat.Type;
                        Messages.Add(msg);
                    }
                    _currentPage++;
                    if (response.Data.Data.Count < PageSize)
                        _hasMoreMessages = false;
                }
                else
                {
                    _hasMoreMessages = false;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        private async Task EditMessageTask()
        {
            try
            {
                if (SelectedMessage is not Message message) return;
                SelectedMessage = null;
                var newText = NewMessageText;
                NewMessageText = string.Empty;
                if (string.IsNullOrWhiteSpace(newText)) return;
                var result = await _chatService.EditMessageAsync(message.Id, newText);
                message.Content = newText;
                var index = Messages.IndexOf(message);
                if (index >= 0)
                    Messages[index] = message;
            }
            catch (Exception ex)
            {
                CurrentAction = null;
                await ShowErrorAsync(ex.Message);
            }
        }
        private async Task SendMessageTask(int? reply = null)
        {
            try
            {
                var mes = NewMessageText.Trim();
                if (string.IsNullOrWhiteSpace(mes)) return;
                var messageModel = new MessageSendModel { content_type = "text", content = mes, reply = reply };
                var response = await _chatService.SendMessageAsync(ChatId, messageModel);
                //var newMessage = new Message
                //{
                //    Id = response.Data?.Id ?? 0,
                //    Content = mes,
                //    ContentType = "text",
                //    CreatedAt = DateTime.UtcNow,
                //    SenderId = CurrentUserId,
                //    SenderName = _userService.CurrentUser?.FullName ?? "Вы",
                //    SenderProfilePicture = _userService.CurrentUser?.ProfilePicture
                //};
                Messages.Add(response.Data);
                NewMessageText = string.Empty;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        private bool CanSendMessage() => !string.IsNullOrWhiteSpace(NewMessageText) && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanSendMessage))]
        private async Task SendMessage()
        {
            try
            {
                switch (CurrentAction)
                {
                    case ChatAction.Edit:
                        await EditMessageTask();
                        break;
                    case ChatAction.Reply:
                        await SendMessageTask(SelectedMessage.Id);
                        SelectedMessage = null;
                        break;
                    default:
                        await SendMessageTask();
                        break;
                }
                CurrentAction = null;
            }
            catch (Exception ex)
            {
                CurrentAction = null;
                await ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand]
        private async Task CancelAction()
        {
            if (CurrentAction == ChatAction.Edit)
            {
                NewMessageText = NewMessageTextSave;
            }
            CurrentAction = null;
        }

        [RelayCommand]
        private async Task AddMember()
        {
            try
            {
                var idString = await Application.Current!.MainPage!.DisplayPromptAsync("Добавить участника", "Введите ID пользователя:");
                if (!int.TryParse(idString, out int userId)) return;
                var result = await _chatService.AddMemberAsync(ChatId, userId);
                await LoadChatDetailAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifyMember))]
        private async Task RemoveMember(User user)
        {
            try
            {
                var confirm = await Application.Current!.MainPage!.DisplayAlert("Удаление", $"Удалить {user.FullName} из чата?", "Да", "Нет");
                if (!confirm) return;
                var result = await _chatService.RemoveMemberAsync(ChatId, user.Id);
                Participants.Remove(user);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }
        private bool CanModifyMember() => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanModifyMessage))]
        private async Task EditMessage()
        {
            try
            {
                if (SelectedMessage is not Message message) return;
                NewMessageTextSave = NewMessageText;
                NewMessageText = message.Content;
                CurrentAction = ChatAction.Edit;
                await PopupManager.navigation.ClosePopupAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand]
        private async Task ReplyMessage()
        {
            try
            {
                if (SelectedMessage is not Message message) return;
                CurrentAction = ChatAction.Reply;
                await PopupManager.navigation.ClosePopupAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifyMessage))]
        private async Task DeleteMessage()
        {
            try
            {
                if (SelectedMessage is not Message message) return;
                SelectedMessage = null;
                var confirm = await Application.Current!.MainPage!.DisplayAlert("Удаление", "Удалить сообщение?", "Да", "Нет");
                if (!confirm) return;
                await PopupManager.navigation.ClosePopupAsync();
                var result = await _chatService.DeleteMessageAsync(message.Id);
                Messages.Remove(message);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }
        private bool CanModifyMessage() => !IsBusy;

        [RelayCommand]
        private async Task OpenMenu(object view)
        {
            if (view is not MessageView MV) return;
            SelectedMessage = MV.Message;
            if (MV.IsOwnMessage)
            {
                await PopupManager.Show(MessageOwnerPopup, MV.point.Value.X, MV.point.Value.Y);
            }
            else
            {
                await PopupManager.Show(MessageNotOwnerPopup, MV.point.Value.X, MV.point.Value.Y);
            }
        }

        [RelayCommand]
        private async Task CopyMessage()
        {
            if (SelectedMessage is not Message message) return;
            SelectedMessage = null;
            await PopupManager.navigation.ClosePopupAsync();
            await Clipboard.Default.SetTextAsync(message.Content);
        }

        [RelayCommand]
        private async Task GoToReplyedMessage(int? id)
        {
            if (id is null || CV is null) return;
            CV.ScrollTo(Messages.First(x => x.Id == id));
        }

        private void RefreshCommands()
        {
            SendMessageCommand.NotifyCanExecuteChanged();
            AddMemberCommand.NotifyCanExecuteChanged();
            RemoveMemberCommand.NotifyCanExecuteChanged();
            EditMessageCommand.NotifyCanExecuteChanged();
            DeleteMessageCommand.NotifyCanExecuteChanged();
            LoadMessagesCommand.NotifyCanExecuteChanged();
            LoadMoreMessagesCommand.NotifyCanExecuteChanged();
        }
    }
}