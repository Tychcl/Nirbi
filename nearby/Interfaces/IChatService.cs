using nearby.Classes;
using nearby.Models;

namespace nearby.Interfaces;

public interface IChatService
{
    Task<ApiResponse<List<Chat>>> GetChatsAsync(int page = 1, int limit = 20);
    Task<ApiResponse<DetailChatInfo>> GetChatByIdAsync(int chatId, int page = 1, int limit = 50);
    Task<ApiResponse<int>> CreateChatAsync(string type, string? name, List<int> userIds);
    Task<ApiResponse<bool>> AddMemberAsync(int chatId, int userId);
    Task<ApiResponse<bool>> RemoveMemberAsync(int chatId, int userId);
    Task<ApiResponse<Message>> SendMessageAsync(int chatId, MessageSendModel message);
    Task<ApiResponse<nearby.Models.Messages>> GetMessagesAsync(int chatId, int page = 1, int limit = 50);
    Task<ApiResponse<bool>> MarkMessagesAsReadAsync(int chatId, int messageId);
    Task<ApiResponse<Message>> EditMessageAsync(int messageId, string newContent);
    Task<ApiResponse<bool>> DeleteMessageAsync(int messageId);
}