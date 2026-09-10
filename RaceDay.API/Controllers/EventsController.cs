using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context)
    {
        _context = context;
    }

    /// <summary>Lists all events. Public - no authentication required.</summary>
    /// <response code="200">List of events returned.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.Events.Include(e => e.Categories).ToListAsync();
        return Ok(events);
    }

    /// <summary>Returns a single event by id. Public - no authentication required.</summary>
    /// <response code="200">Event returned.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _context.Events.Include(e => e.Categories).FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null) return NotFound();
        return Ok(ev);
    }

    /// <summary>Creates a new event. Organiser only.</summary>
    /// <param name="dto">Event name, date, location, description, distance, and event type.</param>
    /// <response code="201">Event created.</response>
    /// <response code="401">No active session.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(EventCreateDto dto)
    {
        var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var ev = new Event
        {
            OrganiserID = organiserId,
            EventName = dto.EventName,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Description = dto.Description,
            Distance = dto.Distance,
            EventType = dto.EventType
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventID }, ev);
    }

    /// <summary>Updates an event. Organiser only, and only the organiser who owns the event.</summary>
    /// <response code="200">Event updated.</response>
    /// <response code="403">Not the organiser who owns this event.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, EventCreateDto dto)
    {
        var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _context.Events.FindAsync(id);

        if (ev == null) return NotFound();
        if (ev.OrganiserID != organiserId) return Forbid();

        ev.EventName = dto.EventName;
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location;
        ev.Description = dto.Description;
        ev.Distance = dto.Distance;
        ev.EventType = dto.EventType;

        await _context.SaveChangesAsync();

        return Ok(ev);
    }

    /// <summary>Deletes an event. Organiser only, and only the organiser who owns the event.</summary>
    /// <response code="204">Event deleted.</response>
    /// <response code="403">Not the organiser who owns this event.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _context.Events.FindAsync(id);

        if (ev == null) return NotFound();
        if (ev.OrganiserID != organiserId) return Forbid();

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
