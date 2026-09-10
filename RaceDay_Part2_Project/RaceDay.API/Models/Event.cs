namespace RaceDay.API.Models;

// NOTE: Distance and EventType were added here beyond the original Part 1 ERD
// to satisfy the Part 2 functional requirement that "each event must capture
// a name, description, date, location, distance, and event type (run, walk, cycle)".
// This deviation is documented in the README as required by the brief.
public class Event
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Distance { get; set; }
    public EventType EventType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Organiser? Organiser { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
