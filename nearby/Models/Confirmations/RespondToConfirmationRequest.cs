using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models
{
    public class RespondToConfirmationRequest
    {
        [JsonProperty("isAccepted")]
        public bool IsAccepted { get; set; }

        [JsonProperty("rejectionReason")]
        public string? RejectionReason { get; set; }
    }
}
