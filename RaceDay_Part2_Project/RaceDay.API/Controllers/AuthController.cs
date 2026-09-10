using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;

    public AuthController(RaceDayContext context)
    {
        _context = context;
    }

    /// <summary>Registers a new user as either an Organiser or a Participant.</summary>
    /// <param name="dto">Full name, email, password, and role (Organiser or Participant).</param>
    /// <response code="201">Account created successfully.</response>
    /// <response code="400">Missing required fields, or an account with this email already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "FullName, Email, and Password are required." });
        }

        var emailInUse = await _context.Organisers.AnyAsync(o => o.Email == dto.Email)
                          || await _context.Participants.AnyAsync(p => p.Email == dto.Email);

        if (emailInUse)
        {
            return BadRequest(new { message = "An account with this email already exists." });
        }

        var hash = PasswordService.Hash(dto.Password);

        if (dto.Role == UserRole.Organiser)
        {
            var organiser = new Organiser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hash,
                Phone = dto.Phone
            };

            _context.Organisers.Add(organiser);
            await _context.SaveChangesAsync();

            return Created(string.Empty, new
            {
                organiser.OrganiserID,
                organiser.FullName,
                organiser.Email,
                Role = "Organiser"
            });
        }
        else
        {
            var participant = new Participant
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hash,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return Created(string.Empty, new
            {
                participant.ParticipantID,
                participant.FullName,
                participant.Email,
                Role = "Participant"
            });
        }
    }

    /// <summary>Authenticates a user and starts a cookie session carrying their role.</summary>
    /// <param name="dto">Registered email and password.</param>
    /// <response code="200">Login successful; session cookie issued.</response>
    /// <response code="401">Email or password is incorrect.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.Email == dto.Email);
        if (organiser != null && PasswordService.Verify(dto.Password, organiser.PasswordHash))
        {
            await SignInAsync(organiser.OrganiserID.ToString(), organiser.FullName, UserRole.Organiser);
            return Ok(new { organiser.OrganiserID, organiser.FullName, Role = "Organiser" });
        }

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.Email == dto.Email);
        if (participant != null && PasswordService.Verify(dto.Password, participant.PasswordHash))
        {
            await SignInAsync(participant.ParticipantID.ToString(), participant.FullName, UserRole.Participant);
            return Ok(new { participant.ParticipantID, participant.FullName, Role = "Participant" });
        }

        return Unauthorized(new { message = "Invalid email or password." });
    }

    /// <summary>Ends the current session.</summary>
    /// <response code="200">Logged out successfully.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Logged out." });
    }

    private async Task SignInAsync(string userId, string fullName, UserRole role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
