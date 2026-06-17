using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models
{
    public class CreateConfirmationRequest
    {
        [JsonProperty("confirmationType")]
        public string? ConfirmationType { get; set; }

        [JsonProperty("entityId")]
        public Guid EntityId { get; set; }

        [JsonProperty("expirationHours")]
        public int ExpirationHours { get; set; }

        [JsonProperty("metaData")]
        public Metadata MetaData { get; set; }

        [JsonProperty("reviewerId")]
        public Guid ReviewerId { get; set; }

    }

    public class Metadata
    {
        [JsonProperty("applicantUsername")]
        public string ApplicantUsername;

        [JsonProperty("taskName")]
        public string TaskName;
    }
}
