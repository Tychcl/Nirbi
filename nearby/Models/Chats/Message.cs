using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models;

public class Message
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("sender")]
    public Guid Sender { get; set; }
    [JsonIgnore]
    public string? SenderName { get; set; }

    [JsonProperty("chatId")]
    public Guid ChatId { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("isUpdated")]
    public bool IsUpdated { get; set; } = false;

    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [JsonProperty("content")]
    public string? Content { get; set; }
}
