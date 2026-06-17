using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models
{
    public class FullNames
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("secondName")]
        public string SecondName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
    }
}
