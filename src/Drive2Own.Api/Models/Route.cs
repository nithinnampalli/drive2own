namespace Drive2Own.Api.Models;

public enum TravelMode
{
    Car,
    Walk,
    Cycle,
    Run
}

public class Route
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TravelMode TravelMode { get; set; } = TravelMode.Car;
    public double TotalDistanceMeters { get; set; }
    public double TotalDurationSeconds { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<RoutePoint> Points { get; set; } = new List<RoutePoint>();
    public ICollection<RouteShare> Shares { get; set; } = new List<RouteShare>();
}
