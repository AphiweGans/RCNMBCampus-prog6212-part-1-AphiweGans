using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    /// <summary>Captures a result for an enrolment. Organiser only, and only the organiser who owns the event.</summary>
    /// <response code="201">Result created.</response>
    /// <response code="403">Not the organiser who owns the event.</response>
    /// <response code="404">Enrolment does not exist.</response>
    /// <response code="409">A result already exists for this enrolment.</response>
    [HttpPost("api/enrolments/{enrolmentId}/results")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int enrolmentId, ResultCreateDto dto)
    {
        var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrolment = await _context.Enrolments
            .Include(en => en.Category)
            .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(en => en.EnrolmentID == enrolmentId);

        if (enrolment == null || enrolment.Category?.Event == null) return NotFound();
        if (enrolment.Category.Event.OrganiserID != organiserId) return Forbid();

        var existing = await _context.Results.AnyAsync(r => r.EnrolmentID == enrolmentId);
        if (existing) return Conflict(new { message = "A result already exists for this enrolment." });

        var result = new Result
        {
            EnrolmentID = enrolmentId,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            Status = dto.Status
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return Created(string.Empty, result);
    }

    /// <summary>Returns the logged-in participant's personal results history. Participant only.</summary>
    /// <response code="200">Results returned.</response>
    [HttpGet("api/participants/me/results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMine()
    {
        var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var results = await _context.Results
            .Include(r => r.Enrolment)
            .ThenInclude(en => en!.Category)
            .Where(r => r.Enrolment!.ParticipantID == participantId)
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Returns the results leaderboard for a category. Public - no authentication required.</summary>
    /// <response code="200">Leaderboard returned.</response>
    [HttpGet("api/categories/{categoryId}/results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForCategory(int categoryId)
    {
        var results = await _context.Results
            .Include(r => r.Enrolment)
            .ThenInclude(en => en!.Participant)
            .Where(r => r.Enrolment!.CategoryID == categoryId)
            .OrderBy(r => r.Position)
            .ToListAsync();

        return Ok(results);
    }
}
