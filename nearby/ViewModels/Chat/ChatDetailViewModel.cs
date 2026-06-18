using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using System.Windows.Markup;
using CommunityToolkit.Maui.Core.Extensions;
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
    [QueryProperty(nameof(Chat), "chat")]
    public partial class ChatDetailViewModel : BaseViewModel
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

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
        private Guid? _curentUserId;
        [ObservableProperty]
        private Chat? _chat;
        async partial void OnChatChanged(Chat? value)
        {
            if (value is not null && value.Id is not null)
            {
                IsBusy = true;
                try
                {
                    PageTitle = value.Name;
                    await LoadMessagesBaseAsync(true);
                }
                finally
                {
                    if (CV is not null)
                        CV.ScrollTo(Messages.Last(), position: ScrollToPosition.MakeVisible, animate: false);
                    IsBusy = false;
                }
            }
        }


        private Guid _recipientId;

        [ObservableProperty]
        private ObservableCollection<Message> _messages = new();

        [ObservableProperty]
        private string _newMessageText = string.Empty;
        [ObservableProperty]
        private string _newMessageTextSave = string.Empty;

        [ObservableProperty]
        private Message? _selectedMessage;


        public ChatDetailViewModel(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
            CurentUserId = _userService.CurrentUserId; 
            //MessageNotOwnerPopupItems.Add(new((string)ResourceManager.Get("Reply"), "Ответить", ReplyMessageCommand));
            MessageNotOwnerPopupItems.Add(new((string)ResourceManager.Get("Copy"), "Копировать", CopyMessageCommand));
            
            MessageOwnerPopupItems = new(MessageNotOwnerPopupItems)
            {
                new((string)ResourceManager.Get("EditBox"), "Редактировать", EditMessageCommand),
                new((string)ResourceManager.Get("Delete"), "Удалить", DeleteMessageCommand)
            };

            MessageNotOwnerPopup = PopupManager.Create(MessageNotOwnerPopupItems, new Thickness(0));
            MessageOwnerPopup = PopupManager.Create(MessageOwnerPopupItems, new Thickness(0));
        }

        
        protected override void OnBusyStateChanged(bool isBusy)
        {
            base.OnBusyStateChanged(isBusy);
            RefreshCommands();
        }
        partial void OnNewMessageTextChanged(string value) => SendMessageCommand.NotifyCanExecuteChanged();

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
                    Messages.Clear();
                }
                if (Chat.IsPersonalChat)
                    _recipientId = Chat.ChatUsers.FirstOrDefault(x => x != _userService.CurrentUserId);
                Chat.ChatUsersFullNames = await _userService.GetUserFullNamesAsync(Chat.ChatUsers);
                Chat.ChatUsersFullNamesDict = Chat.ChatUsersFullNames.ToDictionary(
                    fn => fn.Id, fn => $"{fn.SecondName} {fn.FirstName} {fn.LastName}");
                var response = await _chatService.GetMessagesAsync((Guid)Chat.Id);
                response = response.Where(x => !x.IsDeleted).ToList();
                if (response != null && response.Any())
                {
                    response.ConvertAll(x => x.SenderName = Chat.ChatUsersFullNamesDict.GetValueOrDefault(x.Sender, "Неизвестный"));
                    Messages = response.ToObservableCollection();
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
                await _chatService.UpdateMessageAsync(message.Id, newText);
                message.Content = newText;
                message.IsUpdated = true;
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
        private async Task SendMessageTask(Guid? reply = null)
        {
            try
            {
                var mes = NewMessageText.Trim();
                if (string.IsNullOrWhiteSpace(mes)) return;
                Guid messageId;
                if (Chat.IsPersonalChat)
                {
                    messageId = await _chatService.SendPrivateMessageAsync(_recipientId, NewMessageText);
                }
                else
                {
                    messageId = await _chatService.SendGroupMessageAsync((Guid)Chat.Id, NewMessageText);
                }
                var newMessage = new Message
                {
                    Id = messageId,
                    Sender = (Guid)CurentUserId,
                    ChatId = (Guid)Chat.Id,
                    CreatedAt = DateTime.Now,
                    Content = mes,
                };
                NewMessageText = string.Empty;
                Messages.Add(newMessage);
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
                    //case ChatAction.Reply:
                    //    await SendMessageTask(SelectedMessage.Id);
                    //    SelectedMessage = null;
                    //    break;
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

        //[RelayCommand]
        //private async Task AddMember()
        //{
        //   
        //}

        //[RelayCommand(CanExecute = nameof(CanModifyMember))]
        //private async Task RemoveMember(User user)
        //{
        //    try
        //    {
        //        var confirm = await Application.Current!.MainPage!.DisplayAlert("Удаление", $"Удалить {user.FullName} из чата?", "Да", "Нет");
        //        if (!confirm) return;
        //        var result = await _chatService.RemoveMemberAsync(ChatId, user.Id);
        //        Participants.Remove(user);
        //    }
        //    catch (Exception ex)
        //    {
        //        await ShowErrorAsync(ex.Message);
        //    }
        //}
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
                await _chatService.DeleteMessageAsync(message.Id);
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
            //if (id is null || CV is null) return;
            //CV.ScrollTo(Messages.First(x => x.Id == id));
        }

        private void RefreshCommands()
        {
            SendMessageCommand.NotifyCanExecuteChanged();
            //AddMemberCommand.NotifyCanExecuteChanged();
            //RemoveMemberCommand.NotifyCanExecuteChanged();
            EditMessageCommand.NotifyCanExecuteChanged();
            DeleteMessageCommand.NotifyCanExecuteChanged();
            LoadMessagesCommand.NotifyCanExecuteChanged();
            LoadMoreMessagesCommand.NotifyCanExecuteChanged();
        }
    }
}