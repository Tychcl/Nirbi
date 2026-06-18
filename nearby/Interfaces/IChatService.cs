using nearby.Models;

namespace nearby.Interfaces
{
    public interface IChatService
    {
        // Чаты
        Task<List<Chat>> GetChatsAsync();
        Task<List<User>> GetChatUsersAsync(Guid chatId);

        // Сообщения
        Task<List<Message>> GetMessagesAsync(Guid chatId);
        Task<Guid> SendPrivateMessageAsync(Guid recipientId, string content);
        Task<Guid> SendGroupMessageAsync(Guid chatId, string content);
        Task<Message> UpdateMessageAsync(Guid messageId, string newContent);
        Task DeleteMessageAsync(Guid messageId);
        Task<List<ChatPreview>> GetMessagePreviewsAsync(List<Guid?> chatIds);
    }
}