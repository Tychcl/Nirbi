using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using nearby.Classes;
using Newtonsoft.Json;

namespace nearby.Models
{
    public class User: Clone<User>
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("secondName")]
        public string Surname { get; set; }
        [JsonProperty("firstName")]
        public string Name { get; set; }
        [JsonProperty("lastName")]
        public string Patronymic { get; set; }
        [JsonProperty("phone")]
        public string? Phone { get; set; }
        [JsonProperty("email")]
        public string? Email { get; set; }
        [JsonProperty("city")]
        public string? City { get; set; }
        [JsonProperty("birthDate")]
        public DateTime? BirthDate { get; set; }
        [JsonProperty("about")]
        public string? About { get; set; }
        [JsonProperty("educationPlace")]
        public string? EducationInstitution { get; set; }
        public string? EducationDegree { get; set; }
        [JsonProperty("educationField")]
        public string? EducationField { get; set; }
        [JsonProperty("educationStartYear")]
        public int? EducationStartYear { get; set; }
        [JsonProperty("educationEndYear")]
        public int? EducationEndYear { get; set; }

        public string FullName
        {
            get => $"{Surname} {Name} {Patronymic}";
        }
    }
}
