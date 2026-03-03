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
public class SharesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SharesController(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    private int? CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return value != null ? int.Parse(value) : null;
        }
    }

    [HttpPost("routes/{routeId}")]
    [Authorize]
    public async Task<ActionResult<RouteShareDto>> ShareRoute(int routeId, [FromBody] CreateRouteShareRequest request)
    {
        var userId = CurrentUserId;
        var route = await _db.Routes.FirstOrDefaultAsync(r => r.Id == routeId && r.UserId == userId);
        if (route == null) return NotFound();

        var share = new RouteShare
        {
            RouteId = routeId,
            SharedByUserId = userId!.Value,
            ShareToken = Guid.NewGuid().ToString("N"),
            IsPublic = request.IsPublic,
            ExpiresAt = request.ExpiresInHours.HasValue
                ? DateTime.UtcNow.AddHours(request.ExpiresInHours.Value)
                : null
        };

        _db.RouteShares.Add(share);
        await _db.SaveChangesAsync();

        var baseUrl = GetBaseUrl();
        return Ok(ToDto(share, baseUrl));
    }

    [HttpGet("{token}")]
    public async Task<ActionResult<SharedRouteDto>> GetSharedRoute(string token)
    {
        var share = await _db.RouteShares
            .Include(s => s.Route).ThenInclude(r => r.Points)
            .Include(s => s.SharedByUser)
            .FirstOrDefaultAsync(s => s.ShareToken == token);

        if (share == null) return NotFound();
        if (share.ExpiresAt.HasValue && share.ExpiresAt.Value < DateTime.UtcNow)
            return Gone();

        var routeDto = new RouteDto(
            share.Route.Id, share.Route.UserId, share.Route.Name, share.Route.Description,
            share.Route.TravelMode.ToString(), share.Route.TotalDistanceMeters, share.Route.TotalDurationSeconds,
            share.Route.StartedAt, share.Route.CompletedAt, share.Route.IsCompleted, share.Route.CreatedAt,
            share.Route.Points.OrderBy(p => p.Sequence)
                .Select(p => new RoutePointDto(p.Id, p.Latitude, p.Longitude, p.Altitude, p.SpeedMps, p.Sequence, p.RecordedAt))
                .ToList()
        );

        return Ok(new SharedRouteDto(routeDto, share.SharedByUser.DisplayName, share.CreatedAt));
    }

    [HttpGet("my-shares")]
    [Authorize]
    public async Task<ActionResult<List<RouteShareDto>>> GetMyShares()
    {
        var userId = CurrentUserId;
        var baseUrl = GetBaseUrl();

        var shares = await _db.RouteShares
            .Where(s => s.SharedByUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(shares.Select(s => ToDto(s, baseUrl)).ToList());
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteShare(int id)
    {
        var share = await _db.RouteShares.FirstOrDefaultAsync(s => s.Id == id && s.SharedByUserId == CurrentUserId);
        if (share == null) return NotFound();

        _db.RouteShares.Remove(share);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return request != null ? $"{request.Scheme}://{request.Host}" : string.Empty;
    }

    private static ObjectResult Gone() => new ObjectResult(new { message = "Share link has expired" }) { StatusCode = 410 };

    private static RouteShareDto ToDto(RouteShare share, string baseUrl) =>
        new(share.Id, share.RouteId, share.ShareToken, share.IsPublic, share.CreatedAt, share.ExpiresAt,
            $"{baseUrl}/shared/{share.ShareToken}");
}
