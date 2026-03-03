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
public class RoutesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RoutesController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<RouteDto>>> GetRoutes()
    {
        var routes = await _db.Routes
            .Include(r => r.Points)
            .Where(r => r.UserId == CurrentUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(routes.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<RouteDto>> CreateRoute([FromBody] CreateRouteRequest request)
    {
        if (!Enum.TryParse<TravelMode>(request.TravelMode, true, out var mode))
            return BadRequest(new { message = "Invalid travel mode. Use: Car, Walk, Cycle, Run" });

        var route = new Models.Route
        {
            UserId = CurrentUserId,
            Name = request.Name,
            Description = request.Description,
            TravelMode = mode,
            StartedAt = request.StartedAt
        };

        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRoute), new { id = route.Id }, ToDto(route));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RouteDto>> GetRoute(int id)
    {
        var route = await _db.Routes
            .Include(r => r.Points)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == CurrentUserId);

        if (route == null) return NotFound();
        return Ok(ToDto(route));
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<RouteDto>> CompleteRoute(int id, [FromBody] CompleteRouteRequest request)
    {
        var route = await _db.Routes
            .Include(r => r.Points)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == CurrentUserId);

        if (route == null) return NotFound();
        if (route.IsCompleted) return Conflict(new { message = "Route already completed" });

        route.IsCompleted = true;
        route.CompletedAt = request.CompletedAt;
        route.TotalDistanceMeters = request.TotalDistanceMeters;
        route.TotalDurationSeconds = request.TotalDurationSeconds;

        await _db.SaveChangesAsync();
        return Ok(ToDto(route));
    }

    [HttpPost("{id}/points")]
    public async Task<ActionResult<RoutePointDto>> AddPoint(int id, [FromBody] AddRoutePointRequest request)
    {
        var route = await _db.Routes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == CurrentUserId);
        if (route == null) return NotFound();

        var point = new RoutePoint
        {
            RouteId = id,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Altitude = request.Altitude,
            SpeedMps = request.SpeedMps,
            Sequence = request.Sequence,
            RecordedAt = request.RecordedAt
        };

        _db.RoutePoints.Add(point);
        await _db.SaveChangesAsync();

        return Ok(ToPointDto(point));
    }

    [HttpPost("{id}/points/batch")]
    public async Task<ActionResult> AddPointsBatch(int id, [FromBody] List<AddRoutePointRequest> requests)
    {
        var route = await _db.Routes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == CurrentUserId);
        if (route == null) return NotFound();

        var points = requests.Select(r => new RoutePoint
        {
            RouteId = id,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            Altitude = r.Altitude,
            SpeedMps = r.SpeedMps,
            Sequence = r.Sequence,
            RecordedAt = r.RecordedAt
        }).ToList();

        _db.RoutePoints.AddRange(points);
        await _db.SaveChangesAsync();

        return Ok(new { count = points.Count });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        var route = await _db.Routes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == CurrentUserId);
        if (route == null) return NotFound();

        _db.Routes.Remove(route);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static RouteDto ToDto(Models.Route route) =>
        new(route.Id, route.UserId, route.Name, route.Description,
            route.TravelMode.ToString(), route.TotalDistanceMeters, route.TotalDurationSeconds,
            route.StartedAt, route.CompletedAt, route.IsCompleted, route.CreatedAt,
            route.Points.OrderBy(p => p.Sequence).Select(ToPointDto).ToList());

    private static RoutePointDto ToPointDto(RoutePoint p) =>
        new(p.Id, p.Latitude, p.Longitude, p.Altitude, p.SpeedMps, p.Sequence, p.RecordedAt);
}
