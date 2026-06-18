using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models
{
    public class UsersSearchResponse
    {
        [JsonProperty("total")]
        public int Total {  get; set; }
        [JsonProperty("items")]
        public List<User> Items { get; set; }
    }
}
