using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models.Task
{
    public class Position
    {
        [JsonProperty("lat")]
        public string? Lat { get; set; }
        [JsonProperty("lon")]
        public string? Lon { get; set; }
        [JsonProperty("display_name")]
        public string? DisplayName { get; set; }
    }
}
