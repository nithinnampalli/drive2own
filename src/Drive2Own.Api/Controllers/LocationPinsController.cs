using System.Security.Claims;
using Drive2Own.Api.Data;
using Drive2Own.Api.DTOs;
using Drive2Own.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Drive2Own.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationPinsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LocationPinsController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<LocationPinDto>>> GetPins()
    {
        var pins = await _db.LocationPins
            .Where(p => p.UserId == CurrentUserId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToDto(p))
            .ToListAsync();
        return Ok(pins);
    }

    [HttpPost]
    public async Task<ActionResult<LocationPinDto>> CreatePin([FromBody] CreateLocationPinRequest request)
    {
        var pin = new LocationPin
        {
            UserId = CurrentUserId,
            Title = request.Title,
            Description = request.Description,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IconColor = request.IconColor ?? "#FF5733"
        };

        _db.LocationPins.Add(pin);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPin), new { id = pin.Id }, ToDto(pin));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationPinDto>> GetPin(int id)
    {
        var pin = await _db.LocationPins.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (pin == null) return NotFound();
        return Ok(ToDto(pin));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LocationPinDto>> UpdatePin(int id, [FromBody] UpdateLocationPinRequest request)
    {
        var pin = await _db.LocationPins.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (pin == null) return NotFound();

        pin.Title = request.Title;
        pin.Description = request.Description;
        pin.IconColor = request.IconColor;

        await _db.SaveChangesAsync();
        return Ok(ToDto(pin));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePin(int id)
    {
        var pin = await _db.LocationPins.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (pin == null) return NotFound();

        _db.LocationPins.Remove(pin);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static LocationPinDto ToDto(LocationPin pin) =>
        new(pin.Id, pin.UserId, pin.Title, pin.Description, pin.Latitude, pin.Longitude, pin.IconColor, pin.CreatedAt);
}
