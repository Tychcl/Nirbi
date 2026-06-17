using nearby.Classes;
using Newtonsoft.Json;

namespace nearby.Models;

public class TaskItem : Clone<TaskItem>
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    [JsonProperty("name")]
    public string Title { get; set; } = string.Empty;
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("latitude")]
    public double? Latitude { get; set; }
    [JsonProperty("longitude")]
    public double? Longitude { get; set; }
    [JsonProperty("numberVolunteers")]
    public int NeededVolunteers { get; set; }
    public string? Location { get; set; }
    [JsonProperty("encouragement")]
    public decimal Reward { get; set; }
    [JsonProperty("status")]
    public string Status { get; set; } = "searching";
    [JsonProperty("consumerId")]
    public Guid CreatorId { get; set; }
    [JsonProperty("fileCollectionId")]
    public Guid FileCollectionId { get; set; }
    public string CreatorFIO { get; set; }
    //public string Priority { get; set; } = "medium";
    //public DateTime Deadline { get; set; }
    //public DateTime CreatedAt { get; set; } = DateTime.Now;
    //public DateTime UpdatedAt { get; set; } = DateTime.Now;
}