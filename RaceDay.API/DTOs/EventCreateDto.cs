using RaceDay.API.Models;

namespace RaceDay.API.DTOs;

public class EventCreateDto
{
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Distance { get; set; }
    public EventType EventType { get; set; }
}
