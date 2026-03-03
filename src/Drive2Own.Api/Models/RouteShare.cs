namespace Drive2Own.Api.Models;

public class RouteShare
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public int SharedByUserId { get; set; }
    public string ShareToken { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public Route Route { get; set; } = null!;
    public User SharedByUser { get; set; } = null!;
}
