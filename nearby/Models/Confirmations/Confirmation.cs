using Newtonsoft.Json;

namespace nearby.Models
{
    public class Confirmation
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonIgnore]
        public string? Type { get; set; }
        [JsonIgnore]
        private string? _confirmationType { get; set; }
        [JsonProperty("confirmationType")]
        public string? ConfirmationType 
        {
            get => _confirmationType;
            set
            {
                _confirmationType = value;
                Type = value switch
                {
                    "Respond to minor task" => "Отклик",
                    "Invite to task" => "Приглашение"
                };
            }
        }

        [JsonProperty("entityId")]
        public Guid EntityId { get; set; }
        [JsonProperty("initiatorId")]
        public Guid InitiatorId { get; set; }
        [JsonProperty("reviewerId")]
        public Guid ReviewerId { get; set; }
        [JsonProperty("status")]
        public string? Status { get; set; }
        [JsonProperty("metaData")]
        public string? MetaData { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("expiresAt")]
        public DateTime ExpiresAt { get; set; }
        [JsonProperty("respondedAt")]
        public DateTime? RespondedAt { get; set; }
        [JsonProperty("rejectionReason")]
        public string? RejectionReason { get; set; }
        [JsonIgnore]
        public string? TaskName { get; set; }
        [JsonIgnore]
        public User? RelatedUser { get; set; }

        public bool IsPending => Status == "Created";
        public bool IsAccepted => Status == "Accepted";
        public bool IsRejected => Status == "Rejected";
        public bool IsRevoked => Status == "Revoked";
    }
}