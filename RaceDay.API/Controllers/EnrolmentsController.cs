using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
    {
        _context = context;
    }

    /// <summary>Enrols the logged-in participant into a category. Participant only.</summary>
    /// <response code="201">Enrolment created.</response>
    /// <response code="403">Logged in as Organiser, not Participant.</response>
    /// <response code="404">Category does not exist.</response>
    /// <response code="409">Already enrolled in this category.</response>
    [HttpPost("api/categories/{categoryId}/enrol")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(int categoryId)
    {
        var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var category = await _context.Categories.FindAsync(categoryId);
        if (category == null) return NotFound(new { message = "Category not found." });

        var alreadyEnrolled = await _context.Enrolments
            .AnyAsync(en => en.ParticipantID == participantId && en.CategoryID == categoryId);

        if (alreadyEnrolled)
        {
            return Conflict(new { message = "Already enrolled in this category." });
        }

        var enrolment = new Enrolment
        {
            ParticipantID = participantId,
            CategoryID = categoryId
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return Created(string.Empty, enrolment);
    }

    /// <summary>Returns the logged-in participant's own enrolments. Participant only.</summary>
    /// <response code="200">Enrolments returned.</response>
    [HttpGet("api/enrolments/me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMine()
    {
        var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrolments = await _context.Enrolments
            .Include(en => en.Category)
            .Where(en => en.ParticipantID == participantId)
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Returns all enrolments for an event. Organiser only, and only the organiser who owns the event.</summary>
    /// <response code="200">Enrolments returned.</response>
    /// <response code="403">Not the organiser who owns this event.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("api/events/{eventId}/enrolments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _context.Events.FindAsync(eventId);

        if (ev == null) return NotFound();
        if (ev.OrganiserID != organiserId) return Forbid();

        var enrolments = await _context.Enrolments
            .Include(en => en.Participant)
            .Include(en => en.Category)
            .Where(en => en.Category!.EventID == eventId)
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Cancels an enrolment. Participant only, and only their own enrolment.</summary>
    /// <response code="204">Enrolment cancelled.</response>
    /// <response code="403">Not the participant who owns this enrolment.</response>
    /// <response code="404">Enrolment does not exist.</response>
    [HttpDelete("api/enrolments/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var enrolment = await _context.Enrolments.FindAsync(id);

        if (enrolment == null) return NotFound();
        if (enrolment.ParticipantID != participantId) return Forbid();

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
