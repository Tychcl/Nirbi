using System.Text;
using Newtonsoft.Json;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;

namespace nearby.Services;

public class ChatService : IChatService
{
    private readonly ApiClient _apiClient;

    public ChatService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    //Метод получения чатов пользователя
    public async Task<ApiResponse<List<Chat>>> GetChatsAsync(int page = 1, int limit = 20)
    {
        var response = await _apiClient.GetAsync($"chats?page={page}&limit={limit}");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var result = JsonConvert.DeserializeObject<ApiResponse<List<Chat>>>(json);
        return result;
    }
    //мтеод получения информации о чате
    public async Task<ApiResponse<DetailChatInfo>> GetChatByIdAsync(int chatId, int page = 1, int limit = 50)
    {
        var response = await _apiClient.GetAsync($"chats/{chatId}?page={page}&limit={limit}");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var chatDetail = JsonConvert.DeserializeObject<DetailChatInfo>(json);
        return new ApiResponse<DetailChatInfo>("", chatDetail);
    }
    //метод создания чата
    public async Task<ApiResponse<int>> CreateChatAsync(string type, string? name, List<Guid> userIds)
    {
        var payload = new
        {
            type,
            name,
            user_ids = userIds
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync("chats", content);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        json = await response.Content.ReadAsStringAsync();
        Dictionary<string, object>? data;
        if (!response.IsSuccessStatusCode)
        {
            if(response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (data != null && data.TryGetValue("id", out var idVT) && idVT is long idLT)
                {
                    return new ApiResponse<int>("", (int)idLT);
                }
            }
            throw new Exception(json);
        }
        data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        if (data != null && data.TryGetValue("id", out var idObj) && idObj is long idLong)
        {
            return new ApiResponse<int>("", (int)idLong);
        }
        throw new Exception("Не известная ошибка");
    }
    //метод добавления пользователя в чат
    public async Task<ApiResponse<bool>> AddMemberAsync(int chatId, Guid userId)
    {
        var payload = new { user_id = userId };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync($"chats/{chatId}/members", content);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<bool>(json, response.IsSuccessStatusCode);
    }
    //метод удаления пользователя из чата
    public async Task<ApiResponse<bool>> RemoveMemberAsync(int chatId, Guid userId)
    {
        var response = await _apiClient.DeleteAsync($"chats/{chatId}/members/{userId}");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<bool>(json, response.IsSuccessStatusCode);
    }
    //метод отправки сообщения
    public async Task<ApiResponse<Message>> SendMessageAsync(int chatId, MessageSendModel message)
    {
        var json = JsonConvert.SerializeObject(message);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync($"chats/{chatId}/messages", content);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var data = JsonConvert.DeserializeObject<ApiResponse<Message>>(json);
        return data;
    }
    //метод получения сообщений чата
    public async Task<ApiResponse<nearby.Models.Messages>> GetMessagesAsync(int chatId, int page = 1, int limit = 50)
    {
        var response = await _apiClient.GetAsync($"chats/{chatId}/messages?page={page}&limit={limit}");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var messages = JsonConvert.DeserializeObject<nearby.Models.Messages>(json);
        return new ApiResponse<nearby.Models.Messages>("", messages);
    }
    //метод отметки сообщения как прочитаного 
    public async Task<ApiResponse<bool>> MarkMessagesAsReadAsync(int chatId, int messageId)
    {
        var response = await _apiClient.PutAsync($"chats/{chatId}/read?message_id={messageId}", null);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<bool>(json, response.IsSuccessStatusCode);
    }
    //метод редактирования сообщения
    public async Task<ApiResponse<Message>> EditMessageAsync(int messageId, string newContent)
    {
        var payload = new { content = newContent };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _apiClient.PutAsync($"chats/messages/{messageId}", content);
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        var msg = new Message
        {
            Id = messageId,
            Content = newContent
        };
        return new ApiResponse<Message>("", msg);
    }
    //метод удаления сообщения
    public async Task<ApiResponse<bool>> DeleteMessageAsync(int messageId)
    {
        var response = await _apiClient.DeleteAsync($"chats/messages/{messageId}");
        if (response is null)
        {
            throw new Exception("Ошибка подключения к серверу");
        }
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }
        return new ApiResponse<bool>(json, response.IsSuccessStatusCode);
    }
}