using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nearby.Models.Task
{
    public class address_data
    {
        public Address? address;
    }

    public class Address
    {
        [JsonProperty("house_number")] //дом
        public string? House { get; set; }
        [JsonProperty("road")] //улица
        public string? Road { get; set; }
        [JsonProperty("suburb")] //микро район
        public string? Suburb { get; set; }
        [JsonProperty("city")] //город
        public string? City { get; set; }
        //[JsonProperty("state")] //область
        //public string? State { get; set; }
        //[JsonProperty("region")] //регионы 
        //public string? Region { get; set; }
        //[JsonProperty("country")] //страна
        //public string? Country { get; set; }
    }
}
