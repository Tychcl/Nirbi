using System.Text;
using Newtonsoft.Json;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;

namespace nearby.Services
{
    public class ChatService : IChatService
    {
        private readonly ApiClient _apiClient;

        public ChatService(ApiClient apiClient) => _apiClient = apiClient;

        public async Task<List<Chat>> GetChatsAsync()
        {
            var response = await _apiClient.GetAsync("chats");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<Chat>>(json) ?? new List<Chat>();
        }

        public async Task<List<User>> GetChatUsersAsync(Guid chatId)
        {
            var response = await _apiClient.GetAsync($"chat/{chatId}/chatUsers");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        // ---------- Сообщения ----------
        public async Task<List<Message>> GetMessagesAsync(Guid chatId)
        {
            var response = await _apiClient.GetAsync($"chats/{chatId}/messages");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<Message>>(json) ?? new List<Message>();
        }

        public async Task<Guid> SendPrivateMessageAsync(Guid recipientId, string content)
        {
            var body = new CreateMessagePrivateChatRequest { recipient = recipientId, content = content };
            var json = JsonConvert.SerializeObject(body);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync("messages/private", httpContent);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<Guid>(json)!;
        }

        public async Task<Guid> SendGroupMessageAsync(Guid chatId, string content)
        {
            var body = new CreateMessageGroupChatRequest { chatId = chatId, content = content };
            var json = JsonConvert.SerializeObject(body);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync("messages/group", httpContent);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<Guid>(json)!;
        }

        public async Task<Message> UpdateMessageAsync(Guid messageId, string newContent)
        {
            var body = new UpdateMessageRequest { messageId = messageId, content = newContent };
            var json = JsonConvert.SerializeObject(body);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PutAsync("messages", httpContent);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<Message>(json)!;
        }

        public async Task DeleteMessageAsync(Guid messageId)
        {
            var response = await _apiClient.DeleteAsync($"messages/{messageId}");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }

        public async Task<List<ChatPreview>> GetMessagePreviewsAsync(List<Guid?> chatIds)
        {
            //var json = JsonConvert.SerializeObject(chatIds);
            var url = "messages/preview?chatIds=" + string.Join("&chatIds=", chatIds);
            //var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.GetAsync(url);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<ChatPreview>>(json) ?? new List<ChatPreview>();
        }
    }
}