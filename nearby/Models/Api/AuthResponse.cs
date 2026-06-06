using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models.Api
{
    public class AuthResponse
    {
        [JsonProperty("userId")]
        public Guid UserId { get; set; }
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
        [JsonProperty("tokenType")]
        public string TokenType { get; set; }
        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; } //в секундах
    }
}
