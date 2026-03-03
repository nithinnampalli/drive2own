using System.Security.Claims;
using Drive2Own.Api.Data;
using Drive2Own.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Drive2Own.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var userId = CurrentUserId;

        var routes = await _db.Routes
            .Where(r => r.UserId == userId && r.IsCompleted)
            .ToListAsync();

        var pinCount = await _db.LocationPins.CountAsync(p => p.UserId == userId);

        var totalDistance = routes.Sum(r => r.TotalDistanceMeters);
        var totalDuration = routes.Sum(r => r.TotalDurationSeconds);

        var recentRoutes = routes
            .OrderByDescending(r => r.StartedAt)
            .Take(10)
            .Select(r => new RouteMetricDto(r.Id, r.Name, r.TravelMode.ToString(), r.TotalDistanceMeters, r.TotalDurationSeconds, r.StartedAt))
            .ToList();

        var distanceByMode = routes
            .GroupBy(r => r.TravelMode.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(r => r.TotalDistanceMeters));

        return Ok(new DashboardDto(
            routes.Count,
            totalDistance,
            totalDuration,
            pinCount,
            recentRoutes,
            distanceByMode
        ));
    }
}
