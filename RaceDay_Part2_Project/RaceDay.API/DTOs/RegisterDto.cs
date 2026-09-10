using RaceDay.API.Models;

namespace RaceDay.API.DTOs;

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    // Organiser-specific (optional)
    public string? Phone { get; set; }

    // Participant-specific (optional)
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}
