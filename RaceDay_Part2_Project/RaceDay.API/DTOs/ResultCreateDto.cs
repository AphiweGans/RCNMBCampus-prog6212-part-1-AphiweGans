namespace RaceDay.API.DTOs;

public class ResultCreateDto
{
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string Status { get; set; } = "Finished";
}
