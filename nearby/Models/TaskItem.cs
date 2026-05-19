using nearby.Classes;

namespace nearby.Models;

public class TaskItem : Clone<TaskItem>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty; //
    public string? Description { get; set; } //
    public int NeededVolunteers { get; set; } //
    public string Priority { get; set; } = "medium"; //
    public string? Location { get; set; } //
    public decimal Reward { get; set; } //
    public string Status { get; set; } = "searching";
    public int CreatorId { get; set; }
    public string CreatorFIO { get; set; }
    public DateTime Deadline { get; set; } //
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}