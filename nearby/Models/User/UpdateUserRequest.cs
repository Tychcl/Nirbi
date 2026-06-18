using Newtonsoft.Json;

namespace nearby.Models.Api
{
    public class UpdateUserRequest
    {
        [JsonProperty("Id")]
        public string? Id { get; set; }

        [JsonProperty("FirstName")]
        public string? FirstName { get; set; }

        [JsonProperty("SecondName")]
        public string? SecondName { get; set; }

        [JsonProperty("LastName")]
        public string? LastName { get; set; }

        [JsonProperty("Phone")]
        public string? Phone { get; set; }

        [JsonProperty("Email")]
        public string? Email { get; set; }

        [JsonProperty("BirthDate")]
        public string? BirthDate { get; set; }

        [JsonProperty("City")]
        public string? City { get; set; }

        [JsonProperty("About")]
        public string? About { get; set; }

        [JsonProperty("EducationPlace")]
        public string? EducationPlace { get; set; }

        [JsonProperty("EducationStartYear")]
        public string? EducationStartYear { get; set; }

        [JsonProperty("EducationEndYear")]
        public string? EducationEndYear { get; set; }

        [JsonProperty("EducationField")]
        public string? EducationField { get; set; }

        [JsonProperty("Vk")]
        public string? Vk { get; set; }

        [JsonProperty("Tg")]
        public string? Tg { get; set; }

        [JsonProperty("Max")]
        public string? Max { get; set; }

        [JsonProperty("CurrentPassword")]
        public string? CurrentPassword { get; set; }

        [JsonProperty("NewPassword")]
        public string? NewPassword { get; set; }
    }
}