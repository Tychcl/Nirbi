using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models;

public class Chat
{
    [JsonProperty("id")]
    public Guid? Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonIgnore]
    public bool IsPersonalChat { get; set; } = true;
    [JsonIgnore]
    public string Type { get; set; }
    [JsonIgnore]
    private Guid? _typeId { get; set; }
    [JsonProperty("chatTypeId")]
    public Guid? ChatTypeId 
    {
        get => _typeId;
        set
        {
            _typeId = value;
            Type = value.ToString() == "c0e3b007-f0bc-4c92-9f32-a87f1582812b" ? "Личный чат" : "Групповой чат";
            IsPersonalChat = value.ToString() == "c0e3b007-f0bc-4c92-9f32-a87f1582812b";
        }
    }

    [JsonProperty("chatUsers")]
    public List<Guid> ChatUsers { get; set; }
    [JsonIgnore]
    public List<FullNames>? ChatUsersFullNames { get; set; }
    [JsonIgnore]
    public Dictionary<Guid, string>? ChatUsersFullNamesDict { get; set; }

    [JsonIgnore]
    public ChatPreview? Preview{ get; set; }

}

public class ChatPreview
{
    [JsonProperty("id")]
    public Guid? Id { get; set; }
    [JsonProperty("chatId")]
    public Guid? ChatId { get; set; }
    [JsonProperty("sender")]
    public Guid? Sender { get; set; }
    [JsonProperty("createdAt")]
    public DateTime? CreatedAt { get; set; }
    [JsonProperty("content")]
    public string? Content { get; set; }
}

public class CreateMessagePrivateChatRequest
{
    [JsonProperty("recipient")]
    public Guid recipient { get; set; }

    [JsonProperty("content")]
    public string? content { get; set; }
}

public class CreateMessageGroupChatRequest
{
    [JsonProperty("chatId")]
    public Guid chatId { get; set; }

    [JsonProperty("content")]
    public string? content { get; set; }
}

public class UpdateMessageRequest
{
    [JsonProperty("messageId")]
    public Guid messageId { get; set; }

    [JsonProperty("content")]
    public string? content { get; set; }
}
