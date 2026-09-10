namespace RaceDay.API.DTOs;

public class CategoryCreateDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int MaxParticipants { get; set; } = 100;
    public decimal EntryFee { get; set; }
}
